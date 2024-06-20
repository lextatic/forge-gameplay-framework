#pragma warning disable SA1600 // Elements should be documented
namespace GameplayTags.Runtime.Tests;

[TestClass]
public class TagNameTests
{
	[TestMethod]
	[TestCategory("Equality")]
	public void TagNames_are_case_insensitive()
	{
		var tagName1 = TagName.FromString("Hello World");
		var tagName2 = TagName.FromString("Hello world");

		Assert.AreEqual(tagName1, tagName2);
	}

	[TestMethod]
	[TestCategory("Equality")]
	public void TagNames_are_equatable()
	{
		var tagNameA = TagName.FromString("tagNameA");
		var tagNameB = TagName.FromString("tagnameb");
		var tagNameC = TagName.FromString("tagnamea");

		Assert.IsTrue(tagNameA.CompareTo(tagNameC) == 0);

		Assert.IsTrue(tagNameA != tagNameB);
		Assert.IsTrue(tagNameA == tagNameC);
		Assert.IsTrue(tagNameA.Equals(tagNameC));

		object tagObjectA = tagNameA;
		object tagObjectB = tagNameB;
		object tagObjectC = tagNameC;

		Assert.IsTrue(tagNameA.Equals(tagObjectA));
		Assert.IsTrue(tagNameA.Equals(tagObjectC));
		Assert.IsTrue(tagObjectA.Equals(tagObjectC));

		Assert.IsTrue((TagName)tagObjectA == (TagName)tagObjectC);

		// Those are defaul C# object == and !=, not the overriden ones.
		// So they should be considered different objects
		Assert.IsTrue(tagObjectA != tagObjectB);
		Assert.IsTrue(tagObjectA != tagObjectC);
		Assert.IsFalse(tagObjectA == tagObjectB);
		Assert.IsFalse(tagObjectA == tagObjectC);
	}

	[TestMethod]
	[TestCategory("Comparability")]
	public void TagNames_are_comparable_in_alphabetical_order()
	{
		var tagNameA = TagName.FromString("tagNameA");
		var tagNameB = TagName.FromString("tagnameb");
		var tagNameC = TagName.FromString("tagnamea");

		Assert.IsTrue(tagNameA.CompareTo(tagNameB) < 0);
		Assert.IsTrue(tagNameB.CompareTo(tagNameA) > 0);

		Assert.IsTrue(tagNameA < tagNameB);
		Assert.IsTrue(tagNameB > tagNameC);
		Assert.IsTrue(tagNameA <= tagNameC);
		Assert.IsTrue(tagNameA >= tagNameC);
		Assert.IsTrue(tagNameA <= tagNameB);
		Assert.IsTrue(tagNameB >= tagNameC);
	}
}
