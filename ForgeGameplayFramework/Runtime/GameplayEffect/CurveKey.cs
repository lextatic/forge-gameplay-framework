namespace GameplayTags.Runtime.GameplayEffect;

public struct CurveKey
{
	public float Time { get; set; }
	public float Value { get; set; }

	public CurveKey(float time, float value)
	{
		Time = time;
		Value = value;
	}
}
