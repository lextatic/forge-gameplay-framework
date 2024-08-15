namespace GameplayTags.Runtime.GameplayEffect;

public struct ScalableInt
{
	public int BaseValue { get; set; }

	public Curve ScalingCurve { get; set; }

	public ScalableInt(int baseValue)
	{
		BaseValue = baseValue;
		ScalingCurve = new Curve();

		ScalingCurve.AddKey(0, 0);
		ScalingCurve.AddKey(1, 1);
		ScalingCurve.AddKey(2, 2);
		ScalingCurve.AddKey(3, 3);
	}

	public int GetValue(float time)
	{
		float scalingFactor = ScalingCurve.Evaluate(time);
		return (int)(BaseValue * scalingFactor);
	}
}
