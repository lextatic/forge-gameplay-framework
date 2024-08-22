namespace GameplayTags.Runtime.GameplayEffect;

internal class ActiveGameplayEffect
{
	private float _internalTime;

	internal GameplayEffectEvaluatedData GameplayEffectEvaluatedData { get; }

	internal float RemainingDuration { get; private set; }

	internal float NextPeriodicTick { get; private set; }

	internal float ExecutionCount { get; private set; }

	internal bool IsExpired => GameplayEffectEvaluatedData.GameplayEffect.EffectData.DurationData.Type ==
		DurationType.HasDuration &&
		RemainingDuration <= 0;

	internal ActiveGameplayEffect(GameplayEffectEvaluatedData evaluatedEffectData)
	{
		GameplayEffectEvaluatedData = evaluatedEffectData;
	}

	internal void Apply()
	{
		_internalTime = 0;
		ExecutionCount = 0;
		RemainingDuration = GameplayEffectEvaluatedData.Duration;

		if (GameplayEffectEvaluatedData.GameplayEffect.EffectData.PeriodicData.HasValue)
		{
			if (GameplayEffectEvaluatedData.GameplayEffect.EffectData.PeriodicData.Value.ExecuteOnApplication)
			{
				GameplayEffectEvaluatedData.GameplayEffect.Execute(GameplayEffectEvaluatedData);
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

	internal void Unapply()
	{
		if (!GameplayEffectEvaluatedData.GameplayEffect.EffectData.PeriodicData.HasValue)
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
	}

	internal void Update(float deltaTime)
	{
		_internalTime += deltaTime;

		if (GameplayEffectEvaluatedData.GameplayEffect.EffectData.PeriodicData.HasValue && _internalTime >= NextPeriodicTick)
		{
			GameplayEffectEvaluatedData.GameplayEffect.Execute(GameplayEffectEvaluatedData);
			ExecutionCount++;

			NextPeriodicTick += GameplayEffectEvaluatedData.Period;
		}

		if (GameplayEffectEvaluatedData.GameplayEffect.EffectData.DurationData.Type == DurationType.HasDuration)
		{
			RemainingDuration -= deltaTime;
		}

		if (IsExpired)
		{
			Unapply();
		}
	}
}
