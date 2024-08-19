#pragma warning disable SA1600
using GameplayTags.Runtime.GameplayEffect;
using System.Threading.Channels;

namespace GameplayTags.Runtime.Attribute;

public class ChannelData
{
	public int? Override { get; set; }

	public int FlatModifier { get; set; }

	public float PercentModifier { get; set; }
}

public sealed class Attribute
{
	public event Action<Attribute, int>? OnValueChanged;

	public event Func<Attribute, GameplayEffectEvaluatedData, bool> OnPreGameplayEffectExecute;

	public event Action<Attribute, GameplayEffectEvaluatedData>? OnPostGameplayEffectExecute;

	private readonly List<ChannelData> _channels = new();

	public int BaseValue { get; private set; }

	public int Max { get; private set; }

	public int Min { get; private set; }

	/// <summary>
	/// Gets the total modifier value kept so we can make Status Effect application consise, but this value could be
	/// clamped and thus having an invalid part.
	/// </summary>
	public int Modifier { get; private set; }

	public int Overflow { get; private set; }

	public int TotalValue { get; private set; }

	internal Attribute()
	{
		Min = int.MinValue;
		Max = int.MaxValue;
		BaseValue = 0;
		Modifier = 0;
		Overflow = 0;
		TotalValue = 0;

		_channels.Add(new ChannelData
		{
			Override = null,
			FlatModifier = 0,
			PercentModifier = 1,
		});
	}

	internal void Initialize(int defaultValue, int minValue = int.MinValue, int maxValue = int.MaxValue, int channels = 0)
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

	internal void ExecuteOverride(int newValue)
	{
		int oldValue = TotalValue;

		BaseValue = Math.Clamp(newValue, Min, Max);

		UpdateCachedValues();

		if (TotalValue != oldValue)
		{
			OnValueChanged?.Invoke(this, TotalValue - oldValue);
		}
	}

	internal void ExecuteFlatModifier(int value)
	{
		int oldValue = TotalValue;

		BaseValue = Math.Clamp(BaseValue + value, Min, Max);

		UpdateCachedValues();

		if (TotalValue != oldValue)
		{
			OnValueChanged?.Invoke(this, TotalValue - oldValue);
		}
	}

	internal void ExecutePercentModifier(float value)
	{
		int oldValue = TotalValue;

		BaseValue = Math.Clamp((int)(BaseValue * (1 + value)), Min, Max);

		UpdateCachedValues();

		if (TotalValue != oldValue)
		{
			OnValueChanged?.Invoke(this, TotalValue - oldValue);
		}
	}

	internal void AddOverride(int value, int channel)
	{
		int oldValue = TotalValue;

		_channels[channel].Override = value;

		UpdateCachedValues();

		if (TotalValue != oldValue)
		{
			OnValueChanged?.Invoke(this, TotalValue - oldValue);
		}
	}

	internal void ClearOverride(int channel)
	{
		int oldValue = TotalValue;

		_channels[channel].Override = null;

		UpdateCachedValues();

		if (TotalValue != oldValue)
		{
			OnValueChanged?.Invoke(this, TotalValue - oldValue);
		}
	}

	internal void AddFlatModifier(int value, int channel)
	{
		int oldValue = TotalValue;

		_channels[channel].FlatModifier += value;

		UpdateCachedValues();

		if (TotalValue != oldValue)
		{
			OnValueChanged?.Invoke(this, TotalValue - oldValue);
		}
	}

	internal void AddPercentModifier(float value, int channel)
	{
		int oldValue = TotalValue;

		_channels[channel].PercentModifier += value;

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
		var evaluatedValue = (float)BaseValue;

		foreach (var channel in _channels)
		{
			if (channel.Override.HasValue)
			{
				evaluatedValue = channel.Override.Value;
				continue;
			}

			evaluatedValue = (BaseValue + channel.FlatModifier) * channel.PercentModifier;
		}

		TotalValue = Math.Clamp((int)evaluatedValue, Min, Max);
		Modifier = (int)evaluatedValue - BaseValue;

		if (evaluatedValue > Max)
		{
			Overflow = (int)evaluatedValue - Max;
		}
		else
		{
			Overflow = 0;
		}
	}
}
