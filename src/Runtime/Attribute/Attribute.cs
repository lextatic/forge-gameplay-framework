#pragma warning disable SA1600

namespace GameplayTags.Runtime.Attribute;

public class Attribute
{
	internal event Action<Attribute, int>? OnValueChanged;

	public int BaseValue { get; private set; }

	public int ModifierValue { get; private set; }

	public int MaxValue { get; private set; }

	public int MinValue { get; private set; }

	public int TotalValue { get; private set; }

	public Attribute()
	{
		BaseValue = 0;
		MinValue = int.MinValue;
		MaxValue = int.MaxValue;
		ModifierValue = 0;
		TotalValue = 0;
	}

	public Attribute(int defaultValue, int minValue = int.MinValue, int maxValue = int.MaxValue)
	{
		BaseValue = defaultValue;
		MinValue = minValue;
		MaxValue = maxValue;
		ModifierValue = 0;
		TotalValue = Math.Clamp(BaseValue + ModifierValue, MinValue, MaxValue);
	}

	internal void Reset()
	{
		ModifierValue = 0;
	}

	internal void SetMaxValue(int newMaxValue)
	{
		int oldValue = TotalValue;

		MaxValue = Math.Max(newMaxValue, MinValue);
		BaseValue = Math.Min(BaseValue, MaxValue);

		TotalValue = Math.Clamp(BaseValue + ModifierValue, MinValue, MaxValue);

		if (TotalValue != oldValue)
		{
			OnValueChanged?.Invoke(this, TotalValue - oldValue);
		}
	}

	internal void SetMinValue(int newMinValue)
	{
		int oldValue = TotalValue;

		MinValue = Math.Min(newMinValue, MaxValue);
		BaseValue = Math.Max(BaseValue, MinValue);

		TotalValue = Math.Clamp(BaseValue + ModifierValue, MinValue, MaxValue);

		if (TotalValue != oldValue)
		{
			OnValueChanged?.Invoke(this, TotalValue - oldValue);
		}
	}

	internal void SetBaseValue(int newValue)
	{
		int oldValue = TotalValue;

		BaseValue = Math.Clamp(newValue, MinValue, MaxValue);

		TotalValue = Math.Clamp(BaseValue + ModifierValue, MinValue, MaxValue);

		if (TotalValue != oldValue)
		{
			OnValueChanged?.Invoke(this, TotalValue - oldValue);
		}
	}

	internal void AddToBaseValue(int value)
	{
		int oldValue = TotalValue;

		BaseValue = Math.Clamp(BaseValue + value, MinValue, MaxValue);

		TotalValue = Math.Clamp(BaseValue + ModifierValue, MinValue, MaxValue);

		if (TotalValue != oldValue)
		{
			OnValueChanged?.Invoke(this, TotalValue - oldValue);
		}
	}

	internal void ApplyModifier(int value)
	{
		int oldValue = TotalValue;

		ModifierValue += value;

		TotalValue = Math.Clamp(BaseValue + ModifierValue, MinValue, MaxValue);

		if (TotalValue != oldValue)
		{
			OnValueChanged?.Invoke(this, TotalValue - oldValue);
		}
	}
}
