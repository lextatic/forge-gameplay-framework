#pragma warning disable SA1600 // Elements should be documented
using System;
using System.Reflection;

namespace GameplayTags.Runtime.Attribute;

/// <summary>
/// A set of attributes.
/// </summary>
public abstract class AttributeSet
{
	public Dictionary<TagName, Attribute> AttributesMap { get; }

	/// <summary>
	/// Initializes a new instance of the <see cref="AttributeSet"/> class.
	/// </summary>
	protected AttributeSet()
	{
		// Fetch fields of type Attribute
		var fields = GetType().GetFields(BindingFlags.Public | BindingFlags.Instance)
									  .Where(f => f.FieldType == typeof(Attribute));

		AttributesMap = new Dictionary<TagName, Attribute>(fields.Count());

		foreach (var field in fields)
		{
			var attributeName = $"{GetType().Name}.{field.Name}";

			// Create an instance of the Attribute class
			if (Activator.CreateInstance(field.FieldType, true) is Attribute attributeInstance)
			{
				// Set the value using the field
				field.SetValue(this, attributeInstance);

				AttributesMap.Add(TagName.FromString(attributeName), attributeInstance);
				attributeInstance.OnValueChanged += Attribute_OnValueChanged;
			}
		}

		InitializeAttributes();
	}

	// Use this method to initialize all attributes
	protected abstract void InitializeAttributes();

	protected virtual void PreGameplayEffectExecute() { }

	protected virtual void PostGameplayEffectExecute() { }

	protected virtual void Attribute_OnValueChanged(Attribute attribute, int change) { }

	protected void InitializeAttribute(Attribute attribute, int defaultValue, int minValue = int.MinValue, int maxValue = int.MaxValue)
	{
		attribute.Initialize(defaultValue, minValue, maxValue);
	}

	protected void SetAttributeMaxValue(Attribute attribute, int maxValue)
	{
		attribute.SetMaxValue(maxValue);
	}

	protected void SetAttributeMinValue(Attribute attribute, int minValue)
	{
		attribute.SetMinValue(minValue);
	}

	protected void SetAttributeBaseValue(Attribute attribute, int newValue)
	{
		attribute.ExecuteOverride(newValue);
	}

	protected void AddToAttributeBaseValue(Attribute attribute, int value)
	{
		attribute.ExecuteFlatModifier(value);
	}
}
