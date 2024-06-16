#if DEBUG
using System.Diagnostics;
using System.Text;
using System.Text.RegularExpressions;
#endif

namespace GameplayTags.Runtime;

/// <summary>
/// A Singleton containing data about the tag dictionary.
/// </summary>
public class GameplayTagsManager
{
#if DEBUG
	private const string InvalidTagCharacters = @"\, ";
#endif

	private readonly Dictionary<GameplayTag, GameplayTagNode> _gameplayTagNodeMap = new ();

	private readonly List<GameplayTagNode> _networkGameplayTagNodeIndex = new ();

	private bool _networkIndexInvalidated = true;

	/// <summary>
	/// Gets the singleton instance of the <see cref="GameplayTagsManager"/>.
	/// </summary>
	public static GameplayTagsManager Instance { get; } = new ();

	/// <summary>
	/// Gets the roots <see cref="GameplayTagNode"/> of gameplay tag nodes.
	/// </summary>
	public GameplayTagNode RootNode { get; private set; } = new ();

	private GameplayTagsManager()
	{
	}

	/// <summary>
	/// Constructs a <see cref="GameplayTagNode"/> tree from a file where each line represents a single tag.
	/// </summary>
	/// <param name="filePath">Path to the file containing the tags.</param>
	public void ConstructGameplayTagTreeFromFile(string filePath)
	{
		var content = File.ReadLines(filePath);

		RootNode = new GameplayTagNode();

		foreach (var line in content)
		{
			AddGameplayTagToTree(line);
		}
	}

	/// <summary>
	/// Gets a <see cref="GameplayTagContainer"/> of all registered tags.
	/// </summary>
	/// <remarks>Setting <paramref name="onlyIncludeDictionaryTags"/> will exclude implicitly added tags if possible.
	/// </remarks>
	/// <param name="onlyIncludeDictionaryTags">Whether to include only explicit tags or not.</param>
	/// <returns>A <see cref="GameplayTagContainer"/> of all registered tags.</returns>
	public GameplayTagContainer RequestAllGameplayTags(bool onlyIncludeDictionaryTags)
	{
		GameplayTagContainer tagContainer = default;

		foreach (var nodePair in _gameplayTagNodeMap)
		{
			var tagNode = nodePair.Value;
			if (!onlyIncludeDictionaryTags || tagNode.IsExplicitTag)
			{
				tagContainer.AddTagFast(tagNode.CompleteTag);
			}
		}

		return tagContainer;
	}

	/// <summary>
	/// Returns the number of parents a particular <see cref="GameplayTag"/> has. Useful as a quick way to determine
	/// which tags may be more "specific" than other tags without comparing whether they are in the same hierarchy or
	/// anything else.
	/// </summary>
	/// <remarks>Example: "TagA.SubTagA" has 2 <see cref="GameplayTagNode"/>s. "TagA.SubTagA.LeafTagA" has 3
	/// <see cref="GameplayTagNode"/>s.</remarks>
	/// <param name="gameplayTag">The <see cref="GameplayTag"/> to request the number of <see cref="GameplayTagNode"/>s
	/// from.</param>
	/// <returns>The number of parents a particular <see cref="GameplayTag"/> has.</returns>
	public int GetNumberOfTagNodes(GameplayTag gameplayTag)
	{
		int count = 0;

		var tagNode = FindTagNode(gameplayTag);
		while (tagNode is not null)
		{
			// Increment the count of valid tag nodes.
			++count;

			// Continue up the chain of parents.
			tagNode = tagNode.ParentTagNode;
		}

		return count;
	}

	/// <summary>
	/// Splits a <see cref="GameplayTag"/> such as x.y.z into an array of names {x,y,z}.
	/// </summary>
	/// <param name="tag">The <see cref="GameplayTag"/> to split.</param>
	/// <returns>A <see cref="T:string[]"/> containing the name of the nodes of a split <see cref="GameplayTag"/>.</returns>
	public string[] SplitGameplayTagName(GameplayTag tag)
	{
		var outNames = new List<string>();
		var currentNode = FindTagNode(tag);
		while (currentNode is not null)
		{
			outNames.Insert(0, currentNode.TagName.ToString());
			currentNode = currentNode.ParentTagNode;
		}

		return outNames.ToArray();
	}

	/// <summary>
	/// Naively prints all nodes in the tag node tree.
	/// </summary>
	public void PrintAllTagsInNodeMap()
	{
		foreach (var value in _gameplayTagNodeMap.Keys)
		{
			Console.WriteLine(value);
		}
	}

	/// <summary>
	/// Returns a <see cref="GameplayTagContainer"/> with the tags corresponding to the strings in the array
	/// <paramref name="tagStrings"/>.
	/// </summary>
	/// <param name="tagStrings">A string array containing the desired tag names.</param>
	/// <param name="errorIfNotFound">Throws an exception if any tag doesn't exist.</param>
	/// <returns>A <see cref="GameplayTagContainer"/> containing the tags found.</returns>
	public GameplayTagContainer RequestGameplayTagContainer(string[] tagStrings, bool errorIfNotFound = true)
	{
		GameplayTagContainer gameplayTagContainer = default;

		foreach (var tagString in tagStrings)
		{
			var requestedTag = RequestGameplayTag(TagName.FromString(tagString), errorIfNotFound);

			if (requestedTag != GameplayTag.EmptyTag)
			{
				gameplayTagContainer.AddTag(requestedTag);
			}
		}

		return gameplayTagContainer;
	}

	/// <summary>
	/// Gets a <see cref="GameplayTagContainer"/> containing the supplied tag and all of its parents as explicit tags.
	/// </summary>
	/// <remarks>
	/// <para>For example, passing in x.y.z would return a tag container with x.y.z, x.y, and x.</para>
	/// <para>This will only work for tags that have been properly registered.</para>
	/// </remarks>
	/// <param name="gameplayTag">The tag to use at the child most tag for this container.</param>
	/// <returns>A <see cref="GameplayTagContainer"/> with the supplied tag and all its parents added explicitly, or an
	/// empty container if that failed.</returns>
	public GameplayTagContainer RequestGameplayTagParents(GameplayTag gameplayTag)
	{
		var parentTags = GetSingleTagContainer(gameplayTag);

		if (parentTags != GameplayTagContainer.EmptyContainer)
		{
			return parentTags.GetExplicitGameplayTagParents();
		}

		return GameplayTagContainer.EmptyContainer;
	}

	/// <summary>
	/// Gets a <see cref="GameplayTagContainer"/> containing the all tags in the hierarchy that are children of this
	/// tag. Does not return the original tag.
	/// </summary>
	/// <param name="gameplayTag">The <see cref="GameplayTag"/> to use as the parent tag.</param>
	/// <returns>A <see cref="GameplayTagContainer"/> with all its parents added explicitly.
	/// </returns>
	public GameplayTagContainer RequestGameplayTagChildren(GameplayTag gameplayTag)
	{
		GameplayTagContainer tagContainer = default;

		// Note this purposefully does not include the passed in GameplayTag in the container.
		var gameplayTagNode = FindTagNode(gameplayTag);
		if (gameplayTagNode is not null)
		{
			AddChildrenTags(tagContainer, gameplayTagNode, true);
		}

		return tagContainer;
	}

	/// <summary>
	/// Checks node tree to see if a <see cref="GameplayTagNode"/> with the name exists.
	/// </summary>
	/// <param name="gameplayTag">The name of the <see cref="GameplayTagNode"/> to search for.</param>
	/// <returns>The <see cref="GameplayTagNode"/> found, or <see langword="null"/> if not found.</returns>
	internal GameplayTagNode? FindTagNode(GameplayTag gameplayTag)
	{
		if (_gameplayTagNodeMap.TryGetValue(gameplayTag, out var gameplayTagNode))
		{
			return gameplayTagNode;
		}

		return null;
	}

	/// <summary>
	/// Gets the <see cref="GameplayTag"/> that corresponds to the <see cref="TagName"/>.
	/// </summary>
	/// <param name="tagName">The Name of the tag to search for.</param>
	/// <param name="errorIfNotFound">Throws an exception if that tag doesn't exist.</param>
	/// <returns>Will return the corresponding <see cref="GameplayTag"/> or an empty one if not found.</returns>
	/// <exception cref="GameplayTagNotFoundException">Exception thrown for when a <see cref="GameplayTag"/> is not
	/// found.</exception>
	internal GameplayTag RequestGameplayTag(TagName tagName, bool errorIfNotFound)
	{
		var possibleTag = new GameplayTag(tagName);

		if (_gameplayTagNodeMap.ContainsKey(possibleTag))
		{
			return possibleTag;
		}

#if DEBUG
		if (errorIfNotFound)
		{
			throw new GameplayTagNotFoundException(tagName);
		}
#endif

		return GameplayTag.EmptyTag;
	}

	/// <summary>
	/// Check to see how closely two <see cref="GameplayTag"/>s match. Higher values indicate more matching terms in the
	/// tags.
	/// </summary>
	/// <param name="tagA">The first <see cref="GameplayTag"/> to compare.</param>
	/// <param name="tagB">The second <see cref="GameplayTag"/> to compare.</param>
	/// <returns>The length of the longest matching pair.</returns>
	internal int GameplayTagsMatchDepth(GameplayTag tagA, GameplayTag tagB)
	{
		var tags1 = new HashSet<TagName>();
		var tags2 = new HashSet<TagName>();
		var tagNode = FindTagNode(tagA);

		if (tagNode is not null)
		{
			GetAllParentNodeNames(tags1, tagNode);
		}

		tagNode = FindTagNode(tagB);
		if (tagNode is not null)
		{
			GetAllParentNodeNames(tags2, tagNode);
		}

		return tags1.Intersect(tags2).ToList().Count;
	}

	/// <summary>
	/// Fills in a <see cref="HashSet{GameplayTag}"/> with all of tags that are the parents of the tag passed in
	/// <paramref name="gameplayTag"/>.
	/// </summary>
	/// <remarks>
	/// <para>For example, passing in x.y.z would add x.y and x to <paramref name="uniqueParentTags"/> if they was not
	/// already there.</para>
	/// <para>This is used by the <see cref="GameplayTagContainer"/> code and may work for unregistered tags depending
	/// on serialization settings.</para>
	/// </remarks>
	/// <param name="gameplayTag">The <see cref="GameplayTag"/> to extract parent tags from.</param>
	/// <param name="uniqueParentTags">A <see cref="HashSet{GameplayTag}"/> where tags will be added to if necessary.
	/// </param>
	/// <returns><see langword="true"/> if any tags were added to <paramref name="uniqueParentTags"/>.</returns>
	internal bool ExtractParentTags(GameplayTag gameplayTag, HashSet<GameplayTag> uniqueParentTags)
	{
		if (!gameplayTag.IsValid)
		{
			return false;
		}

#if DEBUG
		var validationCopy = new HashSet<GameplayTag>(uniqueParentTags);
#endif

		var oldSize = uniqueParentTags.Count;

		if (_gameplayTagNodeMap.TryGetValue(gameplayTag, out var gameplayTagNode))
		{
			var singleContainer = gameplayTagNode.SingleTagContainer;
			foreach (var tagParent in singleContainer.ParentTags)
			{
				uniqueParentTags.Add(tagParent);
			}
#if DEBUG
			validationCopy.UnionWith(gameplayTag.ParseParentTags());
			System.Diagnostics.Debug.Assert(
				validationCopy.SetEquals(uniqueParentTags),
				$"ExtractParentTags results are inconsistent for tag {gameplayTag}");
#endif
		}
		//// Should not clear invalid tags
		//else if ()
		//{
		//	uniqueParentTags = gameplayTag.ParseParentTags();
		//}

		return uniqueParentTags.Count != oldSize;
	}

	/// <summary>
	/// Returns direct parent <see cref="GameplayTag"/> of this tag, calling on x.y will return x.
	/// </summary>
	/// <param name="gameplayTag">The <see cref="GameplayTag"/> to extract the parent from.</param>
	/// <returns>The parent <see cref="GameplayTag"/> of the requested tag.</returns>
	internal GameplayTag RequestGameplayTagDirectParent(GameplayTag gameplayTag)
	{
		if (_gameplayTagNodeMap.TryGetValue(gameplayTag, out var gameplayTagNode))
		{
			var parentTag = gameplayTagNode.ParentTagNode;
			if (parentTag is not null)
			{
				return parentTag.CompleteTag;
			}
		}

		return GameplayTag.EmptyTag;
	}

	/// <summary>
	/// Not really implemented yet.
	/// </summary>
	/// <param name="tag">A tag to find the index for.</param>
	/// <returns>The index for the provided Tag.</returns>
	internal ushort GetNetIndexFromTag(GameplayTag tag)
	{
		VerifyNetworkIndex();

		var gameplayTagNode = FindTagNode(tag);

		if (gameplayTagNode is not null)
		{
			return gameplayTagNode.NetIndex;
		}

		return ushort.MaxValue;
		//return InvalidTagNetIndex;
	}

	/// <summary>
	/// Not really implemented yet.
	/// </summary>
	/// <param name="tagIndex">The index to look for.</param>
	/// <returns>A TagName for the provided index.</returns>
	/// <exception cref="Exception">If can't find the index.</exception>
	internal TagName GetTagNameFromNetIndex(ushort tagIndex)
	{
		VerifyNetworkIndex();

		if (tagIndex >= _networkGameplayTagNodeIndex.Count)
		{
			// Ensure Index is the invalid index. If its higher than that, then something is wrong.
			throw new Exception($"Received invalid tag net index {tagIndex}! Tag index is out of sync on client!");
		}

		return _networkGameplayTagNodeIndex[tagIndex].TagName;
	}

	private void AddGameplayTagToTree(string tagName)
	{
		var currentNode = RootNode;

		var originalTagName = TagName.FromString(tagName);
		string fullTagString = tagName;

#if DEBUG
		if (!IsValidGameplayTagString(fullTagString, out string outError, out string outFixedString))
		{
			if (string.IsNullOrEmpty(outFixedString))
			{
				return;
			}

			fullTagString = outFixedString;
			originalTagName = TagName.FromString(fullTagString);
		}
#endif

		var subTags = fullTagString.Split('.');

		fullTagString = string.Empty;
		int numSubTags = subTags.Length;

		for (int i = 0; i < numSubTags; i++)
		{
			bool isExplicitTag = i == (numSubTags - 1);
			var shortTagName = TagName.FromString(subTags[i]);
			TagName fullTagName;

			if (isExplicitTag)
			{
				// We already know the final name.
				fullTagName = originalTagName;
			}
			else if (i == 0)
			{
				// Full tag is the same as short tag, and start building full tag string.
				fullTagName = shortTagName;
				fullTagString = subTags[i];
			}
			else
			{
				// Add .Tag and use that as full tag.
				fullTagString += ".";
				fullTagString += subTags[i];

				fullTagName = TagName.FromString(fullTagString);
			}

			var childTags = currentNode.ChildTags;
			int insertionIdx = InsertTagIntoNodeArray(shortTagName, fullTagName, currentNode, childTags, isExplicitTag);

			currentNode = childTags[insertionIdx];
		}
	}

	private int InsertTagIntoNodeArray(
		TagName tagName,
		TagName fullTagName,
		GameplayTagNode parentNode,
		List<GameplayTagNode> nodeArray,
		bool isExplicitTag)
	{
		int? foundNodeIdx = null;
		int? whereToInsert = null;

		// See if the tag is already in the array.
		int lowerBoundIndex = 0;
		if (nodeArray.Count > 0)
		{
			lowerBoundIndex = nodeArray.FindIndex(node => node.TagName >= tagName);

			if (lowerBoundIndex < 0)
			{
				lowerBoundIndex = nodeArray.Count;
			}
		}

		if (lowerBoundIndex < nodeArray.Count)
		{
			var currentNode = nodeArray[lowerBoundIndex];
			if (currentNode.TagName == tagName)
			{
				foundNodeIdx = lowerBoundIndex;
			}
			else
			{
				// Insert new node before this.
				whereToInsert = lowerBoundIndex;
			}
		}

		if (foundNodeIdx is null)
		{
			if (whereToInsert is null)
			{
				// Insert at end.
				whereToInsert = nodeArray.Count;
			}

			// Don't add the root node as parent.
			var tagNode = new GameplayTagNode(tagName, fullTagName, parentNode, isExplicitTag);
			//MakeShareable(new FGameplayTagNode(Tag, FullTag, ParentNode != GameplayRootTag ? ParentNode : nullptr, bIsExplicitTag, bIsRestrictedTag, bAllowNonRestrictedChildren));

			// Add at the sorted location.
			nodeArray.Insert((int)whereToInsert, tagNode);
			foundNodeIdx = whereToInsert;

			var gameplayTag = tagNode.CompleteTag;

			_gameplayTagNodeMap.Add(gameplayTag, tagNode);

#if DEBUG
			// These should always match.
			if (gameplayTag.TagName != fullTagName)
			{
				throw new Exception("Failed");
			}
#endif
		}

		return (int)foundNodeIdx;
	}

	private void GetAllParentNodeNames(HashSet<TagName> namesList, GameplayTagNode tag)
	{
		namesList.Add(tag.CompleteTagName);

		var parent = tag.ParentTagNode;
		Debug.Assert(parent is not null, "Parent should never be null.");

		if (parent != RootNode)
		{
			GetAllParentNodeNames(namesList, parent);
		}
	}

#if DEBUG
	private bool IsValidGameplayTagString(string tagString, out string outError, out string outFixedString)
	{
		outFixedString = tagString;

		if (string.IsNullOrEmpty(outFixedString))
		{
			outError = "Tag is empty";
			return false;
		}

		bool isValid = true;
		var errorStringBuilder = new StringBuilder();

		while (outFixedString.StartsWith('.'))
		{
			errorStringBuilder.AppendLine("Tag names can't start with .");
			outFixedString = outFixedString.Remove(0, 1);
			isValid = false;
		}

		while (outFixedString.EndsWith('.'))
		{
			errorStringBuilder.AppendLine("Tag names can't end with .");
			outFixedString = outFixedString.Remove(outFixedString.Length - 1);
			isValid = false;
		}

		if (outFixedString.StartsWith(' ') || outFixedString.EndsWith(' '))
		{
			errorStringBuilder.AppendLine("Tag names can't start or end with space");
			outFixedString = outFixedString.Trim();
			isValid = false;
		}

		if (Regex.IsMatch(outFixedString, $"[{Regex.Escape(InvalidTagCharacters)}]"))
		{
			errorStringBuilder.AppendLine("Tag has invalid characters");
			outFixedString = Regex.Replace(outFixedString, $"[{Regex.Escape(InvalidTagCharacters)}]", "_");
			isValid = false;
		}

		outError = errorStringBuilder.ToString();

		return isValid;
	}
#endif

	private void AddChildrenTags(GameplayTagContainer tagContainer, GameplayTagNode gameplayTagNode, bool recurseAll)
	{
		if (gameplayTagNode is not null)
		{
			var childrenNodes = gameplayTagNode.ChildTags;
			foreach (var childNode in childrenNodes)
			{
				if (childNode is not null)
				{
					tagContainer.AddTag(childNode.CompleteTag);

					if (recurseAll)
					{
						AddChildrenTags(tagContainer, childNode, true);
					}
				}
			}
		}
	}

	private GameplayTagContainer GetSingleTagContainer(GameplayTag gameplayTag)
	{
		if (_gameplayTagNodeMap.TryGetValue(gameplayTag, out var gameplayTagNode))
		{
			return gameplayTagNode.SingleTagContainer;
		}

		return GameplayTagContainer.EmptyContainer;
	}

	private void VerifyNetworkIndex()
	{
		if (_networkIndexInvalidated)
		{
			ConstructNetIndex();
		}
	}

	private void ConstructNetIndex()
	{
		_networkIndexInvalidated = false;

		_networkGameplayTagNodeIndex.Clear();

		_networkGameplayTagNodeIndex.AddRange(_gameplayTagNodeMap.Values);

		_networkGameplayTagNodeIndex.Sort();

		// Should make some checks.
		// Then move commonly replicated tags to the beginning.
		for (ushort i = 0; i < _networkGameplayTagNodeIndex.Count; ++i)
		{
			if (_networkGameplayTagNodeIndex[i] is not null)
			{
				_networkGameplayTagNodeIndex[i].NetIndex = i;

				Console.WriteLine($"Assigning NetIndex {i} to Tag {_networkGameplayTagNodeIndex[i]}");
			}
			else
			{
				throw new Exception($"TagNode index {i} is invalid!");
			}
		}
	}
}
