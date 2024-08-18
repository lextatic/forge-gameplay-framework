namespace GameplayTags.Runtime.GameplayEffect;

public struct ScalableInt
{
	public int BaseValue { get; set; }

	public Curve ScalingCurve { get; set; }

	public ScalableInt(int baseValue)
	{
		BaseValue = baseValue;
		ScalingCurve = new Curve();
	}

	public int GetValue(float time)
	{
		float scalingFactor = ScalingCurve.Evaluate(time);
		return (int)(BaseValue * scalingFactor);
	}
}
