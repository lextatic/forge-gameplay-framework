namespace GameplayTags.Runtime.GameplayEffect;

public struct ScalableFloat
{
	public float BaseValue { get; set; }
	public Curve ScalingCurve { get; set; }

	public ScalableFloat(float baseValue)
	{
		BaseValue = baseValue;
		ScalingCurve = new Curve();
	}

	public float GetValue(float time)
	{
		float scalingFactor = ScalingCurve.Evaluate(time);
		return BaseValue * scalingFactor;
	}
}
