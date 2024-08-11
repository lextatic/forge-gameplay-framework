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
			"Buff",
			10,
			new DurationData
			{
				Type = DurationType.Instant,
				Duration = 0,
			},
			null);

		effectData.Modifiers.Add(new Modifier
		{
			Attribute = TagName.FromString("PlayerAttributeSet.Strength"),
			Value = 10,
		});

		var effect = new GameplayEffect.GameplayEffect(effectData, 1, new GameplayEffectContext());

		Assert.AreEqual(10, effect.GetScaledMagnitude());

		var playerAttributes = new PlayerAttributeSet();

		var manager = new GameplayEffectsManager(playerAttributes);

		manager.ApplyEffect(effect);

		Assert.AreEqual(11, playerAttributes.Strength.TotalValue);
		Assert.AreEqual(11, playerAttributes.Strength.BaseValue);
		Assert.AreEqual(0, playerAttributes.Strength.ValidModifierValue);
		Assert.AreEqual(0, playerAttributes.Strength.TotalModifierValue);
	}

	[TestMethod]
	public void Inifinite_effect_should_modify_attribute_modifier_value()
	{
		var owner = new object();

		var effectData = new GameplayEffectData(
			"Buff",
			10,
			new DurationData
			{
				Type = DurationType.Infinite,
				Duration = 0,
			},
			null);

		effectData.Modifiers.Add(new Modifier
		{
			Attribute = TagName.FromString("PlayerAttributeSet.Strength"),
			Value = 10,
		});

		var effect = new GameplayEffect.GameplayEffect(effectData, 1, new GameplayEffectContext());

		Assert.AreEqual(10, effect.GetScaledMagnitude());

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
			10,
			new DurationData
			{
				Type = DurationType.HasDuration,
				Duration = 10,
			},
			null);

		effectData.Modifiers.Add(new Modifier
		{
			Attribute = TagName.FromString("PlayerAttributeSet.Strength"),
			Value = 10,
		});

		var effect = new GameplayEffect.GameplayEffect(effectData, 1, new GameplayEffectContext());

		Assert.AreEqual(10, effect.GetScaledMagnitude());

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
