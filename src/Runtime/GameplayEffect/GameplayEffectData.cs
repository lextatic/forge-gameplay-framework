using static System.Runtime.InteropServices.JavaScript.JSType;

namespace GameplayTags.Runtime.GameplayEffect;

// TODO: Change to struct
public class GameplayEffectData // Immutable
{
	public List<Modifier> Modifiers { get; } = new ();
	//public List<Executions> Executions { get; } = new ();

	public string Name { get; }

	public DurationData DurationData { get; }

	public StackingData? StackingData { get; }

	public PeriodicData? PeriodicData { get; }

	public bool SnapshopLevel { get; }

	public GameplayEffectData(
		string name,
		DurationData durationData,
		StackingData? stackingData,
		PeriodicData? periodicData,
		bool snapshopLevel = true)
	{
		Name = name;
		DurationData = durationData;
		StackingData = stackingData;
		PeriodicData = periodicData;
		SnapshopLevel = snapshopLevel;

		// Should I really throw? Or just ignore (force null) the periodic data?
		if (periodicData.HasValue && durationData.Type == DurationType.Instant)
		{
			throw new Exception("Periodic effects can't be set as instant.");
		}

		// Should I really throw? Or just ignore (force null) the periodic data?
		if (durationData.Type != DurationType.HasDuration && durationData.Duration != null)
		{
			throw new Exception($"Can't set duration if {nameof(DurationType)} is set to {durationData.Type}.");
		}

		if (durationData.Type != DurationType.Instant && !periodicData.HasValue)
		{
			foreach (var modifier in Modifiers)
			{
				if (modifier.Operation == ModifierOperation.Override)
				{
					throw new ArgumentException($"Only {DurationType.Instant} or Periodic effects can have" +
							$"operation of type {ModifierOperation.Override}.");
				}
			}
		}

		if (stackingData.HasValue)
		{
			if (durationData.Type == DurationType.Instant)
			{
				throw new Exception($"{DurationType.Instant} effects can't have stacks.");
			}

			if (stackingData.Value.InitialStack.BaseValue > stackingData.Value.StackLimit.BaseValue ||
				stackingData.Value.InitialStack.BaseValue == 0)
			{
				throw new Exception("Shouldn't set InitialStack count to be higher than the StackLimit nor zero. It's" +
					" probably a bad configuration.");
			}

			if (stackingData.Value.StackPolicy == StackPolicy.AggregateByTarget !=
				stackingData.Value.InstigatorDenialPolicy.HasValue)
			{
				throw new Exception($"If {nameof(StackPolicy)} is set {StackPolicy.AggregateByTarget}, " +
					$"{nameof(StackInstigatorDenialPolicy)} must be defined. And not defined if otherwise.");
			}

			if ((stackingData.Value.StackPolicy == StackPolicy.AggregateByTarget &&
				stackingData.Value.InstigatorDenialPolicy == StackInstigatorDenialPolicy.AlwaysAllow) !=
				stackingData.Value.InstigatorOverridePolicy.HasValue)
			{
				throw new Exception($"If {nameof(StackPolicy)} is set {StackPolicy.AggregateByTarget} and " +
					$"{nameof(StackInstigatorDenialPolicy)} is set to {StackInstigatorDenialPolicy.AlwaysAllow}, " +
					$"{nameof(StackInstigatorOverridePolicy)} must be defined. And not defined if otherwise.");
			}

			if ((stackingData.Value.StackPolicy == StackPolicy.AggregateByTarget &&
				stackingData.Value.InstigatorOverridePolicy.HasValue &&
				stackingData.Value.InstigatorOverridePolicy.Value == StackInstigatorOverridePolicy.Override) !=
				stackingData.Value.InstigatorOverrideStackCountPolicy.HasValue)
			{
				throw new Exception($"If {nameof(StackInstigatorOverridePolicy)} is set {StackInstigatorOverridePolicy.Override}, " +
					$"{nameof(StackInstigatorOverrideStackCountPolicy)} must be defined. And not defined if otherwise.");
			}

			if (stackingData.Value.StackLevelPolicy == StackLevelPolicy.AggregateLevels !=
				stackingData.Value.LevelDenialPolicy.HasValue)
			{
				throw new Exception($"If {nameof(StackLevelPolicy)} is set {StackLevelPolicy.AggregateLevels}, " +
					$"{nameof(LevelComparison)} must be defined. And not defined if otherwise.");
			}

			if (stackingData.Value.StackLevelPolicy == StackLevelPolicy.AggregateLevels !=
				stackingData.Value.LevelOverridePolicy.HasValue)
			{
				throw new Exception($"If {nameof(StackLevelPolicy)} is set {StackLevelPolicy.AggregateLevels}, " +
					$"LevelOverridePolicy must be defined. And not defined if otherwise.");
			}

			if ((stackingData.Value.StackLevelPolicy == StackLevelPolicy.AggregateLevels &&
				stackingData.Value.LevelOverridePolicy.HasValue &&
				stackingData.Value.LevelOverridePolicy.Value != LevelComparison.None) !=
				stackingData.Value.LevelOverrideStackCountPolicy.HasValue)
			{
				throw new Exception($"If LevelOverridePolicy is different from {LevelComparison.None}, " +
					$"{nameof(StackLevelOverrideStackCountPolicy)} must be defined. And not defined if otherwise.");
			}

			if (stackingData.Value.LevelDenialPolicy.HasValue &&
				stackingData.Value.LevelOverridePolicy.HasValue &&
				stackingData.Value.LevelDenialPolicy.Value != LevelComparison.None &&
				(stackingData.Value.LevelDenialPolicy.Value & stackingData.Value.LevelOverridePolicy.Value) != 0)
			{
				throw new Exception($"LevelDenialPolicy and LevelOverridePolicy should't " +
					$"have the same value. If it's getting denied, how will it override?");
			}

			if (stackingData.Value.StackApplicationResetPeriodPolicy.HasValue != PeriodicData.HasValue)
			{
				throw new Exception($"Both {nameof(PeriodicData)} and {nameof(StackApplicationResetPeriodPolicy)} " +
					$"must be either defined or undefined.");
			}

			if (durationData.Type == DurationType.HasDuration !=
				stackingData.Value.ApplicationRefreshPolicy.HasValue)
			{
				throw new Exception($"Effects set as {DurationType.HasDuration} must define " +
					$"{nameof(StackApplicationRefreshPolicy)} and not define it if otherwise.");
			}
		}

		if (durationData.Type == DurationType.Instant)
		{
			foreach (var modifier in Modifiers)
			{
				if (modifier.Magnitude.MagnitudeCalculationType == MagnitudeCalculationType.AttributeBased &&
					!modifier.Magnitude.AttributeBasedFloat.BackingAttribute.Snapshot)
				{
					throw new ArgumentException($"Effects set as {DurationType.Instant} and " +
						$"{MagnitudeCalculationType.AttributeBased} cannot be set as non Snapshot.");
				}
			}

			if (!snapshopLevel)
			{
				throw new ArgumentException($"Effects set as {DurationType.Instant} cannot be set as non Snapshot " +
					$"for Level.");
			}
		}
	}
}
