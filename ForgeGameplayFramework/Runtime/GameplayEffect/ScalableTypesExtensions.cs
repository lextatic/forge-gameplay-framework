namespace GameplayTags.Runtime.GameplayEffect;

public static class ScalableTypesExtensions
{
	public static ScalableFloat AddKey(this ScalableFloat scalableFloat, float time, float value)
	{
		scalableFloat.ScalingCurve.AddKey(time, value);
		return scalableFloat;
	}

	public static ScalableInt AddKey(this ScalableInt scalableInt, float time, float value)
	{
		scalableInt.ScalingCurve.AddKey(time, value);
		return scalableInt;
	}
}
