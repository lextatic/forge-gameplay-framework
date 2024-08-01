#pragma warning disable SA1600

namespace GameplayTags.Runtime.Attribute;

public class Attribute
{
	internal delegate void PreAttributeChangeDelegate(Attribute attribute, ref float newValue);

	internal event PreAttributeChangeDelegate OnPreBaseValueChange;

	internal event Action<Attribute, float, float> OnPostBaseValueChange;

	internal event PreAttributeChangeDelegate OnPreModifierValueChange;

	internal event Action<Attribute, float, float> OnPostModifierValueChange;

	public float BaseValue{ get; private set; }

	public float ModifierValue => TotalValue - BaseValue;

	public float TotalValue { get; private set; }

	internal void Reset()
	{
		TotalValue = BaseValue;
	}

	internal void SetBaseValue(float newValue)
	{
		float oldValue = BaseValue;
		OnPreBaseValueChange?.Invoke(this, ref newValue);

		BaseValue = newValue;
		TotalValue += newValue - oldValue;

		OnPostBaseValueChange(this, oldValue, newValue);
	}

	internal void SetTotalValue(float newValue)
	{
		float oldValue = TotalValue;
		OnPreModifierValueChange?.Invoke(this, ref newValue);

		TotalValue = newValue;

		OnPostModifierValueChange(this, oldValue, newValue);
	}

	internal void AddBaseValue(float value)
	{
		float oldValue = BaseValue;
		float newValue = BaseValue + value;
		OnPreBaseValueChange?.Invoke(this, ref newValue);

		var change = newValue - oldValue;

		BaseValue += change;
		TotalValue += change;

		OnPostBaseValueChange(this, oldValue, newValue);
	}

	internal void AddModifier(float value)
	{
		float oldValue = TotalValue;
		float newValue = TotalValue + value;
		OnPreModifierValueChange?.Invoke(this, ref newValue);

		var change = newValue - oldValue;

		TotalValue += change;

		OnPostModifierValueChange(this, oldValue, newValue);
	}
}
