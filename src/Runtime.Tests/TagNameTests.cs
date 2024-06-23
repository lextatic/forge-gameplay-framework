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
	public void TagName_from_empty_string_is_Empty_tag_name()
	{
		var tagName1 = TagName.FromString(string.Empty);

		Assert.IsTrue(tagName1 == TagName.Empty);
	}

	[TestMethod]
	[TestCategory("Equality")]
	public void TagNames_are_equatable()
	{
		var tagNameA = TagName.FromString("tagNameA");
		var tagNameB = TagName.FromString("tagnameb");
		var tagNameC = TagName.FromString("tagnamea");

		Assert.IsTrue(tagNameA != tagNameB);
		Assert.IsTrue(tagNameA == tagNameC);
		Assert.IsTrue(tagNameA.Equals(tagNameC));

		object tagNameObjectA = tagNameA;
		object tagNameObjectB = tagNameB;
		object tagNameObjectC = tagNameC;

		Assert.IsTrue(tagNameA.Equals(tagNameObjectA));
		Assert.IsTrue(tagNameA.Equals(tagNameObjectC));
		Assert.IsTrue(tagNameObjectA.Equals(tagNameObjectC));

		Assert.IsTrue((TagName)tagNameObjectA == (TagName)tagNameObjectC);

		// Those are defaul C# object == and !=, not the overriden ones.
		// So they should be considered different objects
		Assert.IsTrue(tagNameObjectA != tagNameObjectB);
		Assert.IsTrue(tagNameObjectA != tagNameObjectC);
		Assert.IsFalse(tagNameObjectA == tagNameObjectB);
		Assert.IsFalse(tagNameObjectA == tagNameObjectC);
	}

	[TestMethod]
	[TestCategory("Comparability")]
	public void TagNames_are_comparable_in_alphabetical_order()
	{
		var tagNameA = TagName.FromString("tagNameA");
		var tagNameB = TagName.FromString("tagnameb");
		var tagNameC = TagName.FromString("tagnamea");

		Assert.IsTrue(tagNameA.CompareTo(tagNameC) == 0);
		Assert.IsTrue(tagNameA.CompareTo(tagNameB) < 0);
		Assert.IsTrue(tagNameB.CompareTo(tagNameA) > 0);

		Assert.IsTrue(tagNameA < tagNameB);
		Assert.IsTrue(tagNameB > tagNameC);
		Assert.IsTrue(tagNameA <= tagNameC);
		Assert.IsTrue(tagNameA >= tagNameC);
		Assert.IsTrue(tagNameA <= tagNameB);
		Assert.IsTrue(tagNameB >= tagNameC);
	}

	[TestMethod]
	[TestCategory("Comparability")]
	public void TagNames_are_always_higher_than_Empty_tag_name()
	{
		var tagNameA = TagName.FromString("tagNameA");
		var tagNameB = TagName.FromString("tagnameb");
		var tagNameC = TagName.FromString("tagnamea");

		Assert.IsTrue(TagName.Empty.CompareTo(tagNameA) < 0);
		Assert.IsTrue(TagName.Empty.CompareTo(tagNameB) < 0);
		Assert.IsTrue(TagName.Empty.CompareTo(tagNameC) < 0);

		Assert.IsTrue(tagNameA.CompareTo(TagName.Empty) > 0);
		Assert.IsTrue(tagNameB.CompareTo(TagName.Empty) > 0);
		Assert.IsTrue(tagNameC.CompareTo(TagName.Empty) > 0);

		Assert.IsTrue(TagName.Empty.CompareTo(TagName.Empty) == 0);

		Assert.IsTrue(tagNameA > TagName.Empty);
		Assert.IsTrue(tagNameB > TagName.Empty);
		Assert.IsTrue(tagNameA > TagName.Empty);
		Assert.IsTrue(TagName.Empty < tagNameA);
		Assert.IsTrue(TagName.Empty < tagNameB);
		Assert.IsTrue(TagName.Empty < tagNameC);

		Assert.IsTrue(TagName.Empty == TagName.Empty);
		Assert.IsTrue(TagName.Empty >= TagName.Empty);
		Assert.IsTrue(TagName.Empty <= TagName.Empty);
	}

	[TestMethod]
	[TestCategory("ToString")]
	public void TagNames_ToString_always_returns_lowercase_string()
	{
		var tagName1 = TagName.FromString("Hello World");
		var tagName2 = TagName.FromString("HELLO TAGS");

		Assert.IsTrue(string.Compare(tagName1.ToString(), "hello world", false) == 0);
		Assert.IsTrue(string.Compare(tagName2.ToString(), "hello tags", false) == 0);
	}

	[TestMethod]
	[TestCategory("ToString")]
	public void Empty_tag_name_ToString_returns_empty_string()
	{
		Assert.IsTrue(string.Compare(TagName.Empty.ToString(), string.Empty) == 0);
	}
}
