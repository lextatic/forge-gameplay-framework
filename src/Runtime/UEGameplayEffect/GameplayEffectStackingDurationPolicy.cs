namespace GameplayTags.Runtime.GameplayEffect;

/** Enumeration of policies for dealing with duration of a gameplay effect while stacking */
public enum GameplayEffectStackingDurationPolicy : byte
{
	/** The duration of the effect will be refreshed from any successful stack application */
	RefreshOnSuccessfulApplication,

	/** The duration of the effect will never be refreshed */
	NeverRefresh,
}
