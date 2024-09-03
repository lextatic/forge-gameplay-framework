using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace GameplayTags.Runtime.GameplayEffect;

public struct ModifierEvaluatedData
{
	public Attribute.Attribute Attribute;
	public ModifierOperation ModifierOperation;
	public float Magnitude;
	public int Channel;
	public Attribute.Attribute? BackingAttribute;
	public bool Snapshot;
}

public struct GameplayEffectEvaluatedData
{
	public GameplayEffect GameplayEffect;
	public List<ModifierEvaluatedData> ModifiersEvaluatedData;
	public int Level;
	public int Stack;
	public float Duration;
	public float Period;
	public GameplaySystem Target;

	public GameplayEffectEvaluatedData(GameplayEffect gameplayEffect,
		GameplaySystem target,
		int stack = 1,
		int? level = null)
	{
		var modifiersEvaluatedData = new List<ModifierEvaluatedData>();

		float stackMultiplier = stack;
		if (gameplayEffect.EffectData.StackingData.HasValue &&
			gameplayEffect.EffectData.StackingData.Value.MagnitudePolicy == StackMagnitudePolicy.DontStack)
		{
			stackMultiplier = 1;
		}

		foreach (var modifier in gameplayEffect.EffectData.Modifiers)
		{
			// This is totally not legible
			modifiersEvaluatedData.Add(new ModifierEvaluatedData
			{
				Attribute = target.Attributes[modifier.Attribute],
				ModifierOperation = modifier.Operation,
				Magnitude = modifier.Magnitude.GetMagnitude(gameplayEffect, target, level) * stackMultiplier,
				Channel = modifier.Channel,
				Snapshot = gameplayEffect.EffectData.DurationData.Type == DurationType.Instant ||
					modifier.Magnitude.MagnitudeCalculationType != MagnitudeCalculationType.AttributeBased ||
					modifier.Magnitude.AttributeBasedFloat.BackingAttribute.Snapshot,
				BackingAttribute = gameplayEffect.EffectData.DurationData.Type != DurationType.Instant &&
					modifier.Magnitude.MagnitudeCalculationType == MagnitudeCalculationType.AttributeBased &&
					!modifier.Magnitude.AttributeBasedFloat.BackingAttribute.Snapshot ?
					modifier.Magnitude.AttributeBasedFloat.BackingAttribute.GetAttribute(
						modifier.Magnitude.AttributeBasedFloat.BackingAttribute.Source == AttributeCaptureSource.Source ?
						gameplayEffect.Context.Instigator.GameplaySystem : target) : null,
			});
		}

		var evaluatedLevel = level.HasValue ? level.Value : gameplayEffect.Level;

		GameplayEffect = gameplayEffect;
		ModifiersEvaluatedData = modifiersEvaluatedData;
		Level = evaluatedLevel;
		Stack = stack;
		Duration = gameplayEffect.EffectData.DurationData.Duration is not null ?
			gameplayEffect.EffectData.DurationData.Duration.GetValue(evaluatedLevel) : 0;
		Period = gameplayEffect.EffectData.PeriodicData.HasValue ?
			gameplayEffect.EffectData.PeriodicData.Value.Period.GetValue(evaluatedLevel) : 0;
		Target = target;
	}
}
