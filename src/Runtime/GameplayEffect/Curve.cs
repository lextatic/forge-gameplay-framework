namespace GameplayTags.Runtime.GameplayEffect;

public class Curve
{
	private List<CurveKey> keys = new List<CurveKey>();

	public void AddKey(float time, float value)
	{
		keys.Add(new CurveKey(time, value));
		keys.Sort((a, b) => a.Time.CompareTo(b.Time));
	}

	public float Evaluate(float time)
	{
		if (keys.Count == 0)
		{
			return 1.0f; // Default scaling factor if no keys are defined
		}

		if (time <= keys[0].Time)
		{
			return keys[0].Value;
		}

		if (time >= keys[keys.Count - 1].Time)
		{
			return keys[keys.Count - 1].Value;
		}

		for (int i = 0; i < keys.Count - 1; i++)
		{
			if (time >= keys[i].Time && time <= keys[i + 1].Time)
			{
				float t = (time - keys[i].Time) / (keys[i + 1].Time - keys[i].Time);
				return keys[i].Value + (t * (keys[i + 1].Value - keys[i].Value));
			}
		}

		return 1.0f; // Fallback
	}
}
