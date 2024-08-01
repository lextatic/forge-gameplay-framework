namespace GameplayTags.Runtime.GameplayEffect;

/** Enumeration of policies for dealing with the period of a gameplay effect when inhibition is removed */
public enum GameplayEffectPeriodInhibitionRemovedPolicy : byte
{
	/** Does not reset. The period timing will continue as if the inhibition hadn't occurred. */
	NeverReset,

	/** Resets the period. The next execution will occur one full period from when inhibition is removed. */
	ResetPeriod,

	/** Executes immediately and resets the period. */
	ExecuteAndResetPeriod,
}
