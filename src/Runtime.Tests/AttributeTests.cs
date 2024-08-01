#pragma warning disable SA1600 // Elements should be documented
using GameplayTags.Runtime.Attribute;

namespace GameplayTags.Runtime.Tests;

[TestClass]
public class AttributeTests
{
	[TestMethod]
	public void AttributesTests()
	{
		var strenghtAttribute = new Attribute.Attribute();
		strenghtAttribute.AddBaseValue(10f);

		strenghtAttribute.AddModifier(5f);
		strenghtAttribute.AddBaseValue(1f);

		Assert.AreEqual(16f, strenghtAttribute.TotalValue);

		strenghtAttribute.AddBaseValue(-2f);

		Assert.AreEqual(14f, strenghtAttribute.TotalValue);

		strenghtAttribute.Reset();

		Assert.AreEqual(9f, strenghtAttribute.TotalValue);
	}

	[TestMethod]
	public void ClampTests()
	{
		var resourceAttributes = new ResourceAttributeSet();

		resourceAttributes.MaxHealth.AddBaseValue(10);

		resourceAttributes.Health.AddBaseValue(12);

		Assert.AreEqual(10f, resourceAttributes.Health.TotalValue);
	}
}
