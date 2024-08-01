#pragma warning disable SA1600 // Elements should be documented
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
		var className = GetType().Name;
		var properties = GetType().GetProperties(BindingFlags.Public | BindingFlags.Instance)
							  .Where(f => f.PropertyType == typeof(Attribute));

		AttributesMap = new Dictionary<TagName, Attribute>(properties.Count());

		foreach (var property in properties)
		{
			var attributeName = $"{className}.{property.Name}";
			if (Activator.CreateInstance(property.PropertyType) is Attribute attributeInstance)
			{
				property.SetValue(this, attributeInstance);
				AttributesMap.Add(TagName.FromString(attributeName), attributeInstance);

				attributeInstance.OnPreBaseValueChange += PreAttributeBaseChange;
				attributeInstance.OnPostBaseValueChange += PostAttributeBaseChange;
			}
		}
	}

	public virtual void PreGameplayEffectExecute() { }

	public virtual void PostGameplayEffectExecute() { }

	public virtual void PreAttributeBaseChange(Attribute attribute, ref float newValue) { }

	public virtual void PostAttributeBaseChange(Attribute attribute, float oldValue, float newValue) { }

	public virtual void PreAttributeModifierChange(Attribute attribute, ref float newValue) { }

	public virtual void PostAttributeModifierChange(Attribute attribute, ref float newValue) { }
}

public class PlayerAttributeSet : AttributeSet
{
	public Attribute Strength { get; set; }

	public Attribute Intelligence { get; set; }

	public Attribute Dexterity { get; set; }

	public Attribute Vitality { get; set; }

	public Attribute Agility { get; set; }

	public Attribute Luck { get; set; }
}

public class ResourceAttributeSet : AttributeSet
{
	public Attribute Health { get; set; }

	public Attribute MaxHealth { get; set; }

	public Attribute Mana { get; set; }

	public Attribute MaxMana { get; set; }

	public Attribute Stamina { get; set; }

	public Attribute MaxStamina { get; set; }

	public override void PreAttributeBaseChange(Attribute attribute, ref float newValue)
	{
		base.PreAttributeBaseChange(attribute, ref newValue);

		if (attribute == Health)
		{
			newValue = Math.Clamp(newValue, 0, MaxHealth.TotalValue);
		}
	}
}
