#pragma warning disable SA1600
using GameplayTags.Runtime.GameplayEffect;

namespace GameplayTags.Runtime.Attribute;

public struct GameplayEffectModifier
{
	public GameplayEffect.GameplayEffect GameplayEffect;
	public GameplayModifierEvaluatedData EvaluatedData;
	public object Target;
}

public struct GameplayModifierEvaluatedData
{
	public Attribute Attribute;
	public ModifierOperation ModifierOperation;
	public int Magnitude;
	public bool IsValid;
}

public sealed class Attribute
{
	public event Action<Attribute, int>? OnValueChanged;

	public event Func<Attribute, GameplayEffectModifier, bool> OnPreGameplayEffectExecute;

	public event Action<Attribute, GameplayEffectModifier>? OnPostGameplayEffectExecute;

	public int BaseValue { get; private set; }

	/// <summary>
	/// Gets the total modifier value kept so we can make Status Effect application consise, but this value could be
	/// clamped and thus having an invalid part.
	/// </summary>
	public int TotalModifierValue { get; private set; }

	public int ValidModifierValue { get; private set; }

	public int MaxValue { get; private set; }

	public int MinValue { get; private set; }

	public int TotalValue { get; private set; }

	internal Attribute()
	{
		MinValue = int.MinValue;
		MaxValue = int.MaxValue;
		BaseValue = 0;
		TotalModifierValue = 0;
		ValidModifierValue = 0;
		TotalValue = 0;
	}

	internal void Initialize(int defaultValue, int minValue = int.MinValue, int maxValue = int.MaxValue)
	{
		if (minValue > maxValue)
		{
			// Do I expect it to be handled somehow? No
			// Should it be an Assert then? (Yea, probably...)
			throw new ArgumentException("MinValue cannot be greater than MaxValue.");
		}

		if (defaultValue < minValue || defaultValue > maxValue)
		{
			throw new ArgumentException("DefaultValue should be withing MinValue and MaxValue.");
		}

		MinValue = minValue;
		MaxValue = maxValue;
		BaseValue = defaultValue;
		TotalModifierValue = 0;
		ValidModifierValue = 0;
		TotalValue = BaseValue;
	}

	internal void SetMaxValue(int newMaxValue)
	{
		// Do I expect it to be handled somehow? (This one though... it could happen in runtime)
		// Or should I fix it right now? (I don't think so...)
		// Should I warn if so? (Since I won't fix, I won't notify)
		// Should it be an Assert then? (Yea, probably... but maybe an exception)
		if (newMaxValue < MinValue)
		{
			throw new ArgumentException("MaxValue cannot be lower than MinValue.");
		}

		int oldValue = TotalValue;

		BaseValue = Math.Min(BaseValue, MaxValue);

		UpdateCachedValues();

		if (TotalValue != oldValue)
		{
			OnValueChanged?.Invoke(this, TotalValue - oldValue);
		}
	}

	internal void SetMinValue(int newMinValue)
	{
		if (newMinValue > MaxValue)
		{
			throw new ArgumentException("MinValue cannot be lower than MaxValue.");
		}

		int oldValue = TotalValue;

		BaseValue = Math.Max(BaseValue, MinValue);

		UpdateCachedValues();

		if (TotalValue != oldValue)
		{
			OnValueChanged?.Invoke(this, TotalValue - oldValue);
		}
	}

	internal void SetBaseValue(int newValue)
	{
		int oldValue = TotalValue;

		BaseValue = Math.Clamp(newValue, MinValue, MaxValue);

		UpdateCachedValues();

		if (TotalValue != oldValue)
		{
			OnValueChanged?.Invoke(this, TotalValue - oldValue);
		}
	}

	internal void AddToBaseValue(int value)
	{
		int oldValue = TotalValue;

		BaseValue = Math.Clamp(BaseValue + value, MinValue, MaxValue);

		UpdateCachedValues();

		if (TotalValue != oldValue)
		{
			OnValueChanged?.Invoke(this, TotalValue - oldValue);
		}
	}

	internal void ApplyModifier(int value)
	{
		int oldValue = TotalValue;

		TotalModifierValue += value;

		UpdateCachedValues();

		if (TotalValue != oldValue)
		{
			OnValueChanged?.Invoke(this, TotalValue - oldValue);
		}
	}

	internal bool PreGameplayEffectExecute(GameplayEffectModifier modifier)
	{
		if (OnPreGameplayEffectExecute is null)
		{
			return false;
		}

		return OnPreGameplayEffectExecute.Invoke(this, modifier);
	}

	internal void PostGameplayEffectExecute(GameplayEffectModifier modifier)
	{
		OnPostGameplayEffectExecute?.Invoke(this, modifier);
	}

	private void UpdateCachedValues()
	{
		TotalValue = Math.Clamp(BaseValue + TotalModifierValue, MinValue, MaxValue);
		ValidModifierValue = TotalValue - BaseValue;
	}
}
