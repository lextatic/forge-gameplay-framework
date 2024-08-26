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
		var instigator = new Entity();
		var target = new Entity();

		var effectData = new GameplayEffectData(
			"Level Up",
			new DurationData
			{
				Type = DurationType.Instant,
			},
			null,
			null);

		effectData.Modifiers.Add(new Modifier
		{
			Attribute = TagName.FromString("TestAttributeSet.Attribute1"),
			Operation = ModifierOperation.Flat,
			Magnitude = new ModifierMagnitude
			{
				MagnitudeCalculationType = MagnitudeCalculationType.ScalableFloat,
				ScalableFloatMagnitude = new ScalableFloat(10),
			},
		});

		var effect = new GameplayEffect.GameplayEffect(effectData, 1, new GameplayEffectContext()
		{
			EffectCauser = new Entity(),
			Instigator = instigator,
		});

		target.GameplaySystem.GameplayEffectsManager.ApplyEffect(effect);

		Assert.AreEqual(11, target.PlayerAttributeSet.Attribute1.TotalValue);
		Assert.AreEqual(11, target.PlayerAttributeSet.Attribute1.BaseValue);
		Assert.AreEqual(0, target.PlayerAttributeSet.Attribute1.Modifier);
		Assert.AreEqual(0, target.PlayerAttributeSet.Attribute1.Overflow);
	}

	[TestMethod]
	public void Attribute_based_effect_should_modify_values_accordinly()
	{
		var instigator = new Entity();
		var target = new Entity();

		var effectData = new GameplayEffectData(
			"Level Up",
			new DurationData
			{
				Type = DurationType.Instant,
			},
			null,
			null);

		effectData.Modifiers.Add(new Modifier
		{
			Attribute = TagName.FromString("TestAttributeSet.Attribute1"),
			Operation = ModifierOperation.Flat,
			Magnitude = new ModifierMagnitude
			{
				MagnitudeCalculationType = MagnitudeCalculationType.AttributeBased,
				AttributeBasedFloat = new AttributeBasedFloat
				{
					BackingAttribute = new AttributeCaptureDefinition
					{
						Attribute = TagName.FromString("TestAttributeSet.Attribute1"),
						Source = AttributeCaptureSource.Source,
						Snapshot = true,
					},
					AttributeCalculationType = AttributeBasedFloatCalculationType.AttributeBaseValue,
					Coeficient = new ScalableFloat(2),
					PreMultiplyAdditiveValue = new ScalableFloat(1),
					PostMultiplyAdditiveValue = new ScalableFloat(2),
					// ((1 + 1) * 2) + 2 = 6
				},
			},
		});

		var effect = new GameplayEffect.GameplayEffect(effectData, 1, new GameplayEffectContext()
		{
			EffectCauser = new Entity(),
			Instigator = instigator,
		});

		target.GameplaySystem.GameplayEffectsManager.ApplyEffect(effect);

		Assert.AreEqual(7, target.PlayerAttributeSet.Attribute1.TotalValue);
		Assert.AreEqual(7, target.PlayerAttributeSet.Attribute1.BaseValue);
		Assert.AreEqual(0, target.PlayerAttributeSet.Attribute1.Modifier);
		Assert.AreEqual(0, target.PlayerAttributeSet.Attribute1.Overflow);
	}

	[TestMethod]
	public void Instant_effect_of_different_operations_should_modify_base_value_accordingly()
	{
		var instigator = new Entity();
		var target = new Entity();

		var effectData = new GameplayEffectData(
			"Level Up",
			new DurationData
			{
				Type = DurationType.Instant,
			},
			null,
			null);

		effectData.Modifiers.Add(new Modifier
		{
			Attribute = TagName.FromString("TestAttributeSet.Attribute1"),
			Operation = ModifierOperation.Flat,
			Magnitude = new ModifierMagnitude
			{
				MagnitudeCalculationType = MagnitudeCalculationType.ScalableFloat,
				ScalableFloatMagnitude = new ScalableFloat(4),
			},
		});

		var effect = new GameplayEffect.GameplayEffect(effectData, 1, new GameplayEffectContext()
		{
			EffectCauser = new Entity(),
			Instigator = instigator,
		});

		target.GameplaySystem.GameplayEffectsManager.ApplyEffect(effect);

		Assert.AreEqual(5, target.PlayerAttributeSet.Attribute1.TotalValue);
		Assert.AreEqual(5, target.PlayerAttributeSet.Attribute1.BaseValue);
		Assert.AreEqual(0, target.PlayerAttributeSet.Attribute1.Modifier);
		Assert.AreEqual(0, target.PlayerAttributeSet.Attribute1.Overflow);

		var effectData2 = new GameplayEffectData(
			"Rank Up",
			new DurationData
			{
				Type = DurationType.Instant,
			},
			null,
			null);

		effectData2.Modifiers.Add(new Modifier
		{
			Attribute = TagName.FromString("TestAttributeSet.Attribute1"),
			Operation = ModifierOperation.Percent,
			Magnitude = new ModifierMagnitude // 400% bonus
			{
				MagnitudeCalculationType = MagnitudeCalculationType.ScalableFloat,
				ScalableFloatMagnitude = new ScalableFloat(4),
			},
		});

		var effect2 = new GameplayEffect.GameplayEffect(effectData2, 1, new GameplayEffectContext()
		{
			EffectCauser = new Entity(),
			Instigator = instigator,
		});

		target.GameplaySystem.GameplayEffectsManager.ApplyEffect(effect2);

		Assert.AreEqual(25, target.PlayerAttributeSet.Attribute1.TotalValue);
		Assert.AreEqual(25, target.PlayerAttributeSet.Attribute1.BaseValue);
		Assert.AreEqual(0, target.PlayerAttributeSet.Attribute1.Modifier);
		Assert.AreEqual(0, target.PlayerAttributeSet.Attribute1.Overflow);

		var effectData3 = new GameplayEffectData(
			"Rank Down",
			new DurationData
			{
				Type = DurationType.Instant,
			},
			null,
			null);

		effectData3.Modifiers.Add(new Modifier
		{
			Attribute = TagName.FromString("TestAttributeSet.Attribute1"),
			Operation = ModifierOperation.Percent,
			Magnitude = new ModifierMagnitude // Divides by 3 (66% reduction)
			{
				MagnitudeCalculationType = MagnitudeCalculationType.ScalableFloat,
				ScalableFloatMagnitude = new ScalableFloat(-0.66f),
			},
		});

		var effect3 = new GameplayEffect.GameplayEffect(effectData3, 1, new GameplayEffectContext()
		{
			EffectCauser = new Entity(),
			Instigator = instigator,
		});

		target.GameplaySystem.GameplayEffectsManager.ApplyEffect(effect3);

		Assert.AreEqual(8, target.PlayerAttributeSet.Attribute1.TotalValue);
		Assert.AreEqual(8, target.PlayerAttributeSet.Attribute1.BaseValue);
		Assert.AreEqual(0, target.PlayerAttributeSet.Attribute1.Modifier);
		Assert.AreEqual(0, target.PlayerAttributeSet.Attribute1.Overflow);

		var effectData4 = new GameplayEffectData(
			"Rank Fix",
			new DurationData
			{
				Type = DurationType.Instant,
			},
			null,
			null);

		effectData4.Modifiers.Add(new Modifier
		{
			Attribute = TagName.FromString("TestAttributeSet.Attribute1"),
			Operation = ModifierOperation.Override,
			Magnitude = new ModifierMagnitude
			{
				MagnitudeCalculationType = MagnitudeCalculationType.ScalableFloat,
				ScalableFloatMagnitude = new ScalableFloat(42),
			},
		});

		var effect4 = new GameplayEffect.GameplayEffect(effectData4, 1, new GameplayEffectContext()
		{
			EffectCauser = new Entity(),
			Instigator = instigator,
		});

		target.GameplaySystem.GameplayEffectsManager.ApplyEffect(effect4);

		Assert.AreEqual(42, target.PlayerAttributeSet.Attribute1.TotalValue);
		Assert.AreEqual(42, target.PlayerAttributeSet.Attribute1.BaseValue);
		Assert.AreEqual(0, target.PlayerAttributeSet.Attribute1.Modifier);
		Assert.AreEqual(0, target.PlayerAttributeSet.Attribute1.Overflow);
	}

	[TestMethod]
	public void Higher_level_effect_should_apply_higher_level_modifier()
	{
		var instigator = new Entity();
		var target = new Entity();

		var effectData = new GameplayEffectData(
			"Level Up",
			new DurationData
			{
				Type = DurationType.Instant,
			},
			null,
			null);

		effectData.Modifiers.Add(new Modifier
		{
			Attribute = TagName.FromString("TestAttributeSet.Attribute1"),
			Operation = ModifierOperation.Flat,
			Magnitude = new ModifierMagnitude
			{
				MagnitudeCalculationType = MagnitudeCalculationType.ScalableFloat,
				ScalableFloatMagnitude = new ScalableFloat(10)
				.AddKey(1, 1)
				.AddKey(2, 2),
			},
		});

		var effect = new GameplayEffect.GameplayEffect(effectData, 1, new GameplayEffectContext()
		{
			EffectCauser = new Entity(),
			Instigator = instigator,
		});

		target.GameplaySystem.GameplayEffectsManager.ApplyEffect(effect);

		Assert.AreEqual(11, target.PlayerAttributeSet.Attribute1.TotalValue);
		Assert.AreEqual(11, target.PlayerAttributeSet.Attribute1.BaseValue);
		Assert.AreEqual(0, target.PlayerAttributeSet.Attribute1.Modifier);
		Assert.AreEqual(0, target.PlayerAttributeSet.Attribute1.Overflow);

		effect.LevelUp();

		target.GameplaySystem.GameplayEffectsManager.ApplyEffect(effect);

		Assert.AreEqual(31, target.PlayerAttributeSet.Attribute1.TotalValue);
		Assert.AreEqual(31, target.PlayerAttributeSet.Attribute1.BaseValue);
		Assert.AreEqual(0, target.PlayerAttributeSet.Attribute1.Modifier);
		Assert.AreEqual(0, target.PlayerAttributeSet.Attribute1.Overflow);
	}

	[TestMethod]
	public void Non_snapshot_level_effect_should_update_value_on_level_up()
	{
		var instigator = new Entity();
		var target = new Entity();

		var effectData = new GameplayEffectData(
			"Level Up",
			new DurationData
			{
				Type = DurationType.Infinite,
			},
			null,
			null,
			false);

		effectData.Modifiers.Add(new Modifier
		{
			Attribute = TagName.FromString("TestAttributeSet.Attribute1"),
			Operation = ModifierOperation.Flat,
			Magnitude = new ModifierMagnitude
			{
				MagnitudeCalculationType = MagnitudeCalculationType.ScalableFloat,
				ScalableFloatMagnitude = new ScalableFloat(10)
					.AddKey(1, 1)
					.AddKey(2, 2),
			},
		});

		var effect = new GameplayEffect.GameplayEffect(effectData, 1, new GameplayEffectContext()
		{
			EffectCauser = new Entity(),
			Instigator = instigator,
		});

		target.GameplaySystem.GameplayEffectsManager.ApplyEffect(effect);

		Assert.AreEqual(11, target.PlayerAttributeSet.Attribute1.TotalValue);
		Assert.AreEqual(1, target.PlayerAttributeSet.Attribute1.BaseValue);
		Assert.AreEqual(10, target.PlayerAttributeSet.Attribute1.Modifier);
		Assert.AreEqual(0, target.PlayerAttributeSet.Attribute1.Overflow);

		effect.LevelUp();

		Assert.AreEqual(21, target.PlayerAttributeSet.Attribute1.TotalValue);
		Assert.AreEqual(1, target.PlayerAttributeSet.Attribute1.BaseValue);
		Assert.AreEqual(20, target.PlayerAttributeSet.Attribute1.Modifier);
		Assert.AreEqual(0, target.PlayerAttributeSet.Attribute1.Overflow);
	}

	[TestMethod]
	public void Non_snapshot_level_periodic_effect_should_update_scalable_float_values_on_level_up()
	{
		var instigator = new Entity();
		var target = new Entity();

		var effectData = new GameplayEffectData(
			"Buff",
			new DurationData
			{
				Type = DurationType.Infinite,
			},
			null,
			new PeriodicData
			{
				ExecuteOnApplication = true,
				Period = new ScalableFloat(1)
					.AddKey(1, 1)
					.AddKey(2, 0.5f),
			},
			false);

		effectData.Modifiers.Add(new Modifier
		{
			Attribute = TagName.FromString("TestAttributeSet.Attribute1"),
			Operation = ModifierOperation.Flat,
			Magnitude = new ModifierMagnitude
			{
				MagnitudeCalculationType = MagnitudeCalculationType.ScalableFloat,
				ScalableFloatMagnitude = new ScalableFloat(10)
					.AddKey(1, 1)
					.AddKey(2, 2),
			},
		});

		var effect = new GameplayEffect.GameplayEffect(effectData, 1, new GameplayEffectContext()
		{
			EffectCauser = new Entity(),
			Instigator = instigator,
		});

		target.GameplaySystem.GameplayEffectsManager.ApplyEffect(effect);

		Assert.AreEqual(11, target.PlayerAttributeSet.Attribute1.TotalValue);
		Assert.AreEqual(11, target.PlayerAttributeSet.Attribute1.BaseValue);
		Assert.AreEqual(0, target.PlayerAttributeSet.Attribute1.Modifier);
		Assert.AreEqual(0, target.PlayerAttributeSet.Attribute1.Overflow);

		// Simulate for 1 turn or seconds
		target.GameplaySystem.GameplayEffectsManager.UpdateEffects(1);

		Assert.AreEqual(21, target.PlayerAttributeSet.Attribute1.TotalValue);
		Assert.AreEqual(21, target.PlayerAttributeSet.Attribute1.BaseValue);
		Assert.AreEqual(0, target.PlayerAttributeSet.Attribute1.Modifier);
		Assert.AreEqual(0, target.PlayerAttributeSet.Attribute1.Overflow);

		effect.LevelUp();

		// Simulate 1.5 more second (3 periods)
		target.GameplaySystem.GameplayEffectsManager.UpdateEffects(1.5f);

		// 21 + (20 * 3)
		Assert.AreEqual(81, target.PlayerAttributeSet.Attribute1.TotalValue);
		Assert.AreEqual(81, target.PlayerAttributeSet.Attribute1.BaseValue);
		Assert.AreEqual(0, target.PlayerAttributeSet.Attribute1.Modifier);
		Assert.AreEqual(0, target.PlayerAttributeSet.Attribute1.Overflow);
	}

	[TestMethod]
	public void Inifinite_effect_should_modify_attribute_modifier_value()
	{
		var instigator = new Entity();
		var target = new Entity();

		var effectData = new GameplayEffectData(
			"Buff",
			new DurationData
			{
				Type = DurationType.Infinite,
			},
			null,
			null);

		effectData.Modifiers.Add(new Modifier
		{
			Attribute = TagName.FromString("TestAttributeSet.Attribute1"),
			Operation = ModifierOperation.Flat,
			Magnitude = new ModifierMagnitude
			{
				MagnitudeCalculationType = MagnitudeCalculationType.ScalableFloat,
				ScalableFloatMagnitude = new ScalableFloat(10),
			},
		});

		var effect = new GameplayEffect.GameplayEffect(effectData, 1, new GameplayEffectContext()
		{
			EffectCauser = new Entity(),
			Instigator = instigator,
		});

		target.GameplaySystem.GameplayEffectsManager.ApplyEffect(effect);

		Assert.AreEqual(11, target.PlayerAttributeSet.Attribute1.TotalValue);
		Assert.AreEqual(1, target.PlayerAttributeSet.Attribute1.BaseValue);
		Assert.AreEqual(10, target.PlayerAttributeSet.Attribute1.Modifier);
		Assert.AreEqual(0, target.PlayerAttributeSet.Attribute1.Overflow);

		// Simulate 5 seconds
		for (int i = 0; i < 32 * 5; i++)
		{
			target.GameplaySystem.GameplayEffectsManager.UpdateEffects(0.03125f);
		}

		Assert.AreEqual(11, target.PlayerAttributeSet.Attribute1.TotalValue);
		Assert.AreEqual(1, target.PlayerAttributeSet.Attribute1.BaseValue);
		Assert.AreEqual(10, target.PlayerAttributeSet.Attribute1.Modifier);
		Assert.AreEqual(0, target.PlayerAttributeSet.Attribute1.Overflow);
	}

	[TestMethod]
	public void Infinite_effect_of_different_operations_should_modify_modifier_and_multiplier_value_accordingly()
	{
		var instigator = new Entity();
		var target = new Entity();

		var effectData = new GameplayEffectData(
			"Buff",
			new DurationData
			{
				Type = DurationType.Infinite,
			},
			null,
			null);

		effectData.Modifiers.Add(new Modifier
		{
			Attribute = TagName.FromString("TestAttributeSet.Attribute1"),
			Operation = ModifierOperation.Flat,
			Magnitude = new ModifierMagnitude
			{
				MagnitudeCalculationType = MagnitudeCalculationType.ScalableFloat,
				ScalableFloatMagnitude = new ScalableFloat(4),
			},
		});

		var effect = new GameplayEffect.GameplayEffect(effectData, 1, new GameplayEffectContext()
		{
			EffectCauser = new Entity(),
			Instigator = instigator,
		});

		target.GameplaySystem.GameplayEffectsManager.ApplyEffect(effect);

		Assert.AreEqual(5, target.PlayerAttributeSet.Attribute1.TotalValue);
		Assert.AreEqual(1, target.PlayerAttributeSet.Attribute1.BaseValue);
		Assert.AreEqual(4, target.PlayerAttributeSet.Attribute1.Modifier);
		Assert.AreEqual(0, target.PlayerAttributeSet.Attribute1.Overflow);

		var effectData2 = new GameplayEffectData(
			"Buff",
			new DurationData
			{
				Type = DurationType.Infinite,
			},
			null,
			null);

		effectData2.Modifiers.Add(new Modifier
		{
			Attribute = TagName.FromString("TestAttributeSet.Attribute1"),
			Operation = ModifierOperation.Percent,
			Magnitude = new ModifierMagnitude // 400% bonus
			{
				MagnitudeCalculationType = MagnitudeCalculationType.ScalableFloat,
				ScalableFloatMagnitude = new ScalableFloat(4),
			},
		});

		var effect2 = new GameplayEffect.GameplayEffect(effectData2, 1, new GameplayEffectContext()
		{
			EffectCauser = new Entity(),
			Instigator = instigator,
		});

		target.GameplaySystem.GameplayEffectsManager.ApplyEffect(effect2);

		Assert.AreEqual(25, target.PlayerAttributeSet.Attribute1.TotalValue);
		Assert.AreEqual(1, target.PlayerAttributeSet.Attribute1.BaseValue);
		Assert.AreEqual(24, target.PlayerAttributeSet.Attribute1.Modifier);
		Assert.AreEqual(0, target.PlayerAttributeSet.Attribute1.Overflow);

		var effectData3 = new GameplayEffectData(
			"Buff",
			new DurationData
			{
				Type = DurationType.Infinite,
			},
			null,
			null);

		effectData3.Modifiers.Add(new Modifier
		{
			Attribute = TagName.FromString("TestAttributeSet.Attribute1"),
			Operation = ModifierOperation.Percent,
			Magnitude = new ModifierMagnitude // Divide by 3 (-66%)
			{
				MagnitudeCalculationType = MagnitudeCalculationType.ScalableFloat,
				ScalableFloatMagnitude = new ScalableFloat(-0.66f),
			},
		});

		var effect3 = new GameplayEffect.GameplayEffect(effectData3, 1, new GameplayEffectContext()
		{
			EffectCauser = new Entity(),
			Instigator = instigator,
		});

		target.GameplaySystem.GameplayEffectsManager.ApplyEffect(effect3);

		Assert.AreEqual(21, target.PlayerAttributeSet.Attribute1.TotalValue);
		Assert.AreEqual(1, target.PlayerAttributeSet.Attribute1.BaseValue);
		Assert.AreEqual(20, target.PlayerAttributeSet.Attribute1.Modifier);
		Assert.AreEqual(0, target.PlayerAttributeSet.Attribute1.Overflow);
	}

	[TestMethod]
	public void Infinite_effect_should_compute_channels_accordingly()
	{
		var instigator = new Entity();
		var target = new Entity();

		var effectData = new GameplayEffectData(
			"Buff",
			new DurationData
			{
				Type = DurationType.Infinite,
			},
			null,
			null);

		effectData.Modifiers.Add(new Modifier
		{
			Attribute = TagName.FromString("TestAttributeSet.Attribute1"),
			Operation = ModifierOperation.Flat,
			Magnitude = new ModifierMagnitude
			{
				MagnitudeCalculationType = MagnitudeCalculationType.ScalableFloat,
				ScalableFloatMagnitude = new ScalableFloat(4),
			},
		});

		effectData.Modifiers.Add(new Modifier
		{
			Attribute = TagName.FromString("TestAttributeSet.Attribute1"),
			Operation = ModifierOperation.Percent,
			Magnitude = new ModifierMagnitude // 400% bonus
			{
				MagnitudeCalculationType = MagnitudeCalculationType.ScalableFloat,
				ScalableFloatMagnitude = new ScalableFloat(4),
			},
		});

		effectData.Modifiers.Add(new Modifier
		{
			Attribute = TagName.FromString("TestAttributeSet.Attribute1"),
			Operation = ModifierOperation.Percent,
			Magnitude = new ModifierMagnitude // Divide by 3 (-66%)
			{
				MagnitudeCalculationType = MagnitudeCalculationType.ScalableFloat,
				ScalableFloatMagnitude = new ScalableFloat(-0.66f),
			},
			Channel = 1,
		});

		var effect = new GameplayEffect.GameplayEffect(effectData, 1, new GameplayEffectContext()
		{
			EffectCauser = new Entity(),
			Instigator = instigator,
		});

		target.GameplaySystem.GameplayEffectsManager.ApplyEffect(effect);

		Assert.AreEqual(8, target.PlayerAttributeSet.Attribute1.TotalValue);
		Assert.AreEqual(1, target.PlayerAttributeSet.Attribute1.BaseValue);
		Assert.AreEqual(7, target.PlayerAttributeSet.Attribute1.Modifier);
		Assert.AreEqual(0, target.PlayerAttributeSet.Attribute1.Overflow);
	}

	[TestMethod]
	public void Duration_effect_should_modify_attribute_modifier_value_and_expire_after_duration_time()
	{
		var instigator = new Entity();
		var target = new Entity();

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
			Attribute = TagName.FromString("TestAttributeSet.Attribute1"),
			Operation = ModifierOperation.Flat,
			Magnitude = new ModifierMagnitude
			{
				MagnitudeCalculationType = MagnitudeCalculationType.ScalableFloat,
				ScalableFloatMagnitude = new ScalableFloat(10),
			},
		});

		var effect = new GameplayEffect.GameplayEffect(effectData, 1, new GameplayEffectContext()
		{
			EffectCauser = new Entity(),
			Instigator = instigator,
		});

		target.GameplaySystem.GameplayEffectsManager.ApplyEffect(effect);

		Assert.AreEqual(11, target.PlayerAttributeSet.Attribute1.TotalValue);
		Assert.AreEqual(1, target.PlayerAttributeSet.Attribute1.BaseValue);
		Assert.AreEqual(10, target.PlayerAttributeSet.Attribute1.Modifier);
		Assert.AreEqual(0, target.PlayerAttributeSet.Attribute1.Overflow);

		// Simulate 5 seconds
		for (int i = 0; i < 32 * 5; i++)
		{
			target.GameplaySystem.GameplayEffectsManager.UpdateEffects(0.03125f);
		}

		Assert.AreEqual(11, target.PlayerAttributeSet.Attribute1.TotalValue);
		Assert.AreEqual(1, target.PlayerAttributeSet.Attribute1.BaseValue);
		Assert.AreEqual(10, target.PlayerAttributeSet.Attribute1.Modifier);
		Assert.AreEqual(0, target.PlayerAttributeSet.Attribute1.Overflow);

		// Simulate 5 more seconds
		for (int i = 0; i < 32 * 5; i++)
		{
			target.GameplaySystem.GameplayEffectsManager.UpdateEffects(0.03125f);
		}

		Assert.AreEqual(1, target.PlayerAttributeSet.Attribute1.TotalValue);
		Assert.AreEqual(1, target.PlayerAttributeSet.Attribute1.BaseValue);
		Assert.AreEqual(0, target.PlayerAttributeSet.Attribute1.Modifier);
		Assert.AreEqual(0, target.PlayerAttributeSet.Attribute1.Overflow);
	}

	[TestMethod]
	public void Periodic_effect_should_modify_base_attribute_value()
	{
		var instigator = new Entity();
		var target = new Entity();

		var effectData = new GameplayEffectData(
			"Buff",
			new DurationData
			{
				Type = DurationType.Infinite,
			},
			null,
			new PeriodicData
			{
				ExecuteOnApplication = true,
				Period = new ScalableFloat(1),
			});

		effectData.Modifiers.Add(new Modifier
		{
			Attribute = TagName.FromString("TestAttributeSet.Attribute1"),
			Operation = ModifierOperation.Flat,
			Magnitude = new ModifierMagnitude
			{
				MagnitudeCalculationType = MagnitudeCalculationType.ScalableFloat,
				ScalableFloatMagnitude = new ScalableFloat(10),
			},
		});

		var effect = new GameplayEffect.GameplayEffect(effectData, 1, new GameplayEffectContext()
		{
			EffectCauser = new Entity(),
			Instigator = instigator,
		});

		target.GameplaySystem.GameplayEffectsManager.ApplyEffect(effect);

		Assert.AreEqual(11, target.PlayerAttributeSet.Attribute1.TotalValue);
		Assert.AreEqual(11, target.PlayerAttributeSet.Attribute1.BaseValue);
		Assert.AreEqual(0, target.PlayerAttributeSet.Attribute1.Modifier);
		Assert.AreEqual(0, target.PlayerAttributeSet.Attribute1.Overflow);

		// Simulate for 1 turn or seconds
		target.GameplaySystem.GameplayEffectsManager.UpdateEffects(1);

		Assert.AreEqual(21, target.PlayerAttributeSet.Attribute1.TotalValue);
		Assert.AreEqual(21, target.PlayerAttributeSet.Attribute1.BaseValue);
		Assert.AreEqual(0, target.PlayerAttributeSet.Attribute1.Modifier);
		Assert.AreEqual(0, target.PlayerAttributeSet.Attribute1.Overflow);

		// Simulate 5 more seconds
		for (int i = 0; i < 32 * 5; i++)
		{
			target.GameplaySystem.GameplayEffectsManager.UpdateEffects(0.03125f);
		}

		Assert.AreEqual(71, target.PlayerAttributeSet.Attribute1.TotalValue);
		Assert.AreEqual(71, target.PlayerAttributeSet.Attribute1.BaseValue);
		Assert.AreEqual(0, target.PlayerAttributeSet.Attribute1.Modifier);
		Assert.AreEqual(0, target.PlayerAttributeSet.Attribute1.Overflow);
	}

	[TestMethod]
	public void Periodic_effect_should_modify_base_attribute_with_same_value_even_after_level_up()
	{
		var instigator = new Entity();
		var target = new Entity();

		var effectData = new GameplayEffectData(
			"Buff",
			new DurationData
			{
				Type = DurationType.Infinite,
			},
			null,
			new PeriodicData
			{
				ExecuteOnApplication = true,
				Period = new ScalableFloat(1),
			});

		effectData.Modifiers.Add(new Modifier
		{
			Attribute = TagName.FromString("TestAttributeSet.Attribute1"),
			Operation = ModifierOperation.Flat,
			Magnitude = new ModifierMagnitude
			{
				MagnitudeCalculationType = MagnitudeCalculationType.ScalableFloat,
				ScalableFloatMagnitude = new ScalableFloat(10),
			},
		});

		var effect = new GameplayEffect.GameplayEffect(effectData, 1, new GameplayEffectContext()
		{
			EffectCauser = new Entity(),
			Instigator = instigator,
		});

		target.GameplaySystem.GameplayEffectsManager.ApplyEffect(effect);

		Assert.AreEqual(11, target.PlayerAttributeSet.Attribute1.TotalValue);
		Assert.AreEqual(11, target.PlayerAttributeSet.Attribute1.BaseValue);
		Assert.AreEqual(0, target.PlayerAttributeSet.Attribute1.Modifier);
		Assert.AreEqual(0, target.PlayerAttributeSet.Attribute1.Overflow);

		effect.LevelUp();
		// Simulate for 1 turn or seconds
		target.GameplaySystem.GameplayEffectsManager.UpdateEffects(1);

		Assert.AreEqual(21, target.PlayerAttributeSet.Attribute1.TotalValue);
		Assert.AreEqual(21, target.PlayerAttributeSet.Attribute1.BaseValue);
		Assert.AreEqual(0, target.PlayerAttributeSet.Attribute1.Modifier);
		Assert.AreEqual(0, target.PlayerAttributeSet.Attribute1.Overflow);
	}

	[TestMethod]
	public void Periodic_effect_with_non_snapshot_attribute_based_magnitude_should_update_when_attribute_updates()
	{
		var instigator = new Entity();
		var target = new Entity();

		var effectData = new GameplayEffectData(
			"Buff",
			new DurationData
			{
				Type = DurationType.Infinite,
			},
			null,
			new PeriodicData
			{
				ExecuteOnApplication = true,
				Period = new ScalableFloat(1),
			});

		effectData.Modifiers.Add(new Modifier
		{
			Attribute = TagName.FromString("TestAttributeSet.Attribute1"),
			Operation = ModifierOperation.Flat,
			Magnitude = new ModifierMagnitude
			{
				MagnitudeCalculationType = MagnitudeCalculationType.AttributeBased,
				AttributeBasedFloat = new AttributeBasedFloat
				{
					BackingAttribute = new AttributeCaptureDefinition
					{
						Attribute = TagName.FromString("TestAttributeSet.Attribute2"),
						Source = AttributeCaptureSource.Source,
						Snapshot = false,
					},
					AttributeCalculationType = AttributeBasedFloatCalculationType.AttributeBaseValue,
					Coeficient = new ScalableFloat(2),
					PreMultiplyAdditiveValue = new ScalableFloat(1),
					PostMultiplyAdditiveValue = new ScalableFloat(2),
					// (([2] + 1) * 2) + 2 = 8
				},
			},
		});

		var effect = new GameplayEffect.GameplayEffect(effectData, 1, new GameplayEffectContext()
		{
			EffectCauser = new Entity(),
			Instigator = instigator,
		});

		target.GameplaySystem.GameplayEffectsManager.ApplyEffect(effect);

		Assert.AreEqual(9, target.PlayerAttributeSet.Attribute1.TotalValue);
		Assert.AreEqual(9, target.PlayerAttributeSet.Attribute1.BaseValue);
		Assert.AreEqual(0, target.PlayerAttributeSet.Attribute1.Modifier);
		Assert.AreEqual(0, target.PlayerAttributeSet.Attribute1.Overflow);

		// Simulate for 1 turn or seconds
		target.GameplaySystem.GameplayEffectsManager.UpdateEffects(1);

		Assert.AreEqual(17, target.PlayerAttributeSet.Attribute1.TotalValue);
		Assert.AreEqual(17, target.PlayerAttributeSet.Attribute1.BaseValue);
		Assert.AreEqual(0, target.PlayerAttributeSet.Attribute1.Modifier);
		Assert.AreEqual(0, target.PlayerAttributeSet.Attribute1.Overflow);

		var effectData2 = new GameplayEffectData(
			"Buff2",
			new DurationData
			{
				Type = DurationType.Instant,
			},
			null,
			null);

		effectData2.Modifiers.Add(new Modifier
		{
			Attribute = TagName.FromString("TestAttributeSet.Attribute2"),
			Operation = ModifierOperation.Flat,
			Magnitude = new ModifierMagnitude
			{
				MagnitudeCalculationType = MagnitudeCalculationType.ScalableFloat,
				ScalableFloatMagnitude = new ScalableFloat(2f),
			},
		});

		var effect2 = new GameplayEffect.GameplayEffect(effectData2, 1, new GameplayEffectContext()
		{
			EffectCauser = new Entity(),
			Instigator = instigator,
		});

		instigator.GameplaySystem.GameplayEffectsManager.ApplyEffect(effect2);

		Assert.AreEqual(4, instigator.PlayerAttributeSet.Attribute2.TotalValue);
		Assert.AreEqual(4, instigator.PlayerAttributeSet.Attribute2.BaseValue);
		Assert.AreEqual(0, instigator.PlayerAttributeSet.Attribute2.Modifier);
		Assert.AreEqual(0, instigator.PlayerAttributeSet.Attribute2.Overflow);

		// Simulate for 1 turn or seconds
		// New magnitude = (([4] + 1) * 2) + 2 = 12
		target.GameplaySystem.GameplayEffectsManager.UpdateEffects(1);

		Assert.AreEqual(29, target.PlayerAttributeSet.Attribute1.TotalValue);
		Assert.AreEqual(29, target.PlayerAttributeSet.Attribute1.BaseValue);
		Assert.AreEqual(0, target.PlayerAttributeSet.Attribute1.Modifier);
		Assert.AreEqual(0, target.PlayerAttributeSet.Attribute1.Overflow);

		instigator.GameplaySystem.GameplayEffectsManager.ApplyEffect(effect2);

		Assert.AreEqual(6, instigator.PlayerAttributeSet.Attribute2.TotalValue);
		Assert.AreEqual(6, instigator.PlayerAttributeSet.Attribute2.BaseValue);
		Assert.AreEqual(0, instigator.PlayerAttributeSet.Attribute2.Modifier);
		Assert.AreEqual(0, instigator.PlayerAttributeSet.Attribute2.Overflow);

		Assert.AreEqual(29, target.PlayerAttributeSet.Attribute1.TotalValue);
		Assert.AreEqual(29, target.PlayerAttributeSet.Attribute1.BaseValue);
		Assert.AreEqual(0, target.PlayerAttributeSet.Attribute1.Modifier);
		Assert.AreEqual(0, target.PlayerAttributeSet.Attribute1.Overflow);

		// New magnitude = (([6] + 1) * 2) + 2 = 16
		target.GameplaySystem.GameplayEffectsManager.UpdateEffects(1);

		Assert.AreEqual(45, target.PlayerAttributeSet.Attribute1.TotalValue);
		Assert.AreEqual(45, target.PlayerAttributeSet.Attribute1.BaseValue);
		Assert.AreEqual(0, target.PlayerAttributeSet.Attribute1.Modifier);
		Assert.AreEqual(0, target.PlayerAttributeSet.Attribute1.Overflow);
	}

	[TestMethod]
	public void Non_periodic_effect_with_non_snapshot_attribute_based_magnitude_should_update_when_attribute_updates()
	{
		var instigator = new Entity();
		var target = new Entity();

		var effectData = new GameplayEffectData(
			"Buff",
			new DurationData
			{
				Type = DurationType.Infinite,
			},
			null,
			null);

		effectData.Modifiers.Add(new Modifier
		{
			Attribute = TagName.FromString("TestAttributeSet.Attribute1"),
			Operation = ModifierOperation.Flat,
			Magnitude = new ModifierMagnitude
			{
				MagnitudeCalculationType = MagnitudeCalculationType.AttributeBased,
				AttributeBasedFloat = new AttributeBasedFloat
				{
					BackingAttribute = new AttributeCaptureDefinition
					{
						Attribute = TagName.FromString("TestAttributeSet.Attribute2"),
						Source = AttributeCaptureSource.Source,
						Snapshot = false,
					},
					AttributeCalculationType = AttributeBasedFloatCalculationType.AttributeBaseValue,
					Coeficient = new ScalableFloat(2),
					PreMultiplyAdditiveValue = new ScalableFloat(1),
					PostMultiplyAdditiveValue = new ScalableFloat(2),
					// (([2] + 1) * 2) + 2 = 8
				},
			},
		});

		var effect = new GameplayEffect.GameplayEffect(effectData, 1, new GameplayEffectContext()
		{
			EffectCauser = new Entity(),
			Instigator = instigator,
		});

		target.GameplaySystem.GameplayEffectsManager.ApplyEffect(effect);

		Assert.AreEqual(9, target.PlayerAttributeSet.Attribute1.TotalValue);
		Assert.AreEqual(1, target.PlayerAttributeSet.Attribute1.BaseValue);
		Assert.AreEqual(8, target.PlayerAttributeSet.Attribute1.Modifier);
		Assert.AreEqual(0, target.PlayerAttributeSet.Attribute1.Overflow);

		var effectData2 = new GameplayEffectData(
			"Buff2",
			new DurationData
			{
				Type = DurationType.Instant,
			},
			null,
			null);

		effectData2.Modifiers.Add(new Modifier
		{
			Attribute = TagName.FromString("TestAttributeSet.Attribute2"),
			Operation = ModifierOperation.Flat,
			Magnitude = new ModifierMagnitude
			{
				MagnitudeCalculationType = MagnitudeCalculationType.ScalableFloat,
				ScalableFloatMagnitude = new ScalableFloat(2f),
			},
		});

		var effect2 = new GameplayEffect.GameplayEffect(effectData2, 1, new GameplayEffectContext()
		{
			EffectCauser = new Entity(),
			Instigator = instigator,
		});

		instigator.GameplaySystem.GameplayEffectsManager.ApplyEffect(effect2);

		Assert.AreEqual(4, instigator.PlayerAttributeSet.Attribute2.TotalValue);
		Assert.AreEqual(4, instigator.PlayerAttributeSet.Attribute2.BaseValue);
		Assert.AreEqual(0, instigator.PlayerAttributeSet.Attribute2.Modifier);
		Assert.AreEqual(0, instigator.PlayerAttributeSet.Attribute2.Overflow);

		// New magnitude = (([4] + 1) * 2) + 2 = 12
		Assert.AreEqual(13, target.PlayerAttributeSet.Attribute1.TotalValue);
		Assert.AreEqual(1, target.PlayerAttributeSet.Attribute1.BaseValue);
		Assert.AreEqual(12, target.PlayerAttributeSet.Attribute1.Modifier);
		Assert.AreEqual(0, target.PlayerAttributeSet.Attribute1.Overflow);

		instigator.GameplaySystem.GameplayEffectsManager.ApplyEffect(effect2);

		Assert.AreEqual(6, instigator.PlayerAttributeSet.Attribute2.TotalValue);
		Assert.AreEqual(6, instigator.PlayerAttributeSet.Attribute2.BaseValue);
		Assert.AreEqual(0, instigator.PlayerAttributeSet.Attribute2.Modifier);
		Assert.AreEqual(0, instigator.PlayerAttributeSet.Attribute2.Overflow);

		// New magnitude = (([6] + 1) * 2) + 2 = 16
		Assert.AreEqual(17, target.PlayerAttributeSet.Attribute1.TotalValue);
		Assert.AreEqual(1, target.PlayerAttributeSet.Attribute1.BaseValue);
		Assert.AreEqual(16, target.PlayerAttributeSet.Attribute1.Modifier);
		Assert.AreEqual(0, target.PlayerAttributeSet.Attribute1.Overflow);
	}

	[TestMethod]
	public void Periodic_effect_should_modify_base_attribute_value_and_expire_after_duration_time()
	{
		var instigator = new Entity();
		var target = new Entity();

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
			Attribute = TagName.FromString("TestAttributeSet.Attribute1"),
			Operation = ModifierOperation.Flat,
			Magnitude = new ModifierMagnitude
			{
				MagnitudeCalculationType = MagnitudeCalculationType.ScalableFloat,
				ScalableFloatMagnitude = new ScalableFloat(10),
			},
		});

		var effect = new GameplayEffect.GameplayEffect(effectData, 1, new GameplayEffectContext()
		{
			EffectCauser = new Entity(),
			Instigator = instigator,
		});

		target.GameplaySystem.GameplayEffectsManager.ApplyEffect(effect);

		Assert.AreEqual(11, target.PlayerAttributeSet.Attribute1.TotalValue);
		Assert.AreEqual(11, target.PlayerAttributeSet.Attribute1.BaseValue);
		Assert.AreEqual(0, target.PlayerAttributeSet.Attribute1.Modifier);
		Assert.AreEqual(0, target.PlayerAttributeSet.Attribute1.Overflow);

		// Simulate for 1 turn or seconds
		target.GameplaySystem.GameplayEffectsManager.UpdateEffects(1);

		Assert.AreEqual(21, target.PlayerAttributeSet.Attribute1.TotalValue);
		Assert.AreEqual(21, target.PlayerAttributeSet.Attribute1.BaseValue);
		Assert.AreEqual(0, target.PlayerAttributeSet.Attribute1.Modifier);
		Assert.AreEqual(0, target.PlayerAttributeSet.Attribute1.Overflow);

		// Simulate 5 more seconds
		for (int i = 0; i < 32 * 5; i++)
		{
			target.GameplaySystem.GameplayEffectsManager.UpdateEffects(0.03125f);
		}

		Assert.AreEqual(41, target.PlayerAttributeSet.Attribute1.TotalValue);
		Assert.AreEqual(41, target.PlayerAttributeSet.Attribute1.BaseValue);
		Assert.AreEqual(0, target.PlayerAttributeSet.Attribute1.Modifier);
		Assert.AreEqual(0, target.PlayerAttributeSet.Attribute1.Overflow);
	}

	[TestMethod]
	public void Effect_should_stack_values()
	{
		var instigator1 = new Entity();
		var instigator2 = new Entity();
		var target = new Entity();

		var effectData = new GameplayEffectData(
			"Buff",
			new DurationData
			{
				Type = DurationType.Infinite,
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
			Attribute = TagName.FromString("TestAttributeSet.Attribute1"),
			Operation = ModifierOperation.Flat,
			Magnitude = new ModifierMagnitude
			{
				MagnitudeCalculationType = MagnitudeCalculationType.ScalableFloat,
				ScalableFloatMagnitude = new ScalableFloat(10),
			},
		});

		var effect = new GameplayEffect.GameplayEffect(effectData, 1, new GameplayEffectContext()
		{
			EffectCauser = new Entity(),
			Instigator = instigator1,
		});

		target.GameplaySystem.GameplayEffectsManager.ApplyEffect(effect);

		Assert.AreEqual(11, target.PlayerAttributeSet.Attribute1.TotalValue);
		Assert.AreEqual(1, target.PlayerAttributeSet.Attribute1.BaseValue);
		Assert.AreEqual(10, target.PlayerAttributeSet.Attribute1.Modifier);
		Assert.AreEqual(0, target.PlayerAttributeSet.Attribute1.Overflow);

		var effect2 = new GameplayEffect.GameplayEffect(effectData, 1, new GameplayEffectContext()
		{
			EffectCauser = new Entity(),
			Instigator = instigator2,
		});

		target.GameplaySystem.GameplayEffectsManager.ApplyEffect(effect2);
	}

	private class TestAttributeSet : AttributeSet
	{
		public readonly Attribute Attribute1;

		public readonly Attribute Attribute2;

		public readonly Attribute Attribute3;

		public readonly Attribute Attribute5;

		public readonly Attribute Attribute90;

		protected override void InitializeAttributes()
		{
			InitializeAttribute(Attribute1, 1, 0, 99, 2);
			InitializeAttribute(Attribute2, 2, 0, 99);
			InitializeAttribute(Attribute3, 3, 0, 99);
			InitializeAttribute(Attribute5, 5, 0, 99);
			InitializeAttribute(Attribute90, 90, 0, 99);
		}
	}

	private class Entity : IGameplaySystem
	{
		public GameplaySystem GameplaySystem { get; }

		public TestAttributeSet PlayerAttributeSet { get; }

		public Entity()
		{
			GameplaySystem = new GameplaySystem();
			PlayerAttributeSet = new TestAttributeSet();
			GameplaySystem.AddAttributeSet(PlayerAttributeSet);
		}
	}
}
