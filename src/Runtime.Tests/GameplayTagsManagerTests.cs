#pragma warning disable SA1600 // Elements should be documented
namespace GameplayTags.Runtime.Tests;

[TestClass]
public class GameplayTagsManagerTests
{
	[AssemblyInitialize]
	public static void Construct_GameplayTag_tree(TestContext _)
	{
		GameplayTagsManager.Instance.ConstructGameplayTagTreeFromFile("./tags.txt");
	}

	[TestMethod]
	public void PrintTree()
	{
		PrintTree(GameplayTagsManager.Instance.RootNode);
	}

	[TestMethod]
	public void PrintNodeMap()
	{
		GameplayTagsManager.Instance.PrintAllTagsInNodeMap();
	}

	private void PrintTree(GameplayTagNode currentNode)
	{
		Console.WriteLine(currentNode.CompleteTagName);

		foreach (var node in currentNode.ChildTags)
		{
			PrintTree(node);
		}
	}
}
