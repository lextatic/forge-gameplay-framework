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
public enum StackOverflowPolicy : byte // done, not tested
{
	AllowApplication,
	DenyApplication,
}

// StackPolicy == AggregateByTarget
public enum StackInstigatorDenialPolicy : byte
{
	AlwaysAllow,
	DenyIfDifferent,
}

// StackPolicy == AggregateByTarget
public enum StackInstigatorOverridePolicy : byte // done, not tested
{
	KeepCurrent,
	Override,
}

// StackPolicy == AggregateByTarget && StackInstigatorOverridePolicy == Override
public enum StackInstigatorOverrideStackCountPolicy : byte
{
	IncreaseStacks,
	ResetStacks,
}

// StackLevelPolicy == AggregateLevels
[Flags]
public enum LevelComparison : byte
{
	None = 0,
	Equal = 1 << 0,
	Higher = 1 << 1,
	Lower = 1 << 2,
}

// StackLevelPolicy == AggregateLevels && StackLevelOverridePolicy == Any
public enum StackLevelOverrideStackCountPolicy : byte
{
	IncreaseStacks,
	ResetStacks,
}

public enum StackExpirationPolicy : byte // done, not tested
{
	ClearEntireStack,
	RemoveSingleStackAndRefreshDuration,
}

public enum StackApplicationRefreshPolicy : byte // done, not tested
{
	RefreshOnSuccessfulApplication,
	NeverRefresh,
}

public enum StackApplicationResetPeriodPolicy : byte // done, not tested
{
	ResetOnSuccessfulApplication,
	NeverReset,
}

public struct StackingData
{
	public ScalableInt StackLimit; // All stackable effects
	public ScalableInt InitialStack; // All stackable effects
	public StackPolicy StackPolicy; // All stackable effects
	public StackLevelPolicy StackLevelPolicy; // All stackable effects
	public StackMagnitudePolicy MagnitudePolicy; // All stackable effects
	public StackOverflowPolicy OverflowPolicy; // All stackable effects
	public StackExpirationPolicy ExpirationPolicy; // All stackable effects, infinite effects removal will count as expiration
	public StackInstigatorDenialPolicy? InstigatorDenialPolicy; // StackPolicy == AggregateByTarget
	public StackInstigatorOverridePolicy? InstigatorOverridePolicy; // StackPolicy == AggregateByTarget
	public StackInstigatorOverrideStackCountPolicy? InstigatorOverrideStackCountPolicy; // StackPolicy == AggregateByTarget && StackInstigatorOverridePolicy == Override
	public LevelComparison? LevelDenialPolicy; // StackLevelPolicy == AggregateLevels
	public LevelComparison? LevelOverridePolicy; // StackLevelPolicy == AggregateLevels
	public StackLevelOverrideStackCountPolicy? LevelOverrideStackCountPolicy; // StackLevelPolicy == AggregateLevels && StackLevelOverridePolicy == Any
	public StackApplicationRefreshPolicy? ApplicationRefreshPolicy; // Effects with duration (non infinite)
	public StackApplicationResetPeriodPolicy? ApplicationResetPeriodPolicy; // Periodic effects
	public bool? ExecuteOnSuccessfulApplication; // Periodic effects
}
