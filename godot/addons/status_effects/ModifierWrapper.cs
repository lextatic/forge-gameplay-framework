using GameplayTags.Runtime;
using GameplayTags.Runtime.Attribute;
using GameplayTags.Runtime.GameplayEffect;
using Godot;
using Godot.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

[Tool]
[GlobalClass]
public partial class ModifierWrapper : Resource
{
	private MagnitudeCalculationType _calculationType;
	private AttributeBasedFloatCalculationType _attributeCalculationType;

	[Export]
	public string Attribute { get; set; }

	[Export]
	public ModifierOperation Operation { get; set; }

	[Export]
	public int Channel { get; set; }

	[ExportGroup("Magnitude")]
	[Export]
	public MagnitudeCalculationType CalculationType
	{
		get
		{
			return _calculationType;
		}

		set
		{
			_calculationType = value;
			NotifyPropertyListChanged();
		}
	}

	[ExportGroup("Scalable Float")]
	[Export]
	public float ScalableFloat;

	[ExportGroup("Attribute Based")]
	[Export]
	public string CapturedAttribute { get; set; }

	[ExportSubgroup("Attribute Based Capture Definition")]
	[Export]
	public AttributeCaptureSource CaptureSource { get; set; }

	[Export]
	public bool SnapshotAttribute { get; set; }

	[ExportSubgroup("Attribute Based Calculation")]
	[Export]
	public AttributeBasedFloatCalculationType AttributeCalculationType
	{
		get
		{
			return _attributeCalculationType;
		}

		set
		{
			_attributeCalculationType = value;
			NotifyPropertyListChanged();
		}
	}

	[Export]
	public float Coeficient = 1;

	[Export]
	public float PreMultiplyAdditiveValue;

	[Export]
	public float PostMultiplyAdditiveValue;

	[Export]
	public int FinalChannel;

	// Override to provide custom properties to the inspector
	public override void _ValidateProperty(Dictionary property)
	{
		if (property["name"].AsStringName() == PropertyName.Attribute ||
			property["name"].AsStringName() == PropertyName.CapturedAttribute)
		{
			property["hint"] = (int)PropertyHint.Enum;
			property["hint_string"] = string.Join(",", GetAttributeOptions());
		}

		if (property["name"].AsStringName() == PropertyName.ScalableFloat && CalculationType != MagnitudeCalculationType.ScalableFloat)
		{
			property["usage"] = (int)PropertyUsageFlags.NoEditor;
		}

		if (CalculationType != MagnitudeCalculationType.AttributeBased)
		{
			if (property["name"].AsStringName() == PropertyName.CapturedAttribute ||
				property["name"].AsStringName() == PropertyName.CaptureSource ||
				property["name"].AsStringName() == PropertyName.SnapshotAttribute ||
				property["name"].AsStringName() == PropertyName.AttributeCalculationType ||
				property["name"].AsStringName() == PropertyName.Coeficient ||
				property["name"].AsStringName() == PropertyName.PreMultiplyAdditiveValue ||
				property["name"].AsStringName() == PropertyName.PostMultiplyAdditiveValue ||
				property["name"].AsStringName() == PropertyName.FinalChannel)
			{
				property["usage"] = (int)PropertyUsageFlags.NoEditor;
			}
		}

		if (property["name"].AsStringName() == PropertyName.FinalChannel &&
			AttributeCalculationType != AttributeBasedFloatCalculationType.AttributeMagnitudeEvaluatedUpToChannel)
		{
			property["usage"] = (int)PropertyUsageFlags.NoEditor;
		}
	}

	// Use reflection to gather all classes inheriting from AttributeSet and their fields of type Attribute
	private string[] GetAttributeOptions()
	{
		var options = new List<string>();

		// Get all types in the current assembly
		var allTypes = Assembly.GetExecutingAssembly().GetTypes();

		// Find all types that subclass AttributeSet
		var attributeSetTypes = allTypes.Where(t => t.IsSubclassOf(typeof(AttributeSet)));

		foreach (var attributeSetType in attributeSetTypes)
		{
			// Get public instance fields of type Attribute
			var attributeFields = attributeSetType.GetFields(BindingFlags.Public | BindingFlags.Instance)
												  .Where(f => f.FieldType == typeof(Attribute));

			foreach (var field in attributeFields)
			{
				// Build the dropdown option string in the format ClassName.FieldName
				string option = $"{attributeSetType.Name}.{field.Name}";
				options.Add(option);
			}
		}

		return options.ToArray();
	}

	public Modifier GetModifier()
	{
		return new Modifier
		{
			Attribute = TagName.FromString(Attribute),
			Operation = Operation,
			Magnitude = new ModifierMagnitude
			{
				MagnitudeCalculationType = CalculationType,
				ScalableFloatMagnitude = CalculationType == MagnitudeCalculationType.ScalableFloat ?
					new ScalableFloat(ScalableFloat) : null,
				AttributeBasedFloat = CalculationType == MagnitudeCalculationType.AttributeBased ?
					new AttributeBasedFloat()
					{
						BackingAttribute = new AttributeCaptureDefinition
						{
							Attribute = TagName.FromString(CapturedAttribute),
							Source = CaptureSource,
							Snapshot = SnapshotAttribute,
						},
						AttributeCalculationType = AttributeCalculationType,
						Coeficient = new ScalableFloat(Coeficient),
						PreMultiplyAdditiveValue = new ScalableFloat(PreMultiplyAdditiveValue),
						PostMultiplyAdditiveValue = new ScalableFloat(PostMultiplyAdditiveValue),
						FinalChannel = FinalChannel,
					} : null,
			},
			Channel = Channel,
		};
	}
}
