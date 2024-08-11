#pragma warning disable SA1600 // Elements should be documented
using GameplayTags.Runtime.Attribute;
using GameplayTags.Runtime.GameplayEffect;

namespace GameplayTags.Runtime.Tests;

[TestClass]
public class GameplayEffectsTests
{
	[TestMethod]
	public void GameplayEffectTests()
	{
		var owner = new object();

		var effectData = new GameplayEffectData("Buff", 10, 0);

		effectData.Modifiers.Add(new Modifier
		{
			Attribute = TagName.FromString("PlayerAttributeSet.Strength"),
			Value = 10,
		});

		var effect = new GameplayEffect.GameplayEffect(effectData, 1, new GameplayEffectContext());

		Assert.AreEqual(10, effect.GetScaledMagnitude());

		//var playerAttributes = new PlayerAttributeSet();

		//var manager = new GameplayEffectsManager(playerAttributes);

		//manager.ApplyEffect(effect);

		//Assert.AreEqual(11, playerAttributes.Strength.TotalValue);
	}
}
