namespace GameplayTags.Runtime.GameplayEffect;

/** Enumeration of policies for dealing with the period of a gameplay effect while stacking */
public enum GameplayEffectStackingPeriodPolicy : byte
{
	/** Any progress toward the next tick of a periodic effect is discarded upon any successful stack application */
	ResetOnSuccessfulApplication,

	/** The progress toward the next tick of a periodic effect will never be reset, regardless of stack applications */
	NeverReset,
}
