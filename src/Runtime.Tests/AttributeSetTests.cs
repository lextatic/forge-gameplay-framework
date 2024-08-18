#pragma warning disable SA1600 // Elements should be documented
using GameplayTags.Runtime.Attribute;

namespace GameplayTags.Runtime.Tests;
using Attribute = Attribute.Attribute;

[TestClass]
public class AttributeSetTests
{
	[TestMethod]
	[TestCategory("Initialization")]
	public void Default_constructor_initializes_correctly()
	{
		var set = new SimpleAttributeSet();

		var attribute = set.NonInitializedAttribute;

		Assert.AreEqual(0, attribute.BaseValue);
		Assert.AreEqual(0, attribute.Modifier);
		Assert.AreEqual(0, attribute.PercentBonus);
		Assert.AreEqual(0, attribute.PercentPenalty);
		Assert.AreEqual(0, attribute.Overflow);
		Assert.AreEqual(int.MinValue, attribute.Min);
		Assert.AreEqual(int.MaxValue, attribute.Max);
		Assert.AreEqual(0, attribute.TotalValue);
	}

	[TestMethod]
	[TestCategory("Initialization")]
	public void Should_initialize_values_correctly()
	{
		var set = new SimpleAttributeSet();
		var attribute = set.InitializedAttribute;

		Assert.AreEqual(5, attribute.BaseValue);
		Assert.AreEqual(0, attribute.Modifier);
		Assert.AreEqual(0, attribute.PercentBonus);
		Assert.AreEqual(0, attribute.PercentPenalty);
		Assert.AreEqual(0, attribute.Overflow);
		Assert.AreEqual(0, attribute.Min);
		Assert.AreEqual(10, attribute.Max);
		Assert.AreEqual(5, attribute.TotalValue);
	}

	[TestMethod]
	[TestCategory("Initialization")]
	[ExpectedException(typeof(ArgumentException), "Default value should always be within Min and Max values.")]
	public void Should_throw_exception_when_initialized_with_default_value_exceeding_MaxValue()
	{
		new DefaultValueAboveMaxValueAttributeSet();
	}

	[TestMethod]
	[TestCategory("Initialization")]
	[ExpectedException(typeof(ArgumentException), "Default value should always be within Min and Max values.")]
	public void Should_throw_exception_when_initialized_with_default_value_below_MinValue()
	{
		new DefaultValueBelowMinValueAttributeSet();
	}

	[TestMethod]
	[TestCategory("Initialization")]
	[ExpectedException(typeof(ArgumentException), "Default value should always be within Min and Max values.")]
	public void Should_throw_exception_when_initialized_with_inverted_MinValue_and_MaxValue()
	{
		new InvertedMaxMinValuesAttributeSet();
	}

	private class SimpleAttributeSet : AttributeSet
	{
#pragma warning disable CS8618, S3459, SA1401
		public readonly Attribute InitializedAttribute;

		public readonly Attribute NonInitializedAttribute;

		protected override void InitializeAttributes()
		{
			InitializeAttribute(InitializedAttribute, 5, 0, 10);
		}
	}

	private class DefaultValueAboveMaxValueAttributeSet : AttributeSet
	{
		public readonly Attribute InitializedAttribute;

		protected override void InitializeAttributes()
		{
			InitializeAttribute(InitializedAttribute, 30, 0, 20);
		}
	}

	private class DefaultValueBelowMinValueAttributeSet : AttributeSet
	{
		public readonly Attribute InitializedAttribute;

		protected override void InitializeAttributes()
		{
			InitializeAttribute(InitializedAttribute, -10, 0, 20);
		}
	}

	private class InvertedMaxMinValuesAttributeSet : AttributeSet
	{
		public readonly Attribute InitializedAttribute;

		protected override void InitializeAttributes()
		{
			InitializeAttribute(InitializedAttribute, 10, 20, 0);
		}
	}

	private class PlayerAttributeSet : AttributeSet
	{
		public readonly Attribute Strength;

		public readonly Attribute Intelligence;

		public readonly Attribute Dexterity;

		public readonly Attribute Vitality;

		public readonly Attribute Agility;

		public readonly Attribute Luck;

		protected override void InitializeAttributes()
		{
			InitializeAttribute(Strength, 1, 0, 99);
			InitializeAttribute(Intelligence, 1, 0, 99);
			InitializeAttribute(Dexterity, 1, 0, 99);
			InitializeAttribute(Vitality, 1, 0, 99);
			InitializeAttribute(Agility, 1, 0, 99);
			InitializeAttribute(Luck, 1, 0, 99);
		}
	}

	private class ResourceAttributeSet : AttributeSet
	{
		public readonly Attribute Health;

		public readonly Attribute MaxHealth;

		public readonly Attribute Vitality;

		protected override void InitializeAttributes()
		{
			//
		}

		protected override void Attribute_OnValueChanged(Attribute attribute, int change)
		{
			base.Attribute_OnValueChanged(attribute, change);

			if (attribute == Vitality)
			{
				// Do health to vit calculations here.
				SetAttributeMaxValue(MaxHealth, Vitality.TotalValue * 10);
			}

			if (attribute == MaxHealth)
			{
				SetAttributeMaxValue(Health, MaxHealth.TotalValue);
			}

			if (attribute == Health)
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
}
