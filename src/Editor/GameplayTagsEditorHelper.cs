using GameplayTags.Runtime;
using System.Text;
using System.Text.RegularExpressions;

namespace GameplayTags.Editor
{
	public class GameplayTagsEditorHelper
	{
		//String with outlawed characters inside tags
		//private const string InvalidTagCharacters = @"\, ";

		//public void ConstructGameplayTagTreeFromFile(string filePath)
		//{
		//	var content = File.ReadLines(filePath);

		//	_rootTagNode = new GameplayTagNode();

		//	foreach (var line in content)
		//	{
		//		AddGameplayTagToTree(line);
		//	}
		//}

		//internal void AddGameplayTagToTree(string tagName)
		//{
		//	GameplayTagNode currentNode = _rootTagNode;
		//	//List<GameplayTagNode> ancestorNodes = new();

		//	TagName originalTagName = TagName.FromString(tagName);
		//	string fullTagString = tagName;

		//	// editor
		//	{
		//		if (!IsValidGameplayTagString(fullTagString, out string outError, out string outFixedString))
		//		{
		//			if (string.IsNullOrEmpty(outFixedString))
		//			{
		//				return;
		//			}

		//			fullTagString = outFixedString;
		//			originalTagName = TagName.FromString(fullTagString);
		//		}
		//	}

		//	var subTags = fullTagString.Split('.');

		//	fullTagString = "";
		//	int numSubTags = subTags.Length;

		//	for (int i = 0; i < numSubTags; i++)
		//	{
		//		bool bIsExplicitTag = (i == (numSubTags - 1));
		//		var shortTagName = TagName.FromString(subTags[i]);
		//		TagName fullTagName;

		//		if (bIsExplicitTag)
		//		{
		//			// We already know the final name
		//			fullTagName = originalTagName;
		//		}
		//		else if (i == 0)
		//		{
		//			// Full tag is the same as short tag, and start building full tag string
		//			fullTagName = shortTagName;
		//			fullTagString = subTags[i];
		//		}
		//		else
		//		{
		//			// Add .Tag and use that as full tag
		//			fullTagString += ".";
		//			fullTagString += subTags[i];

		//			fullTagName = TagName.FromString(fullTagString);
		//		}

		//		var childTags = currentNode.ChildTags;
		//		int insertionIdx = InsertTagIntoNodeArray(shortTagName, fullTagName, currentNode, childTags);

		//		currentNode = childTags[insertionIdx];
		//	}
		//}

		//public static bool IsValidGameplayTagString(string tagString, out string outError, out string outFixedString)
		//{
		//	outFixedString = tagString;

		//	if (string.IsNullOrEmpty(outFixedString))
		//	{
		//		outError = "Tag is empty";
		//		return false;
		//	}

		//	bool isValid = true;
		//	var errorStringBuilder = new StringBuilder("");

		//	while (outFixedString.StartsWith("."))
		//	{
		//		errorStringBuilder.AppendLine("Tag names can't start with '.'");
		//		outFixedString = outFixedString.Remove(0, 1);
		//		isValid = false;
		//	}

		//	while (outFixedString.EndsWith("."))
		//	{
		//		errorStringBuilder.AppendLine("Tag names can't end with '.'");
		//		outFixedString = outFixedString.Remove(outFixedString.Length - 1);
		//		isValid = false;
		//	}

		//	if (outFixedString.StartsWith(" ") || outFixedString.EndsWith(" "))
		//	{
		//		errorStringBuilder.AppendLine("Tag names can't start or end with space");
		//		outFixedString = outFixedString.Trim();
		//		isValid = false;
		//	}

		//	if (Regex.IsMatch(outFixedString, $"[{Regex.Escape(InvalidTagCharacters)}]"))
		//	{
		//		errorStringBuilder.AppendLine("Tag has invalid characters");
		//		outFixedString = Regex.Replace(outFixedString, $"[{Regex.Escape(InvalidTagCharacters)}]", "_");
		//		isValid = false;
		//	}

		//	outError = errorStringBuilder.ToString();

		//	return isValid;
		//}
	}
}
