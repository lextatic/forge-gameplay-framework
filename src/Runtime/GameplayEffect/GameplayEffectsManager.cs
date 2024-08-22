using GameplayTags.Runtime.Attribute;

namespace GameplayTags.Runtime.GameplayEffect;

public class GameplayEffectsManager
{
	private readonly List<ActiveGameplayEffect> _activeEffects = new ();

	private GameplaySystem _owner;

	public GameplayEffectsManager(GameplaySystem owner)
	{
		_owner = owner;
	}

	public void ApplyEffect(GameplayEffect gameplayEffect)
	{
		var effectEvaluatedData = EvaluateModifiers(gameplayEffect, _owner);

		if (gameplayEffect.EffectData.DurationData.Type != DurationType.Instant)
		{
			var activeEffect = new ActiveGameplayEffect(effectEvaluatedData);
			_activeEffects.Add(activeEffect);
			activeEffect.Apply();
		}
		else
		{
			// This path is called "Execute" and should work for instant effects
			gameplayEffect.Execute(effectEvaluatedData);
		}
	}

	// For now, remove the first instance of the effect
	public void UnapplyEffect(GameplayEffect gameplayEffect)
	{
		ActiveGameplayEffect? effectToRemove = null;

		foreach (var effect in _activeEffects)
		{
			if (gameplayEffect == effect.GameplayEffectEvaluatedData.GameplayEffect)
			{
				effect.Unapply();
				effectToRemove = effect;
				break;
			}
		}

		if (effectToRemove is not null)
		{
			_activeEffects.Remove(effectToRemove);
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

	private GameplayEffectEvaluatedData EvaluateModifiers(GameplayEffect gameplayEffect, GameplaySystem target)
	{
		var modifiersEvaluatedData = new List<ModifierEvaluatedData>();

		foreach (var modifier in gameplayEffect.EffectData.Modifiers)
		{
			modifiersEvaluatedData.Add(new ModifierEvaluatedData
			{
				Attribute = target.Attributes[modifier.Attribute],
				ModifierOperation = modifier.Operation,
				Magnitude = modifier.Magnitude.GetMagnitude(gameplayEffect, target),
				Channel = modifier.Channel,
			});
		}

		return new GameplayEffectEvaluatedData()
		{
			GameplayEffect = gameplayEffect,
			ModifiersEvaluatedData = modifiersEvaluatedData,
			Level = gameplayEffect.Level,
			Stack = gameplayEffect.EffectData.StackingData.HasValue ?
				gameplayEffect.EffectData.StackingData.Value.StackLimit.GetValue(gameplayEffect.Level) : 0,
			Duration = gameplayEffect.EffectData.DurationData.Duration.HasValue ?
				gameplayEffect.EffectData.DurationData.Duration.Value.GetValue(gameplayEffect.Level) : 0,
			Period = gameplayEffect.EffectData.PeriodicData.HasValue ?
				gameplayEffect.EffectData.PeriodicData.Value.Period.GetValue(gameplayEffect.Level) : 0,
			Target = target,
		};
	}
}
