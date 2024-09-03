using GameplayTags.Runtime.Attribute;
using System;

namespace GameplayTags.Runtime.GameplayEffect;

public enum AttributeCaptureSource : byte
{
	Source,
	Target,
}

public enum AttributeBasedFloatCalculationType : byte
{
	AttributeMagnitude,
	AttributeBaseValue,
	AttributeModifierMagnitude,
	AttributeMagnitudeEvaluatedUpToChannel,
}

// Do I really need this struct since it's not used anywhere else other than AttributeBasedFloat?
public struct AttributeCaptureDefinition
{
	public TagName Attribute;

	public AttributeCaptureSource Source;

	public bool Snapshot; // Only Infinite and HasDuration effects can snapshot

	public Attribute.Attribute GetAttribute(GameplaySystem source)
	{
		return source.Attributes[Attribute];
	}
}

public class AttributeBasedFloat
{
	public AttributeCaptureDefinition BackingAttribute;

	public AttributeBasedFloatCalculationType AttributeCalculationType;

	public ScalableFloat Coeficient;

	public ScalableFloat PreMultiplyAdditiveValue;

	public ScalableFloat PostMultiplyAdditiveValue;

	public int FinalChannel;

	/** If a curve table entry is specified, the attribute will be used as a lookup into the curve instead of using the
	 * attribute directly. */
	public Curve? AttributeCurve;

	public float CalculateMagnitude(GameplayEffect effect, GameplaySystem target, int? level = null)
	{
		Attribute.Attribute? attribute = null;

		switch (BackingAttribute.Source)
		{
			case AttributeCaptureSource.Source:
				attribute = effect.Context.Instigator.GameplaySystem.Attributes[BackingAttribute.Attribute];
				break;

			case AttributeCaptureSource.Target:
				attribute = target.Attributes[BackingAttribute.Attribute];
				break;
		}

		if (attribute is null)
		{
			return 0f;
		}

		float magnitude = 0;

		switch (AttributeCalculationType)
		{
			case AttributeBasedFloatCalculationType.AttributeMagnitude:
				magnitude = attribute.TotalValue;
				break;

			case AttributeBasedFloatCalculationType.AttributeBaseValue:
				magnitude = attribute.BaseValue;
				break;

			case AttributeBasedFloatCalculationType.AttributeModifierMagnitude:
				magnitude = attribute.Modifier - attribute.Overflow;
				break;

			case AttributeBasedFloatCalculationType.AttributeMagnitudeEvaluatedUpToChannel:
				magnitude = attribute.CalculateMagnitudeUpToChannel(FinalChannel);
				break;
		}

		var evaluatedLevel = level.HasValue ? level.Value : effect.Level;

		return (Coeficient.GetValue(evaluatedLevel) * (PreMultiplyAdditiveValue.GetValue(evaluatedLevel) + magnitude))
			+ PostMultiplyAdditiveValue.GetValue(evaluatedLevel);
	}
}
