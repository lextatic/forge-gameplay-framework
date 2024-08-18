#pragma warning disable SA1600
using GameplayTags.Runtime.GameplayEffect;

namespace GameplayTags.Runtime.Attribute;

public sealed class Attribute
{
	public event Action<Attribute, int>? OnValueChanged;

	public event Func<Attribute, GameplayEffectEvaluatedData, bool> OnPreGameplayEffectExecute;

	public event Action<Attribute, GameplayEffectEvaluatedData>? OnPostGameplayEffectExecute;

	public int BaseValue { get; private set; }

	public int Max { get; private set; }

	public int Min { get; private set; }


	/// <summary>
	/// Gets the total modifier value kept so we can make Status Effect application consise, but this value could be
	/// clamped and thus having an invalid part.
	/// </summary>
	public int Modifier { get; private set; }

	public float PercentBonus { get; private set; }

	public float PercentPenalty { get; private set; }

	public int Overflow { get; private set; }

	public int TotalValue { get; private set; }

	internal Attribute()
	{
		Min = int.MinValue;
		Max = int.MaxValue;
		BaseValue = 0;
		Modifier = 0;
		Overflow = 0;
		PercentBonus = 0;
		PercentPenalty = 0;
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

		Min = minValue;
		Max = maxValue;
		BaseValue = defaultValue;
		Modifier = 0;
		Overflow = 0;
		PercentBonus = 0;
		PercentPenalty = 0;
		TotalValue = BaseValue;
	}

	internal void SetMaxValue(int newMaxValue)
	{
		// Do I expect it to be handled somehow? (This one though... it could happen in runtime)
		// Or should I fix it right now? (I don't think so...)
		// Should I warn if so? (Since I won't fix, I won't notify)
		// Should it be an Assert then? (Yea, probably... but maybe an exception)
		if (newMaxValue < Min)
		{
			throw new ArgumentException("MaxValue cannot be lower than MinValue.");
		}

		int oldValue = TotalValue;

		BaseValue = Math.Min(BaseValue, Max);

		UpdateCachedValues();

		if (TotalValue != oldValue)
		{
			OnValueChanged?.Invoke(this, TotalValue - oldValue);
		}
	}

	internal void SetMinValue(int newMinValue)
	{
		if (newMinValue > Max)
		{
			throw new ArgumentException("MinValue cannot be lower than MaxValue.");
		}

		int oldValue = TotalValue;

		BaseValue = Math.Max(BaseValue, Min);

		UpdateCachedValues();

		if (TotalValue != oldValue)
		{
			OnValueChanged?.Invoke(this, TotalValue - oldValue);
		}
	}

	internal void OverrideBaseValue(int newValue)
	{
		int oldValue = TotalValue;

		BaseValue = Math.Clamp(newValue, Min, Max);

		UpdateCachedValues();

		if (TotalValue != oldValue)
		{
			OnValueChanged?.Invoke(this, TotalValue - oldValue);
		}
	}

	internal void ExecuteModifier(int value)
	{
		int oldValue = TotalValue;

		BaseValue = Math.Clamp(BaseValue + value, Min, Max);

		UpdateCachedValues();

		if (TotalValue != oldValue)
		{
			OnValueChanged?.Invoke(this, TotalValue - oldValue);
		}
	}

	internal void ExecutePercentBonus(float percentBonus)
	{
		System.Diagnostics.Debug.Assert(percentBonus > 0, $"percentBonus:({percentBonus}) must be higher than 0.");

		int oldValue = TotalValue;

		BaseValue = Math.Clamp((int)(BaseValue * (1 + percentBonus)), Min, Max);

		UpdateCachedValues();

		if (TotalValue != oldValue)
		{
			OnValueChanged?.Invoke(this, TotalValue - oldValue);
		}
	}

	internal void ExecutePercentPenalty(float percentPenalty)
	{
		System.Diagnostics.Debug.Assert(percentPenalty > 0, $"percentBonus:({percentPenalty}) must be higher than 0.");

		int oldValue = TotalValue;

		BaseValue = Math.Clamp((int)(BaseValue * (1 - percentPenalty)), Min, Max);

		UpdateCachedValues();

		if (TotalValue != oldValue)
		{
			OnValueChanged?.Invoke(this, TotalValue - oldValue);
		}
	}

	internal void AddModifier(int value)
	{
		int oldValue = TotalValue;

		Modifier += value;

		UpdateCachedValues();

		if (TotalValue != oldValue)
		{
			OnValueChanged?.Invoke(this, TotalValue - oldValue);
		}
	}

	internal void AddPercentBonus(float percentBonus)
	{
		System.Diagnostics.Debug.Assert(percentBonus > 0, $"percentBonus:({percentBonus}) must be higher than 0.");

		int oldValue = TotalValue;

		PercentBonus += percentBonus;

		UpdateCachedValues();

		if (TotalValue != oldValue)
		{
			OnValueChanged?.Invoke(this, TotalValue - oldValue);
		}
	}

	internal void AddPercentPenalty(float percentPenalty)
	{
		System.Diagnostics.Debug.Assert(percentPenalty > 0, $"percentBonus:({percentPenalty}) must be higher than 0.");

		int oldValue = TotalValue;

		PercentPenalty += percentPenalty;

		UpdateCachedValues();

		if (TotalValue != oldValue)
		{
			OnValueChanged?.Invoke(this, TotalValue - oldValue);
		}
	}

	internal bool PreGameplayEffectExecute(GameplayEffectEvaluatedData modifier)
	{
		if (OnPreGameplayEffectExecute is null)
		{
			return false;
		}

		return OnPreGameplayEffectExecute.Invoke(this, modifier);
	}

	internal void PostGameplayEffectExecute(GameplayEffectEvaluatedData modifier)
	{
		OnPostGameplayEffectExecute?.Invoke(this, modifier);
	}

	private void UpdateCachedValues()
	{
		var evaluatedValue = (int)((BaseValue + Modifier) * (1f + PercentBonus) * (1f - PercentPenalty));
		TotalValue = Math.Clamp(evaluatedValue, Min, Max);

		if (evaluatedValue > Max)
		{
			Overflow = evaluatedValue - Max;
		}
		else
		{
			Overflow = 0;
		}
	}
}
