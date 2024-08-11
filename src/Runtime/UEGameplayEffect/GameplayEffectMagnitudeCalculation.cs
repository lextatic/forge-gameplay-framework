namespace GameplayTags.Runtime.GameplayEffect;

/** Enumeration outlining the possible gameplay effect magnitude calculation policies. */
public enum GameplayEffectMagnitudeCalculation : byte
{
	/** Use a simple, scalable float for the calculation. */
	ScalableFloat,
	/** Perform a calculation based upon an attribute. */
	AttributeBased,
	/** Perform a custom calculation, capable of capturing and acting on multiple attributes, in either BP or native. */
	CustomCalculationClass,
	/** This magnitude will be set explicitly by the code/blueprint that creates the spec. */
	SetByCaller,
}
