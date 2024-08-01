using GameplayTags.Runtime.Attribute;
using System;

namespace GameplayTags.Runtime.GameplayEffect;
#pragma warning disable SA1600

public struct Modifier
{
	public TagName Attribute;
	public float Value;
}

public class GameplayEffectData
{
	public List<Modifier> Modifiers { get; } = new ();

	public string Name { get; }

	public float BaseMagnitude { get; }

	public float Duration { get; }


	public GameplayEffectData(string name, float baseMagnitude, float duration)
	{
		Name = name;
		BaseMagnitude = baseMagnitude;
		Duration = duration;
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
		RemainingDuration = spec.EffectData.Duration;
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
		if (spec.EffectData.Duration > 0)
		{
			var activeEffect = new ActiveGameplayEffect(spec);
			_activeEffects.Add(activeEffect);

			foreach (var modifier in spec.EffectData.Modifiers)
			{
				_attributeSet.AttributesMap[modifier.Attribute].AddModifier(modifier.Value);
			}
		}
		else
		{
			foreach (var modifier in spec.EffectData.Modifiers)
			{
				var attribute = _attributeSet.AttributesMap[modifier.Attribute];
				//var oldValue = attribute.BaseValue;
				//var newValue = modifier.Value;

				//_attributeSet.PreAttributeBaseChange(attribute, ref newValue);
				//attribute.AddBaseValue(newValue);
				//_attributeSet.PostAttributeBaseChange(attribute, oldValue, newValue);

				attribute.AddBaseValue(modifier.Value);
			}
		}
	}

	public void UpdateEffects(float deltaTime)
	{
		foreach (var effect in _activeEffects)
		{
			effect.Update(deltaTime);
		}

		_activeEffects.RemoveAll(e => e.IsExpired);
	}
}
