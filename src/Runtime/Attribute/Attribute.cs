#pragma warning disable SA1600

namespace GameplayTags.Runtime.Attribute;

public class Attribute
{
	// Should it be triggered with no changes?
	internal event Action<Attribute, int>? OnValueChange;

	public int BaseValue { get; private set; }

	public int ModifierValue { get; private set; }

	public int MaxValue { get; private set; }

	public int MinValue { get; private set; }

	// Could cache for performance
	public int TotalValue => Math.Clamp(BaseValue + ModifierValue, MinValue, MaxValue);

	public Attribute()
	{
		BaseValue = 0;
		MinValue = int.MinValue;
		MaxValue = int.MaxValue;
		ModifierValue = 0;
	}

	public Attribute(int defaultValue, int minValue = int.MinValue, int maxValue = int.MaxValue)
	{
		BaseValue = defaultValue;
		MinValue = minValue;
		MaxValue = maxValue;
		ModifierValue = 0;
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

		OnValueChange?.Invoke(this, TotalValue - oldValue);
	}

	internal void SetMinValue(int newMinValue)
	{
		int oldValue = TotalValue;

		MinValue = Math.Min(newMinValue, MaxValue);
		BaseValue = Math.Max(BaseValue, MinValue);

		OnValueChange?.Invoke(this, TotalValue - oldValue);
	}

	internal void SetBaseValue(int newValue)
	{
		int oldValue = TotalValue;

		BaseValue = Math.Clamp(newValue, MinValue, MaxValue);

		OnValueChange?.Invoke(this, TotalValue - oldValue);
	}

	internal void AddToBaseValue(int value)
	{
		int oldValue = TotalValue;

		BaseValue = Math.Clamp(BaseValue + value, MinValue, MaxValue);

		OnValueChange?.Invoke(this, TotalValue - oldValue);
	}

	internal void ApplyModifier(int value)
	{
		int oldValue = TotalValue;

		ModifierValue += value;

		OnValueChange?.Invoke(this, TotalValue - oldValue);
	}
}
