using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml.Linq;

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
		// No tests written for that yet
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
				_gameplayTags.AddUnique(tagToAdd);
				//if (!_gameplayTags.Contains(tagToAdd))
				//{
				//	_gameplayTags.Add(tagToAdd);
				//}

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
				resultContainer._gameplayTags.AddUnique(tag);
				//if (!resultContainer._gameplayTags.Contains(tag))
				//{
				//	resultContainer._gameplayTags.Add(tag);
				//}
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

		/**
		 * Returns a filtered version of this container, returns all tags that match against any of the tags in OtherContainer, expanding parents
		 * @param OtherContainer		The Container to filter against
		 * @return A FGameplayTagContainer containing the filtered tags
		 */
		public GameplayTagContainer Filter(GameplayTagContainer otherContainer)
		{
			GameplayTagContainer resultContainer = new ();

			foreach (GameplayTag tag in GameplayTags)
	{
				if (tag.MatchesAny(otherContainer))
				{
					resultContainer.AddTagFast(tag);
				}
			}

			return resultContainer;
		}

		/**
		 * Returns a filtered version of this container, returns all tags that match exactly one in OtherContainer
		 * @param OtherContainer		The Container to filter against
		 * @return A FGameplayTagContainer containing the filtered tags
		 */
		public GameplayTagContainer FilterExact(GameplayTagContainer otherContainer)
		{
			GameplayTagContainer resultContainer = new();

			foreach (GameplayTag tag in GameplayTags)
	{
				if (tag.MatchesAnyExact(otherContainer))
				{
					resultContainer.AddTagFast(tag);
				}
			}

			return resultContainer;
		}

		/** 
		 * Checks if this container matches the given query.
		 *
		 * @param Query		Query we are checking against
		 *
		 * @return True if this container matches the query, false otherwise.
		 */
		public bool MatchesQuery(GameplayTagQuery query)
		{
			return query.Matches(this);
		}

		/** 
		 * Adds all the tags from one container to this container 
		 * NOTE: From set theory, this effectively is the union of the container this is called on with Other.
		 *
		 * @param Other TagContainer that has the tags you want to add to this container 
		 */
		public void AppendTags(GameplayTagContainer other)
		{
			GameplayTags.Capacity = GameplayTags.Count + other.GameplayTags.Count;
			ParentTags.Capacity = ParentTags.Count + other.ParentTags.Count;

			// Add other container's tags to our own
			foreach (GameplayTag otherTag in other.GameplayTags)
			{
				GameplayTags.AddUnique(otherTag);
			}

			foreach (GameplayTag otherTag in other.ParentTags)
			{
				ParentTags.AddUnique(otherTag);
			}
		}

		/** 
		 * Adds all the tags that match between the two specified containers to this container.  WARNING: This matches any
		 * parent tag in A, not just exact matches!  So while this should be the union of the container this is called on with
		 * the intersection of OtherA and OtherB, it's not exactly that.  Since OtherB matches against its parents, any tag
		 * in OtherA which has a parent match with a parent of OtherB will count.  For example, if OtherA has Color.Green
		 * and OtherB has Color.Red, that will count as a match due to the Color parent match!
		 * 
		 * !!! This comment is wrong, it wont match unless Color is on one of the container. !!!
		 * 
		 * If you want an exact match, you need to call A.FilterExact(B) (above) to get the intersection of A with B.
		 * If you need the disjunctive union (the union of two sets minus their intersection), use AppendTags to create
		 * Union, FilterExact to create Intersection, and then call Union.RemoveTags(Intersection).
		 *
		 * @param OtherA TagContainer that has the matching tags you want to add to this container, these tags have their parents expanded
		 * @param OtherB TagContainer used to check for matching tags.  If the tag matches on any parent, it counts as a match.
		 */
		// This doesn't look all that useful
		public void AppendMatchingTags(GameplayTagContainer otherA, GameplayTagContainer otherB)
		{
			foreach (GameplayTag otherATag in otherA.GameplayTags)
			{
				if (otherATag.MatchesAny(otherB))
				{
					AddTag(otherATag);
				}
			}
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

		public override string ToString()
		{
			var stringBuilder = new StringBuilder();

			for (int i = 0; i < GameplayTags.Count; ++i)
			{
				stringBuilder.Append($"\"{GameplayTags[i].ToString()}\"");
				
				if (i < GameplayTags.Count - 1)
				{
					stringBuilder.Append(", ");
				}
			}

			return stringBuilder.ToString();
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

		public bool NetSerialize()
		{
			// 1st bit to indicate empty tag container or not (empty tag containers are frequently replicated). Early out if empty.
			var isEmpty = (GameplayTags.Count == 0) ? (byte)1 : (byte)0;
			
			// Write isEmpty

			if (isEmpty == 1)
			{
				if (GameplayTags.Count > 0)
				{
					Reset();
				}

				return true;
			}

			// -------------------------------------------------------

			//int numBitsForContainerSize = GameplayTagsManager.Instance.NumBitsForContainerSize;
			int numBitsForContainerSize = 128;

			var numTags = (byte)GameplayTags.Count;
			
			var maxSize = (1 << numBitsForContainerSize) - 1;

			//if (!ensureMsgf(NumTags <= MaxSize, TEXT("TagContainer has %d elements when max is %d! Tags: %s"), NumTags, MaxSize, *ToStringSimple()))
			//{
			//	NumTags = MaxSize;
			//}

			//Ar.SerializeBits(&NumTags, NumBitsForContainerSize);
			// Write NumTags

			for (int i = 0; i < numTags; ++i)
			{
				GameplayTag Tag = GameplayTags[i];
				Tag.NetSerialize();

//#if !(UE_BUILD_SHIPPING || UE_BUILD_TEST)
//				UGameplayTagsManager::Get().NotifyTagReplicated(Tag, true);
//#endif
			}

			return true;
		}

		public bool NetDeserialize()
		{
			// No Common Container tags, just replicate this like normal
			byte numTags = 0;
			
			// Read into numTags
			//Ar.SerializeBits(&NumTags, NumBitsForContainerSize);

			GameplayTags.Clear();
			GameplayTags.Capacity = numTags;

			//GameplayTags.AddDefaulted(NumTags);

			for (byte i = 0; i < numTags; ++i)
			{
				GameplayTags[i].NetDeserialize();
			}

			FillParentTags();

			return true;
		}
	}
}
