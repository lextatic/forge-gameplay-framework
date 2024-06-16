using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

namespace GameplayTags.Runtime
{
	public class GameplayTagsManager
	{
		private static readonly GameplayTagsManager _instance = new();

		private bool _networkIndexInvalidated = true;

		public static GameplayTagsManager Instance => _instance;

		private GameplayTagNode _rootTagNode = new();
		private Dictionary<GameplayTag, GameplayTagNode> _gameplayTagNodeMap = new();
		private List<GameplayTagNode> _networkGameplayTagNodeIndex = new();

		public GameplayTagNode RootNode => _rootTagNode;

		private GameplayTagsManager()
		{
		}

		public void ConstructGameplayTagTreeFromFile(string filePath)
		{
			var content = File.ReadLines(filePath);

			_rootTagNode = new GameplayTagNode();

			foreach (var line in content)
			{
				AddGameplayTagToTree(line);
			}
		}

		internal void AddGameplayTagToTree(string tagName)
		{
			GameplayTagNode currentNode = _rootTagNode;
			//List<GameplayTagNode> ancestorNodes = new();

			TagName originalTagName = TagName.FromString(tagName);
			string fullTagString = tagName;

			#if EDITOR
			{
				if (!GameplayTagsEditorHelper.IsValidGameplayTagString(fullTagString, out string outError, out string outFixedString))
				{
					if (string.IsNullOrEmpty(outFixedString))
					{
						return;
					}

					fullTagString = outFixedString;
					originalTagName = TagName.FromString(fullTagString);
				}
			}
			#endif

			var subTags = fullTagString.Split('.');

			fullTagString = "";
			int numSubTags = subTags.Length;

			for (int i = 0; i < numSubTags; i++)
			{
				bool bIsExplicitTag = (i == (numSubTags - 1));
				var shortTagName = TagName.FromString(subTags[i]);
				TagName fullTagName;

				if (bIsExplicitTag)
				{
					// We already know the final name
					fullTagName = originalTagName;
				}
				else if (i == 0)
				{
					// Full tag is the same as short tag, and start building full tag string
					fullTagName = shortTagName;
					fullTagString = subTags[i];
				}
				else
				{
					// Add .Tag and use that as full tag
					fullTagString += ".";
					fullTagString += subTags[i];

					fullTagName = TagName.FromString(fullTagString);
				}

				var childTags = currentNode.ChildTags;
				int insertionIdx = InsertTagIntoNodeArray(shortTagName, fullTagName, currentNode, childTags);

				currentNode = childTags[insertionIdx];
			}
		}

		private int InsertTagIntoNodeArray(TagName tagName, TagName fullTagName, GameplayTagNode parentNode, List<GameplayTagNode> nodeArray)
		{
			int? foundNodeIdx = null;
			int? whereToInsert = null;

			// See if the tag is already in the array

			// LowerBoundBy returns Position of the first element >= Value, may be position after last element in range
			//int lowerBoundIndex = Algo::LowerBoundBy(NodeArray, Tag,
			//				[](const TSharedPtr<FGameplayTagNode>&N) -> FName { return N->GetSimpleTagName(); },
			//[] (const FName&A, const FName&B) { return A != B && ComparisonUtility::CompareWithNumericSuffix(A, B) < 0; });

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
				GameplayTagNode currentNode = nodeArray[lowerBoundIndex];
				if (currentNode.TagName == tagName)
				{
					foundNodeIdx = lowerBoundIndex;
				}
				else
				{
					// Insert new node before this
					whereToInsert = lowerBoundIndex;
				}
			}

			if (foundNodeIdx == null)
			{
				if (whereToInsert == null)
				{
					// Insert at end
					whereToInsert = nodeArray.Count;
				}

				// Don't add the root node as parent
				GameplayTagNode tagNode = new GameplayTagNode(tagName, fullTagName, parentNode);//MakeShareable(new FGameplayTagNode(Tag, FullTag, ParentNode != GameplayRootTag ? ParentNode : nullptr, bIsExplicitTag, bIsRestrictedTag, bAllowNonRestrictedChildren));

				// Add at the sorted location
				nodeArray.Insert((int)whereToInsert, tagNode);
				foundNodeIdx = whereToInsert;

				GameplayTag gameplayTag = tagNode.CompleteTag;

				_gameplayTagNodeMap.Add(gameplayTag, tagNode);

				// These should always match
				if (gameplayTag.TagName != fullTagName)
				{
					throw new Exception("Failed");
				}
			}

			return (int)foundNodeIdx;
		}

		internal GameplayTagNode? FindTagNode(GameplayTag gameplayTag)
		{
			if (_gameplayTagNodeMap.TryGetValue(gameplayTag, out GameplayTagNode? gameplayTagNode))
			{
				return gameplayTagNode;
			}

			return null;
		}

		//Runtime
		//public GameplayTagContainer GetSingleTagContainer(GameplayTag gameplayTag)
		//{
		//	return _

		//	var gamplayTagNode = _gameplayTagNodeMap[gameplayTag];
		//	if (gamplayTagNode != null)
		//	{
		//		return gamplayTagNode.GetSingleTagContainer();
		//	}

		//	// default? It returns a nullref at Unreal, but it's still a struct
		//	// I guess structs can be nullref in C++...
		//	return default;
		//}

		// runtime
		internal int GameplayTagsMatchDepth(GameplayTag tagA, GameplayTag tagB)
		{
			var Tags1 = new HashSet<TagName>();
			var Tags2 = new HashSet<TagName>();
			var TagNode = FindTagNode(tagA);
			//if (TagNode.IsValid())
			//{
			GetAllParentNodeNames(Tags1, TagNode);
			//}

			TagNode = FindTagNode(tagB);
			//if (TagNode.IsValid())
			//{
			GetAllParentNodeNames(Tags2, TagNode);
			//}

			// Improve
			return Tags1.Intersect(Tags2).ToList().Count;
		}

		private void GetAllParentNodeNames(HashSet<TagName> namesList, GameplayTagNode tag)
		{
			namesList.Add(tag.CompleteTagName);

			var parent = tag.ParentTagNode;
			if (parent != _rootTagNode)
			{
				GetAllParentNodeNames(namesList, parent);
			}
		}

		//// Editor
		//internal bool IsValidGameplayTagString(string tagString, out string outError, out string outFixedString)
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
		//		errorStringBuilder.AppendLine("Tag names can't start with .");
		//		outFixedString = outFixedString.Remove(0, 1);
		//		isValid = false;
		//	}

		//	while (outFixedString.EndsWith("."))
		//	{
		//		errorStringBuilder.AppendLine("Tag names can't end with .");
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

		// Deletar, fiz para debug
		public void PrintAllTagsInNodeMap()
		{
			foreach (var value in _gameplayTagNodeMap.Keys)
			{
				Console.WriteLine(value);
			}
		}

		/**
		 * Gets the FGameplayTag that corresponds to the TagName
		 *
		 * @param TagName The Name of the tag to search for
		 * @param ErrorIfNotfound: ensure() that tag exists.
		 * 
		 * @return Will return the corresponding FGameplayTag or an empty one if not found.
		 */
		public GameplayTag RequestGameplayTag(TagName tagName, bool errorIfNotFound)
		{
			var possibleTag = new GameplayTag(tagName);

			if (_gameplayTagNodeMap.ContainsKey(possibleTag))
			{
				return possibleTag;
			}

			if (errorIfNotFound)
			{
				throw new GameplayTagNotFoundException(tagName);
			}

			return GameplayTag.EmptyTag;
		}

		/**
		* Adds the gameplay tags corresponding to the strings in the array TagStrings to OutTagsContainer
		*
		* @param TagStrings Array of strings to search for as tags to add to the tag container
		* @param OutTagsContainer Container to add the found tags to.
		* @param ErrorIfNotfound: ensure() that tags exists.
		*
		*/
		public GameplayTagContainer RequestGameplayTagContainer(string[] tagStrings, bool errorIfNotFound = true)
		{
			var gameplayTagContainer = new GameplayTagContainer();

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

		/**
		 * Gets a Tag Container containing the supplied tag and all of its parents as explicit tags.
		 * For example, passing in x.y.z would return a tag container with x.y.z, x.y, and x.
		 * This will only work for tags that have been properly registered.
		 *
		 * @param GameplayTag The tag to use at the child most tag for this container
		 * 
		 * @return A tag container with the supplied tag and all its parents added explicitly, or an empty container if that failed
		 */
		public GameplayTagContainer RequestGameplayTagParents(GameplayTag gameplayTag)
		{
			var parentTags = GetSingleTagContainer(gameplayTag);

			if (parentTags != GameplayTagContainer.EmptyContainer)
			{
				return parentTags.GetExplicitGameplayTagParents();
			}

			return new GameplayTagContainer();
		}

		/**
		 * Fills in an array of gameplay tags with all of tags that are the parents of the passed in tag.
		 * For example, passing in x.y.z would add x.y and x to UniqueParentTags if they was not already there.
		 * This is used by the GameplayTagContainer code and may work for unregistered tags depending on serialization settings.
		 *
		 * @param GameplayTag The gameplay tag to extract parent tags from
		 * @param UniqueParentTags A list of parent tags that will be added to if necessary
		 *
		 * @return true if any tags were added to UniqueParentTags
		 */
		public bool ExtractParentTags(GameplayTag gameplayTag, HashSet<GameplayTag> uniqueParentTags)
		{
			if (!gameplayTag.IsValid)
			{
				return false;
			}

#if DEBUG
			var validationCopy = new HashSet<GameplayTag>(uniqueParentTags);
#endif

			var oldSize = uniqueParentTags.Count;

			if (_gameplayTagNodeMap.TryGetValue(gameplayTag, out GameplayTagNode? gameplayTagNode))
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

		/**
		 * Gets a Tag Container containing the all tags in the hierarchy that are children of this tag. Does not return the original tag
		 *
		 * @param GameplayTag					The Tag to use at the parent tag
		 * 
		 * @return A Tag Container with the supplied tag and all its parents added explicitly
		 */
		public GameplayTagContainer RequestGameplayTagChildren(GameplayTag gameplayTag)
		{
			var tagContainer = new GameplayTagContainer();
			// Note this purposefully does not include the passed in GameplayTag in the container.
			var gameplayTagNode = FindTagNode(gameplayTag);
			if (gameplayTagNode != null)
			{
				AddChildrenTags(tagContainer, gameplayTagNode, true);
			}

			return tagContainer;
		}

		private void AddChildrenTags(GameplayTagContainer TagContainer, GameplayTagNode GameplayTagNode, bool RecurseAll)
		{
			if (GameplayTagNode != null)
			{
				List<GameplayTagNode> ChildrenNodes = GameplayTagNode.ChildTags;
				foreach (var ChildNode in ChildrenNodes)
				{
					if (ChildNode != null)
					{
						TagContainer.AddTag(ChildNode.CompleteTag);
						
						if (RecurseAll)
						{
							AddChildrenTags(TagContainer, ChildNode, true);
						}
					}
				}
			}
		}

		public GameplayTag RequestGameplayTagDirectParent(GameplayTag gameplayTag)
		{
			if (_gameplayTagNodeMap.TryGetValue(gameplayTag, out GameplayTagNode? gameplayTagNode))
			{
				var parentTag = gameplayTagNode.ParentTagNode;
				if (parentTag != null)
				{
					return parentTag.CompleteTag;
				}
			}

			return GameplayTag.EmptyTag;
		}

		internal GameplayTagContainer RequestGameplayTagDirectParents(GameplayTag gameplayTag)
		{
			GameplayTagContainer parentTags = GetSingleTagContainer(gameplayTag);

			if (parentTags != GameplayTagContainer.EmptyContainer)
			{
				return parentTags.GetExplicitGameplayTagParents();
			}

			return GameplayTagContainer.EmptyContainer;
		}

		/** Gets the list of all registered tags, setting OnlyIncludeDictionaryTags will exclude implicitly added tags if possible */
		public GameplayTagContainer RequestAllGameplayTags(bool OnlyIncludeDictionaryTags)
		{
			var tagContainer = new GameplayTagContainer();

			foreach (var nodePair in _gameplayTagNodeMap)
	{
				var tagNode = nodePair.Value;
				if (!OnlyIncludeDictionaryTags)// || tagNode.IsExplicitTag())
				{
					tagContainer.AddTagFast(tagNode.CompleteTag);
				}
			}

			return tagContainer;
		}

		/** Returns the number of parents a particular gameplay tag has.  Useful as a quick way to determine which tags may
		 * be more "specific" than other tags without comparing whether they are in the same hierarchy or anything else.
		 * Example: "TagA.SubTagA" has 2 Tag Nodes.  "TagA.SubTagA.LeafTagA" has 3 Tag Nodes.
		 */
		public int GetNumberOfTagNodes(GameplayTag GameplayTag)
		{
			int count = 0;

			var tagNode = FindTagNode(GameplayTag);
			while (tagNode != null)
			{
				++count;                                // Increment the count of valid tag nodes.
				tagNode = tagNode.ParentTagNode;  // Continue up the chain of parents.
			}

			return count;
		}

		private GameplayTagContainer GetSingleTagContainer(GameplayTag gameplayTag)
		{
			if (_gameplayTagNodeMap.TryGetValue(gameplayTag, out GameplayTagNode? gameplayTagNode))
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

			// Should make some checks
			// Then move commonly replicated tags to the beginning

			for (ushort i = 0; i < _networkGameplayTagNodeIndex.Count; ++i)
			{
				if (_networkGameplayTagNodeIndex[i] != null)
				{
					_networkGameplayTagNodeIndex[i].NetIndex = i;

					Console.WriteLine($"Assigning NetIndex {i} to Tag {_networkGameplayTagNodeIndex[i]}");
				}
				else
				{
					throw new Exception($"TagNode Indice {i} is invalid!");
				}
			}
		}

		internal ushort GetNetIndexFromTag(GameplayTag tag)
		{
			VerifyNetworkIndex();

			var gameplayTagNode = FindTagNode(tag);

			//if (gameplayTagNode.IsValid)
			if (gameplayTagNode != null)
			{
				return gameplayTagNode.NetIndex;
			}

			return ushort.MaxValue;
			//return InvalidTagNetIndex;
		}

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

		public string[] SplitGameplayTagName(GameplayTag tag)
		{
			var outNames = new List<string>();
			var currentNode = FindTagNode(tag);
			while (currentNode != null)
			{
				outNames.Insert(0, currentNode.TagName.ToString());
				currentNode = currentNode.ParentTagNode;
			}

			return outNames.ToArray();
		}
	}
}
