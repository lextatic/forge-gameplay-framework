using GameplayTags.Runtime.GameplayEffect;
using Godot;
using Godot.Collections;

[Tool]
public partial class GameplayEffect : Resource
{
	private DurationType _durationType;
	private bool _hasPeriodicApplication;
	private bool _canStack;

	private StackPolicy _sourcePolicy;
	private StackLevelPolicy _levelPolicy;
	private StackInstigatorOverridePolicy _instigatorOverridePolicy;
	private LevelComparison _levelOverridePolicy;

	[Export]
	public string Name { get; set; }

	[Export]
	public bool SnapshotLevel { get; set; }

	[ExportGroup("Modifier Data")]

	[Export(PropertyHint.ResourceType, "ModifierWrapper")]
	public Array<ModifierWrapper> Modifiers { get; set; } = new Array<ModifierWrapper>();

	[ExportGroup("Duration Data")]
	[Export]
	public DurationType DurationType
	{
		get
		{
			return _durationType;
		}

		set
		{
			_durationType = value;

			if (value == DurationType.Instant)
			{
				_hasPeriodicApplication = false;
				_canStack = false;
			}

			NotifyPropertyListChanged();
		}
	}

	[Export]
	public float Duration { get; set; }

	[ExportGroup("Periodic Data")]

	[Export]
	public bool HasPeriodicApplication
	{
		get
		{
			return _hasPeriodicApplication;
		}

		set
		{
			_hasPeriodicApplication = value;
			NotifyPropertyListChanged();
		}
	}

	[Export]
	public float Period { get; set; }

	[Export]
	public bool ExecuteOnApplication { get; set; }

	[ExportGroup("Stacking Data")]
	[Export]
	public bool CanStack
	{
		get
		{
			return _canStack;
		}

		set
		{
			_canStack = value;
			NotifyPropertyListChanged();
		}
	}

	[Export]
	public int StackLimit { get; set; } = 1;

	[Export]
	public int InitialStack { get; set; } = 1;

	[Export]
	public StackPolicy SourcePolicy
	{
		get
		{
			return _sourcePolicy;
		}

		set
		{
			_sourcePolicy = value;
			NotifyPropertyListChanged();
		}
	}

	[Export]
	[ExportSubgroup("Aggregate by Target", "Instigator")]
	public StackInstigatorDenialPolicy InstigatorDenialPolicy { get; set; }

	[Export]
	public StackInstigatorOverridePolicy InstigatorOverridePolicy
	{
		get
		{
			return _instigatorOverridePolicy;
		}

		set
		{
			_instigatorOverridePolicy = value;
			NotifyPropertyListChanged();
		}
	}

	[Export]
	public StackInstigatorOverrideStackCountPolicy InstigatorOverrideStackCountPolicy { get; set; }

	[Export]
	public StackLevelPolicy LevelPolicy
	{
		get
		{
			return _levelPolicy;
		}

		set
		{
			_levelPolicy = value;
			NotifyPropertyListChanged();
		}
	}

	[Export]
	[ExportSubgroup("Aggregate Levels", "Level")]
	public LevelComparison LevelDenialPolicy { get; set; }

	[Export]
	public LevelComparison LevelOverridePolicy
	{
		get
		{
			return _levelOverridePolicy;
		}

		set
		{
			_levelOverridePolicy = value;
			NotifyPropertyListChanged();
		}
	}

	[Export]
	public StackLevelOverrideStackCountPolicy LevelOverrideStackCountPolicy { get; set; }

	[Export]
	public StackMagnitudePolicy MagnitudePolicy { get; set; }

	[Export]
	public StackOverflowPolicy OverflowPolicy { get; set; }

	[Export]
	public StackExpirationPolicy ExpirationPolicy { get; set; }

	[Export]
	[ExportSubgroup("Has Duration")]
	public StackApplicationRefreshPolicy ApplicationRefreshPolicy { get; set; }

	[Export]
	[ExportSubgroup("Periodic Effects")]
	public StackApplicationResetPeriodPolicy ApplicationResetPeriodPolicy { get; set; }
	
	[Export]
	public bool ExecuteOnSuccessfulApplication { get; set; }

	// This method controls the visibility of properties in the editor
	public override void _ValidateProperty(Dictionary property)
	{
		if (property["name"].AsStringName() == PropertyName.Duration && DurationType != DurationType.HasDuration)
		{
			property["usage"] = (int)(PropertyUsageFlags.Default | PropertyUsageFlags.ReadOnly);
		}

		if (DurationType == DurationType.Instant && property["name"].AsStringName() == PropertyName.HasPeriodicApplication)
		{
			property["usage"] = (int)(PropertyUsageFlags.Default | PropertyUsageFlags.ReadOnly);
		}

		if (!HasPeriodicApplication)
		{
			if (property["name"].AsStringName() == PropertyName.Period ||
				property["name"].AsStringName() == PropertyName.ExecuteOnApplication ||
				property["name"].AsStringName() == PropertyName.ApplicationResetPeriodPolicy ||
				property["name"].AsStringName() == PropertyName.ExecuteOnSuccessfulApplication)
			{
				property["usage"] = (int)(PropertyUsageFlags.Default | PropertyUsageFlags.ReadOnly);
			}
		}

		if (DurationType == DurationType.Instant || !CanStack)
		{
			if (property["name"].AsStringName() == PropertyName.StackLimit ||
				property["name"].AsStringName() == PropertyName.InitialStack ||
				property["name"].AsStringName() == PropertyName.SourcePolicy ||
				property["name"].AsStringName() == PropertyName.InstigatorDenialPolicy ||
				property["name"].AsStringName() == PropertyName.InstigatorOverridePolicy ||
				property["name"].AsStringName() == PropertyName.InstigatorOverrideStackCountPolicy ||
				property["name"].AsStringName() == PropertyName.LevelPolicy ||
				property["name"].AsStringName() == PropertyName.LevelDenialPolicy ||
				property["name"].AsStringName() == PropertyName.LevelOverridePolicy ||
				property["name"].AsStringName() == PropertyName.LevelOverrideStackCountPolicy ||
				property["name"].AsStringName() == PropertyName.MagnitudePolicy ||
				property["name"].AsStringName() == PropertyName.OverflowPolicy ||
				property["name"].AsStringName() == PropertyName.ExpirationPolicy ||
				property["name"].AsStringName() == PropertyName.ApplicationRefreshPolicy ||
				property["name"].AsStringName() == PropertyName.ApplicationResetPeriodPolicy ||
				property["name"].AsStringName() == PropertyName.ExecuteOnSuccessfulApplication)
			{
				property["usage"] = (int)(PropertyUsageFlags.Default | PropertyUsageFlags.ReadOnly);
			}
		}

		if (SourcePolicy == StackPolicy.AggregateBySource)
		{
			
			if (property["name"].AsStringName() == PropertyName.InstigatorDenialPolicy ||
				property["name"].AsStringName() == PropertyName.InstigatorOverridePolicy ||
				property["name"].AsStringName() == PropertyName.InstigatorOverrideStackCountPolicy)
			{
				property["usage"] = (int)(PropertyUsageFlags.Default | PropertyUsageFlags.ReadOnly);
			}
		}

		if (InstigatorOverridePolicy != StackInstigatorOverridePolicy.Override)
		{
			if (property["name"].AsStringName() == PropertyName.InstigatorOverrideStackCountPolicy)
			{
				property["usage"] = (int)(PropertyUsageFlags.Default | PropertyUsageFlags.ReadOnly);
			}
		}

		if (LevelPolicy == StackLevelPolicy.SegregateLevels)
		{

			if (property["name"].AsStringName() == PropertyName.LevelDenialPolicy ||
				property["name"].AsStringName() == PropertyName.LevelOverridePolicy ||
				property["name"].AsStringName() == PropertyName.LevelOverrideStackCountPolicy)
			{
				property["usage"] = (int)(PropertyUsageFlags.Default | PropertyUsageFlags.ReadOnly);
			}
		}

		if (LevelOverridePolicy == 0)
		{
			if (property["name"].AsStringName() == PropertyName.LevelOverrideStackCountPolicy)
			{
				property["usage"] = (int)(PropertyUsageFlags.Default | PropertyUsageFlags.ReadOnly);
			}
		}

		if (DurationType != DurationType.HasDuration &&
			property["name"].AsStringName() == PropertyName.ApplicationRefreshPolicy)
		{
			property["usage"] = (int)(PropertyUsageFlags.Default | PropertyUsageFlags.ReadOnly);
		}
	}

	// Could be re-using the same instance
	public GameplayEffectData GetEffectData()
	{
		var effect = new GameplayEffectData(
			Name,
			GetDurationData(),
			GetStackingData(),
			GetPeriodicData(),
			SnapshotLevel);

		foreach (var modifier in Modifiers)
		{
			effect.Modifiers.Add(modifier.GetModifier());
		}

		return effect;
	}

	private DurationData GetDurationData()
	{
		return new DurationData
		{
			Type = DurationType,
			Duration = GetDuration(),
		};
	}

	private ScalableFloat GetDuration()
	{
		if (DurationType != DurationType.HasDuration)
		{
			return null;
		}

		return new ScalableFloat(Duration);
	}

	private StackingData? GetStackingData()
	{
		if (!CanStack)
		{
			return null;
		}

		return new StackingData
		{
			StackLimit = new ScalableInt(StackLimit),
			InitialStack = new ScalableInt(InitialStack),
			StackPolicy = SourcePolicy,
			StackLevelPolicy = LevelPolicy,
			MagnitudePolicy = MagnitudePolicy,
			OverflowPolicy = OverflowPolicy,
			ExpirationPolicy = ExpirationPolicy,
			InstigatorDenialPolicy = GetInstigatorDenialPolicy(),
			InstigatorOverridePolicy = GetInstigatorOverridePolicy(),
			InstigatorOverrideStackCountPolicy = GetInstigatorOverrideStackCountPolicy(),
			LevelDenialPolicy = GetLevelDenialPolicy(),
			LevelOverridePolicy = GetLevelOverridePolicy(),
			LevelOverrideStackCountPolicy = GetLevelOverrideStackCountPolicy(),
			ApplicationRefreshPolicy = GetApplicationRefreshPolicy(),
			ApplicationResetPeriodPolicy = GetApplicationResetPeriodPolicy(),
			ExecuteOnSuccessfulApplication = GetExecuteOnSuccessfulApplication(),
		};
	}

	private StackInstigatorDenialPolicy? GetInstigatorDenialPolicy()
	{
		if (SourcePolicy != StackPolicy.AggregateByTarget)
		{
			return null;
		}

		return InstigatorDenialPolicy;
	}

	private StackInstigatorOverridePolicy? GetInstigatorOverridePolicy()
	{
		if (SourcePolicy != StackPolicy.AggregateByTarget)
		{
			return null;
		}

		return InstigatorOverridePolicy;
	}

	private StackInstigatorOverrideStackCountPolicy? GetInstigatorOverrideStackCountPolicy()
	{
		if (SourcePolicy != StackPolicy.AggregateByTarget ||
			InstigatorOverridePolicy != StackInstigatorOverridePolicy.Override)
		{
			return null;
		}

		return InstigatorOverrideStackCountPolicy;
	}

	private LevelComparison? GetLevelDenialPolicy()
	{
		if (LevelPolicy != StackLevelPolicy.AggregateLevels)
		{
			return null;
		}

		return LevelDenialPolicy;
	}

	private LevelComparison? GetLevelOverridePolicy()
	{
		if (LevelPolicy != StackLevelPolicy.AggregateLevels)
		{
			return null;
		}

		return LevelOverridePolicy;
	}

	private StackLevelOverrideStackCountPolicy? GetLevelOverrideStackCountPolicy()
	{
		if (LevelPolicy != StackLevelPolicy.AggregateLevels ||
			LevelOverridePolicy == 0)
		{
			return null;
		}

		return LevelOverrideStackCountPolicy;
	}

	private StackApplicationRefreshPolicy? GetApplicationRefreshPolicy()
	{
		if (DurationType != DurationType.HasDuration)
		{
			return null;
		}

		return ApplicationRefreshPolicy;
	}

	private StackApplicationResetPeriodPolicy? GetApplicationResetPeriodPolicy()
	{
		if (!HasPeriodicApplication)
		{
			return null;
		}

		return ApplicationResetPeriodPolicy;
	}

	private bool? GetExecuteOnSuccessfulApplication()
	{
		if (!HasPeriodicApplication)
		{
			return null;
		}

		return ExecuteOnSuccessfulApplication;
	}

	private PeriodicData? GetPeriodicData()
	{
		if (!HasPeriodicApplication)
		{
			return null;
		}

		return new PeriodicData
		{
			Period = new ScalableFloat(Period),
			ExecuteOnApplication = ExecuteOnApplication,
		};
	}
}
