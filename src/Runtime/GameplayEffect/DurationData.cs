namespace GameplayTags.Runtime.GameplayEffect;

public enum DurationType : byte
{
	Instant,
	Infinite,
	HasDuration,
}

public struct DurationData
{
	public DurationType Type;
	public ScalableFloat? Duration;
}
