namespace GameplayTags.Runtime.GameplayEffect;

public enum StackPolicy : byte // done
{
	AggregateBySource,
	AggregateByTarget,
}

public enum StackLevelPolicy : byte // done, not tested
{
	AggregateLevels,
	SegregateLevels,
}

public enum StackMagnitudePolicy : byte // done
{
	DontStack,
	Sum,
}

// What happens when stack limit is reached and a new application happens?
public enum StackOverflowPolicy : byte
{
	Override,
	DontApply,
}

// AggregateByTarget
public enum StackInstigatorOverridePolicy : byte
{
	KeepCurrent,
	Override,
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
	//AddTime,
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
	public StackExpirationPolicy StackExpirationPolicy; // All stackable effects, infinite effects removal will count as expiration
	public StackOverflowPolicy StackOverflowPolicy; // All stackable effects
	public StackApplicationRefreshPolicy? StackApplicationRefreshPolicy; // Effects with duration (non infinite)
	public StackInstigatorOverridePolicy? StackInstigatorOverridePolicy; // Effects with StackPolicy == AggregateByTarget
	public StackLevelOverridePolicy? StackLevelOverridePolicy; // Effects with StackLevelPolicy == AggregateLevels
	public StackApplicationResetPeriodPolicy? StackApplicationResetPeriodPolicy; // Periodic effects
}
