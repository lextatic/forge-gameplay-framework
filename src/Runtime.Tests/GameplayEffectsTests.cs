#pragma warning disable SA1600 // Elements should be documented
using GameplayTags.Runtime.Attribute;
using GameplayTags.Runtime.GameplayEffect;

namespace GameplayTags.Runtime.Tests;
using Attribute = Attribute.Attribute;

[TestClass]
public class GameplayEffectsTests
{
	[TestMethod]
	public void Instant_effect_should_modify_attribute_base_value()
	{
		var owner = new object();

		var effectData = new GameplayEffectData(
			"Level Up",
			new DurationData
			{
				Type = DurationType.Instant,
				Duration = new ScalableFloat(0),
			},
			null,
			null);

		effectData.Modifiers.Add(new Modifier
		{
			Attribute = TagName.FromString("PlayerAttributeSet.Strength"),
			Operation = ModifierOperation.Flat,
			Value = new ScalableFloat(10),
		});

		var effect = new GameplayEffect.GameplayEffect(effectData, 1, new GameplayEffectContext());

		var playerAttributes = new PlayerAttributeSet();

		var manager = new GameplayEffectsManager(playerAttributes, owner);

		manager.ApplyEffect(effect);

		Assert.AreEqual(11, playerAttributes.Strength.TotalValue);
		Assert.AreEqual(11, playerAttributes.Strength.BaseValue);
		Assert.AreEqual(0, playerAttributes.Strength.Modifier);
		Assert.AreEqual(0, playerAttributes.Strength.Overflow);
	}

	[TestMethod]
	public void Instant_effect_of_different_operations_should_modify_base_value_accordingly()
	{
		var owner = new object();

		var effectData = new GameplayEffectData(
			"Level Up",
			new DurationData
			{
				Type = DurationType.Instant,
				Duration = new ScalableFloat(0),
			},
			null,
			null);

		effectData.Modifiers.Add(new Modifier
		{
			Attribute = TagName.FromString("PlayerAttributeSet.Strength"),
			Operation = ModifierOperation.Flat,
			Value = new ScalableFloat(4),
		});

		var effect = new GameplayEffect.GameplayEffect(effectData, 1, new GameplayEffectContext());

		var playerAttributes = new PlayerAttributeSet();

		var manager = new GameplayEffectsManager(playerAttributes, owner);

		manager.ApplyEffect(effect);

		Assert.AreEqual(5, playerAttributes.Strength.TotalValue);
		Assert.AreEqual(5, playerAttributes.Strength.BaseValue);
		Assert.AreEqual(0, playerAttributes.Strength.Modifier);
		Assert.AreEqual(0, playerAttributes.Strength.Overflow);

		var effectData2 = new GameplayEffectData(
			"Rank Up",
			new DurationData
			{
				Type = DurationType.Instant,
				Duration = new ScalableFloat(0),
			},
			null,
			null);

		effectData2.Modifiers.Add(new Modifier
		{
			Attribute = TagName.FromString("PlayerAttributeSet.Strength"),
			Operation = ModifierOperation.Percent,
			Value = new ScalableFloat(4), // 400% bonus
		});

		var effect2 = new GameplayEffect.GameplayEffect(effectData2, 1, new GameplayEffectContext());

		manager.ApplyEffect(effect2);

		Assert.AreEqual(25, playerAttributes.Strength.TotalValue);
		Assert.AreEqual(25, playerAttributes.Strength.BaseValue);
		Assert.AreEqual(0, playerAttributes.Strength.Modifier);
		Assert.AreEqual(0, playerAttributes.Strength.Overflow);

		var effectData3 = new GameplayEffectData(
			"Rank Down",
			new DurationData
			{
				Type = DurationType.Instant,
				Duration = new ScalableFloat(0),
			},
			null,
			null);

		effectData3.Modifiers.Add(new Modifier
		{
			Attribute = TagName.FromString("PlayerAttributeSet.Strength"),
			Operation = ModifierOperation.Percent,
			Value = new ScalableFloat(-0.66f), // Divides by 3 (66% reduction)
		});

		var effect3 = new GameplayEffect.GameplayEffect(effectData3, 1, new GameplayEffectContext());

		manager.ApplyEffect(effect3);

		Assert.AreEqual(8, playerAttributes.Strength.TotalValue);
		Assert.AreEqual(8, playerAttributes.Strength.BaseValue);
		Assert.AreEqual(0, playerAttributes.Strength.Modifier);
		Assert.AreEqual(0, playerAttributes.Strength.Overflow);

		var effectData4 = new GameplayEffectData(
			"Rank Fix",
			new DurationData
			{
				Type = DurationType.Instant,
				Duration = new ScalableFloat(0),
			},
			null,
			null);

		effectData4.Modifiers.Add(new Modifier
		{
			Attribute = TagName.FromString("PlayerAttributeSet.Strength"),
			Operation = ModifierOperation.Override,
			Value = new ScalableFloat(42),
		});

		var effect4 = new GameplayEffect.GameplayEffect(effectData4, 1, new GameplayEffectContext());

		manager.ApplyEffect(effect4);

		Assert.AreEqual(42, playerAttributes.Strength.TotalValue);
		Assert.AreEqual(42, playerAttributes.Strength.BaseValue);
		Assert.AreEqual(0, playerAttributes.Strength.Modifier);
		Assert.AreEqual(0, playerAttributes.Strength.Overflow);
	}

	[TestMethod]
	public void Higher_level_effect_should_apply_higher_level_modifier()
	{
		var owner = new object();

		var effectData = new GameplayEffectData(
			"Level Up",
			new DurationData
			{
				Type = DurationType.Instant,
				Duration = new ScalableFloat(0),
			},
			null,
			null);

		effectData.Modifiers.Add(new Modifier
		{
			Attribute = TagName.FromString("PlayerAttributeSet.Strength"),
			Operation = ModifierOperation.Flat,
			Value = new ScalableFloat(10)
				.AddKey(1, 1)
				.AddKey(2, 2),
		});

		var effect = new GameplayEffect.GameplayEffect(effectData, 1, new GameplayEffectContext());

		var playerAttributes = new PlayerAttributeSet();

		var manager = new GameplayEffectsManager(playerAttributes, owner);

		manager.ApplyEffect(effect);

		Assert.AreEqual(11, playerAttributes.Strength.TotalValue);
		Assert.AreEqual(11, playerAttributes.Strength.BaseValue);
		Assert.AreEqual(0, playerAttributes.Strength.Modifier);
		Assert.AreEqual(0, playerAttributes.Strength.Overflow);

		effect.LevelUp();

		manager.ApplyEffect(effect);

		Assert.AreEqual(31, playerAttributes.Strength.TotalValue);
		Assert.AreEqual(31, playerAttributes.Strength.BaseValue);
		Assert.AreEqual(0, playerAttributes.Strength.Modifier);
		Assert.AreEqual(0, playerAttributes.Strength.Overflow);
	}

	[TestMethod]
	public void Inifinite_effect_should_modify_attribute_modifier_value()
	{
		var owner = new object();

		var effectData = new GameplayEffectData(
			"Buff",
			new DurationData
			{
				Type = DurationType.Infinite,
				Duration = new ScalableFloat(0),
			},
			null,
			null);

		effectData.Modifiers.Add(new Modifier
		{
			Attribute = TagName.FromString("PlayerAttributeSet.Strength"),
			Operation = ModifierOperation.Flat,
			Value = new ScalableFloat(10),
		});

		var effect = new GameplayEffect.GameplayEffect(effectData, 1, new GameplayEffectContext());

		var playerAttributes = new PlayerAttributeSet();

		var manager = new GameplayEffectsManager(playerAttributes, owner);

		manager.ApplyEffect(effect);

		Assert.AreEqual(11, playerAttributes.Strength.TotalValue);
		Assert.AreEqual(1, playerAttributes.Strength.BaseValue);
		Assert.AreEqual(10, playerAttributes.Strength.Modifier);
		Assert.AreEqual(0, playerAttributes.Strength.Overflow);

		// Simulate 5 seconds
		for (int i = 0; i < 32 * 5; i++)
		{
			manager.UpdateEffects(0.03125f);
		}

		Assert.AreEqual(11, playerAttributes.Strength.TotalValue);
		Assert.AreEqual(1, playerAttributes.Strength.BaseValue);
		Assert.AreEqual(10, playerAttributes.Strength.Modifier);
		Assert.AreEqual(0, playerAttributes.Strength.Overflow);
	}

	[TestMethod]
	public void Infinite_effect_of_different_operations_should_modify_modifier_and_multiplier_value_accordingly()
	{
		var owner = new object();

		var effectData = new GameplayEffectData(
			"Buff",
			new DurationData
			{
				Type = DurationType.Infinite,
				Duration = new ScalableFloat(0),
			},
			null,
			null);

		effectData.Modifiers.Add(new Modifier
		{
			Attribute = TagName.FromString("PlayerAttributeSet.Strength"),
			Operation = ModifierOperation.Flat,
			Value = new ScalableFloat(4),
		});

		var effect = new GameplayEffect.GameplayEffect(effectData, 1, new GameplayEffectContext());

		var playerAttributes = new PlayerAttributeSet();

		var manager = new GameplayEffectsManager(playerAttributes, owner);

		manager.ApplyEffect(effect);

		Assert.AreEqual(5, playerAttributes.Strength.TotalValue);
		Assert.AreEqual(1, playerAttributes.Strength.BaseValue);
		Assert.AreEqual(4, playerAttributes.Strength.Modifier);
		Assert.AreEqual(0, playerAttributes.Strength.Overflow);

		var effectData2 = new GameplayEffectData(
			"Buff",
			new DurationData
			{
				Type = DurationType.Infinite,
				Duration = new ScalableFloat(0),
			},
			null,
			null);

		effectData2.Modifiers.Add(new Modifier
		{
			Attribute = TagName.FromString("PlayerAttributeSet.Strength"),
			Operation = ModifierOperation.Percent,
			Value = new ScalableFloat(4), // 400% bonus
		});

		var effect2 = new GameplayEffect.GameplayEffect(effectData2, 1, new GameplayEffectContext());

		manager.ApplyEffect(effect2);

		Assert.AreEqual(25, playerAttributes.Strength.TotalValue);
		Assert.AreEqual(1, playerAttributes.Strength.BaseValue);
		Assert.AreEqual(24, playerAttributes.Strength.Modifier);
		Assert.AreEqual(0, playerAttributes.Strength.Overflow);

		var effectData3 = new GameplayEffectData(
			"Buff",
			new DurationData
			{
				Type = DurationType.Infinite,
				Duration = new ScalableFloat(0),
			},
			null,
			null);

		effectData3.Modifiers.Add(new Modifier
		{
			Attribute = TagName.FromString("PlayerAttributeSet.Strength"),
			Operation = ModifierOperation.Percent,
			Value = new ScalableFloat(-0.66f), // Divide by 3 (-66%)
		});

		var effect3 = new GameplayEffect.GameplayEffect(effectData3, 1, new GameplayEffectContext());

		manager.ApplyEffect(effect3);

		Assert.AreEqual(21, playerAttributes.Strength.TotalValue);
		Assert.AreEqual(1, playerAttributes.Strength.BaseValue);
		Assert.AreEqual(20, playerAttributes.Strength.Modifier);
		Assert.AreEqual(0, playerAttributes.Strength.Overflow);
	}

	[TestMethod]
	public void Duration_effect_should_modify_attribute_modifier_value_and_expire_after_duration_time()
	{
		var owner = new object();

		var effectData = new GameplayEffectData(
			"Buff",
			new DurationData
			{
				Type = DurationType.HasDuration,
				Duration = new ScalableFloat(10),
			},
			null,
			null);

		effectData.Modifiers.Add(new Modifier
		{
			Attribute = TagName.FromString("PlayerAttributeSet.Strength"),
			Operation = ModifierOperation.Flat,
			Value = new ScalableFloat(10),
		});

		var effect = new GameplayEffect.GameplayEffect(effectData, 1, new GameplayEffectContext());

		var playerAttributes = new PlayerAttributeSet();

		var manager = new GameplayEffectsManager(playerAttributes, owner);

		manager.ApplyEffect(effect);

		Assert.AreEqual(11, playerAttributes.Strength.TotalValue);
		Assert.AreEqual(1, playerAttributes.Strength.BaseValue);
		Assert.AreEqual(10, playerAttributes.Strength.Modifier);
		Assert.AreEqual(0, playerAttributes.Strength.Overflow);

		// Simulate 5 seconds
		for (int i = 0; i < 32 * 5; i++)
		{
			manager.UpdateEffects(0.03125f);
		}

		Assert.AreEqual(11, playerAttributes.Strength.TotalValue);
		Assert.AreEqual(1, playerAttributes.Strength.BaseValue);
		Assert.AreEqual(10, playerAttributes.Strength.Modifier);
		Assert.AreEqual(0, playerAttributes.Strength.Overflow);

		// Simulate 5 more seconds
		for (int i = 0; i < 32 * 5; i++)
		{
			manager.UpdateEffects(0.03125f);
		}

		Assert.AreEqual(1, playerAttributes.Strength.TotalValue);
		Assert.AreEqual(1, playerAttributes.Strength.BaseValue);
		Assert.AreEqual(0, playerAttributes.Strength.Modifier);
		Assert.AreEqual(0, playerAttributes.Strength.Overflow);
	}

	[TestMethod]
	public void Periodic_effect_should_modify_base_attribute_value()
	{
		var owner = new object();

		var effectData = new GameplayEffectData(
			"Buff",
			new DurationData
			{
				Type = DurationType.Infinite,
				Duration = new ScalableFloat(0),
			},
			null,
			new PeriodicData
			{
				ExecuteOnApplication = true,
				Period = new ScalableFloat(1),
			});

		effectData.Modifiers.Add(new Modifier
		{
			Attribute = TagName.FromString("PlayerAttributeSet.Strength"),
			Operation = ModifierOperation.Flat,
			Value = new ScalableFloat(10),
		});

		var effect = new GameplayEffect.GameplayEffect(effectData, 1, new GameplayEffectContext());

		var playerAttributes = new PlayerAttributeSet();

		var manager = new GameplayEffectsManager(playerAttributes, owner);

		manager.ApplyEffect(effect);

		Assert.AreEqual(11, playerAttributes.Strength.TotalValue);
		Assert.AreEqual(11, playerAttributes.Strength.BaseValue);
		Assert.AreEqual(0, playerAttributes.Strength.Modifier);
		Assert.AreEqual(0, playerAttributes.Strength.Overflow);

		// Simulate for 1 turn or seconds
		manager.UpdateEffects(1);

		Assert.AreEqual(21, playerAttributes.Strength.TotalValue);
		Assert.AreEqual(21, playerAttributes.Strength.BaseValue);
		Assert.AreEqual(0, playerAttributes.Strength.Modifier);
		Assert.AreEqual(0, playerAttributes.Strength.Overflow);

		// Simulate 5 more seconds
		for (int i = 0; i < 32 * 5; i++)
		{
			manager.UpdateEffects(0.03125f);
		}

		Assert.AreEqual(71, playerAttributes.Strength.TotalValue);
		Assert.AreEqual(71, playerAttributes.Strength.BaseValue);
		Assert.AreEqual(0, playerAttributes.Strength.Modifier);
		Assert.AreEqual(0, playerAttributes.Strength.Overflow);
	}

	[TestMethod]
	public void Periodic_effect_should_modify_base_attribute_with_same_value_even_after_level_up()
	{
		var owner = new object();

		var effectData = new GameplayEffectData(
			"Buff",
			new DurationData
			{
				Type = DurationType.Infinite,
				Duration = new ScalableFloat(0),
			},
			null,
			new PeriodicData
			{
				ExecuteOnApplication = true,
				Period = new ScalableFloat(1),
			});

		effectData.Modifiers.Add(new Modifier
		{
			Attribute = TagName.FromString("PlayerAttributeSet.Strength"),
			Operation = ModifierOperation.Flat,
			Value = new ScalableFloat(10),
		});

		var effect = new GameplayEffect.GameplayEffect(effectData, 1, new GameplayEffectContext());

		var playerAttributes = new PlayerAttributeSet();

		var manager = new GameplayEffectsManager(playerAttributes, owner);

		manager.ApplyEffect(effect);

		Assert.AreEqual(11, playerAttributes.Strength.TotalValue);
		Assert.AreEqual(11, playerAttributes.Strength.BaseValue);
		Assert.AreEqual(0, playerAttributes.Strength.Modifier);
		Assert.AreEqual(0, playerAttributes.Strength.Overflow);

		effect.LevelUp();
		// Simulate for 1 turn or seconds
		manager.UpdateEffects(1);

		Assert.AreEqual(21, playerAttributes.Strength.TotalValue);
		Assert.AreEqual(21, playerAttributes.Strength.BaseValue);
		Assert.AreEqual(0, playerAttributes.Strength.Modifier);
		Assert.AreEqual(0, playerAttributes.Strength.Overflow);
	}

	[TestMethod]
	public void Periodic_effect_should_modify_base_attribute_value_and_expire_after_duration_time()
	{
		var owner = new object();

		var effectData = new GameplayEffectData(
			"Buff",
			new DurationData
			{
				Type = DurationType.HasDuration,
				Duration = new ScalableFloat(3),
			},
			null,
			new PeriodicData
			{
				ExecuteOnApplication = true,
				Period = new ScalableFloat(1),
			});

		effectData.Modifiers.Add(new Modifier
		{
			Attribute = TagName.FromString("PlayerAttributeSet.Strength"),
			Operation = ModifierOperation.Flat,
			Value = new ScalableFloat(10),
		});

		var effect = new GameplayEffect.GameplayEffect(effectData, 1, new GameplayEffectContext());

		var playerAttributes = new PlayerAttributeSet();

		var manager = new GameplayEffectsManager(playerAttributes, owner);

		manager.ApplyEffect(effect);

		Assert.AreEqual(11, playerAttributes.Strength.TotalValue);
		Assert.AreEqual(11, playerAttributes.Strength.BaseValue);
		Assert.AreEqual(0, playerAttributes.Strength.Modifier);
		Assert.AreEqual(0, playerAttributes.Strength.Overflow);

		// Simulate for 1 turn or seconds
		manager.UpdateEffects(1);

		Assert.AreEqual(21, playerAttributes.Strength.TotalValue);
		Assert.AreEqual(21, playerAttributes.Strength.BaseValue);
		Assert.AreEqual(0, playerAttributes.Strength.Modifier);
		Assert.AreEqual(0, playerAttributes.Strength.Overflow);

		// Simulate 5 more seconds
		for (int i = 0; i < 32 * 5; i++)
		{
			manager.UpdateEffects(0.03125f);
		}

		Assert.AreEqual(41, playerAttributes.Strength.TotalValue);
		Assert.AreEqual(41, playerAttributes.Strength.BaseValue);
		Assert.AreEqual(0, playerAttributes.Strength.Modifier);
		Assert.AreEqual(0, playerAttributes.Strength.Overflow);
	}

	[TestMethod]
	public void Effect_should_stack_values()
	{
		var owner = new object();

		var effectData = new GameplayEffectData(
			"Buff",
			new DurationData
			{
				Type = DurationType.Infinite,
				Duration = new ScalableFloat(0),
			},
			new StackingData
			{
				StackLimit = new ScalableInt(5),
				StackPolicy = StackPolicy.AggregateByTarget,
				StackLevelPolicy = StackLevelPolicy.SegregateLevels,
				StackExpirationPolicy = StackExpirationPolicy.ClearEntireStack,
				StackApplicationRefreshPolicy = null,
				StackLevelOverridePolicy = null,
				StackApplicationResetPeriodPolicy = null,
			},
			null);

		effectData.Modifiers.Add(new Modifier
		{
			Attribute = TagName.FromString("PlayerAttributeSet.Strength"),
			Operation = ModifierOperation.Flat,
			Value = new ScalableFloat(10),
		});

		var effect = new GameplayEffect.GameplayEffect(effectData, 1, new GameplayEffectContext());

		var playerAttributes = new PlayerAttributeSet();

		var manager = new GameplayEffectsManager(playerAttributes, owner);

		manager.ApplyEffect(effect);

		Assert.AreEqual(11, playerAttributes.Strength.TotalValue);
		Assert.AreEqual(1, playerAttributes.Strength.BaseValue);
		Assert.AreEqual(10, playerAttributes.Strength.Modifier);
		Assert.AreEqual(0, playerAttributes.Strength.Overflow);
	}

	private class PlayerAttributeSet : AttributeSet
	{
		public readonly Attribute Strength;

		public readonly Attribute Intelligence;

		public readonly Attribute Dexterity;

		public readonly Attribute Vitality;

		public readonly Attribute Agility;

		public readonly Attribute Luck;

		protected override void InitializeAttributes()
		{
			InitializeAttribute(Strength, 1, 0, 99);
			InitializeAttribute(Intelligence, 1, 0, 99);
			InitializeAttribute(Dexterity, 1, 0, 99);
			InitializeAttribute(Vitality, 1, 0, 99);
			InitializeAttribute(Agility, 1, 0, 99);
			InitializeAttribute(Luck, 1, 0, 99);
		}
	}
}
