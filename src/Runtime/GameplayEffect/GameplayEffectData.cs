using GameplayTags.Runtime.Attribute;

namespace GameplayTags.Runtime.GameplayEffect;
#pragma warning disable SA1600

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

	public PeriodicData? PeriodicData { get; }

	public GameplayEffectData(
		string name,
		float baseMagnitude,
		DurationData durationData,
		PeriodicData? periodicData)
	{
		Name = name;
		BaseMagnitude = baseMagnitude;
		DurationData = durationData;
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

	public GameplayEffect(GameplayEffectData effectDefinition, float level, GameplayEffectContext context)
	{
		EffectData = effectDefinition;
		Level = level;
		Context = context;
	}

	public float GetScaledMagnitude()
	{
		return EffectData.BaseMagnitude * Level;
	}
}

public class ActiveGameplayEffect
{
	public GameplayEffect Spec { get; }

	public float RemainingDuration { get; private set; }

	public bool IsExpired => RemainingDuration <= 0;

	public ActiveGameplayEffect(GameplayEffect spec)
	{
		Spec = spec;
		RemainingDuration = spec.EffectData.DurationData.Duration;
	}

	public void Update(float deltaTime)
	{
		if (RemainingDuration > 0)
		{
			RemainingDuration -= deltaTime;
		}
	}
}

public class GameplayEffectsManager
{
	private readonly List<ActiveGameplayEffect> _activeEffects = new ();

	private AttributeSet _attributeSet;

	public GameplayEffectsManager(AttributeSet ownerAttributeSet)
	{
		_attributeSet = ownerAttributeSet;
	}

	public void ApplyEffect(GameplayEffect spec)
	{
		if (spec.EffectData.DurationData.Type != DurationType.Instant)
		{
			var activeEffect = new ActiveGameplayEffect(spec);
			_activeEffects.Add(activeEffect);

			foreach (var modifier in spec.EffectData.Modifiers)
			{
				_attributeSet.AttributesMap[modifier.Attribute].ApplyModifier(modifier.Value);

				if (spec.EffectData.PeriodicData.HasValue && spec.EffectData.PeriodicData.Value.ExecuteOnApplication)
				{
					ExecuteEffect(_attributeSet.AttributesMap[modifier.Attribute], modifier);
				}
			}
		}
		else
		{
			// This path is called "Execute" and should work for instant and periodic effects
			foreach (var modifier in spec.EffectData.Modifiers)
			{
				ExecuteEffect(_attributeSet.AttributesMap[modifier.Attribute], modifier);
			}
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
