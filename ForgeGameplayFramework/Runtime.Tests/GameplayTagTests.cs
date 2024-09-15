#pragma warning disable SA1600 // Elements should be documented
using System.Drawing;
using System.Threading;

namespace GameplayTags.Runtime.Tests;

[TestClass]
public class GameplayTagTests
{
	[TestMethod]
	[TestCategory("RequestGameplayTag")]
	public void Requested_registered_tags_should_be_valid()
	{
		var tagName = TagName.FromString("A.1");
		var tag = GameplayTag.RequestGameplayTag(tagName);

		Assert.IsTrue(tag.IsValid);
		Assert.IsTrue(tag.TagName == tagName);
	}

	[TestMethod]
	[TestCategory("RequestGameplayTag")]
	[ExpectedException(typeof(Exception), "An unregistered tag should not be allowed.")]
	public void Requested_unregistered_tags_should_throw_exception()
	{
		GameplayTag.RequestGameplayTag(TagName.FromString("Foo.Bar"));
	}

	[TestMethod]
	[TestCategory("RequestGameplayTag")]
	public void Requested_unregistered_tags_without_error_should_be_invalid()
	{
		var tag = GameplayTag.RequestGameplayTag(TagName.FromString("Foo.Bar"), false);

		Assert.IsFalse(tag.IsValid);
	}

	[TestMethod]
	[TestCategory("RequestGameplayTag")]
	[ExpectedException(typeof(Exception), "Empty TagName request should not be allowed.")]
	public void Requested_EmptyTag_name_should_throw_exception()
	{
		GameplayTag.RequestGameplayTag(TagName.Empty);
	}

	[TestMethod]
	[TestCategory("RequestGameplayTag")]
	public void Parents_not_explicitly_registered_should_be_valid()
	{
		var tag = GameplayTag.RequestGameplayTag(TagName.FromString("B"));

		Assert.IsTrue(tag.IsValid);
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
	public void Non_registered_tag_should_serialize_successfully_with_InvalidTagNetIndex()
	{
		var tag = GameplayTag.RequestGameplayTag(TagName.FromString("Foo.Bar"), false);

		Assert.IsTrue(GameplayTag.NetSerialize(tag, out var tagNetIndex));

		Assert.IsTrue(tagNetIndex == GameplayTagsManager.Instance.NodesCount + 1);
	}

	[TestMethod]
	[TestCategory("Serialization")]
	public void EmptyTag_should_serialize_successfully_with_InvalidTagNetIndex()
	{
		Assert.IsTrue(GameplayTag.NetSerialize(GameplayTag.EmptyTag, out var tagNetIndex));

		Assert.IsTrue(tagNetIndex == GameplayTagsManager.Instance.NodesCount + 1);
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

	[TestMethod]
	[TestCategory("Serialization")]
	public void InvalidTagNetIndex_should_deserialize_successfully_as_EmptyTag()
	{
		Assert.IsTrue(GameplayTag.NetDeserialize(
			[(byte)(GameplayTagsManager.Instance.NodesCount + 1), 0],
			out var tag));

		Assert.IsTrue(tag == GameplayTag.EmptyTag);
	}

	[TestMethod]
	[TestCategory("Serialization")]
	[ExpectedException(typeof(Exception), "Net index should never be higher than the specified invalid net index.")]
	public void Higher_than_InvalidTagNetIndex_should_not_deserialize_successfully()
	{
		GameplayTag.NetDeserialize(
			[(byte)(GameplayTagsManager.Instance.NodesCount + 2), 0],
			out var _);
	}

#if DEBUG
	[TestMethod]
	[TestCategory("IsValidGameplayTagString")]
	public void Correctly_formatted_strings_should_be_valid_tag_names()
	{
		var isValid = GameplayTag.IsValidGameplayTagString("Entity.Attributes.Strengh", out _, out _);

		Assert.IsTrue(isValid);
	}

	[TestMethod]
	[TestCategory("IsValidGameplayTagString")]
	public void Should_return_a_corrected_string_when_passing_an_invalid_one()
	{
		var isValid = GameplayTag.IsValidGameplayTagString(" Entity,Attr ibutes,Strength  ", out _, out string outFixedString);

		Assert.IsFalse(isValid);
		Assert.IsTrue(outFixedString == "Entity_Attr_ibutes_Strength");
	}
#endif

	[TestMethod]
	[TestCategory("GetSingleTagContainer")]
	public void SingleTag_containers_for_registered_tags_is_valid()
	{
		var tag = GameplayTag.RequestGameplayTag(TagName.FromString("A.1"));

		var tagContainer = tag.GetSingleTagContainer();

		var tagContainerCheck = new GameplayTagContainer();
		tagContainerCheck.AddTag(GameplayTag.RequestGameplayTag(TagName.FromString("A.1")));

		Assert.IsTrue(tagContainer == tagContainerCheck);
	}

	[TestMethod]
	[TestCategory("GetSingleTagContainer")]
	public void SingleTag_containers_for_unregistered_tags_is_EmptyContainer()
	{
		var tag = GameplayTag.RequestGameplayTag(TagName.FromString("Foo.Bar"), false);

		var tagContainer = tag.GetSingleTagContainer();

		Assert.IsTrue(tagContainer == GameplayTagContainer.EmptyContainer);
	}

	[TestMethod]
	[TestCategory("GetSingleTagContainer")]
	public void SingleTag_containers_for_EmptyTag_is_EmptyContainer()
	{
		var tag = GameplayTag.EmptyTag;

		var tagContainer = tag.GetSingleTagContainer();

		Assert.IsTrue(tagContainer == GameplayTagContainer.EmptyContainer);
	}

	[TestMethod]
	[TestCategory("RequestDirectParent")]
	public void Should_return_the_correct_requested_direct_parents()
	{
		var tagA = GameplayTag.RequestGameplayTag(TagName.FromString("A.1"));
		var tagB = GameplayTag.RequestGameplayTag(TagName.FromString("A.B.1"));
		var tagC = GameplayTag.RequestGameplayTag(TagName.FromString("A.B.2"));
		var tagD = GameplayTag.RequestGameplayTag(TagName.FromString("A.C.3"));
		var tagE = GameplayTag.RequestGameplayTag(TagName.FromString("Character.Attributes.Dex"));

		var directParentA = tagA.RequestDirectParent();
		var directParentB = tagB.RequestDirectParent();
		var directParentC = tagC.RequestDirectParent();
		var directParentD = tagD.RequestDirectParent();
		var directParentE = tagE.RequestDirectParent();

		var directParentCheckA = GameplayTag.RequestGameplayTag(TagName.FromString("A"));
		var directParentCheckBC = GameplayTag.RequestGameplayTag(TagName.FromString("A.B"));
		var directParentCheckD = GameplayTag.RequestGameplayTag(TagName.FromString("A.C"));
		var directParentCheckE = GameplayTag.RequestGameplayTag(TagName.FromString("Character.Attributes"));

		Assert.IsTrue(directParentA == directParentCheckA);
		Assert.IsTrue(directParentB == directParentCheckBC);
		Assert.IsTrue(directParentC == directParentCheckBC);
		Assert.IsTrue(directParentD == directParentCheckD);
		Assert.IsTrue(directParentE == directParentCheckE);
	}

	[TestMethod]
	[TestCategory("RequestDirectParent")]
	public void Direct_parent_of_a_root_tag_should_be_EmptyTag()
	{
		var tag = GameplayTag.RequestGameplayTag(TagName.FromString("A"));

		var directParent = tag.RequestDirectParent();

		Assert.IsTrue(directParent == GameplayTag.EmptyTag);
	}

	[TestMethod]
	[TestCategory("GetGameplayTagParents")]
	public void Should_return_the_correct_container_with_all_parent_tags()
	{
		var tagA = GameplayTag.RequestGameplayTag(TagName.FromString("A.1"));
		var tagB = GameplayTag.RequestGameplayTag(TagName.FromString("A.B.1"));
		var tagC = GameplayTag.RequestGameplayTag(TagName.FromString("A.B.2"));
		var tagD = GameplayTag.RequestGameplayTag(TagName.FromString("A.C.3"));
		var tagE = GameplayTag.RequestGameplayTag(TagName.FromString("Character.Attributes.Dex"));

		var parentsContainerA = tagA.GetGameplayTagParents();
		var parentsContainerB = tagB.GetGameplayTagParents();
		var parentsContainerC = tagC.GetGameplayTagParents();
		var parentsContainerD = tagD.GetGameplayTagParents();
		var parentsContainerE = tagE.GetGameplayTagParents();

		var parentsContainerCheckA = new GameplayTagContainer();
		parentsContainerCheckA.AddTag(GameplayTag.RequestGameplayTag(TagName.FromString("A")));
		parentsContainerCheckA.AddTag(GameplayTag.RequestGameplayTag(TagName.FromString("A.1")));

		var parentsContainerCheckB = new GameplayTagContainer();
		parentsContainerCheckB.AddTag(GameplayTag.RequestGameplayTag(TagName.FromString("A")));
		parentsContainerCheckB.AddTag(GameplayTag.RequestGameplayTag(TagName.FromString("A.B")));
		parentsContainerCheckB.AddTag(GameplayTag.RequestGameplayTag(TagName.FromString("A.B.1")));

		var parentsContainerCheckC = new GameplayTagContainer();
		parentsContainerCheckC.AddTag(GameplayTag.RequestGameplayTag(TagName.FromString("A")));
		parentsContainerCheckC.AddTag(GameplayTag.RequestGameplayTag(TagName.FromString("A.B")));
		parentsContainerCheckC.AddTag(GameplayTag.RequestGameplayTag(TagName.FromString("A.B.2")));

		var parentsContainerCheckD = new GameplayTagContainer();
		parentsContainerCheckD.AddTag(GameplayTag.RequestGameplayTag(TagName.FromString("A")));
		parentsContainerCheckD.AddTag(GameplayTag.RequestGameplayTag(TagName.FromString("A.C")));
		parentsContainerCheckD.AddTag(GameplayTag.RequestGameplayTag(TagName.FromString("A.C.3")));

		var parentsContainerCheckE = new GameplayTagContainer();
		parentsContainerCheckE.AddTag(GameplayTag.RequestGameplayTag(TagName.FromString("Character")));
		parentsContainerCheckE.AddTag(GameplayTag.RequestGameplayTag(TagName.FromString("Character.Attributes")));
		parentsContainerCheckE.AddTag(GameplayTag.RequestGameplayTag(TagName.FromString("Character.Attributes.Dex")));

		Assert.IsTrue(parentsContainerA == parentsContainerCheckA);
		Assert.IsTrue(parentsContainerB == parentsContainerCheckB);
		Assert.IsTrue(parentsContainerC == parentsContainerCheckC);
		Assert.IsTrue(parentsContainerD == parentsContainerCheckD);
		Assert.IsTrue(parentsContainerE == parentsContainerCheckE);
	}

	[TestMethod]
	[TestCategory("GetGameplayTagParents")]
	public void Direct_parent_of_a_root_tag_should_be_a_SingleTag_container()
	{
		var tag = GameplayTag.RequestGameplayTag(TagName.FromString("A"));

		var directParent = tag.GetGameplayTagParents();

		Assert.IsTrue(directParent == tag.GetSingleTagContainer());
	}

#if DEBUG
	[TestMethod]
	[TestCategory("ParseParentTags")]
	public void Parse_parents_result_should_parse_parents_correctly()
	{
		var tag = GameplayTag.RequestGameplayTag(TagName.FromString("Character.Attributes.Dex"));

		var directParent = tag.ParseParentTags();

		var tagParent1 = GameplayTag.RequestGameplayTag(TagName.FromString("Character"));
		var tagParent2 = GameplayTag.RequestGameplayTag(TagName.FromString("Character.Attributes"));

		Assert.IsTrue(directParent.Contains(tagParent1));
		Assert.IsTrue(directParent.Contains(tagParent2));
		Assert.IsTrue(directParent.Count == 2);
	}

	[TestMethod]
	[TestCategory("ParseParentTags")]
	public void Parse_parents_result_of_a_root_tag_should_be_empty()
	{
		var tag = GameplayTag.RequestGameplayTag(TagName.FromString("A"));

		var directParent = tag.ParseParentTags();

		Assert.IsTrue(directParent.Count == 0);
	}
#endif

	[TestMethod]
	[TestCategory("MatchesTag")]
	public void Equal_tags_should_Match()
	{
		var tagA = GameplayTag.RequestGameplayTag(TagName.FromString("A.1"));
		var tagB = GameplayTag.RequestGameplayTag(TagName.FromString("A.1"));

		Assert.IsTrue(tagA.MatchesTag(tagB));
	}

	[TestMethod]
	[TestCategory("MatchesTag")]
	public void Valid_tags_should_Match_based_on_parent_nodes()
	{
		var tagA = GameplayTag.RequestGameplayTag(TagName.FromString("A.1"));
		var tagB = GameplayTag.RequestGameplayTag(TagName.FromString("A"));

		Assert.IsTrue(tagA.MatchesTag(tagB));
	}

	[TestMethod]
	[TestCategory("MatchesTag")]
	public void Valid_tags_should_not_Match_tags_with_aditional_child_nodes()
	{
		var tagA = GameplayTag.RequestGameplayTag(TagName.FromString("A"));
		var tagB = GameplayTag.RequestGameplayTag(TagName.FromString("A.1"));

		Assert.IsFalse(tagA.MatchesTag(tagB));
	}

	[TestMethod]
	[TestCategory("MatchesTag")]
	public void Valid_tags_should_not_Match_tags_based_on_child_nodes_only()
	{
		var tagA = GameplayTag.RequestGameplayTag(TagName.FromString("A.1"));
		var tagB = GameplayTag.RequestGameplayTag(TagName.FromString("1"));

		Assert.IsFalse(tagA.MatchesTag(tagB));
	}

	[TestMethod]
	[TestCategory("MatchesTag")]
	public void Invalid_tags_should_never_Match()
	{
		var tagA = GameplayTag.RequestGameplayTag(TagName.FromString("Foo.Bar"), false);
		var tagB = GameplayTag.RequestGameplayTag(TagName.FromString("Foo"), false);

		Assert.IsFalse(tagA.MatchesTag(tagB));
	}

	[TestMethod]
	[TestCategory("MatchesTag")]
	public void Invalid_tags_should_never_Match_even_if_equals()
	{
		var tagA = GameplayTag.RequestGameplayTag(TagName.FromString("Foo.Bar"), false);
		var tagB = GameplayTag.RequestGameplayTag(TagName.FromString("Foo.Bar"), false);

		Assert.IsFalse(tagA.MatchesTag(tagB));
	}

	[TestMethod]
	[TestCategory("MatchesTag")]
	public void Valid_tags_can_Match_non_explicitly_registered_parents()
	{
		var tagA = GameplayTag.RequestGameplayTag(TagName.FromString("B.1"));
		var tagB = GameplayTag.RequestGameplayTag(TagName.FromString("B"));

		Assert.IsTrue(tagA.MatchesTag(tagB));
	}

	[TestMethod]
	[TestCategory("MatchesTag")]
	public void EmptyTag_should_never_Match_anything()
	{
		var tagA = GameplayTag.EmptyTag;
		var tagB = GameplayTag.RequestGameplayTag(TagName.FromString("B"));

		Assert.IsFalse(tagA.MatchesTag(tagB));
	}

	[TestMethod]
	[TestCategory("MatchesTag")]
	public void No_tag_should_Match_EmptyTag()
	{
		var tagA = GameplayTag.RequestGameplayTag(TagName.FromString("A.1"));
		var tagB = GameplayTag.EmptyTag;

		Assert.IsFalse(tagA.MatchesTag(tagB));
	}

	[TestMethod]
	[TestCategory("MatchesTag")]
	public void EmptyTag_should_not_Match_another_EmptyTag()
	{
		var tagA = GameplayTag.EmptyTag;
		var tagB = GameplayTag.EmptyTag;

		Assert.IsFalse(tagA.MatchesTag(tagB));
	}

	[TestMethod]
	[TestCategory("MatchesTagExact")]
	public void Equal_tags_should_MatchExact()
	{
		var tagA = GameplayTag.RequestGameplayTag(TagName.FromString("A.1"));
		var tagB = GameplayTag.RequestGameplayTag(TagName.FromString("A.1"));

		Assert.IsTrue(tagA.MatchesTagExact(tagB));
	}

	[TestMethod]
	[TestCategory("MatchesTagExact")]
	public void Valid_tags_should_not_MatchExact_based_on_parent_nodes()
	{
		var tagA = GameplayTag.RequestGameplayTag(TagName.FromString("A.1"));
		var tagB = GameplayTag.RequestGameplayTag(TagName.FromString("A"));

		Assert.IsFalse(tagA.MatchesTagExact(tagB));
	}

	[TestMethod]
	[TestCategory("MatchesTagExact")]
	public void Valid_tags_should_not_MatchExact_tags_with_aditional_child_nodes()
	{
		var tagA = GameplayTag.RequestGameplayTag(TagName.FromString("A"));
		var tagB = GameplayTag.RequestGameplayTag(TagName.FromString("A.1"));

		Assert.IsFalse(tagA.MatchesTagExact(tagB));
	}

	[TestMethod]
	[TestCategory("MatchesTagExact")]
	public void Valid_tags_should_not_MatchExact_tags_based_on_child_nodes_only()
	{
		var tagA = GameplayTag.RequestGameplayTag(TagName.FromString("A.1"));
		var tagB = GameplayTag.RequestGameplayTag(TagName.FromString("1"));

		Assert.IsFalse(tagA.MatchesTagExact(tagB));
	}

	[TestMethod]
	[TestCategory("MatchesTagExact")]
	public void Invalid_tags_should_never_MatchExact()
	{
		var tagA = GameplayTag.RequestGameplayTag(TagName.FromString("Foo.Bar"), false);
		var tagB = GameplayTag.RequestGameplayTag(TagName.FromString("Foo"), false);

		Assert.IsFalse(tagA.MatchesTagExact(tagB));
	}

	[TestMethod]
	[TestCategory("MatchesTagExact")]
	public void Invalid_tags_should_never_MatchExact_even_if_equals()
	{
		var tagA = GameplayTag.RequestGameplayTag(TagName.FromString("Foo.Bar"), false);
		var tagB = GameplayTag.RequestGameplayTag(TagName.FromString("Foo.Bar"), false);

		Assert.IsFalse(tagA.MatchesTagExact(tagB));
	}

	[TestMethod]
	[TestCategory("MatchesTagExact")]
	public void Valid_tags_should_not_MatchExact_non_explicitly_registered_parents()
	{
		var tagA = GameplayTag.RequestGameplayTag(TagName.FromString("B.1"));
		var tagB = GameplayTag.RequestGameplayTag(TagName.FromString("B"));

		Assert.IsFalse(tagA.MatchesTagExact(tagB));
	}

	[TestMethod]
	[TestCategory("MatchesTagExact")]
	public void EmptyTag_should_never_MatchExact_anything()
	{
		var tagA = GameplayTag.EmptyTag;
		var tagB = GameplayTag.RequestGameplayTag(TagName.FromString("A.1"));

		Assert.IsFalse(tagA.MatchesTagExact(tagB));
	}

	[TestMethod]
	[TestCategory("MatchesTagExact")]
	public void No_tag_should_MatchExact_EmptyTag()
	{
		var tagA = GameplayTag.RequestGameplayTag(TagName.FromString("A.1"));
		var tagB = GameplayTag.EmptyTag;

		Assert.IsFalse(tagA.MatchesTagExact(tagB));
	}

	[TestMethod]
	[TestCategory("MatchesTagExact")]
	public void EmptyTag_should_not_MatchExact_another_EmptyTag()
	{
		var tagA = GameplayTag.EmptyTag;
		var tagB = GameplayTag.EmptyTag;

		Assert.IsFalse(tagA.MatchesTagExact(tagB));
	}

	[TestMethod]
	[TestCategory("MatchesAny")]
	public void Valid_tags_should_MatchAny_container_containing_itself()
	{
		var tag = GameplayTag.RequestGameplayTag(TagName.FromString("A.1"));

		var tagContainer = new GameplayTagContainer();
		tagContainer.AddTag(GameplayTag.RequestGameplayTag(TagName.FromString("A.1")));
		tagContainer.AddTag(GameplayTag.RequestGameplayTag(TagName.FromString("B")));

		Assert.IsTrue(tag.MatchesAny(tagContainer));
	}

	[TestMethod]
	[TestCategory("MatchesAny")]
	public void Valid_tags_should_MatchAny_their_SingleTag_containers()
	{
		var tag = GameplayTag.RequestGameplayTag(TagName.FromString("A.1"));

		Assert.IsTrue(tag.MatchesAny(tag.GetSingleTagContainer()));
	}

	[TestMethod]
	[TestCategory("MatchesAny")]
	public void Valid_tags_should_MatchAny_container_with_parents_tags_only()
	{
		var tag = GameplayTag.RequestGameplayTag(TagName.FromString("A.1"));

		var tagContainer = new GameplayTagContainer();
		tagContainer.AddTag(GameplayTag.RequestGameplayTag(TagName.FromString("A")));
		tagContainer.AddTag(GameplayTag.RequestGameplayTag(TagName.FromString("B")));

		Assert.IsTrue(tag.MatchesAny(tagContainer));
	}

	[TestMethod]
	[TestCategory("MatchesAny")]
	public void Valid_tags_should_not_MatchAny_container_with_tags_with_adicional_child_nodes()
	{
		var tag = GameplayTag.RequestGameplayTag(TagName.FromString("A"));

		var tagContainer = new GameplayTagContainer();
		tagContainer.AddTag(GameplayTag.RequestGameplayTag(TagName.FromString("A.1")));
		tagContainer.AddTag(GameplayTag.RequestGameplayTag(TagName.FromString("B")));

		Assert.IsFalse(tag.MatchesAny(tagContainer));
	}

	[TestMethod]
	[TestCategory("MatchesAny")]
	public void Invalid_tags_should_never_MatchAny_container()
	{
		var tag = GameplayTag.RequestGameplayTag(TagName.FromString("Foo.Bar"), false);

		var tagContainer = new GameplayTagContainer();
		tagContainer.AddTag(GameplayTag.RequestGameplayTag(TagName.FromString("Foo.Bar"), false));
		tagContainer.AddTag(GameplayTag.RequestGameplayTag(TagName.FromString("Foo"), false));

		Assert.IsFalse(tag.MatchesAny(tagContainer));
	}

	[TestMethod]
	[TestCategory("MatchesAny")]
	public void Valid_tags_should_MatchAny_containers_with_non_explicitly_registered_parents()
	{
		var tag = GameplayTag.RequestGameplayTag(TagName.FromString("B.1"));

		var tagContainer = new GameplayTagContainer();
		tagContainer.AddTag(GameplayTag.RequestGameplayTag(TagName.FromString("B"), false));
		tagContainer.AddTag(GameplayTag.RequestGameplayTag(TagName.FromString("Foo"), false));

		Assert.IsTrue(tag.MatchesAny(tagContainer));
	}

	[TestMethod]
	[TestCategory("MatchesAny")]
	public void EmptyTag_should_never_MatchAny_container()
	{
		var tagA = GameplayTag.EmptyTag;

		var tagContainer = new GameplayTagContainer();
		tagContainer.AddTag(GameplayTag.RequestGameplayTag(TagName.FromString("A.1")));
		tagContainer.AddTag(GameplayTag.RequestGameplayTag(TagName.FromString("B.1")));

		Assert.IsFalse(tagA.MatchesAny(tagContainer));
	}

	[TestMethod]
	[TestCategory("MatchesAny")]
	public void No_tag_should_MatchAny_EmptyContainer()
	{
		var tag = GameplayTag.RequestGameplayTag(TagName.FromString("A.1"));

		Assert.IsFalse(tag.MatchesAny(GameplayTagContainer.EmptyContainer));
	}

	[TestMethod]
	[TestCategory("MatchesAnyExact")]
	public void Valid_tags_should_MatchAnyExact_container_containing_itself()
	{
		var tag = GameplayTag.RequestGameplayTag(TagName.FromString("A.1"));

		var tagContainer = new GameplayTagContainer();
		tagContainer.AddTag(GameplayTag.RequestGameplayTag(TagName.FromString("A.1")));
		tagContainer.AddTag(GameplayTag.RequestGameplayTag(TagName.FromString("B")));

		Assert.IsTrue(tag.MatchesAnyExact(tagContainer));
	}

	[TestMethod]
	[TestCategory("MatchesAnyExact")]
	public void Valid_tags_should_MatchAnyExact_their_SingleTag_containers()
	{
		var tag = GameplayTag.RequestGameplayTag(TagName.FromString("A.1"));

		Assert.IsTrue(tag.MatchesAnyExact(tag.GetSingleTagContainer()));
	}

	[TestMethod]
	[TestCategory("MatchesAnyExact")]
	public void Valid_tags_should_not_MatchAnyExact_container_with_parents_tags_only()
	{
		var tag = GameplayTag.RequestGameplayTag(TagName.FromString("A.1"));

		var tagContainer = new GameplayTagContainer();
		tagContainer.AddTag(GameplayTag.RequestGameplayTag(TagName.FromString("A")));
		tagContainer.AddTag(GameplayTag.RequestGameplayTag(TagName.FromString("B")));

		Assert.IsFalse(tag.MatchesAnyExact(tagContainer));
	}

	[TestMethod]
	[TestCategory("MatchesAnyExact")]
	public void Valid_tags_should_not_MatchAnyExact_container_with_tags_with_adicional_child_nodes()
	{
		var tag = GameplayTag.RequestGameplayTag(TagName.FromString("A"));

		var tagContainer = new GameplayTagContainer();
		tagContainer.AddTag(GameplayTag.RequestGameplayTag(TagName.FromString("A.1")));
		tagContainer.AddTag(GameplayTag.RequestGameplayTag(TagName.FromString("B")));

		Assert.IsFalse(tag.MatchesAnyExact(tagContainer));
	}

	[TestMethod]
	[TestCategory("MatchesAnyExact")]
	public void Invalid_tags_should_never_MatchAnyExact_container()
	{
		var tag = GameplayTag.RequestGameplayTag(TagName.FromString("Foo.Bar"), false);

		var tagContainer = new GameplayTagContainer();
		tagContainer.AddTag(GameplayTag.RequestGameplayTag(TagName.FromString("Foo.Bar"), false));
		tagContainer.AddTag(GameplayTag.RequestGameplayTag(TagName.FromString("Foo"), false));

		Assert.IsFalse(tag.MatchesAnyExact(tagContainer));
	}

	[TestMethod]
	[TestCategory("MatchesAnyExact")]
	public void EmptyTag_should_never_MatchAnyExact_container()
	{
		var tagA = GameplayTag.EmptyTag;

		var tagContainer = new GameplayTagContainer();
		tagContainer.AddTag(GameplayTag.RequestGameplayTag(TagName.FromString("A.1")));
		tagContainer.AddTag(GameplayTag.RequestGameplayTag(TagName.FromString("B.1")));

		Assert.IsFalse(tagA.MatchesAnyExact(tagContainer));
	}

	[TestMethod]
	[TestCategory("MatchesAnyExact")]
	public void No_tag_should_MatchAnyExact_EmptyContainer()
	{
		var tag = GameplayTag.RequestGameplayTag(TagName.FromString("A.1"));

		Assert.IsFalse(tag.MatchesAnyExact(GameplayTagContainer.EmptyContainer));
	}

	[TestMethod]
	[TestCategory("MatchesTagDepth")]
	public void Tags_should_match_depth()
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
	[TestCategory("MatchesTagDepth")]
	public void EmptyTag_should_always_match_depth_zero()
	{
		var tagA = GameplayTag.RequestGameplayTag(TagName.FromString("A.1"));
		var tagB = GameplayTag.RequestGameplayTag(TagName.FromString("A.B.1"));
		var tagC = GameplayTag.RequestGameplayTag(TagName.FromString("A.B.2"));
		var tagD = GameplayTag.RequestGameplayTag(TagName.FromString("A.C.3"));
		var tagE = GameplayTag.RequestGameplayTag(TagName.FromString("Character.Attributes.Dex"));

		Assert.IsTrue(GameplayTag.EmptyTag.MatchesTagDepth(GameplayTag.EmptyTag) == 0);

		Assert.IsTrue(tagA.MatchesTagDepth(GameplayTag.EmptyTag) == 0);
		Assert.IsTrue(tagB.MatchesTagDepth(GameplayTag.EmptyTag) == 0);
		Assert.IsTrue(tagC.MatchesTagDepth(GameplayTag.EmptyTag) == 0);
		Assert.IsTrue(tagD.MatchesTagDepth(GameplayTag.EmptyTag) == 0);
		Assert.IsTrue(tagE.MatchesTagDepth(GameplayTag.EmptyTag) == 0);

		Assert.IsTrue(GameplayTag.EmptyTag.MatchesTagDepth(tagA) == 0);
		Assert.IsTrue(GameplayTag.EmptyTag.MatchesTagDepth(tagB) == 0);
		Assert.IsTrue(GameplayTag.EmptyTag.MatchesTagDepth(tagC) == 0);
		Assert.IsTrue(GameplayTag.EmptyTag.MatchesTagDepth(tagD) == 0);
		Assert.IsTrue(GameplayTag.EmptyTag.MatchesTagDepth(tagE) == 0);
	}

	[TestMethod]
	[TestCategory("Equality")]
	public void Tags_are_equatable()
	{
		var tagA = GameplayTag.RequestGameplayTag(TagName.FromString("A.1"));
		var tagB = GameplayTag.RequestGameplayTag(TagName.FromString("B.1"));
		var tagC = GameplayTag.RequestGameplayTag(TagName.FromString("a.1"));

		Assert.AreEqual(tagA, tagC);
		Assert.AreNotEqual(tagA, tagB);
		Assert.AreNotEqual(tagB, tagC);

		Assert.IsTrue(tagA != tagB);
		Assert.IsTrue(tagB != tagC);
		Assert.IsTrue(tagA == tagC);
		Assert.IsTrue(tagA.Equals(tagC));

		object tagObjectA = tagA;
		object tagObjectB = tagB;
		object tagObjectC = tagC;

		Assert.IsTrue(tagA.Equals(tagObjectA));
		Assert.IsTrue(tagA.Equals(tagObjectC));
		Assert.IsTrue(tagObjectA.Equals(tagObjectC));

		Assert.IsTrue((GameplayTag)tagObjectA == (GameplayTag)tagObjectC);

		// Those are defaul C# object == and !=, not the overriden ones.
		// So they should be considered different objects
		Assert.IsTrue(tagObjectA != tagObjectB);
		Assert.IsTrue(tagObjectA != tagObjectC);
		Assert.IsFalse(tagObjectA == tagObjectB);
		Assert.IsFalse(tagObjectA == tagObjectC);
	}

	[TestMethod]
	[TestCategory("Equality")]
	public void TagName_from_empty_string_is_EmptyTag()
	{
		var tagName1 = TagName.FromString(string.Empty);

		Assert.IsTrue(tagName1 == TagName.Empty);
	}

	[TestMethod]
	[TestCategory("ToString")]
	public void Tag_ToString_returns_lowercase_string()
	{
		var tagA = GameplayTag.RequestGameplayTag(TagName.FromString("A.1"));
		var tagB = GameplayTag.RequestGameplayTag(TagName.FromString("Character.Attributes.Dex"));

		Assert.IsTrue(string.Compare(tagA.ToString(), "a.1", false) == 0);
		Assert.IsTrue(string.Compare(tagB.ToString(), "character.attributes.dex", false) == 0);
	}

	[TestMethod]
	[TestCategory("ToString")]
	public void EmptyTag_ToString_returns_empty_string()
	{
		Assert.IsTrue(string.Compare(GameplayTag.EmptyTag.ToString(), string.Empty) == 0);
	}
}
