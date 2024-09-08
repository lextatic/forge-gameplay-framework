using GameplayTags.Runtime;
using GameplayTags.Runtime.GameplayEffect;
using Godot;
using Godot.Collections;
using System.Collections.Generic;

[Tool]
public partial class GameplayEffect : Resource
{
	private DurationType _durationType;
	private bool _hasPeriodicApplication;

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
	public string StackingBlabla { get; set; }

	// This method controls the visibility of properties in the editor
	public override void _ValidateProperty(Dictionary property)
	{
		if (property["name"].AsStringName() == PropertyName.Duration && DurationType != DurationType.HasDuration)
		{
			property["usage"] = (int)PropertyUsageFlags.NoEditor;
		}

		if (DurationType == DurationType.Instant && property["name"].AsStringName() == PropertyName.HasPeriodicApplication)
		{
			property["usage"] = (int)(PropertyUsageFlags.Default | PropertyUsageFlags.ReadOnly);
		}

		if (!HasPeriodicApplication)
		{
			if (property["name"].AsStringName() == PropertyName.Period ||
				property["name"].AsStringName() == PropertyName.ExecuteOnApplication)
			{
				property["usage"] = (int)PropertyUsageFlags.NoEditor;
			}
		}
	}

	// Could be re-using the same instance
	public GameplayEffectData GetEffectData()
	{
		var effect = new GameplayEffectData(
			Name,
			new DurationData
			{
				Type = DurationType,
				Duration = DurationType == DurationType.HasDuration ? new ScalableFloat(Duration) : null,
			},
			null,
			HasPeriodicApplication ?
			new PeriodicData
			{
				Period = new ScalableFloat(Period),
				ExecuteOnApplication = ExecuteOnApplication,
			}
			: null);

		foreach (var modifier in Modifiers)
		{
			effect.Modifiers.Add(new Modifier
			{
				Attribute = TagName.FromString(modifier.Attribute),
				Operation = modifier.Operation,
				Magnitude = new ModifierMagnitude
				{
					MagnitudeCalculationType = MagnitudeCalculationType.ScalableFloat,
					ScalableFloatMagnitude = new ScalableFloat(modifier.Magnitude),
				},
			});
		}

		return effect;
	}
}
