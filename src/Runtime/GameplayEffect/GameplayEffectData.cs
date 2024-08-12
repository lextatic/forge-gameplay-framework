using GameplayTags.Runtime.Attribute;

namespace GameplayTags.Runtime.GameplayEffect;
#pragma warning disable SA1600

public enum StackingType : byte
{
	AggregateBySource,
	AggregateByTarget,
}

// Neeed if any Aggregate
public enum StackingDurationPolicy : byte
{
	RefreshOnSuccessfulApplication,
	NeverRefresh,
}

// Neeed if any Aggregate
public enum StackingExpirationPolicy : byte
{
	ClearEntireStack,
	RemoveSingleStackAndRefreshDuration,
	/** The duration of the gameplay effect is refreshed. This essentially makes the effect infinite in duration. This can be used to manually handle stack decrements via OnStackCountChange callback */
	RefreshDuration,
}

// Neeed if any Aggregate and PeriodData is existing
// Could be in the StackingData ou PeriodicData
public enum StackingPeriodPolicy : byte
{
	ResetOnSuccessfulApplication,
	NeverReset,
}

public struct StackingData
{
	public StackingType Type;
	public int StackLimit;
	public StackingDurationPolicy DurationPolicy;
	public StackingExpirationPolicy ExpirationPolicy;
	public StackingPeriodPolicy? PeriodPolicy;
}

public struct Modifier
{
	public TagName Attribute;
	public int Value;
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

	public string Name { get; }

	public float BaseMagnitude { get; }

	public DurationData DurationData { get; }

	public StackingData? StackingData { get; }

	public PeriodicData? PeriodicData { get; }

	public GameplayEffectData(
		string name,
		float baseMagnitude,
		DurationData durationData,
		StackingData? stackingData,
		PeriodicData? periodicData)
	{
		Name = name;
		BaseMagnitude = baseMagnitude;
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
			throw new Exception($"Can't set duration if DurationType is set to {durationData.Type}.");
		}

		if (stackingData.HasValue)
		{
			if (durationData.Type == DurationType.Instant)
			{
				throw new Exception("Instant effects can't have stacks.");
			}

			if (stackingData.Value.PeriodPolicy.HasValue != PeriodicData.HasValue)
			{
				throw new Exception("Both PeriodicData and PeriodPolicy must be either defined or undefined.");
			}
		}
	}
}

public struct GameplayEffectContext
{
	public object Instigator;
	public object EffectCauser;
}

public class GameplayEffect
{
	public GameplayEffectData EffectData { get; }

	public float Level { get; }

	public GameplayEffectContext Context { get; }

	public GameplayEffect(GameplayEffectData effectData, float level, GameplayEffectContext context)
	{
		EffectData = effectData;
		Level = level;
		Context = context;
	}

	public float GetScaledMagnitude()
	{
		return EffectData.BaseMagnitude * Level;
	}

	public void Execute(AttributeSet targetAttributeSet)
	{
		foreach (var modifier in EffectData.Modifiers)
		{
			targetAttributeSet.AttributesMap[modifier.Attribute].AddToBaseValue(modifier.Value);
		}
	}
}

public class ActiveGameplayEffect
{
	private float _internalTime;

	public GameplayEffect GameplayEffect { get; }

	public AttributeSet TargetAttributeSet { get; }

	public float RemainingDuration { get; private set; }

	public float NextPeriodicTick { get; private set; }

	public float ExecutionCount { get; private set; }

	public bool IsExpired => GameplayEffect.EffectData.DurationData.Type == DurationType.HasDuration &&
		RemainingDuration <= 0;

	public ActiveGameplayEffect(GameplayEffect gameplayEffect, AttributeSet targetAttributeSet)
	{
		GameplayEffect = gameplayEffect;
		TargetAttributeSet = targetAttributeSet;
	}

	public void Apply()
	{
		_internalTime = 0;
		ExecutionCount = 0;
		RemainingDuration = GameplayEffect.EffectData.DurationData.Duration;

		foreach (var modifier in GameplayEffect.EffectData.Modifiers)
		{
			if (GameplayEffect.EffectData.PeriodicData.HasValue)
			{
				if (GameplayEffect.EffectData.PeriodicData.Value.ExecuteOnApplication)
				{
					GameplayEffect.Execute(TargetAttributeSet);
					ExecutionCount++;
				}

				NextPeriodicTick = GameplayEffect.EffectData.PeriodicData.Value.Period;
			}
			else
			{
				TargetAttributeSet.AttributesMap[modifier.Attribute].ApplyModifier(modifier.Value);
			}
		}
	}

	public void Unapply()
	{
		if (!GameplayEffect.EffectData.PeriodicData.HasValue)
		{
			foreach (var modifier in GameplayEffect.EffectData.Modifiers)
			{
				TargetAttributeSet.AttributesMap[modifier.Attribute].ApplyModifier(-modifier.Value);
			}
		}
	}

	public void Update(float deltaTime)
	{
		_internalTime += deltaTime;

		if (GameplayEffect.EffectData.PeriodicData.HasValue && _internalTime >= NextPeriodicTick)
		{
			GameplayEffect.Execute(TargetAttributeSet);
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
		if (gameplayEffect.EffectData.DurationData.Type != DurationType.Instant)
		{
			var activeEffect = new ActiveGameplayEffect(gameplayEffect, _attributeSet);
			_activeEffects.Add(activeEffect);
			activeEffect.Apply();
		}
		else
		{
			// This path is called "Execute" and should work for instant effects
			gameplayEffect.Execute(_attributeSet);
		}
	}

	private void ExecuteEffect(Attribute.Attribute attribute, Modifier modifier)
	{
		// Do some pre-evaluations about the modifier
		
		//if (attribute.PreGameplayEffectExecute(modifier))
		//{
			// This gives a chance for the AttributeSet and others to manipulate the modifier before applying
		//}

		// This is probably a bit more complicated than that as it should use the previously calculated evaluation
		attribute.AddToBaseValue(modifier.Value);

		//attribute.PostGameplayEffectExecute(modifier);
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
}
