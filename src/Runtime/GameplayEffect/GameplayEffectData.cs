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

			if (stackingData.Value.StackApplicationResetPeriodPolicy.HasValue != PeriodicData.HasValue)
			{
				throw new Exception($"Both {nameof(PeriodicData)} and {nameof(StackApplicationResetPeriodPolicy)} " +
					$"must be either defined or undefined.");
			}

			if (stackingData.Value.StackLevelPolicy == StackLevelPolicy.AggregateLevels !=
				stackingData.Value.StackLevelOverridePolicy.HasValue)
			{
				throw new Exception($"If {nameof(StackLevelPolicy)} is set {StackLevelPolicy.AggregateLevels}, " +
					$"{nameof(StackLevelOverridePolicy)} must be defined. And not defined if otherwise.");
			}

			if (durationData.Type == DurationType.HasDuration !=
				stackingData.Value.StackApplicationRefreshPolicy.HasValue)
			{
				throw new Exception($"Effects set as {DurationType.HasDuration} must define " +
					$" {nameof(StackApplicationRefreshPolicy)} and not define it if otherwise.");
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
