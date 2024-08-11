namespace GameplayTags.Runtime.GameplayEffect;

/** Enumeration of policies for dealing gameplay effect stacks that expire (in duration based effects). */
public enum GameplayEffectStackingExpirationPolicy : byte
{
	/** The entire stack is cleared when the active gameplay effect expires  */
	ClearEntireStack,

	/** The current stack count will be decremented by 1 and the duration refreshed. The GE is not "reapplied", just continues to exist with one less stacks. */
	RemoveSingleStackAndRefreshDuration,

	/** The duration of the gameplay effect is refreshed. This essentially makes the effect infinite in duration. This can be used to manually handle stack decrements via OnStackCountChange callback */
	RefreshDuration,
}
