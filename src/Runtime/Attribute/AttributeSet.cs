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

				attributeInstance.OnValueChange += OnAttributeValueChangeHandler;
			}
		}
	}

	public virtual void PreGameplayEffectExecute() { }

	public virtual void PostGameplayEffectExecute() { }

	public virtual void OnAttributeValueChangeHandler(Attribute attribute, int change) { }
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

	public Attribute Vit { get; set; }

	public override void OnAttributeValueChangeHandler(Attribute attribute, int change)
	{
		base.OnAttributeValueChangeHandler(attribute, change);

		if (attribute == Vit && change != 0)
		{
			// Do health to vit calculations here.
		}

		if (attribute == MaxHealth && change != 0)
		{
			Health.SetMaxValue(MaxHealth.TotalValue);
		}

		if (attribute == Health && change != 0)
		{
			if (change < 0)
			{
				Console.WriteLine($"Damage: {change}");

				if (Health.TotalValue <= 0)
				{
					Console.WriteLine("Death");
				}
			}
			else
			{
				Console.WriteLine($"Healing: {change}");
			}
		}
	}
}
