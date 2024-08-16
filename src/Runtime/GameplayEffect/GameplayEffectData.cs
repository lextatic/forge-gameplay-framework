using GameplayTags.Runtime.Attribute;
using System.Reflection;
using System.Reflection.Emit;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace GameplayTags.Runtime.GameplayEffect;
#pragma warning disable SA1600

public enum StackPolicy : byte
{
	AggregateBySource,
	AggregateByTarget,
}

public enum StackLevelPolicy : byte
{
	AggregateLevels,
	SegregateLevels,
}

public enum StackLevelOverridePolicy : byte
{
	AlwaysKeep,
	AlwaysOverride,
	KeepHighest,
	KeepLowest,
}

public enum StackApplicationRefreshPolicy : byte
{
	RefreshOnSuccessfulApplication,
	NeverRefresh,
}

public enum StackRemovalPolicy : byte
{
	ClearEntireStack,
	RemoveSingleStackAndRefreshDuration,
}

public enum StackApplicationResetPeriodPolicy : byte
{
	ResetOnSuccessfulApplication,
	NeverReset,
}

public struct StackingData
{
	public int StackLimit; // All stackable effects
	public StackPolicy StackPolicy; // All stackable effects
	public StackLevelPolicy StackLevelPolicy; // All stackable effects
	public StackRemovalPolicy StackRemovalPolicy; // Aff stackable effects, infinite effects removal will count as expiration
	public StackApplicationRefreshPolicy? StackApplicationRefreshPolicy; // Effects with duration
	public StackLevelOverridePolicy? StackLevelOverridePolicy; // Effects with LevelStacking == AggregateLevels
	public StackApplicationResetPeriodPolicy? StackApplicationResetPeriodPolicy; // Periodic effects
}

public enum ModifierOperation : byte
{
	Add,
	Multiply,
	Divide,
	Override,
}

public struct Modifier
{
	public TagName Attribute;
	public ModifierOperation Operation;
	public ScalableInt Value;
}

public enum DurationType : byte
{
	Instant,
	Infinite,
	HasDuration,
}

public struct DurationData
{
	public DurationType Type;
	public float Duration;
}

public struct PeriodicData
{
	public float Period;
	public bool ExecuteOnApplication;
}

public class GameplayEffectData
{
	public List<Modifier> Modifiers { get; } = new ();
	//public List<Executions> Executions { get; } = new ();

	public string Name { get; }

	public DurationData DurationData { get; }

	public StackingData? StackingData { get; }

	public PeriodicData? PeriodicData { get; }

	public GameplayEffectData(
		string name,
		DurationData durationData,
		StackingData? stackingData,
		PeriodicData? periodicData)
	{
		Name = name;
		DurationData = durationData;
		StackingData = stackingData;
		PeriodicData = periodicData;

		// Should I really throw? Or just ignore (force null) the periodic data?
		if (periodicData.HasValue && durationData.Type == DurationType.Instant)
		{
			throw new Exception("Periodic effects can't be set as instant.");
		}

		// Should I really throw? Or just ignore (force null) the periodic data?
		if (durationData.Type != DurationType.HasDuration && durationData.Duration != 0)
		{
			throw new Exception($"Can't set duration if {nameof(DurationType)} is set to {durationData.Type}.");
		}

		if (stackingData.HasValue)
		{
			if (durationData.Type == DurationType.Instant)
			{
				throw new Exception($"{DurationType.Instant} effects can't have stacks.");
			}

			if (stackingData.Value.StackApplicationResetPeriodPolicy.HasValue != PeriodicData.HasValue)
			{
				throw new Exception($"Both {nameof(PeriodicData)} and {nameof(StackApplicationResetPeriodPolicy)} " +
					$"must be either defined or undefined.");
			}

			if (stackingData.Value.StackLevelPolicy == StackLevelPolicy.AggregateLevels !=
				stackingData.Value.StackLevelOverridePolicy.HasValue)
			{
				throw new Exception($"If {nameof(StackLevelPolicy)} is set {StackLevelPolicy.AggregateLevels}, " +
					$"{nameof(StackLevelOverridePolicy)} must be defined. And not defined if otherwise.");
			}

			if (durationData.Type == DurationType.HasDuration !=
				stackingData.Value.StackApplicationRefreshPolicy.HasValue)
			{
				throw new Exception($"Effects set as {DurationType.HasDuration} must define " +
					$" {nameof(StackApplicationRefreshPolicy)} and not define it if otherwise.");
			}
		}
	}
}

public struct GameplayEffectContext
{
	public object Instigator; // Entity responsible for causing the action or event (eg. Character, NPC, Environment)
	public object EffectCauser; // The actual entity that caused the effect (eg. Weapon, Projectile, Trap)
}

public class GameplayEffect
{
	public GameplayEffectData EffectData { get; }

	public float Level { get; set; }

	public GameplayEffectContext Context { get; }

	public GameplayEffect(GameplayEffectData effectData, float level, GameplayEffectContext context)
	{
		EffectData = effectData;
		Level = level;
		Context = context;
	}

	public void LevelUp()
	{
		Level++;
	}

	internal void Execute(List<GameplayModifierEvaluatedData> modifiersEvaluatedData)
	{
		foreach(var modifier in modifiersEvaluatedData)
		{
			switch (modifier.ModifierOperation)
			{
				default:
				case ModifierOperation.Add:
					modifier.Attribute.AddToBaseValue(modifier.Magnitude);
					break;

				case ModifierOperation.Multiply:
					// Multiply value
					break;

				case ModifierOperation.Divide:
					// Divide value
					break;

				case ModifierOperation.Override:
					// Override value
					break;
			}
		}
	}
}

internal class ActiveGameplayEffect
{
	private readonly List<GameplayModifierEvaluatedData> _modifiersEvaluatedData = new ();

	private float _internalTime;

	internal GameplayEffect GameplayEffect { get; }

	internal float RemainingDuration { get; private set; }

	internal float NextPeriodicTick { get; private set; }

	internal float ExecutionCount { get; private set; }

	internal bool IsExpired => GameplayEffect.EffectData.DurationData.Type == DurationType.HasDuration &&
		RemainingDuration <= 0;

	internal ActiveGameplayEffect(GameplayEffect gameplayEffect, List<GameplayModifierEvaluatedData> modifiersEvaluatedData)
	{
		GameplayEffect = gameplayEffect;
		_modifiersEvaluatedData = modifiersEvaluatedData;
	}

	internal void Apply()
	{
		_internalTime = 0;
		ExecutionCount = 0;
		RemainingDuration = GameplayEffect.EffectData.DurationData.Duration;

		if (GameplayEffect.EffectData.PeriodicData.HasValue)
		{
			if (GameplayEffect.EffectData.PeriodicData.Value.ExecuteOnApplication)
			{
				GameplayEffect.Execute(_modifiersEvaluatedData);
				ExecutionCount++;
			}

			NextPeriodicTick = GameplayEffect.EffectData.PeriodicData.Value.Period;
		}
		else
		{
			foreach (var modifier in _modifiersEvaluatedData)
			{
				modifier.Attribute.ApplyModifier(modifier.Magnitude);
			}
		}
	}

	internal void Unapply()
	{
		if (!GameplayEffect.EffectData.PeriodicData.HasValue)
		{
			foreach (var modifier in _modifiersEvaluatedData)
			{
				modifier.Attribute.ApplyModifier(-modifier.Magnitude);
			}
		}
	}

	internal void Update(float deltaTime)
	{
		_internalTime += deltaTime;

		if (GameplayEffect.EffectData.PeriodicData.HasValue && _internalTime >= NextPeriodicTick)
		{
			GameplayEffect.Execute(_modifiersEvaluatedData);
			ExecutionCount++;

			NextPeriodicTick += GameplayEffect.EffectData.PeriodicData.Value.Period;
		}

		if (GameplayEffect.EffectData.DurationData.Type == DurationType.HasDuration)
		{
			RemainingDuration -= deltaTime;
		}

		if (IsExpired)
		{
			Unapply();
		}
	}
}

public class GameplayEffectsManager
{
	private readonly List<ActiveGameplayEffect> _activeEffects = new ();

	// Could be a list of attributeSets;
	private AttributeSet _attributeSet;

	public GameplayEffectsManager(AttributeSet ownerAttributeSet)
	{
		_attributeSet = ownerAttributeSet;
	}

	public void ApplyEffect(GameplayEffect gameplayEffect)
	{
		var evaluatedModifiers = EvaluateModifiers(gameplayEffect);

		if (gameplayEffect.EffectData.DurationData.Type != DurationType.Instant)
		{
			var activeEffect = new ActiveGameplayEffect(gameplayEffect, evaluatedModifiers);
			_activeEffects.Add(activeEffect);
			activeEffect.Apply();
		}
		else
		{
			// This path is called "Execute" and should work for instant effects
			gameplayEffect.Execute(evaluatedModifiers);
		}
	}

	public void UnapplyEffect(GameplayEffect gameplayEffect)
	{
		//_activeEffects.Remove(gameplayEffect);
	}

	// What if the game is a turn based game?
	public void UpdateEffects(float deltaTime)
	{
		foreach (var effect in _activeEffects)
		{
			effect.Update(deltaTime);
		}

		_activeEffects.RemoveAll(e => e.IsExpired);
	}

	private List<GameplayModifierEvaluatedData> EvaluateModifiers(GameplayEffect gameplayEffect)
	{
		var modifiersEvaluatedData = new List<GameplayModifierEvaluatedData>();

		foreach (var modifier in gameplayEffect.EffectData.Modifiers)
		{
			modifiersEvaluatedData.Add(new GameplayModifierEvaluatedData
			{
				Attribute = _attributeSet.AttributesMap[modifier.Attribute],
				ModifierOperation = modifier.Operation,
				Magnitude = modifier.Value.GetValue(gameplayEffect.Level),
			});
		}

		return modifiersEvaluatedData;
	}
}
