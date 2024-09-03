
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Diagnostics;

namespace GameplayTags.Runtime.GameplayEffect;

internal class ActiveGameplayEffect
{
	private float _internalTime;

	private int _stackCount;

	internal GameplayEffectEvaluatedData GameplayEffectEvaluatedData { get; private set; }

	internal float RemainingDuration { get; private set; }

	internal float NextPeriodicTick { get; private set; }

	internal float ExecutionCount { get; private set; }

	internal bool IsExpired => EffectData.DurationData.Type ==
		DurationType.HasDuration &&
		RemainingDuration <= 0;

	private GameplayEffectData EffectData => GameplayEffectEvaluatedData.GameplayEffect.EffectData;

	private GameplayEffect GameplayEffect => GameplayEffectEvaluatedData.GameplayEffect;

	internal ActiveGameplayEffect(GameplayEffect gameplayEffect, GameplaySystem target)
	{
		GameplayEffectEvaluatedData = new GameplayEffectEvaluatedData(gameplayEffect, target);
		_stackCount = 1;
	}

	internal void Apply(bool reApplication = false)
	{
		_internalTime = 0;
		ExecutionCount = 0;
		RemainingDuration = GameplayEffectEvaluatedData.Duration;

		if (!EffectData.SnapshopLevel)
		{
			GameplayEffect.OnLevelChanged += GameplayEffect_OnLevelChanged;
		}

		foreach (var modifier in GameplayEffectEvaluatedData.ModifiersEvaluatedData)
		{
			if (!modifier.Snapshot && !reApplication)
			{
				System.Diagnostics.Debug.Assert(
						modifier.BackingAttribute is not null,
						"All non-snapshots modifiers should have a BackingAttribute set.");

				modifier.BackingAttribute.OnValueChanged += Attribute_OnValueChanged;
			}
		}

		if (EffectData.PeriodicData.HasValue)
		{
			if (EffectData.PeriodicData.Value.ExecuteOnApplication &&
				!reApplication)
			{
				GameplayEffect.Execute(GameplayEffectEvaluatedData);
				ExecutionCount++;
			}

			NextPeriodicTick = GameplayEffectEvaluatedData.Period;
		}
		else
		{
			foreach (var modifier in GameplayEffectEvaluatedData.ModifiersEvaluatedData)
			{
				switch (modifier.ModifierOperation)
				{
					case ModifierOperation.Flat:
						modifier.Attribute.AddFlatModifier((int)modifier.Magnitude, modifier.Channel);
						break;

					case ModifierOperation.Percent:
						modifier.Attribute.AddPercentModifier(modifier.Magnitude, modifier.Channel);
						break;

					case ModifierOperation.Override:
						modifier.Attribute.AddOverride((int)modifier.Magnitude, modifier.Channel);
						break;
				}
			}
		}
	}

	internal void Unapply(bool reApplication = false)
	{
		if (!EffectData.PeriodicData.HasValue)
		{
			foreach (var modifier in GameplayEffectEvaluatedData.ModifiersEvaluatedData)
			{
				switch (modifier.ModifierOperation)
				{
					case ModifierOperation.Flat:
						modifier.Attribute.AddFlatModifier(-(int)modifier.Magnitude, modifier.Channel);
						break;

					case ModifierOperation.Percent:
						modifier.Attribute.AddPercentModifier(-modifier.Magnitude, modifier.Channel);
						break;

					case ModifierOperation.Override:
						modifier.Attribute.ClearOverride(modifier.Channel);
						break;
				}
			}
		}

		if (!reApplication)
		{
			foreach (var modifier in GameplayEffectEvaluatedData.ModifiersEvaluatedData)
			{
				if (!modifier.Snapshot)
				{
					System.Diagnostics.Debug.Assert(
						modifier.BackingAttribute is not null,
						"All non-snapshots modifiers should have a BackingAttribute set.");

					modifier.BackingAttribute.OnValueChanged -= Attribute_OnValueChanged;
				}
			}

			if (!EffectData.SnapshopLevel)
			{
				GameplayEffect.OnLevelChanged -= GameplayEffect_OnLevelChanged;
			}
		}
	}

	internal bool AddStack(GameplayEffect gameplayEffect)
	{
		Debug.Assert(
			EffectData.StackingData.HasValue,
			"StackingData should never be null at this point.");

		var stackingData = EffectData.StackingData.Value;

		if (_stackCount == stackingData.StackLimit.GetValue(GameplayEffectEvaluatedData.Level) &&
			stackingData.OverflowPolicy == StackOverflowPolicy.DenyApplication)
		{
			return false;
		}

		var hasChanges = false;
		var resetStacks = false;

		var evaluatedLevel = GameplayEffectEvaluatedData.Level;
		var evaluatedGameplayEffect = GameplayEffectEvaluatedData.GameplayEffect;

		if (stackingData.InstigatorDenialPolicy.HasValue)
		{
			if (stackingData.InstigatorDenialPolicy.Value == StackInstigatorDenialPolicy.DenyIfDifferent &&
				GameplayEffectEvaluatedData.GameplayEffect.Context != gameplayEffect.Context)
			{
				return false;
			}

			if (stackingData.InstigatorOverridePolicy.HasValue &&
				stackingData.InstigatorOverridePolicy.Value == StackInstigatorOverridePolicy.Override &&
				GameplayEffectEvaluatedData.GameplayEffect.Context != gameplayEffect.Context)
			{
				evaluatedGameplayEffect = gameplayEffect;
				hasChanges = true;

				Debug.Assert(
				stackingData.InstigatorOverrideStackCountPolicy.HasValue,
				"InstigatorOverrideStackCountPolicy should never be null at this point.");

				switch (stackingData.InstigatorOverrideStackCountPolicy.Value)
				{
					case StackInstigatorOverrideStackCountPolicy.ResetStacks:
						resetStacks = true;
						break;

					case StackInstigatorOverrideStackCountPolicy.IncreaseStacks:
						break;
				}
			}
		}

		if (stackingData.LevelDenialPolicy.HasValue)
		{
			Debug.Assert(
				stackingData.LevelOverridePolicy.HasValue,
				"LevelOverridePolicy should never be null at this point.");

			// Determine the relationship
			var relation = LevelComparison.None;

			if (gameplayEffect.Level > evaluatedLevel)
			{
				relation = LevelComparison.Higher;
			}
			else if (gameplayEffect.Level < evaluatedLevel)
			{
				relation = LevelComparison.Lower;
			}
			else
			{
				relation = LevelComparison.Equal;
			}

			// Check if the relevant flag is set in the denial policy
			if ((stackingData.LevelDenialPolicy.Value & relation) != 0)
			{
				return false;
			}

			if ((stackingData.LevelOverridePolicy.Value & relation) != 0)
			{
				Debug.Assert(
					stackingData.LevelOverrideStackCountPolicy.HasValue,
					"LevelOverrideStackCountPolicy should never be null at this point.");

				evaluatedLevel = gameplayEffect.Level;
				hasChanges = true;

				resetStacks = stackingData.LevelOverrideStackCountPolicy.Value == StackLevelOverrideStackCountPolicy.ResetStacks;
			}
		}

		// It can be a successfull application and still not increase stack count.
		// In some cases we can even skip re-application.
		if (resetStacks)
		{
			if (_stackCount != 1)
			{
				_stackCount = 1;
				hasChanges = true;
			}
		}
		else
		{
			if (_stackCount < stackingData.StackLimit.GetValue(GameplayEffectEvaluatedData.Level))
			{
				_stackCount++;
				hasChanges = true;
			}
		}

		if (hasChanges)
		{
			ReEvaluateAndReApply(evaluatedGameplayEffect, evaluatedLevel);
		}

		if (stackingData.ApplicationRefreshPolicy == StackApplicationRefreshPolicy.RefreshOnSuccessfulApplication)
		{
			RemainingDuration = GameplayEffectEvaluatedData.Duration;
		}

		if (stackingData.StackApplicationResetPeriodPolicy == StackApplicationResetPeriodPolicy.ResetOnSuccessfulApplication)
		{
			NextPeriodicTick = GameplayEffectEvaluatedData.Period;
		}

		return true;
	}

	internal void RemoveStack()
	{
		if (_stackCount == 1)
		{
			Unapply();
			return;
		}

		_stackCount--;
		ReEvaluateAndReApply(GameplayEffectEvaluatedData.GameplayEffect);
	}

	// This update doesn't work right for stackable+periodic effect if you use a high deltaTime.
	// This is because it's going to evaluate all the periodic applications and only then remove
	// all the stacks which would have a different value than if applied in the correct order.
	internal void Update(float deltaTime)
	{
		_internalTime += deltaTime;

		if (EffectData.PeriodicData.HasValue)
		{
			while (_internalTime >= NextPeriodicTick)
			{
				GameplayEffect.Execute(GameplayEffectEvaluatedData);
				ExecutionCount++;
				NextPeriodicTick += GameplayEffectEvaluatedData.Period;
			}
		}

		if (EffectData.DurationData.Type == DurationType.HasDuration)
		{
			RemainingDuration -= deltaTime;
		}

		if (IsExpired)
		{
			if (EffectData.StackingData.HasValue &&
				EffectData.StackingData.Value.ExpirationPolicy ==
				StackExpirationPolicy.RemoveSingleStackAndRefreshDuration)
			{
				while (_stackCount >= 1 && RemainingDuration <= 0)
				{
					RemoveStack();
					RemainingDuration += GameplayEffectEvaluatedData.Duration;
				}

				return;
			}

			Unapply();
		}
	}

	private void ReEvaluateAndReApply(GameplayEffect gameplayEffect, int? level = null)
	{
		Unapply(true);

		GameplayEffectEvaluatedData =
			new GameplayEffectEvaluatedData(
				gameplayEffect,
				GameplayEffectEvaluatedData.Target,
				_stackCount,
				level);

		Apply(true);
	}

	private void Attribute_OnValueChanged(Attribute.Attribute attribute, int change)
	{
		// This could be optimized by re-evaluating only the modifiers with the attribute that changed
		ReEvaluateAndReApply(GameplayEffectEvaluatedData.GameplayEffect);
	}

	private void GameplayEffect_OnLevelChanged(int obj)
	{
		// This one has to re-calculate everything that uses ScalableFloats
		ReEvaluateAndReApply(GameplayEffectEvaluatedData.GameplayEffect);
	}
}
