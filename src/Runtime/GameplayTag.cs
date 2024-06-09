using System.Text;

namespace GameplayTags.Runtime
{
	public struct GameplayTag : IComparable, IComparable<GameplayTag>, IEquatable<GameplayTag>
	{
		public static GameplayTag EmptyTag = default;

		public readonly TagName TagName { get; }

		public bool IsValid => this != EmptyTag;

		[Obsolete("Use GameplayTag.RequestGameplayTag() instead.", true)]
		public GameplayTag()
		{
			TagName = new TagName();
		}

		internal GameplayTag(TagName tagName)
		{
			TagName = tagName;
		}

		public static GameplayTag RequestGameplayTag(TagName tagName, bool errorIfNotFound = true)
		{
			return GameplayTagsManager.Instance.RequestGameplayTag(tagName, errorIfNotFound);
		}

		public override string ToString()
		{
			return TagName.ToString();
		}

		public GameplayTagContainer GetSingleTagContainer()
		{
			var tagNode = GameplayTagsManager.Instance.FindTagNode(this);

			if (tagNode != null)
			{
				return tagNode.SingleTagContainer;
			}

			// assert(!IsValid)
			return GameplayTagContainer.EmptyContainer;
		}

		public GameplayTag RequestDirectParent()
		{
			return GameplayTagsManager.Instance.RequestGameplayTagDirectParent(this);
		}

		public GameplayTagContainer GetGameplayTagParents()
		{
			return GameplayTagsManager.Instance.RequestGameplayTagDirectParents(this);
		}

		// Needs heavy testing
		public void ParseParentTags(List<GameplayTag> uniqueParentTags)
		{
			// This needs to be in the same order as the gameplay tag node ParentTags, which is immediate parent first
			var rawTag = TagName;
			// For efficiency?
			var tagBuffer = new StringBuilder(rawTag.ToString());

			int dotIndex = rawTag.ToString().LastIndexOf('.');

			while (dotIndex != -1)
			{
				// Remove everything starting with the last dot
				var parent = rawTag.ToString().Substring(dotIndex);

				dotIndex = parent.LastIndexOf('.');

				// Add the name to the array
				var parentTag = new GameplayTag(TagName.FromString(parent));

				if (!uniqueParentTags.Contains(parentTag))
				{
					uniqueParentTags.Add(parentTag);
				}
			}
		}

		/**
		 * Determine if this tag matches TagToCheck, expanding our parent tags
		 * "A.1".MatchesTag("A") will return True, "A".MatchesTag("A.1") will return False
		 * If TagToCheck is not Valid it will always return False
		 * 
		 * @return True if this tag matches TagToCheck
		 */
		// runtime
		public bool MatchesTag(GameplayTag tagToCheck)
		{
			var tagNode = GameplayTagsManager.Instance.FindTagNode(this);

			if (tagNode != null)
			{
				return tagNode.SingleTagContainer.HasTag(tagToCheck);
			}

			return false;
		}

		/**
		 * Determine if TagToCheck is valid and exactly matches this tag
		 * "A.1".MatchesTagExact("A") will return False
		 * If TagToCheck is not Valid it will always return False
		 * 
		 * @return True if TagToCheck is Valid and is exactly this tag
		 */
		// runtime
		public bool MatchesTagExact(GameplayTag tagToCheck)
		{
			if (tagToCheck == EmptyTag)
			{
				return false;
			}

			// Only check check explicit tag list
			return TagName == tagToCheck.TagName;
		}

		/**
		 * Checks if this tag matches ANY of the tags in the specified container, also checks against our parent tags
		 * "A.1".MatchesAny({"A","B"}) will return True, "A".MatchesAny({"A.1","B"}) will return False
		 * If ContainerToCheck is empty/invalid it will always return False
		 *
		 * @return True if this tag matches ANY of the tags of in ContainerToCheck
		 */
		// runtime
		public bool MatchesAny(GameplayTagContainer containerToCheck)
		{
			var tagNode = GameplayTagsManager.Instance.FindTagNode(this);

			if (tagNode != null)
			{
				return tagNode.SingleTagContainer.HasAny(containerToCheck);
			}

			return false;
		}

		/**
		 * Checks if this tag matches ANY of the tags in the specified container, only allowing exact matches
		 * "A.1".MatchesAny({"A","B"}) will return False
		 * If ContainerToCheck is empty/invalid it will always return False
		 *
		 * @return True if this tag matches ANY of the tags of in ContainerToCheck exactly
		 */
		// runtime
		public bool MatchesAnyExact(GameplayTagContainer containerToCheck)
		{
			if (containerToCheck.IsEmpty)
			{
				return false;
			}

			return containerToCheck.GameplayTags.Contains(this);
		}

		/**
		 * Check to see how closely two FGameplayTags match. Higher values indicate more matching terms in the tags.
		 *
		 * @param TagToCheck	Tag to match against
		 *
		 * @return The depth of the match, higher means they are closer to an exact match
		 */
		// runtime
		public int MatchesTagDepth(GameplayTag tagToCheck)
		{
			return GameplayTagsManager.Instance.GameplayTagsMatchDepth(this, tagToCheck);
		}

		public bool NetSerialize()
		{
			var netIndex = GameplayTagsManager.Instance.GetNetIndexFromTag(this);
			return true;
		}

		public bool NetDeserialize()
		{
			// Read netIndex from buffer
			// 32? shouldn't be 16?
			//Ar.SerializeIntPacked(NetIndex32);
			ushort netIndex = 0;

			// This should actually change the TagName, so... no read-only?
			var tagName = GameplayTagsManager.Instance.GetTagNameFromNetIndex(netIndex);
			return true;
		}

		public override bool Equals(object? obj)
		{
			if (obj == null)
			{
				return false;
			}

			if (obj is not GameplayTag gameplayTag)
			{
				throw new ArgumentException("Compared object must be of type GameplayTag.");
			}

			return TagName.Equals(gameplayTag.TagName);
		}

		public bool Equals(GameplayTag other)
		{
			return TagName.Equals(other.TagName);
		}

		public override int GetHashCode()
		{
			return TagName.GetHashCode();
		}

		public int CompareTo(object? obj)
		{
			if (obj == null)
			{
				return 1;
			}

			if (obj is not GameplayTag gameplayTag)
			{
				throw new ArgumentException("Compared object must be of type GameplayTag.");
			}

			return TagName.CompareTo(gameplayTag.TagName);
		}

		public int CompareTo(GameplayTag other)
		{
			return TagName.CompareTo(other.TagName);
		}

		public static bool operator ==(GameplayTag a, GameplayTag b) { return a.Equals(b); }
		public static bool operator !=(GameplayTag a, GameplayTag b) { return !a.Equals(b); }
		public static bool operator <(GameplayTag a, GameplayTag b) { return a.CompareTo(b) < 0; }
		public static bool operator >(GameplayTag a, GameplayTag b) { return a.CompareTo(b) > 0; }
		public static bool operator <=(GameplayTag a, GameplayTag b) { return a.CompareTo(b) <= 0; }
		public static bool operator >=(GameplayTag a, GameplayTag b) { return a.CompareTo(b) >= 0; }
	}
}
