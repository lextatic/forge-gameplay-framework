#pragma warning disable SA1600 // Elements should be documented
using GameplayTags.Runtime.Attribute;

namespace GameplayTags.Runtime.Tests;

[TestClass]
public class AttributeTests
{
	[TestMethod]
	public void AttributesTests()
	{
		var strenghtAttribute = new Attribute.Attribute(10, 1, 99);

		strenghtAttribute.ApplyModifier(5);
		strenghtAttribute.SetBaseValue(11);

		Assert.AreEqual(16, strenghtAttribute.TotalValue);

		strenghtAttribute.SetBaseValue(9);

		Assert.AreEqual(14, strenghtAttribute.TotalValue);

		strenghtAttribute.Reset();

		Assert.AreEqual(9, strenghtAttribute.TotalValue);
	}

	[TestMethod]
	public void ClampTests()
	{
		var resourceAttributes = new ResourceAttributeSet();

		resourceAttributes.MaxHealth.SetBaseValue(10);

		resourceAttributes.Health.SetBaseValue(12);

		Assert.AreEqual(10, resourceAttributes.Health.TotalValue);

		resourceAttributes.Health.ApplyModifier(4);

		Assert.AreEqual(10, resourceAttributes.Health.TotalValue);
	}
}
