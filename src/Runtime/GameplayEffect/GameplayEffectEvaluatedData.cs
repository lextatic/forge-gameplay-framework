namespace GameplayTags.Runtime.GameplayEffect;

public struct ModifierEvaluatedData
{
	public Attribute.Attribute Attribute;
	public ModifierOperation ModifierOperation;
	public float Magnitude;
	public int Channel;
	public Attribute.Attribute? BackingAttribute;
	public bool Snapshot;
	//public bool IsValid; // remove if not used
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

	public GameplayEffectEvaluatedData(GameplayEffect gameplayEffect, GameplaySystem target)
	{
		var modifiersEvaluatedData = new List<ModifierEvaluatedData>();

		foreach (var modifier in gameplayEffect.EffectData.Modifiers)
		{
			// This is totally not legible
			modifiersEvaluatedData.Add(new ModifierEvaluatedData
			{
				Attribute = target.Attributes[modifier.Attribute],
				ModifierOperation = modifier.Operation,
				Magnitude = modifier.Magnitude.GetMagnitude(gameplayEffect, target),
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

		GameplayEffect = gameplayEffect;
		ModifiersEvaluatedData = modifiersEvaluatedData;
		Level = gameplayEffect.Level;
		Stack = gameplayEffect.EffectData.StackingData.HasValue ?
			gameplayEffect.EffectData.StackingData.Value.StackLimit.GetValue(gameplayEffect.Level) : 0;
		Duration = gameplayEffect.EffectData.DurationData.Duration != null ?
			gameplayEffect.EffectData.DurationData.Duration.GetValue(gameplayEffect.Level) : 0;
		Period = gameplayEffect.EffectData.PeriodicData.HasValue ?
			gameplayEffect.EffectData.PeriodicData.Value.Period.GetValue(gameplayEffect.Level) : 0;
		Target = target;
	}
}
