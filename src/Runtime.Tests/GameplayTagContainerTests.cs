namespace GameplayTags.Runtime.Tests
{
	[TestClass]
	public class GameplayTagContainerTests
	{
		[TestMethod]
		public void Container_should_have_tag()
		{
			var tagContainer = new GameplayTagContainer();
			tagContainer.AddTag(GameplayTag.RequestGameplayTag(TagName.FromString("A.1")));
			tagContainer.AddTag(GameplayTag.RequestGameplayTag(TagName.FromString("B.1")));

			var tag = GameplayTag.RequestGameplayTag(TagName.FromString("A"));

			Assert.IsTrue(tagContainer.HasTag(tag));
		}

		[TestMethod]
		public void Container_should_not_have_tag()
		{
			var tagContainer = new GameplayTagContainer();
			tagContainer.AddTag(GameplayTag.RequestGameplayTag(TagName.FromString("A")));
			tagContainer.AddTag(GameplayTag.RequestGameplayTag(TagName.FromString("B.1")));

			var tag = GameplayTag.RequestGameplayTag(TagName.FromString("A.1"));

			Assert.IsFalse(tagContainer.HasTag(tag));
		}

		[TestMethod]
		public void Container_should_have_tag_exact()
		{
			var tagContainer = new GameplayTagContainer();
			tagContainer.AddTag(GameplayTag.RequestGameplayTag(TagName.FromString("A.1")));
			tagContainer.AddTag(GameplayTag.RequestGameplayTag(TagName.FromString("B.1")));

			var tag = GameplayTag.RequestGameplayTag(TagName.FromString("A.1"));

			Assert.IsTrue(tagContainer.HasTagExact(tag));
		}

		[TestMethod]
		public void Container_should_not_have_tag_exact()
		{
			var tagContainer = new GameplayTagContainer();
			tagContainer.AddTag(GameplayTag.RequestGameplayTag(TagName.FromString("A.1")));
			tagContainer.AddTag(GameplayTag.RequestGameplayTag(TagName.FromString("B.1")));

			var tag = GameplayTag.RequestGameplayTag(TagName.FromString("A"));

			Assert.IsFalse(tagContainer.HasTagExact(tag));
		}
	}
}