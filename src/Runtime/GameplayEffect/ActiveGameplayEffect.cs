
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
		var hasChanges = false;

		if (_stackCount == stackingData.StackLimit.GetValue(
				GameplayEffectEvaluatedData.Level))
		{
			if (stackingData.StackOverflowPolicy == StackOverflowPolicy.DontApply)
			{
				return false;
			}
		}
		// It can be a successfull application and still not increase stack count.
		// In some cases we can even skip re-application.
		else
		{
			_stackCount++;
			hasChanges = true;
		}

		if (stackingData.StackApplicationRefreshPolicy == StackApplicationRefreshPolicy.RefreshOnSuccessfulApplication)
		{
			RemainingDuration = GameplayEffectEvaluatedData.Duration;
		}

		if (stackingData.StackApplicationResetPeriodPolicy == StackApplicationResetPeriodPolicy.ResetOnSuccessfulApplication)
		{
			NextPeriodicTick = GameplayEffectEvaluatedData.Period;
		}

		var context = GameplayEffectEvaluatedData.Context;

		if (stackingData.StackPolicy == StackPolicy.AggregateByTarget &&
			stackingData.StackInstigatorOverridePolicy == StackInstigatorOverridePolicy.Override &&
			context != gameplayEffect.Context)
		{
			context = gameplayEffect.Context;
			hasChanges = true;
		}

		var level = GameplayEffectEvaluatedData.Level;

		if (stackingData.StackLevelPolicy == StackLevelPolicy.AggregateLevels &&
			level != gameplayEffect.Level)
		{
			switch (stackingData.StackLevelOverridePolicy)
			{
				case StackLevelOverridePolicy.AlwaysOverride:
					level = gameplayEffect.Level;
					break;

				case StackLevelOverridePolicy.AlwaysKeep:
					break;

				case StackLevelOverridePolicy.KeepLowest:
					level = Math.Min(level, gameplayEffect.Level);
					break;

				case StackLevelOverridePolicy.KeepHighest:
					level = Math.Max(level, gameplayEffect.Level);
					break;
			}

			hasChanges |= stackingData.StackLevelOverridePolicy != StackLevelOverridePolicy.AlwaysKeep;
		}

		if (!hasChanges)
		{
			return true;
		}

		ReEvaluateAndReApply(context, level);
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
		ReEvaluateAndReApply();
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
				EffectData.StackingData.Value.StackExpirationPolicy ==
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

	private void ReEvaluateAndReApply(GameplayEffectContext? context = null, int? level = null)
	{
		Unapply(true);

		GameplayEffectEvaluatedData =
			new GameplayEffectEvaluatedData(
				GameplayEffectEvaluatedData.GameplayEffect,
				GameplayEffectEvaluatedData.Target,
				_stackCount,
				context,
				level);

		Apply(true);
	}

	private void Attribute_OnValueChanged(Attribute.Attribute _, int __)
	{
		// This could be optimized by re-evaluating only the modifiers with the attribute that changed
		ReEvaluateAndReApply();
	}

	private void GameplayEffect_OnLevelChanged(int obj)
	{
		// This one has to re-calculate everything that uses ScalableFloats
		ReEvaluateAndReApply();
	}
}
