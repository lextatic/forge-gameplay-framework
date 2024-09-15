#pragma warning disable SA1600 // Elements should be documented
using GameplayTags.Runtime.GameplayEffect;

namespace GameplayTags.Runtime.Tests;

[TestClass]
public class ScalableFloatTests
{
	[TestMethod]
	public void ScalableFloatTest()
	{
		var scalableDamage = new ScalableFloat(100.0f);

		// Define the scaling curve
		scalableDamage.ScalingCurve.AddKey(0.0f, 1.0f);  // At time 0, scale is 1.0
		scalableDamage.ScalingCurve.AddKey(1.0f, 1.5f);  // At time 1, scale is 1.5

		// Evaluate the scalable float at different times
		float adjustedDamageAt0 = scalableDamage.GetValue(0.0f);   // 100.0
		float adjustedDamageAt0_5 = scalableDamage.GetValue(0.5f); // 125.0
		float adjustedDamageAt1 = scalableDamage.GetValue(1.0f);   // 150.0

		Assert.AreEqual(100f, adjustedDamageAt0);
		Assert.AreEqual(125f, adjustedDamageAt0_5);
		Assert.AreEqual(150f, adjustedDamageAt1);
	}
}
