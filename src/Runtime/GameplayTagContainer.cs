using System.Collections;

namespace GameplayTags.Runtime
{
	public struct GameplayTagContainer : IEnumerable<GameplayTag>
	{
		// Questionable...
		public static GameplayTagContainer EmptyContainer = new GameplayTagContainer();

		// HashSet?
		private List<GameplayTag> _gameplayTags = new List<GameplayTag>();
		private List<GameplayTag> _parentTags = new List<GameplayTag>();

		// Could be using this isntead of index and IEnumerable?
		public List<GameplayTag> GameplayTags => _gameplayTags;
		public List<GameplayTag> ParentTags => _parentTags;

		public GameplayTag this[int index]
		{
			get
			{
				return _gameplayTags[index];
			}
		}

		public int Count => _gameplayTags.Count;
		public bool IsValid => _gameplayTags.Count > 0;
		public bool IsEmpty => _gameplayTags.Count == 0;

		public GameplayTagContainer()
		{

		}

		// Is this really needed? copy constructor. Where are they used?
		//public GameplayTagContainer(GameplayTagContainer other)
		//{
		//	this = other;
		//}

		public GameplayTagContainer(GameplayTag tag)
		{
			AddTag(tag);
		}

		// Move contrusctor (which here is actually a copy constructor) Where are they used?
		public GameplayTagContainer(GameplayTagContainer other)
		{
			_gameplayTags.AddRange(other.GameplayTags);
			_parentTags.AddRange(other.ParentTags);
		}

		// Isn't it easier to have a constructor for that?
		public static GameplayTagContainer CreateFromArray(List<GameplayTag> sourceTags)
		{
			var container = new GameplayTagContainer();
			container._gameplayTags.AddRange(sourceTags);
			container.FillParentTags();
			return container;
		}

		private void FillParentTags()
		{
			_parentTags.Clear();

			if (_gameplayTags.Count > 0)
			{
				var tagsManager = GameplayTagsManager.Instance;

				foreach (var tag in _gameplayTags)
				{
					tagsManager.ExtractParentTags(tag, _parentTags);
				}
			}
		}

		public void AddTag(GameplayTag tagToAdd)
		{
			if (tagToAdd != GameplayTag.EmptyTag)
			{
				// Unreal code is AddUnique, is the check really necessary?
				if (!_gameplayTags.Contains(tagToAdd))
				{
					_gameplayTags.Add(tagToAdd);
				}

				GameplayTagsManager.Instance.ExtractParentTags(tagToAdd, _parentTags);
			}
		}

		public void AddTagFast(GameplayTag tagToAdd)
		{
			_gameplayTags.Add(tagToAdd);
			GameplayTagsManager.Instance.ExtractParentTags(tagToAdd, _parentTags);
		}

		public bool AddLeaftTag(GameplayTag tagToAdd)
		{
			// Check tag is not already explicitly in container
			if (HasTagExact(tagToAdd))
			{
				return true;
			}

			// If this tag is parent of explicitly added tag, fail
			if (HasTag(tagToAdd))
			{
				return false;
			}

			var tagNode = GameplayTagsManager.Instance.FindTagNode(tagToAdd);

			if (tagNode != null)
			{
				// Remove any tags in the container that are a parent to TagToAdd
				foreach (var parentTag in tagNode.SingleTagContainer.ParentTags)
				{
					if (HasTagExact(parentTag))
					{
						RemoveTag(parentTag);
					}
				}
			}

			// Add the tag
			AddTag(tagToAdd);
			return true;
		}

		public bool RemoveTag(GameplayTag tagToRemove, bool deferParentTags = false)
		{
			if (GameplayTags.Remove(tagToRemove))
			{
				if (!deferParentTags)
				{
					// Have to recompute parent table from scratch because there could be duplicates providing the same parent tag
					FillParentTags();
				}

				return true;
			}

			return false;
		}

		public void RemoveTags(GameplayTagContainer tagsToRemove)
		{
			bool changed = false;

			foreach (var tag in tagsToRemove)
			{
				changed = changed && GameplayTags.Remove(tag);
			}

			if (changed)
			{
				// Recompute once at the end
				FillParentTags();
			}
		}

		public void Reset()
		{
			GameplayTags.Clear();

			// ParentTags is usually around size of GameplayTags on average
			ParentTags.Clear();
		}

		public GameplayTagContainer GetGameplayTagParents()
		{
			GameplayTagContainer resultContainer = new()
			{
				// Should be the same or not?
				_gameplayTags = new List<GameplayTag>(_gameplayTags)
			};

			// Add parent tags to explicit tags, the rest got copied over already
			foreach (var tag in _parentTags)
			{
				if (!resultContainer._gameplayTags.Contains(tag))
				{
					resultContainer._gameplayTags.Add(tag);
				}
			}

			return resultContainer;
		}

		/**
		 * Determine if TagToCheck is present in this container, also checking against parent tags
		 * {"A.1"}.HasTag("A") will return True, {"A"}.HasTag("A.1") will return False
		 * If TagToCheck is not Valid it will always return False
		 * 
		 * @return True if TagToCheck is in this container, false if it is not
		 */
		public bool HasTag(GameplayTag tagToCheck)
		{
			if (tagToCheck == GameplayTag.EmptyTag)
			{
				return false;
			}

			return _gameplayTags.Contains(tagToCheck) || _parentTags.Contains(tagToCheck);
		}

		/**
		 * Determine if TagToCheck is explicitly present in this container, only allowing exact matches
		 * {"A.1"}.HasTagExact("A") will return False
		 * If TagToCheck is not Valid it will always return False
		 * 
		 * @return True if TagToCheck is in this container, false if it is not
		 */
		public bool HasTagExact(GameplayTag tagToCheck)
		{
			if (tagToCheck == GameplayTag.EmptyTag)
			{
				return false;
			}

			return _gameplayTags.Contains(tagToCheck);
		}

		/**
		 * Checks if this container contains ANY of the tags in the specified container, also checks against parent tags
		 * {"A.1"}.HasAny({"A","B"}) will return True, {"A"}.HasAny({"A.1","B"}) will return False
		 * If ContainerToCheck is empty/invalid it will always return False
		 *
		 * @return True if this container has ANY of the tags of in ContainerToCheck
		 */
		public bool HasAny(GameplayTagContainer containerToCheck)
		{
			if (containerToCheck.IsEmpty || containerToCheck == EmptyContainer)
			{
				return false;
			}

			foreach (var otherTag in containerToCheck)
			{
				if (_gameplayTags.Contains(otherTag) || _parentTags.Contains(otherTag))
				{
					return true;
				}
			}

			return false;
		}

		/**
		 * Checks if this container contains ANY of the tags in the specified container, only allowing exact matches
		 * {"A.1"}.HasAny({"A","B"}) will return False
		 * If ContainerToCheck is empty/invalid it will always return False
		 *
		 * @return True if this container has ANY of the tags of in ContainerToCheck
		 */
		public bool HasAnyExact(GameplayTagContainer containerToCheck)
		{
			if (containerToCheck.IsEmpty || containerToCheck == EmptyContainer)
			{
				return false;
			}

			foreach (var otherTag in containerToCheck)
			{
				if (_gameplayTags.Contains(otherTag))
				{
					return true;
				}
			}

			return false;
		}

		/**
		 * Checks if this container contains ALL of the tags in the specified container, also checks against parent tags
		 * {"A.1","B.1"}.HasAll({"A","B"}) will return True, {"A","B"}.HasAll({"A.1","B.1"}) will return False
		 * If ContainerToCheck is empty/invalid it will always return True, because there were no failed checks
		 *
		 * @return True if this container has ALL of the tags of in ContainerToCheck, including if ContainerToCheck is empty
		 */
		public bool HasAll(GameplayTagContainer containerToCheck)
		{
			if (containerToCheck.IsEmpty || containerToCheck == EmptyContainer)
			{
				return false;
			}

			foreach (var otherTag in containerToCheck)
			{
				if (!_gameplayTags.Contains(otherTag) && !_parentTags.Contains(otherTag))
				{
					return false;
				}
			}

			return true;
		}

		/**
		 * Checks if this container contains ALL of the tags in the specified container, only allowing exact matches
		 * {"A.1","B.1"}.HasAll({"A","B"}) will return False
		 * If ContainerToCheck is empty/invalid it will always return True, because there were no failed checks
		 *
		 * @return True if this container has ALL of the tags of in ContainerToCheck, including if ContainerToCheck is empty
		 */
		public bool HasAllExact(GameplayTagContainer containerToCheck)
		{
			if (containerToCheck.IsEmpty || containerToCheck == EmptyContainer)
			{
				return false;
			}

			foreach (var otherTag in containerToCheck)
			{
				if (!_gameplayTags.Contains(otherTag))
				{
					return false;
				}
			}

			return true;
		}

		public IEnumerator<GameplayTag> GetEnumerator()
		{
			return _gameplayTags.GetEnumerator();
		}

		IEnumerator IEnumerable.GetEnumerator()
		{
			return _gameplayTags.GetEnumerator();
		}

		public void CopyDataFrom(GameplayTagContainer otherContainer)
		{
			if (otherContainer.Equals(this))
			{
				return;
			}

			_gameplayTags.Clear();
			_gameplayTags.AddRange(otherContainer._gameplayTags);

			_parentTags.Clear();
			_parentTags.AddRange(otherContainer._parentTags);
		}

		public static bool operator ==(GameplayTagContainer a, GameplayTagContainer b)
		{
			// This is to handle the case where the two containers are in different orders
			if (a._gameplayTags.Count != b._gameplayTags.Count)
			{
				return false;
			}

			return a.HasAllExact(b);
		}

		public static bool operator !=(GameplayTagContainer a, GameplayTagContainer b)
		{
			return !(a == b);
		}

		public override bool Equals(object? obj)
		{
			if (obj == null)
			{
				return false;
			}

			if (obj is not GameplayTagContainer container)
			{
				throw new ArgumentException("Compared object must be of type TagName.");
			}

			return _gameplayTags.Equals(container._gameplayTags);
		}

		public override int GetHashCode()
		{
			return _gameplayTags?.GetHashCode() ?? 0;
		}
	}
}
