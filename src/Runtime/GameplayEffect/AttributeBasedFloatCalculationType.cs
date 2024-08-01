namespace GameplayTags.Runtime.GameplayEffect;

/** Enumeration outlining the possible attribute based float calculation policies. */
public enum AttributeBasedFloatCalculationType : byte
{
	/** Use the final evaluated magnitude of the attribute. */
	AttributeMagnitude,
	/** Use the base value of the attribute. */
	AttributeBaseValue,
	/** Use the "bonus" evaluated magnitude of the attribute: Equivalent to (FinalMag - BaseValue). */
	AttributeBonusMagnitude,
	/** Use a calculated magnitude stopping with the evaluation of the specified "Final Channel" */
	AttributeMagnitudeEvaluatedUpToChannel
}
