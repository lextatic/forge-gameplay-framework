namespace GameplayTags.Runtime.GameplayEffect;

public enum StackPolicy : byte
{
	AggregateBySource,
	AggregateByTarget,
}

public enum StackLevelPolicy : byte
{
	AggregateLevels,
	SegregateLevels,
}

public enum StackMagnitudePolicy : byte
{
	DontStack,
	Sum,
	Multiply,
}

public enum StackLevelOverridePolicy : byte
{
	AlwaysKeep,
	AlwaysOverride,
	KeepHighest,
	KeepLowest,
}

public enum StackApplicationRefreshPolicy : byte
{
	RefreshOnSuccessfulApplication,
	NeverRefresh,
}

public enum StackExpirationPolicy : byte
{
	ClearEntireStack,
	RemoveSingleStackAndRefreshDuration,
}

public enum StackApplicationResetPeriodPolicy : byte
{
	ResetOnSuccessfulApplication,
	NeverReset,
}

public struct StackingData
{
	public ScalableInt StackLimit; // All stackable effects
	public StackPolicy StackPolicy; // All stackable effects
	public StackLevelPolicy StackLevelPolicy; // All stackable effects
	public StackMagnitudePolicy StackMagnitudePolicy; // All stackable effects
	public StackExpirationPolicy StackExpirationPolicy; // Aff stackable effects, infinite effects removal will count as expiration
	public StackApplicationRefreshPolicy? StackApplicationRefreshPolicy; // Effects with duration
	public StackLevelOverridePolicy? StackLevelOverridePolicy; // Effects with LevelStacking == AggregateLevels
	public StackApplicationResetPeriodPolicy? StackApplicationResetPeriodPolicy; // Periodic effects
}
