#pragma warning disable SA1600 // Elements should be documented
namespace GameplayTags.Runtime.Tests;

[TestClass]
public class GameplayTagContainerTests
{
	[TestMethod]
	[TestCategory("Constructor")]
	public void Freashly_created_container_should_be_empty_and_invalid()
	{
		var tagContainer = new GameplayTagContainer();

		Assert.IsTrue(tagContainer.IsEmpty);
		Assert.IsFalse(tagContainer.IsValid);
		Assert.IsTrue(tagContainer == GameplayTagContainer.EmptyContainer);
	}

	[TestMethod]
	[TestCategory("Constructor")]
	public void Container_created_with_tag_should_contain_given_tag()
	{
		var tag = GameplayTag.RequestGameplayTag(TagName.FromString("A.1"));
		var tagContainer = new GameplayTagContainer(tag);

		Assert.IsFalse(tagContainer.IsEmpty);
		Assert.IsTrue(tagContainer.IsValid);
		Assert.IsTrue(tagContainer != GameplayTagContainer.EmptyContainer);

		Assert.IsTrue(tagContainer.HasTag(tag));
	}

	[TestMethod]
	[TestCategory("Constructor")]
	public void Container_created_from_another_container_should_be_equal()
	{
		var tag = GameplayTag.RequestGameplayTag(TagName.FromString("A.1"));
		var tagContainer = new GameplayTagContainer(tag);

		var cloneContainer = new GameplayTagContainer(tagContainer);

		Assert.IsTrue(tagContainer == cloneContainer);

		var cloneEmptyContainer = new GameplayTagContainer(GameplayTagContainer.EmptyContainer);

		Assert.IsTrue(cloneEmptyContainer == GameplayTagContainer.EmptyContainer);
	}

	[TestMethod]
	[TestCategory("Constructor")]
	public void Container_created_with_tag_set_should_contain_given_tags()
	{
		var tagA = GameplayTag.RequestGameplayTag(TagName.FromString("A.B.1"));
		var tagB = GameplayTag.RequestGameplayTag(TagName.FromString("A.B.2"));
		var tagC = GameplayTag.RequestGameplayTag(TagName.FromString("A.B.3"));

		var tagSet = new HashSet<GameplayTag>() { tagA, tagB, tagC };
		var container = new GameplayTagContainer(tagSet);

		Assert.IsFalse(container.IsEmpty);
		Assert.IsTrue(container.IsValid);
		Assert.IsTrue(container.Count == 3);

		Assert.IsTrue(container.HasTag(tagA));
		Assert.IsTrue(container.HasTag(tagB));
		Assert.IsTrue(container.HasTag(tagC));
	}

	[TestMethod]
	[TestCategory("Serialization")]
	public void Container_with_registered_tags_should_serialize_successfully()
	{
		var tagA = GameplayTag.RequestGameplayTag(TagName.FromString("A.1"));
		var tagB = GameplayTag.RequestGameplayTag(TagName.FromString("A.B.1"));
		var tagC = GameplayTag.RequestGameplayTag(TagName.FromString("A.B.2"));
		var tagD = GameplayTag.RequestGameplayTag(TagName.FromString("A.C.3"));
		var tagE = GameplayTag.RequestGameplayTag(TagName.FromString("Character.Attributes.Dex"));

		var tagSet = new HashSet<GameplayTag>() { tagA, tagB, tagC, tagD, tagE };
		var tagContainer = new GameplayTagContainer(tagSet);

		GameplayTagContainer.NetSerialize(tagContainer, out var containerStream);

		Assert.IsTrue(containerStream[0] == 0);
		Assert.IsTrue(containerStream.Length == 12);
		Assert.IsTrue(containerStream.SequenceEqual(new byte[] { 0, 5, 2, 0, 4, 0, 5, 0, 9, 0, 14, 0 }));
	}

	[TestMethod]
	[TestCategory("Serialization")]
	public void EmptyContainer_should_serialize_successfully()
	{
		var tagContainer = new GameplayTagContainer();

		GameplayTagContainer.NetSerialize(tagContainer, out var containerStream);

		Assert.IsTrue(containerStream[0] == 1);
		Assert.IsTrue(containerStream.Length == 1);
	}

	[TestMethod]
	[TestCategory("Serialization")]
	public void Container_should_deserialize_successfully()
	{
		Assert.IsTrue(GameplayTagContainer.NetDeserialize(
			[0, 5, 2, 0, 4, 0, 5, 0, 9, 0, 14, 0],
			out var deserializedContainer));

		var tagA = GameplayTag.RequestGameplayTag(TagName.FromString("A.1"));
		var tagB = GameplayTag.RequestGameplayTag(TagName.FromString("A.B.1"));
		var tagC = GameplayTag.RequestGameplayTag(TagName.FromString("A.B.2"));
		var tagD = GameplayTag.RequestGameplayTag(TagName.FromString("A.C.3"));
		var tagE = GameplayTag.RequestGameplayTag(TagName.FromString("Character.Attributes.Dex"));

		var tagSet = new HashSet<GameplayTag>() { tagA, tagB, tagC, tagD, tagE };
		var tagContainerCheck = new GameplayTagContainer(tagSet);

		Assert.IsTrue(deserializedContainer == tagContainerCheck);
	}

	[TestMethod]
	[TestCategory("Serialization")]
	public void EmptyContainer_should_deserialize_successfully()
	{
		Assert.IsTrue(GameplayTagContainer.NetDeserialize(
			[1],
			out var deserializedContainer));

		Assert.IsTrue(deserializedContainer == GameplayTagContainer.EmptyContainer);
	}

	[TestMethod]
	[TestCategory("Serialization")]
	public void Container_should_deserialize_successfully_ignoring_invalid_tags()
	{
		Assert.IsTrue(GameplayTagContainer.NetDeserialize(
			[0, 2, (byte)(GameplayTagsManager.Instance.NodesCount + 1), 0, 2, 0],
			out var deserializedContainer));

		var tagContainerCheck = new GameplayTagContainer();
		tagContainerCheck.AddTag(GameplayTag.RequestGameplayTag(TagName.FromString("A.1")));

		Assert.IsTrue(deserializedContainer == tagContainerCheck);
	}

	[TestMethod]
	[TestCategory("Serialization")]
	[ExpectedException(typeof(Exception), "Net index should never be higher than the specified invalid net index.")]
	public void Container_should_throw_exception_when_deserializing_higher_than_InvalidTagNetIndex_tags()
	{
		Assert.IsTrue(GameplayTagContainer.NetDeserialize(
			[0, 2, (byte)(GameplayTagsManager.Instance.NodesCount + 23), 0, 2, 0],
			out var _));
	}

	[TestMethod]
	[TestCategory("AddTag")]
	public void Container_should_not_add_EmptyTag()
	{
		var tag = GameplayTag.RequestGameplayTag(TagName.FromString("A.1"));
		var tagContainer = new GameplayTagContainer(tag);

		tagContainer.AddTag(GameplayTag.EmptyTag);

		Assert.IsFalse(tagContainer.HasTagExact(GameplayTag.EmptyTag));
		Assert.IsTrue(tagContainer.Count == 1);
	}

	[TestMethod]
	[TestCategory("RemoveTag")]
	public void Container_should_not_contain_tag_removed()
	{
		var tagA = GameplayTag.RequestGameplayTag(TagName.FromString("A.B.1"));
		var tagB = GameplayTag.RequestGameplayTag(TagName.FromString("A.B.2"));
		var tagC = GameplayTag.RequestGameplayTag(TagName.FromString("A.B.3"));

		var tagSet = new HashSet<GameplayTag>() { tagA, tagB, tagC };
		var tagContainer = new GameplayTagContainer(tagSet);

		tagContainer.RemoveTag(tagA);

		Assert.IsFalse(tagContainer.HasTagExact(tagA));
		Assert.IsTrue(tagContainer.HasTagExact(tagB));
		Assert.IsTrue(tagContainer.HasTagExact(tagC));
		Assert.IsTrue(tagContainer.Count == 2);
	}

	[TestMethod]
	[TestCategory("RemoveTag")]
	public void Container_should_not_contain_tags_removed()
	{
		var tagA = GameplayTag.RequestGameplayTag(TagName.FromString("A.B.1"));
		var tagB = GameplayTag.RequestGameplayTag(TagName.FromString("A.B.2"));
		var tagC = GameplayTag.RequestGameplayTag(TagName.FromString("A.B.3"));

		var tagSet = new HashSet<GameplayTag>() { tagA, tagB, tagC };
		var tagContainer = new GameplayTagContainer(tagSet);

		var removeTagsSet = new HashSet<GameplayTag>() { tagB, tagC };
		var removeTagsContainer = new GameplayTagContainer(removeTagsSet);

		tagContainer.RemoveTags(removeTagsContainer);

		Assert.IsTrue(tagContainer.HasTagExact(tagA));
		Assert.IsFalse(tagContainer.HasTagExact(tagB));
		Assert.IsFalse(tagContainer.HasTagExact(tagC));
		Assert.IsTrue(tagContainer.Count == 1);
	}

	[TestMethod]
	[TestCategory("Reset")]
	public void Container_reset_should_be_EmptyContainer()
	{
		var tagA = GameplayTag.RequestGameplayTag(TagName.FromString("A.B.1"));
		var tagB = GameplayTag.RequestGameplayTag(TagName.FromString("A.B.2"));
		var tagC = GameplayTag.RequestGameplayTag(TagName.FromString("A.B.3"));

		var tagSet = new HashSet<GameplayTag>() { tagA, tagB, tagC };
		var tagContainer = new GameplayTagContainer(tagSet);

		tagContainer.Reset(3);

		Assert.IsTrue(tagContainer == GameplayTagContainer.EmptyContainer);
		Assert.IsTrue(tagContainer.IsEmpty);
		Assert.IsFalse(tagContainer.IsValid);
		Assert.IsTrue(tagContainer.Count == 0);
	}

	[TestMethod]
	[TestCategory("HasTag")]
	public void Container_should_HaveTag_if_tag_is_exact_match()
	{
		var tagA = GameplayTag.RequestGameplayTag(TagName.FromString("A.1"));
		var tagB = GameplayTag.RequestGameplayTag(TagName.FromString("B.1"));

		var tagSet = new HashSet<GameplayTag>() { tagA, tagB };
		var tagContainer = new GameplayTagContainer(tagSet);

		var tag = GameplayTag.RequestGameplayTag(TagName.FromString("A.1"));

		Assert.IsTrue(tagContainer.HasTag(tag));
	}

	[TestMethod]
	[TestCategory("HasTag")]
	public void Container_should_HaveTag_based_on_parent_nodes()
	{
		var tagA = GameplayTag.RequestGameplayTag(TagName.FromString("A.1"));
		var tagB = GameplayTag.RequestGameplayTag(TagName.FromString("B.1"));

		var tagSet = new HashSet<GameplayTag>() { tagA, tagB };
		var tagContainer = new GameplayTagContainer(tagSet);

		var tag = GameplayTag.RequestGameplayTag(TagName.FromString("A"));

		Assert.IsTrue(tagContainer.HasTag(tag));
	}

	[TestMethod]
	[TestCategory("HasTag")]
	public void Container_should_not_HaveTag_based_on_aditional_child_nodes()
	{
		var tagA = GameplayTag.RequestGameplayTag(TagName.FromString("A"));
		var tagB = GameplayTag.RequestGameplayTag(TagName.FromString("B.1"));

		var tagSet = new HashSet<GameplayTag>() { tagA, tagB };
		var tagContainer = new GameplayTagContainer(tagSet);

		var tag = GameplayTag.RequestGameplayTag(TagName.FromString("A.1"));

		Assert.IsFalse(tagContainer.HasTag(tag));
	}

	[TestMethod]
	[TestCategory("HasTag")]
	public void Container_should_not_HaveTag_EmptyTag()
	{
		var tagA = GameplayTag.RequestGameplayTag(TagName.FromString("A.1"));
		var tagB = GameplayTag.RequestGameplayTag(TagName.FromString("B.1"));

		var tagSet = new HashSet<GameplayTag>() { tagA, tagB };
		var tagContainer = new GameplayTagContainer(tagSet);

		Assert.IsFalse(tagContainer.HasTag(GameplayTag.EmptyTag));
	}

	[TestMethod]
	[TestCategory("HasTag")]
	public void EmptyContainer_should_never_HaveTag()
	{
		var tagA = GameplayTag.RequestGameplayTag(TagName.FromString("A.1"));
		var tagB = GameplayTag.RequestGameplayTag(TagName.FromString("B.1"));

		Assert.IsFalse(GameplayTagContainer.EmptyContainer.HasTag(tagA));
		Assert.IsFalse(GameplayTagContainer.EmptyContainer.HasTag(tagB));
		Assert.IsFalse(GameplayTagContainer.EmptyContainer.HasTag(GameplayTag.EmptyTag));
	}

	[TestMethod]
	[TestCategory("HasTagExact")]
	public void Container_should_HaveTagExact_if_tag_is_exact_match()
	{
		var tagA = GameplayTag.RequestGameplayTag(TagName.FromString("A.1"));
		var tagB = GameplayTag.RequestGameplayTag(TagName.FromString("B.1"));

		var tagSet = new HashSet<GameplayTag>() { tagA, tagB };
		var tagContainer = new GameplayTagContainer(tagSet);

		var tag = GameplayTag.RequestGameplayTag(TagName.FromString("A.1"));

		Assert.IsTrue(tagContainer.HasTagExact(tag));
	}

	[TestMethod]
	[TestCategory("HasTagExact")]
	public void Container_should_not_HaveTagExact_based_on_parent_nodes()
	{
		var tagA = GameplayTag.RequestGameplayTag(TagName.FromString("A.1"));
		var tagB = GameplayTag.RequestGameplayTag(TagName.FromString("B.1"));

		var tagSet = new HashSet<GameplayTag>() { tagA, tagB };
		var tagContainer = new GameplayTagContainer(tagSet);

		var tag = GameplayTag.RequestGameplayTag(TagName.FromString("A"));

		Assert.IsFalse(tagContainer.HasTagExact(tag));
	}

	[TestMethod]
	[TestCategory("HasTagExact")]
	public void Container_should_not_HaveTagExact_based_on_aditional_child_nodes()
	{
		var tagA = GameplayTag.RequestGameplayTag(TagName.FromString("A"));
		var tagB = GameplayTag.RequestGameplayTag(TagName.FromString("B.1"));

		var tagSet = new HashSet<GameplayTag>() { tagA, tagB };
		var tagContainer = new GameplayTagContainer(tagSet);

		var tag = GameplayTag.RequestGameplayTag(TagName.FromString("A.1"));

		Assert.IsFalse(tagContainer.HasTagExact(tag));
	}

	[TestMethod]
	[TestCategory("HasTagExact")]
	public void Container_should_not_HaveTagExact_EmptyTag()
	{
		var tagA = GameplayTag.RequestGameplayTag(TagName.FromString("A.1"));
		var tagB = GameplayTag.RequestGameplayTag(TagName.FromString("B.1"));

		var tagSet = new HashSet<GameplayTag>() { tagA, tagB };
		var tagContainer = new GameplayTagContainer(tagSet);

		Assert.IsFalse(tagContainer.HasTagExact(GameplayTag.EmptyTag));
	}

	[TestMethod]
	[TestCategory("HasTagExact")]
	public void EmptyContainer_should_never_HaveTagExact()
	{
		var tagA = GameplayTag.RequestGameplayTag(TagName.FromString("A.1"));
		var tagB = GameplayTag.RequestGameplayTag(TagName.FromString("B.1"));

		Assert.IsFalse(GameplayTagContainer.EmptyContainer.HasTagExact(tagA));
		Assert.IsFalse(GameplayTagContainer.EmptyContainer.HasTagExact(tagB));
		Assert.IsFalse(GameplayTagContainer.EmptyContainer.HasTagExact(GameplayTag.EmptyTag));
	}

	[TestMethod]
	[TestCategory("HasAny")]
	public void Container_A_should_HaveAny_container_B_tag()
	{
		var tagA1 = GameplayTag.RequestGameplayTag(TagName.FromString("A.1"));
		var tagContainerA = new GameplayTagContainer(tagA1);

		var tagB1 = GameplayTag.RequestGameplayTag(TagName.FromString("A"));
		var tagB2 = GameplayTag.RequestGameplayTag(TagName.FromString("B"));

		var tagSet = new HashSet<GameplayTag>() { tagB1, tagB2 };
		var tagContainerB = new GameplayTagContainer(tagSet);

		Assert.IsTrue(tagContainerA.HasAny(tagContainerB));
	}

	[TestMethod]
	[TestCategory("HasAny")]
	public void Container_A_should_not_HaveAny_container_B_tag()
	{
		var tagA1 = GameplayTag.RequestGameplayTag(TagName.FromString("A"));
		var tagContainerA = new GameplayTagContainer(tagA1);

		var tagB1 = GameplayTag.RequestGameplayTag(TagName.FromString("A.1"));
		var tagB2 = GameplayTag.RequestGameplayTag(TagName.FromString("B"));

		var tagSet = new HashSet<GameplayTag>() { tagB1, tagB2 };
		var tagContainerB = new GameplayTagContainer(tagSet);

		Assert.IsFalse(tagContainerA.HasAny(tagContainerB));
	}

	[TestMethod]
	[TestCategory("HasAny")]
	public void Container_A_should_not_HaveAny_EmptyContainer()
	{
		var tagA = GameplayTag.RequestGameplayTag(TagName.FromString("A.1"));
		var tagB = GameplayTag.RequestGameplayTag(TagName.FromString("B.1"));

		var tagSet = new HashSet<GameplayTag>() { tagA, tagB };
		var tagContainer = new GameplayTagContainer(tagSet);

		Assert.IsFalse(tagContainer.HasAny(GameplayTagContainer.EmptyContainer));
	}

	[TestMethod]
	[TestCategory("HasAny")]
	public void EmptyContainer_should_not_HaveAny_container_A_tags()
	{
		var tagA = GameplayTag.RequestGameplayTag(TagName.FromString("A.1"));
		var tagB = GameplayTag.RequestGameplayTag(TagName.FromString("B.1"));

		var tagSet = new HashSet<GameplayTag>() { tagA, tagB };
		var tagContainer = new GameplayTagContainer(tagSet);

		Assert.IsFalse(GameplayTagContainer.EmptyContainer.HasAny(tagContainer));
	}

	[TestMethod]
	[TestCategory("HasAnyExact")]
	public void Container_A_should_HaveAnyExact_container_B_tag_exact()
	{
		var tagA1 = GameplayTag.RequestGameplayTag(TagName.FromString("A.1"));
		var tagContainerA = new GameplayTagContainer(tagA1);

		var tagB1 = GameplayTag.RequestGameplayTag(TagName.FromString("A.1"));
		var tagB2 = GameplayTag.RequestGameplayTag(TagName.FromString("B"));

		var tagSet = new HashSet<GameplayTag>() { tagB1, tagB2 };
		var tagContainerB = new GameplayTagContainer(tagSet);

		Assert.IsTrue(tagContainerA.HasAnyExact(tagContainerB));
	}

	[TestMethod]
	[TestCategory("HasAnyExact")]
	public void Container_A_should_not_HaveAnyExact_container_B_tag_exact()
	{
		var tagA1 = GameplayTag.RequestGameplayTag(TagName.FromString("A.1"));
		var tagContainerA = new GameplayTagContainer(tagA1);

		var tagB1 = GameplayTag.RequestGameplayTag(TagName.FromString("A"));
		var tagB2 = GameplayTag.RequestGameplayTag(TagName.FromString("B"));

		var tagSet = new HashSet<GameplayTag>() { tagB1, tagB2 };
		var tagContainerB = new GameplayTagContainer(tagSet);

		Assert.IsFalse(tagContainerA.HasAnyExact(tagContainerB));
	}

	[TestMethod]
	[TestCategory("HasAnyExact")]
	public void Container_A_should_not_HaveAnyExact_EmptyContainer()
	{
		var tagA = GameplayTag.RequestGameplayTag(TagName.FromString("A.1"));
		var tagB = GameplayTag.RequestGameplayTag(TagName.FromString("B.1"));

		var tagSet = new HashSet<GameplayTag>() { tagA, tagB };
		var tagContainer = new GameplayTagContainer(tagSet);

		Assert.IsFalse(tagContainer.HasAnyExact(GameplayTagContainer.EmptyContainer));
	}

	[TestMethod]
	[TestCategory("HasAnyExact")]
	public void EmptyContainer_should_not_HaveAnyExact_container_A_tags()
	{
		var tagA = GameplayTag.RequestGameplayTag(TagName.FromString("A.1"));
		var tagB = GameplayTag.RequestGameplayTag(TagName.FromString("B.1"));

		var tagSet = new HashSet<GameplayTag>() { tagA, tagB };
		var tagContainer = new GameplayTagContainer(tagSet);

		Assert.IsFalse(GameplayTagContainer.EmptyContainer.HasAnyExact(tagContainer));
	}

	[TestMethod]
	[TestCategory("HasAll")]
	public void Container_A_should_HaveAll_container_B_tags()
	{
		var tagA1 = GameplayTag.RequestGameplayTag(TagName.FromString("A.1"));
		var tagA2 = GameplayTag.RequestGameplayTag(TagName.FromString("B.1"));

		var tagSetA = new HashSet<GameplayTag>() { tagA1, tagA2 };
		var tagContainerA = new GameplayTagContainer(tagSetA);

		var tagB1 = GameplayTag.RequestGameplayTag(TagName.FromString("A"));
		var tagB2 = GameplayTag.RequestGameplayTag(TagName.FromString("B"));

		var tagSetB = new HashSet<GameplayTag>() { tagB1, tagB2 };
		var tagContainerB = new GameplayTagContainer(tagSetB);

		Assert.IsTrue(tagContainerA.HasAll(tagContainerB));
	}

	[TestMethod]
	[TestCategory("HasAll")]
	public void Container_A_should_not_HaveAll_container_B_tags()
	{
		var tagA1 = GameplayTag.RequestGameplayTag(TagName.FromString("A"));
		var tagA2 = GameplayTag.RequestGameplayTag(TagName.FromString("B"));

		var tagSetA = new HashSet<GameplayTag>() { tagA1, tagA2 };
		var tagContainerA = new GameplayTagContainer(tagSetA);

		var tagB1 = GameplayTag.RequestGameplayTag(TagName.FromString("A.1"));
		var tagB2 = GameplayTag.RequestGameplayTag(TagName.FromString("B"));

		var tagSetB = new HashSet<GameplayTag>() { tagB1, tagB2 };
		var tagContainerB = new GameplayTagContainer(tagSetB);

		Assert.IsFalse(tagContainerA.HasAll(tagContainerB));
	}

	[TestMethod]
	[TestCategory("HasAll")]
	public void Container_A_should_HaveAll_tags_from_EmptyContainer()
	{
		var tagA = GameplayTag.RequestGameplayTag(TagName.FromString("A.1"));
		var tagB = GameplayTag.RequestGameplayTag(TagName.FromString("B.1"));

		var tagSet = new HashSet<GameplayTag>() { tagA, tagB };
		var tagContainer = new GameplayTagContainer(tagSet);

		Assert.IsTrue(tagContainer.HasAll(GameplayTagContainer.EmptyContainer));
	}

	[TestMethod]
	[TestCategory("HasAll")]
	public void EmptyContainer_should_not_HaveAll_container_A_tags()
	{
		var tagA = GameplayTag.RequestGameplayTag(TagName.FromString("A.1"));
		var tagB = GameplayTag.RequestGameplayTag(TagName.FromString("B.1"));

		var tagSet = new HashSet<GameplayTag>() { tagA, tagB };
		var tagContainer = new GameplayTagContainer(tagSet);

		Assert.IsFalse(GameplayTagContainer.EmptyContainer.HasAll(tagContainer));
	}

	[TestMethod]
	[TestCategory("HasAllExact")]
	public void Container_A_should_HaveAllExact_container_B_tags_exact()
	{
		var tagA1 = GameplayTag.RequestGameplayTag(TagName.FromString("A.B.1"));
		var tagA2 = GameplayTag.RequestGameplayTag(TagName.FromString("A.1"));
		var tagA3 = GameplayTag.RequestGameplayTag(TagName.FromString("B.1"));

		var tagSetA = new HashSet<GameplayTag>() { tagA1, tagA2, tagA3 };
		var tagContainerA = new GameplayTagContainer(tagSetA);

		var tagB1 = GameplayTag.RequestGameplayTag(TagName.FromString("A.1"));
		var tagB2 = GameplayTag.RequestGameplayTag(TagName.FromString("B.1"));

		var tagSetB = new HashSet<GameplayTag>() { tagB1, tagB2 };
		var tagContainerB = new GameplayTagContainer(tagSetB);

		Assert.IsTrue(tagContainerA.HasAllExact(tagContainerB));
	}

	[TestMethod]
	[TestCategory("HasAllExact")]
	public void Container_A_should_not_HaveAllExact_container_B_tags_exact()
	{
		var tagA1 = GameplayTag.RequestGameplayTag(TagName.FromString("A.B.1"));
		var tagA2 = GameplayTag.RequestGameplayTag(TagName.FromString("A.1"));
		var tagA3 = GameplayTag.RequestGameplayTag(TagName.FromString("B.1"));

		var tagSetA = new HashSet<GameplayTag>() { tagA1, tagA2, tagA3 };
		var tagContainerA = new GameplayTagContainer(tagSetA);

		var tagB1 = GameplayTag.RequestGameplayTag(TagName.FromString("A.1"));
		var tagB2 = GameplayTag.RequestGameplayTag(TagName.FromString("B"));

		var tagSetB = new HashSet<GameplayTag>() { tagB1, tagB2 };
		var tagContainerB = new GameplayTagContainer(tagSetB);

		Assert.IsFalse(tagContainerA.HasAllExact(tagContainerB));
	}

	[TestMethod]
	[TestCategory("HasAllExact")]
	public void Container_A_should_HaveAllExact_tags_from_EmptyContainer()
	{
		var tagA = GameplayTag.RequestGameplayTag(TagName.FromString("A.1"));
		var tagB = GameplayTag.RequestGameplayTag(TagName.FromString("B.1"));

		var tagSet = new HashSet<GameplayTag>() { tagA, tagB };
		var tagContainer = new GameplayTagContainer(tagSet);

		Assert.IsTrue(tagContainer.HasAllExact(GameplayTagContainer.EmptyContainer));
	}

	[TestMethod]
	[TestCategory("HasAllExact")]
	public void EmptyContainer_should_not_HaveAllExact_container_A_tags()
	{
		var tagA = GameplayTag.RequestGameplayTag(TagName.FromString("A.1"));
		var tagB = GameplayTag.RequestGameplayTag(TagName.FromString("B.1"));

		var tagSet = new HashSet<GameplayTag>() { tagA, tagB };
		var tagContainer = new GameplayTagContainer(tagSet);

		Assert.IsFalse(GameplayTagContainer.EmptyContainer.HasAllExact(tagContainer));
	}

	[TestMethod]
	[TestCategory("Filter")]
	public void Container_A_Filter_container_B_should_have_tags_A1_and_B1_only()
	{
		var tagA1 = GameplayTag.RequestGameplayTag(TagName.FromString("A.1"));
		var tagA2 = GameplayTag.RequestGameplayTag(TagName.FromString("B.1"));
		var tagA3 = GameplayTag.RequestGameplayTag(TagName.FromString("1"));
		var tagA4 = GameplayTag.RequestGameplayTag(TagName.FromString("Color.Red"));

		var tagSetA = new HashSet<GameplayTag>() { tagA1, tagA2, tagA3, tagA4 };
		var tagContainerA = new GameplayTagContainer(tagSetA);

		var tagB1 = GameplayTag.RequestGameplayTag(TagName.FromString("A.1"));
		var tagB2 = GameplayTag.RequestGameplayTag(TagName.FromString("B"));

		var tagSetB = new HashSet<GameplayTag>() { tagB1, tagB2 };
		var tagContainerB = new GameplayTagContainer(tagSetB);

		var tagContainerC = tagContainerA.Filter(tagContainerB);

		var validationTagSetB = new HashSet<GameplayTag>() { tagA1, tagA2 };
		var validationContainer = new GameplayTagContainer(validationTagSetB);

		Assert.IsTrue(tagContainerC.HasAllExact(validationContainer));
		Assert.IsTrue(tagContainerC.Count == 2);
	}

	[TestMethod]
	[TestCategory("Filter")]
	public void Container_A_Filter_EmptyContainer_should_be_empty()
	{
		var tagA1 = GameplayTag.RequestGameplayTag(TagName.FromString("A.1"));
		var tagA2 = GameplayTag.RequestGameplayTag(TagName.FromString("B.1"));
		var tagA3 = GameplayTag.RequestGameplayTag(TagName.FromString("1"));
		var tagA4 = GameplayTag.RequestGameplayTag(TagName.FromString("Color.Red"));

		var tagSetA = new HashSet<GameplayTag>() { tagA1, tagA2, tagA3, tagA4 };
		var tagContainerA = new GameplayTagContainer(tagSetA);

		var tagContainerC = tagContainerA.Filter(GameplayTagContainer.EmptyContainer);

		Assert.IsTrue(tagContainerC.IsEmpty);
	}

	[TestMethod]
	[TestCategory("Filter")]
	public void EmptyContainer_Filter_container_A_should_be_empty()
	{
		var tagA1 = GameplayTag.RequestGameplayTag(TagName.FromString("A.1"));
		var tagA2 = GameplayTag.RequestGameplayTag(TagName.FromString("B.1"));
		var tagA3 = GameplayTag.RequestGameplayTag(TagName.FromString("1"));
		var tagA4 = GameplayTag.RequestGameplayTag(TagName.FromString("Color.Red"));

		var tagSetA = new HashSet<GameplayTag>() { tagA1, tagA2, tagA3, tagA4 };
		var tagContainerA = new GameplayTagContainer(tagSetA);

		var tagContainerC = GameplayTagContainer.EmptyContainer.Filter(tagContainerA);

		Assert.IsTrue(tagContainerC.IsEmpty);
	}

	[TestMethod]
	[TestCategory("FilterExact")]
	public void Container_A_FilterExact_container_B_should_not_have_tag_A1_only()
	{
		var tagA1 = GameplayTag.RequestGameplayTag(TagName.FromString("A.1"));
		var tagA2 = GameplayTag.RequestGameplayTag(TagName.FromString("B.1"));
		var tagA3 = GameplayTag.RequestGameplayTag(TagName.FromString("1"));
		var tagA4 = GameplayTag.RequestGameplayTag(TagName.FromString("Color.Red"));

		var tagSetA = new HashSet<GameplayTag>() { tagA1, tagA2, tagA3, tagA4 };
		var tagContainerA = new GameplayTagContainer(tagSetA);

		var tagB1 = GameplayTag.RequestGameplayTag(TagName.FromString("A.1"));
		var tagB2 = GameplayTag.RequestGameplayTag(TagName.FromString("B"));

		var tagSetB = new HashSet<GameplayTag>() { tagB1, tagB2 };
		var tagContainerB = new GameplayTagContainer(tagSetB);

		var tagContainerC = tagContainerA.FilterExact(tagContainerB);

		var validationContainer = new GameplayTagContainer(tagA1);

		Assert.IsTrue(tagContainerC.HasAllExact(validationContainer));
		Assert.IsTrue(tagContainerC.Count == 1);
	}

	[TestMethod]
	[TestCategory("FilterExact")]
	public void Container_A_FilterExact_EmptyContainer_should_be_empty()
	{
		var tagA1 = GameplayTag.RequestGameplayTag(TagName.FromString("A.1"));
		var tagA2 = GameplayTag.RequestGameplayTag(TagName.FromString("B.1"));
		var tagA3 = GameplayTag.RequestGameplayTag(TagName.FromString("1"));
		var tagA4 = GameplayTag.RequestGameplayTag(TagName.FromString("Color.Red"));

		var tagSetA = new HashSet<GameplayTag>() { tagA1, tagA2, tagA3, tagA4 };
		var tagContainerA = new GameplayTagContainer(tagSetA);

		var tagContainerC = tagContainerA.FilterExact(GameplayTagContainer.EmptyContainer);

		Assert.IsTrue(tagContainerC.IsEmpty);
	}

	[TestMethod]
	[TestCategory("FilterExact")]
	public void EmptyContainer_FilterExact_container_A_should_be_empty()
	{
		var tagA1 = GameplayTag.RequestGameplayTag(TagName.FromString("A.1"));
		var tagA2 = GameplayTag.RequestGameplayTag(TagName.FromString("B.1"));
		var tagA3 = GameplayTag.RequestGameplayTag(TagName.FromString("1"));
		var tagA4 = GameplayTag.RequestGameplayTag(TagName.FromString("Color.Red"));

		var tagSetA = new HashSet<GameplayTag>() { tagA1, tagA2, tagA3, tagA4 };
		var tagContainerA = new GameplayTagContainer(tagSetA);

		var tagContainerC = GameplayTagContainer.EmptyContainer.FilterExact(tagContainerA);

		Assert.IsTrue(tagContainerC.IsEmpty);
	}

	[TestMethod]
	[TestCategory("MatchesQuery")]
	public void Container_MatchesQuery_should_match()
	{
		var tagA1 = GameplayTag.RequestGameplayTag(TagName.FromString("A.1"));
		var tagA2 = GameplayTag.RequestGameplayTag(TagName.FromString("B.1"));
		var tagA3 = GameplayTag.RequestGameplayTag(TagName.FromString("1"));
		var tagA4 = GameplayTag.RequestGameplayTag(TagName.FromString("Color.Red"));

		var tagSetA = new HashSet<GameplayTag>() { tagA1, tagA2, tagA3, tagA4 };
		var tagContainerA = new GameplayTagContainer(tagSetA);

		var query = new GameplayTagQuery();
		query.Build(new GameplayTagQueryExpression()
			.AllTagsMatch()
				.AddTag("A.1")
				.AddTag("B.1"));

		Assert.IsTrue(tagContainerA.MatchesQuery(query));
	}

	[TestMethod]
	[TestCategory("MatchesQuery")]
	public void Container_MatchesQuery_should_not_match()
	{
		var tagA1 = GameplayTag.RequestGameplayTag(TagName.FromString("A.1"));
		var tagA2 = GameplayTag.RequestGameplayTag(TagName.FromString("B"));
		var tagA3 = GameplayTag.RequestGameplayTag(TagName.FromString("1"));
		var tagA4 = GameplayTag.RequestGameplayTag(TagName.FromString("Color.Red"));

		var tagSetA = new HashSet<GameplayTag>() { tagA1, tagA2, tagA3, tagA4 };
		var tagContainerA = new GameplayTagContainer(tagSetA);

		var query = new GameplayTagQuery();
		query.Build(new GameplayTagQueryExpression()
			.AllTagsMatch()
				.AddTag("A.1")
				.AddTag("B.1"));

		Assert.IsFalse(tagContainerA.MatchesQuery(query));
	}

	[TestMethod]
	[TestCategory("MatchesQuery")]
	public void EmptyContainer_MatchesQuery_should_match()
	{
		var query = new GameplayTagQuery();
		query.Build(new GameplayTagQueryExpression()
			.NoTagsMatch()
				.AddTag("A.1")
				.AddTag("B.1"));

		Assert.IsTrue(GameplayTagContainer.EmptyContainer.MatchesQuery(query));
	}

	[TestMethod]
	[TestCategory("MatchesQuery")]
	public void EmptyContainer_MatchesQuery_should_not_match()
	{
		var query = new GameplayTagQuery();
		query.Build(new GameplayTagQueryExpression()
			.AllTagsMatch()
				.AddTag("A.1")
				.AddTag("B.1"));

		Assert.IsFalse(GameplayTagContainer.EmptyContainer.MatchesQuery(query));
	}

	[TestMethod]
	[TestCategory("Append")]
	public void Container_A_Append_container_B_should_have_all_tags_from_both()
	{
		var tagA1 = GameplayTag.RequestGameplayTag(TagName.FromString("A.1"));
		var tagA2 = GameplayTag.RequestGameplayTag(TagName.FromString("B.1"));
		var tagA3 = GameplayTag.RequestGameplayTag(TagName.FromString("1"));
		var tagA4 = GameplayTag.RequestGameplayTag(TagName.FromString("Color.Red"));

		var tagSetA = new HashSet<GameplayTag>() { tagA1, tagA2, tagA3, tagA4 };
		var tagContainerA = new GameplayTagContainer(tagSetA);

		var tagB1 = GameplayTag.RequestGameplayTag(TagName.FromString("A.1"));
		var tagB2 = GameplayTag.RequestGameplayTag(TagName.FromString("B"));

		var tagSetB = new HashSet<GameplayTag>() { tagB1, tagB2 };
		var tagContainerB = new GameplayTagContainer(tagSetB);

		tagContainerA.AppendTags(tagContainerB);

		var validationTagSet = new HashSet<GameplayTag>() { tagA1, tagA2, tagA3, tagA4, tagB2 };
		var validationContainer = new GameplayTagContainer(validationTagSet);

		Assert.IsTrue(tagContainerA.HasAllExact(validationContainer));
		Assert.IsTrue(tagContainerA.Count == 5);
	}

	[TestMethod]
	[TestCategory("Append")]
	public void Container_A_Append_EmptyContainer_should_have_all_container_A_tags()
	{
		var tagA1 = GameplayTag.RequestGameplayTag(TagName.FromString("A.1"));
		var tagA2 = GameplayTag.RequestGameplayTag(TagName.FromString("B.1"));
		var tagA3 = GameplayTag.RequestGameplayTag(TagName.FromString("1"));
		var tagA4 = GameplayTag.RequestGameplayTag(TagName.FromString("Color.Red"));

		var tagSetA = new HashSet<GameplayTag>() { tagA1, tagA2, tagA3, tagA4 };
		var tagContainerA = new GameplayTagContainer(tagSetA);

		Assert.IsTrue(tagContainerA.Count == 4);
		tagContainerA.AppendTags(GameplayTagContainer.EmptyContainer);
		Assert.IsTrue(tagContainerA.Count == 4);
	}

	[TestMethod]
	[TestCategory("Append")]
	public void An_empty_container_Append_container_A_should_have_all_container_A_tags()
	{
		var tagA1 = GameplayTag.RequestGameplayTag(TagName.FromString("A.1"));
		var tagA2 = GameplayTag.RequestGameplayTag(TagName.FromString("B.1"));
		var tagA3 = GameplayTag.RequestGameplayTag(TagName.FromString("1"));
		var tagA4 = GameplayTag.RequestGameplayTag(TagName.FromString("Color.Red"));

		var tagSetA = new HashSet<GameplayTag>() { tagA1, tagA2, tagA3, tagA4 };
		var tagContainerA = new GameplayTagContainer(tagSetA);

		var newEmptyContainer = GameplayTagContainer.EmptyContainer;

		Assert.IsTrue(newEmptyContainer.Count == 0);
		newEmptyContainer.AppendTags(tagContainerA);
		Assert.IsTrue(newEmptyContainer.Count == 4);
	}

	[TestMethod]
	[TestCategory("Append")]
	public void The_EmptyContainer_Append_container_A_should_continue_empty()
	{
		var tagA1 = GameplayTag.RequestGameplayTag(TagName.FromString("A.1"));
		var tagA2 = GameplayTag.RequestGameplayTag(TagName.FromString("B.1"));
		var tagA3 = GameplayTag.RequestGameplayTag(TagName.FromString("1"));
		var tagA4 = GameplayTag.RequestGameplayTag(TagName.FromString("Color.Red"));

		var tagSetA = new HashSet<GameplayTag>() { tagA1, tagA2, tagA3, tagA4 };
		var tagContainerA = new GameplayTagContainer(tagSetA);

		GameplayTagContainer.EmptyContainer.AppendTags(tagContainerA);
		Assert.IsTrue(GameplayTagContainer.EmptyContainer.Count == 0);
	}

	[TestMethod]
	[TestCategory("ToString")]
	public void Container_ToString_return_expected_string()
	{
		var tagA1 = GameplayTag.RequestGameplayTag(TagName.FromString("A.1"));
		var tagA2 = GameplayTag.RequestGameplayTag(TagName.FromString("B.1"));
		var tagA3 = GameplayTag.RequestGameplayTag(TagName.FromString("1"));
		var tagA4 = GameplayTag.RequestGameplayTag(TagName.FromString("Color.Red"));

		var tagSet = new HashSet<GameplayTag>() { tagA1, tagA2, tagA3, tagA4 };
		var tagContainer = new GameplayTagContainer(tagSet);

		Console.WriteLine(tagContainer.ToString());
		Assert.IsTrue(string.Compare(tagContainer.ToString(), "'a.1', 'b.1', '1', 'color.red'", false) == 0);
	}

	[TestMethod]
	[TestCategory("ToString")]
	public void EmptyContainer_ToString_returns_empty_string()
	{
		Assert.IsTrue(string.Compare(GameplayTagContainer.EmptyContainer.ToString(), string.Empty) == 0);
	}

	[TestMethod]
	[TestCategory("Equality")]
	public void Containers_are_equatable()
	{
		var tagA1 = GameplayTag.RequestGameplayTag(TagName.FromString("A.1"));
		var tagA2 = GameplayTag.RequestGameplayTag(TagName.FromString("B.1"));
		var tagA3 = GameplayTag.RequestGameplayTag(TagName.FromString("1"));
		var tagA4 = GameplayTag.RequestGameplayTag(TagName.FromString("Color.Red"));

		var tagSetA = new HashSet<GameplayTag>() { tagA1, tagA2, tagA3, tagA4 };
		var tagSetB = new HashSet<GameplayTag>() { tagA1, tagA2, tagA3 };
		var tagContainerA = new GameplayTagContainer(tagSetA);
		var tagContainerB = new GameplayTagContainer(tagSetB);
		var tagContainerC = new GameplayTagContainer(tagSetA);

		Assert.AreEqual(tagContainerA, tagContainerC);
		Assert.AreNotEqual(tagContainerA, tagContainerB);
		Assert.AreNotEqual(tagContainerB, tagContainerC);

		Assert.IsTrue(tagContainerA != tagContainerB);
		Assert.IsTrue(tagContainerB != tagContainerC);
		Assert.IsTrue(tagContainerA == tagContainerC);
		Assert.IsTrue(tagContainerA.Equals(tagContainerC));

		object tagObjectA = tagContainerA;
		object tagObjectB = tagContainerB;
		object tagObjectC = tagContainerC;

		Assert.IsTrue(tagContainerA.Equals(tagObjectA));
		Assert.IsTrue(tagContainerA.Equals(tagObjectC));
		Assert.IsTrue(tagObjectA.Equals(tagObjectC));

		Assert.IsTrue((GameplayTagContainer)tagObjectA == (GameplayTagContainer)tagObjectC);

		// Those are defaul C# object == and !=, not the overriden ones.
		// So they should be considered different objects
		Assert.IsTrue(tagObjectA != tagObjectB);
		Assert.IsTrue(tagObjectA != tagObjectC);
		Assert.IsFalse(tagObjectA == tagObjectB);
		Assert.IsFalse(tagObjectA == tagObjectC);
	}

	[TestMethod]
	[TestCategory("Equality")]
	public void An_empty_containers_is_equal_EmptyContainer()
	{
		var tagContainerA = new GameplayTagContainer();

		Assert.IsTrue(tagContainerA == GameplayTagContainer.EmptyContainer);
	}
}
