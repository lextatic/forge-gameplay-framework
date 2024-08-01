namespace GameplayTags.Runtime.GameplayEffect;

/** Gameplay effect duration policies */
public enum GameplayEffectDurationType : byte
{
	/** This effect applies instantly */
	Instant,
	/** This effect lasts forever */
	Infinite,
	/** The duration of this effect will be specified by a magnitude */
	HasDuration
}
