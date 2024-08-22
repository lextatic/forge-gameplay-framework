namespace GameplayTags.Runtime.GameplayEffect;

public enum MagnitudeCalculationType : byte
{
	ScalableFloat,
	AttributeBased,
	//CustomCalculationClass,
	//SetByCaller,
}

public class ModifierMagnitude
{
	public MagnitudeCalculationType MagnitudeCalculationType;

	public ScalableFloat? ScalableFloatMagnitude;

	public AttributeBasedFloat? AttributeBasedFloat;
	// CustomCalculationClass
	// Setbycaller

	public float GetMagnitude(GameplayEffect effect, GameplaySystem target)
	{
		switch (MagnitudeCalculationType)
		{
			case MagnitudeCalculationType.ScalableFloat:
				return ScalableFloatMagnitude.Value.GetValue(effect.Level);

			case MagnitudeCalculationType.AttributeBased:
				return AttributeBasedFloat.Value.CalculateMagnitude(effect, target);
		}

		return 0;
	}
}

public enum ModifierOperation : byte
{
	Flat,
	Percent,
	Override,
}

public struct Modifier
{
	public TagName Attribute;
	public ModifierOperation Operation;
	public ModifierMagnitude Magnitude;
	public int Channel;
}
