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
				Duration = 0,
			},
			null,
			null);

		effectData.Modifiers.Add(new Modifier
		{
			Attribute = TagName.FromString("PlayerAttributeSet.Strength"),
			Operation = ModifierOperation.Add,
			Value = new ScalableInt(10),
		});

		var effect = new GameplayEffect.GameplayEffect(effectData, 1, new GameplayEffectContext());

		var playerAttributes = new PlayerAttributeSet();

		var manager = new GameplayEffectsManager(playerAttributes);

		manager.ApplyEffect(effect);

		Assert.AreEqual(11, playerAttributes.Strength.TotalValue);
		Assert.AreEqual(11, playerAttributes.Strength.BaseValue);
		Assert.AreEqual(0, playerAttributes.Strength.ValidModifierValue);
		Assert.AreEqual(0, playerAttributes.Strength.TotalModifierValue);
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
				Duration = 0,
			},
			null,
			null);

		effectData.Modifiers.Add(new Modifier
		{
			Attribute = TagName.FromString("PlayerAttributeSet.Strength"),
			Operation = ModifierOperation.Add,
			Value = new ScalableInt(10),
		});

		var effect = new GameplayEffect.GameplayEffect(effectData, 1, new GameplayEffectContext());

		var playerAttributes = new PlayerAttributeSet();

		var manager = new GameplayEffectsManager(playerAttributes);

		manager.ApplyEffect(effect);

		Assert.AreEqual(11, playerAttributes.Strength.TotalValue);
		Assert.AreEqual(11, playerAttributes.Strength.BaseValue);
		Assert.AreEqual(0, playerAttributes.Strength.ValidModifierValue);
		Assert.AreEqual(0, playerAttributes.Strength.TotalModifierValue);

		effect.LevelUp();

		manager.ApplyEffect(effect);

		Assert.AreEqual(31, playerAttributes.Strength.TotalValue);
		Assert.AreEqual(31, playerAttributes.Strength.BaseValue);
		Assert.AreEqual(0, playerAttributes.Strength.ValidModifierValue);
		Assert.AreEqual(0, playerAttributes.Strength.TotalModifierValue);
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
				Duration = 0,
			},
			null,
			null);

		effectData.Modifiers.Add(new Modifier
		{
			Attribute = TagName.FromString("PlayerAttributeSet.Strength"),
			Operation = ModifierOperation.Add,
			Value = new ScalableInt(10),
		});

		var effect = new GameplayEffect.GameplayEffect(effectData, 1, new GameplayEffectContext());

		var playerAttributes = new PlayerAttributeSet();

		var manager = new GameplayEffectsManager(playerAttributes);

		manager.ApplyEffect(effect);

		Assert.AreEqual(11, playerAttributes.Strength.TotalValue);
		Assert.AreEqual(1, playerAttributes.Strength.BaseValue);
		Assert.AreEqual(10, playerAttributes.Strength.ValidModifierValue);
		Assert.AreEqual(10, playerAttributes.Strength.TotalModifierValue);

		// Simulate 5 seconds
		for (int i = 0; i < 32 * 5; i++)
		{
			manager.UpdateEffects(0.03125f);
		}

		Assert.AreEqual(11, playerAttributes.Strength.TotalValue);
		Assert.AreEqual(1, playerAttributes.Strength.BaseValue);
		Assert.AreEqual(10, playerAttributes.Strength.ValidModifierValue);
		Assert.AreEqual(10, playerAttributes.Strength.TotalModifierValue);
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
				Duration = 10,
			},
			null,
			null);

		effectData.Modifiers.Add(new Modifier
		{
			Attribute = TagName.FromString("PlayerAttributeSet.Strength"),
			Operation = ModifierOperation.Add,
			Value = new ScalableInt(10),
		});

		var effect = new GameplayEffect.GameplayEffect(effectData, 1, new GameplayEffectContext());

		var playerAttributes = new PlayerAttributeSet();

		var manager = new GameplayEffectsManager(playerAttributes);

		manager.ApplyEffect(effect);

		Assert.AreEqual(11, playerAttributes.Strength.TotalValue);
		Assert.AreEqual(1, playerAttributes.Strength.BaseValue);
		Assert.AreEqual(10, playerAttributes.Strength.ValidModifierValue);
		Assert.AreEqual(10, playerAttributes.Strength.TotalModifierValue);

		// Simulate 5 seconds
		for (int i = 0; i < 32 * 5; i++)
		{
			manager.UpdateEffects(0.03125f);
		}

		Assert.AreEqual(11, playerAttributes.Strength.TotalValue);
		Assert.AreEqual(1, playerAttributes.Strength.BaseValue);
		Assert.AreEqual(10, playerAttributes.Strength.ValidModifierValue);
		Assert.AreEqual(10, playerAttributes.Strength.TotalModifierValue);

		// Simulate 5 more seconds
		for (int i = 0; i < 32 * 5; i++)
		{
			manager.UpdateEffects(0.03125f);
		}

		Assert.AreEqual(1, playerAttributes.Strength.TotalValue);
		Assert.AreEqual(1, playerAttributes.Strength.BaseValue);
		Assert.AreEqual(0, playerAttributes.Strength.ValidModifierValue);
		Assert.AreEqual(0, playerAttributes.Strength.TotalModifierValue);
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
				Duration = 0,
			},
			null,
			new PeriodicData
			{
				ExecuteOnApplication = true,
				Period = 1,
			});

		effectData.Modifiers.Add(new Modifier
		{
			Attribute = TagName.FromString("PlayerAttributeSet.Strength"),
			Operation = ModifierOperation.Add,
			Value = new ScalableInt(10),
		});

		var effect = new GameplayEffect.GameplayEffect(effectData, 1, new GameplayEffectContext());

		var playerAttributes = new PlayerAttributeSet();

		var manager = new GameplayEffectsManager(playerAttributes);

		manager.ApplyEffect(effect);

		Assert.AreEqual(11, playerAttributes.Strength.TotalValue);
		Assert.AreEqual(11, playerAttributes.Strength.BaseValue);
		Assert.AreEqual(0, playerAttributes.Strength.ValidModifierValue);
		Assert.AreEqual(0, playerAttributes.Strength.TotalModifierValue);

		// Simulate for 1 turn or seconds
		manager.UpdateEffects(1);

		Assert.AreEqual(21, playerAttributes.Strength.TotalValue);
		Assert.AreEqual(21, playerAttributes.Strength.BaseValue);
		Assert.AreEqual(0, playerAttributes.Strength.ValidModifierValue);
		Assert.AreEqual(0, playerAttributes.Strength.TotalModifierValue);

		// Simulate 5 more seconds
		for (int i = 0; i < 32 * 5; i++)
		{
			manager.UpdateEffects(0.03125f);
		}

		Assert.AreEqual(71, playerAttributes.Strength.TotalValue);
		Assert.AreEqual(71, playerAttributes.Strength.BaseValue);
		Assert.AreEqual(0, playerAttributes.Strength.ValidModifierValue);
		Assert.AreEqual(0, playerAttributes.Strength.TotalModifierValue);
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
				Duration = 0,
			},
			null,
			new PeriodicData
			{
				ExecuteOnApplication = true,
				Period = 1,
			});

		effectData.Modifiers.Add(new Modifier
		{
			Attribute = TagName.FromString("PlayerAttributeSet.Strength"),
			Operation = ModifierOperation.Add,
			Value = new ScalableInt(10),
		});

		var effect = new GameplayEffect.GameplayEffect(effectData, 1, new GameplayEffectContext());

		var playerAttributes = new PlayerAttributeSet();

		var manager = new GameplayEffectsManager(playerAttributes);

		manager.ApplyEffect(effect);

		Assert.AreEqual(11, playerAttributes.Strength.TotalValue);
		Assert.AreEqual(11, playerAttributes.Strength.BaseValue);
		Assert.AreEqual(0, playerAttributes.Strength.ValidModifierValue);
		Assert.AreEqual(0, playerAttributes.Strength.TotalModifierValue);

		effect.LevelUp();
		// Simulate for 1 turn or seconds
		manager.UpdateEffects(1);

		Assert.AreEqual(21, playerAttributes.Strength.TotalValue);
		Assert.AreEqual(21, playerAttributes.Strength.BaseValue);
		Assert.AreEqual(0, playerAttributes.Strength.ValidModifierValue);
		Assert.AreEqual(0, playerAttributes.Strength.TotalModifierValue);
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
				Duration = 3,
			},
			null,
			new PeriodicData
			{
				ExecuteOnApplication = true,
				Period = 1,
			});

		effectData.Modifiers.Add(new Modifier
		{
			Attribute = TagName.FromString("PlayerAttributeSet.Strength"),
			Operation = ModifierOperation.Add,
			Value = new ScalableInt(10),
		});

		var effect = new GameplayEffect.GameplayEffect(effectData, 1, new GameplayEffectContext());

		var playerAttributes = new PlayerAttributeSet();

		var manager = new GameplayEffectsManager(playerAttributes);

		manager.ApplyEffect(effect);

		Assert.AreEqual(11, playerAttributes.Strength.TotalValue);
		Assert.AreEqual(11, playerAttributes.Strength.BaseValue);
		Assert.AreEqual(0, playerAttributes.Strength.ValidModifierValue);
		Assert.AreEqual(0, playerAttributes.Strength.TotalModifierValue);

		// Simulate for 1 turn or seconds
		manager.UpdateEffects(1);

		Assert.AreEqual(21, playerAttributes.Strength.TotalValue);
		Assert.AreEqual(21, playerAttributes.Strength.BaseValue);
		Assert.AreEqual(0, playerAttributes.Strength.ValidModifierValue);
		Assert.AreEqual(0, playerAttributes.Strength.TotalModifierValue);

		// Simulate 5 more seconds
		for (int i = 0; i < 32 * 5; i++)
		{
			manager.UpdateEffects(0.03125f);
		}

		Assert.AreEqual(41, playerAttributes.Strength.TotalValue);
		Assert.AreEqual(41, playerAttributes.Strength.BaseValue);
		Assert.AreEqual(0, playerAttributes.Strength.ValidModifierValue);
		Assert.AreEqual(0, playerAttributes.Strength.TotalModifierValue);
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
				Duration = 0,
			},
			new StackingData
			{
				StackLimit = 5,
				StackPolicy = StackPolicy.AggregateByTarget,
				StackLevelPolicy = StackLevelPolicy.SegregateLevels,
				StackRemovalPolicy = StackRemovalPolicy.ClearEntireStack,
				StackApplicationRefreshPolicy = null,
				StackLevelOverridePolicy = null,
				StackApplicationResetPeriodPolicy = null,
			},
			null);

		effectData.Modifiers.Add(new Modifier
		{
			Attribute = TagName.FromString("PlayerAttributeSet.Strength"),
			Operation = ModifierOperation.Add,
			Value = new ScalableInt(10),
		});

		var effect = new GameplayEffect.GameplayEffect(effectData, 1, new GameplayEffectContext());

		var playerAttributes = new PlayerAttributeSet();

		var manager = new GameplayEffectsManager(playerAttributes);

		manager.ApplyEffect(effect);

		Assert.AreEqual(11, playerAttributes.Strength.TotalValue);
		Assert.AreEqual(1, playerAttributes.Strength.BaseValue);
		Assert.AreEqual(10, playerAttributes.Strength.ValidModifierValue);
		Assert.AreEqual(10, playerAttributes.Strength.TotalModifierValue);
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
