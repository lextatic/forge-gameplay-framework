
using Microsoft.VisualStudio.TestTools.UnitTesting;

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

	internal ActiveGameplayEffect(GameplayEffectEvaluatedData evaluatedEffectData)
	{
		GameplayEffectEvaluatedData = evaluatedEffectData;
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

	internal bool AddStack()
	{
		System.Diagnostics.Debug.Assert(
			EffectData.StackingData.HasValue,
			"StackingData should never be null at this point.");

		if (_stackCount == EffectData.StackingData.Value.StackLimit.GetValue(
				GameplayEffectEvaluatedData.Level))
		{
			return false;
		}

		_stackCount++;
		ReEvaluateAndReApply();
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
			Unapply();
		}
	}

	private void ReEvaluateAndReApply()
	{
		Unapply(true);

		GameplayEffectEvaluatedData =
			new GameplayEffectEvaluatedData(
				GameplayEffectEvaluatedData.GameplayEffect,
				GameplayEffectEvaluatedData.Target,
				_stackCount);

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
