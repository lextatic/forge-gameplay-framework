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
		var effectEvaluatedData = new GameplayEffectEvaluatedData(gameplayEffect, _owner);

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
}
