#pragma warning disable SA1600 // Elements should be documented
using System.Threading;

namespace GameplayTags.Runtime.Tests;

[TestClass]
public class GameplayTagTests
{
	[TestMethod]
	public void Is_invalid_GameplayTag_name()
	{
		var isValid = GameplayTag.IsValidGameplayTagString("Entity.Attributes.Strengh", out _, out _);

		Assert.IsTrue(isValid);
	}

	[TestMethod]
	public void Is_invalid_GameplayTag_name_but_got_fixed()
	{
		var isValid = GameplayTag.IsValidGameplayTagString(" Entity,Attr ibutes,Strength  ", out _, out string outFixedString);

		Assert.IsFalse(isValid);
		Assert.IsTrue(outFixedString == "Entity_Attr_ibutes_Strength");
	}

	[TestMethod]
	[TestCategory("MatchesTag")]
	public void Tag_should_match()
	{
		var tagA = GameplayTag.RequestGameplayTag(TagName.FromString("A.1"));
		var tagB = GameplayTag.RequestGameplayTag(TagName.FromString("A"));

		Assert.IsTrue(tagA.MatchesTag(tagB));
	}

	[TestMethod]
	[TestCategory("MatchesTag")]
	public void Tag_shouldnt_match()
	{
		var tagA = GameplayTag.RequestGameplayTag(TagName.FromString("A"));
		var tagB = GameplayTag.RequestGameplayTag(TagName.FromString("A.1"));

		Assert.IsFalse(tagA.MatchesTag(tagB));
	}

	[TestMethod]
	[TestCategory("MatchesTag")]
	public void Tag_shouldnt_match_2()
	{
		var tagA = GameplayTag.RequestGameplayTag(TagName.FromString("A.1"));
		var tagB = GameplayTag.RequestGameplayTag(TagName.FromString("1"));

		Assert.IsFalse(tagA.MatchesTag(tagB));
	}

	[TestMethod]
	[TestCategory("MatchesTagExact")]
	public void Tag_should_match_exact()
	{
		var tagA = GameplayTag.RequestGameplayTag(TagName.FromString("A.1"));
		var tagB = GameplayTag.RequestGameplayTag(TagName.FromString("A.1"));

		Assert.IsTrue(tagA.MatchesTagExact(tagB));
		Assert.IsTrue(tagA == tagB);
	}

	[TestMethod]
	[TestCategory("MatchesTagExact")]
	public void Tag_shouldnt_match_exact()
	{
		var tagA = GameplayTag.RequestGameplayTag(TagName.FromString("A.1"));
		var tagB = GameplayTag.RequestGameplayTag(TagName.FromString("A"));

		Assert.IsFalse(tagA.MatchesTagExact(tagB));
		Assert.IsFalse(tagA == tagB);
	}

	[TestMethod]
	[TestCategory("MatchesAny")]
	public void Tag_should_match_any()
	{
		var tag = GameplayTag.RequestGameplayTag(TagName.FromString("A.1"));

		var tagContainer = new GameplayTagContainer();
		tagContainer.AddTag(GameplayTag.RequestGameplayTag(TagName.FromString("A")));
		tagContainer.AddTag(GameplayTag.RequestGameplayTag(TagName.FromString("B")));

		Assert.IsTrue(tag.MatchesAny(tagContainer));
	}

	[TestMethod]
	[TestCategory("MatchesAny")]
	public void Tag_shouldnt_match_any()
	{
		var tag = GameplayTag.RequestGameplayTag(TagName.FromString("A"));

		var tagContainer = new GameplayTagContainer();
		tagContainer.AddTag(GameplayTag.RequestGameplayTag(TagName.FromString("A.1")));
		tagContainer.AddTag(GameplayTag.RequestGameplayTag(TagName.FromString("B")));

		Assert.IsFalse(tag.MatchesAny(tagContainer));
	}

	[TestMethod]
	[TestCategory("MatchesAnyExact")]
	public void Tag_should_match_any_exact()
	{
		var tag = GameplayTag.RequestGameplayTag(TagName.FromString("A.1"));

		var tagContainer = new GameplayTagContainer();
		tagContainer.AddTag(GameplayTag.RequestGameplayTag(TagName.FromString("A.1")));
		tagContainer.AddTag(GameplayTag.RequestGameplayTag(TagName.FromString("B")));

		Assert.IsTrue(tag.MatchesAnyExact(tagContainer));
	}

	[TestMethod]
	[TestCategory("MatchesAnyExact")]
	public void Tag_shouldnt_match_any_exact()
	{
		var tag = GameplayTag.RequestGameplayTag(TagName.FromString("A.1"));

		var tagContainer = new GameplayTagContainer();
		tagContainer.AddTag(GameplayTag.RequestGameplayTag(TagName.FromString("A")));
		tagContainer.AddTag(GameplayTag.RequestGameplayTag(TagName.FromString("B")));

		Assert.IsFalse(tag.MatchesAnyExact(tagContainer));
	}

	[TestMethod]
	[TestCategory("MatchesTagDepth")]
	public void Tag_should_match_depth()
	{
		var tagA = GameplayTag.RequestGameplayTag(TagName.FromString("A.1"));
		var tagB = GameplayTag.RequestGameplayTag(TagName.FromString("A.B.1"));
		var tagC = GameplayTag.RequestGameplayTag(TagName.FromString("A.B.2"));
		var tagD = GameplayTag.RequestGameplayTag(TagName.FromString("A.C.3"));
		var tagE = GameplayTag.RequestGameplayTag(TagName.FromString("Character.Attributes.Dex"));

		Assert.IsTrue(tagA.MatchesTagDepth(tagB) == 1);
		Assert.IsTrue(tagA.MatchesTagDepth(tagC) == 1);
		Assert.IsTrue(tagA.MatchesTagDepth(tagD) == 1);
		Assert.IsTrue(tagA.MatchesTagDepth(tagE) == 0);

		Assert.IsTrue(tagB.MatchesTagDepth(tagC) == 2);
		Assert.IsTrue(tagB.MatchesTagDepth(tagD) == 1);
		Assert.IsTrue(tagB.MatchesTagDepth(tagE) == 0);

		Assert.IsTrue(tagC.MatchesTagDepth(tagD) == 1);
		Assert.IsTrue(tagC.MatchesTagDepth(tagE) == 0);

		Assert.IsTrue(tagD.MatchesTagDepth(tagE) == 0);
	}

	[TestMethod]
	[TestCategory("Serialization")]
	public void Tags_should_serialize_successfully()
	{
		var tagA = GameplayTag.RequestGameplayTag(TagName.FromString("A.1"));
		var tagB = GameplayTag.RequestGameplayTag(TagName.FromString("A.B.1"));
		var tagC = GameplayTag.RequestGameplayTag(TagName.FromString("A.B.2"));
		var tagD = GameplayTag.RequestGameplayTag(TagName.FromString("A.C.3"));
		var tagE = GameplayTag.RequestGameplayTag(TagName.FromString("Character.Attributes.Dex"));

		Assert.IsTrue(GameplayTag.NetSerialize(tagA, out var tagANetIndex));
		Assert.IsTrue(GameplayTag.NetSerialize(tagB, out var tagBNetIndex));
		Assert.IsTrue(GameplayTag.NetSerialize(tagC, out var tagCNetIndex));
		Assert.IsTrue(GameplayTag.NetSerialize(tagD, out var tagDNetIndex));
		Assert.IsTrue(GameplayTag.NetSerialize(tagE, out var tagENetIndex));

		Assert.IsTrue(tagANetIndex == 2);
		Assert.IsTrue(tagBNetIndex == 4);
		Assert.IsTrue(tagCNetIndex == 5);
		Assert.IsTrue(tagDNetIndex == 9);
		Assert.IsTrue(tagENetIndex == 14);
	}

	[TestMethod]
	[TestCategory("Serialization")]
	public void Tags_should_deserialize_successfully()
	{
		Assert.IsTrue(GameplayTag.NetDeserialize([2, 0], out var tagA));
		Assert.IsTrue(GameplayTag.NetDeserialize([4, 0], out var tagB));
		Assert.IsTrue(GameplayTag.NetDeserialize([5, 0], out var tagC));
		Assert.IsTrue(GameplayTag.NetDeserialize([9, 0], out var tagD));
		Assert.IsTrue(GameplayTag.NetDeserialize([14, 0], out var tagE));

		var tagACheck = GameplayTag.RequestGameplayTag(TagName.FromString("A.1"));
		var tagBCheck = GameplayTag.RequestGameplayTag(TagName.FromString("A.B.1"));
		var tagCCheck = GameplayTag.RequestGameplayTag(TagName.FromString("A.B.2"));
		var tagDCheck = GameplayTag.RequestGameplayTag(TagName.FromString("A.C.3"));
		var tagECheck = GameplayTag.RequestGameplayTag(TagName.FromString("Character.Attributes.Dex"));

		Assert.IsTrue(tagA == tagACheck);
		Assert.IsTrue(tagB == tagBCheck);
		Assert.IsTrue(tagC == tagCCheck);
		Assert.IsTrue(tagD == tagDCheck);
		Assert.IsTrue(tagE == tagECheck);
	}
}
