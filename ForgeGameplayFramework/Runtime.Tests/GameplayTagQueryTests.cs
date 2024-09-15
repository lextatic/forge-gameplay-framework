#pragma warning disable SA1600 // Elements should be documented
namespace GameplayTags.Runtime.Tests;

[TestClass]
public class GameplayTagQueryTests
{
	[TestMethod]
	public void Container_matches_query_should_match()
	{
		var tagContainerA = new GameplayTagContainer();
		tagContainerA.AddTag(GameplayTag.RequestGameplayTag(TagName.FromString("Color.Red")));

		var tagContainerB = new GameplayTagContainer();
		tagContainerB.AddTag(GameplayTag.RequestGameplayTag(TagName.FromString("Color.Blue")));

		var tagContainerC = new GameplayTagContainer();
		tagContainerC.AddTag(GameplayTag.RequestGameplayTag(TagName.FromString("Color.Red")));
		tagContainerC.AddTag(GameplayTag.RequestGameplayTag(TagName.FromString("Color.Blue")));

		var tagContainerD = new GameplayTagContainer();
		tagContainerD.AddTag(GameplayTag.RequestGameplayTag(TagName.FromString("Color.Red")));
		tagContainerD.AddTag(GameplayTag.RequestGameplayTag(TagName.FromString("Color.Green")));

		var query = new GameplayTagQuery();
		query.Build(new GameplayTagQueryExpression()
			.AllExpressionsMatch()
				.AddExpression(new GameplayTagQueryExpression()
					.AnyTagsMatch()
						.AddTag("Color.Red")
						.AddTag("Color.Blue"))
				.AddExpression(new GameplayTagQueryExpression()
					.NoExpressionsMatch()
						.AddExpression(new GameplayTagQueryExpression()
							.AllTagsMatch()
								.AddTag("Color.Red")
								.AddTag("Color.Blue"))
						.AddExpression(new GameplayTagQueryExpression()
							.AnyTagsMatch()
								.AddTag("Color.Green"))));

		Assert.IsTrue(query.Matches(tagContainerA));
		Assert.IsTrue(query.Matches(tagContainerB));
		Assert.IsFalse(query.Matches(tagContainerC));
		Assert.IsFalse(query.Matches(tagContainerD));
	}
}
