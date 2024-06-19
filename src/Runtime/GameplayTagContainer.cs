using System.Collections;
using System.Text;

namespace GameplayTags.Runtime;

/// <summary>
/// A <see cref="GameplayTagContainer"/> holds a collection of <see cref="GameplayTag"/>s, tags are included explicitly
/// by adding them, and implicitly from adding child tags.
/// </summary>
public readonly struct GameplayTagContainer : IEnumerable<GameplayTag>
{
	/// <summary>
	/// Gets an empty <see cref="GameplayTagContainer"/>.
	/// </summary>
	public static GameplayTagContainer EmptyContainer => default;

	/// <summary>
	/// Gets the set of <see cref="GameplayTag"/>s in this container.
	/// </summary>
	public readonly HashSet<GameplayTag> GameplayTags { get; } = new ();

	/// <summary>
	/// Gets a set of expanded parent tags, in addition to <see cref="GameplayTag"/>.
	/// </summary>
	/// <remarks>Used to accelerate parent searches.</remarks>
	public readonly HashSet<GameplayTag> ParentTags { get; } = new ();

	/// <summary>
	/// Gets the number of explicitly added tags.
	/// </summary>
	public readonly int Count => GameplayTags.Count;

	/// <summary>
	/// Gets a value indicating whether the container has any valid tags.
	/// </summary>
	public readonly bool IsValid => GameplayTags.Count > 0;

	/// <summary>
	/// Gets a value indicating whether the container is empty or not.
	/// </summary>
	public readonly bool IsEmpty => GameplayTags.Count == 0;

	/// <summary>
	/// Initializes a new instance of the <see cref="GameplayTagContainer"/> struct.
	/// </summary>
	public GameplayTagContainer()
	{
	}

	/// <summary>
	/// Initializes a new instance of the <see cref="GameplayTagContainer"/> struct.
	/// </summary>
	/// <param name="tag">A <see cref="GameplayTag"/> to be added in the container.</param>
	public GameplayTagContainer(GameplayTag tag)
	{
		AddTag(tag);
	}

	/// <summary>
	/// Initializes a new instance of the <see cref="GameplayTagContainer"/> struct copying the values from another
	/// <see cref="GameplayTagContainer"/>.
	/// </summary>
	/// <param name="other">The other <see cref="GameplayTagContainer"/> to copy the values from.</param>
	public GameplayTagContainer(GameplayTagContainer other)
	{
		GameplayTags.UnionWith(other.GameplayTags);
		ParentTags.UnionWith(other.ParentTags);
	}

	/// <summary>
	/// Initializes a new instance of the <see cref="GameplayTagContainer"/> struct based on a list of
	/// <see cref="GameplayTag"/>s.
	/// </summary>
	/// <param name="sourceTags">The set of <see cref="GameplayTag"/>s to initialize this container with.</param>
	public GameplayTagContainer(HashSet<GameplayTag> sourceTags)
	{
		GameplayTags.UnionWith(sourceTags);
		FillParentTags();
	}

	/// <summary>
	/// Add the specified <see cref="GameplayTag"/> to the container.
	/// </summary>
	/// <param name="tagToAdd"><see cref="GameplayTag"/> to add to the container.</param>
	public void AddTag(GameplayTag tagToAdd)
	{
		if (tagToAdd != GameplayTag.EmptyTag && GameplayTags.Add(tagToAdd))
		{
			GameplayTagsManager.Instance.ExtractParentTags(tagToAdd, ParentTags);
		}
	}

	/// <summary>
	/// Removes a specified <see cref="GameplayTag"/> from the container if present.
	/// </summary>
	/// <param name="tagToRemove"><see cref="GameplayTag"/> to remove from the container.</param>
	/// <param name="deferParentTags">Skip calling <see cref="FillParentTags"/> for performance.
	/// <para>If <see langword="true"/>, <see cref="FillParentTags"/> must be handled by calling code.</para>
	/// </param>
	/// <returns><see langword="true"/> if the tag was removed; otherwise, <see langword="false"/>.</returns>
	public bool RemoveTag(GameplayTag tagToRemove, bool deferParentTags = false)
	{
		if (GameplayTags.Remove(tagToRemove))
		{
			if (!deferParentTags)
			{
				// Have to recompute parent table from scratch because there could be duplicates providing the same
				// parent tag.
				FillParentTags();
			}

			return true;
		}

		return false;
	}

	/// <summary>
	/// Removes all tags in <paramref name="tagsToRemove"/> from this container.
	/// </summary>
	/// <param name="tagsToRemove"><see cref="GameplayTagContainer"/> with the tags to remove from the container.
	/// </param>
	public void RemoveTags(GameplayTagContainer tagsToRemove)
	{
		bool changed = false;

		foreach (var tag in tagsToRemove)
		{
			changed = changed || GameplayTags.Remove(tag);
		}

		if (changed)
		{
			// Recompute once at the end.
			FillParentTags();
		}
	}

	/// <summary>
	/// Fills in <see cref="ParentTags"/> from <see cref="GameplayTags"/>.
	/// </summary>
	public void FillParentTags()
	{
		ParentTags.Clear();

		if (GameplayTags.Count > 0)
		{
			var tagsManager = GameplayTagsManager.Instance;

			foreach (var tag in GameplayTags)
			{
				tagsManager.ExtractParentTags(tag, ParentTags);
			}
		}
	}

	/// <summary>
	/// Remove all tags from the container. Will maintain slack by default.
	/// </summary>
	/// <param name="capacity">The desired initial capacity after the reset.</param>
	public void Reset(int capacity)
	{
		GameplayTags.Clear();
		GameplayTags.EnsureCapacity(capacity);

		// ParentTags is usually around size of GameplayTags on average.
		ParentTags.Clear();
		ParentTags.EnsureCapacity(capacity);
	}

	/// <summary>
	/// Determine if <paramref name="tagToCheck"/> is present in this container, also checking against parent tags.
	/// </summary>
	/// <remarks>
	/// <para>{"A.1"}.HasTag("A") will return <see langword="true"/>, {"A"}.HasTag("A.1") will return
	/// <see langword="false"/>.</para>
	/// <para>If <paramref name="tagToCheck"/> is not Valid it will always return <see langword="false"/>.</para>
	/// </remarks>
	/// <param name="tagToCheck"><see cref="GameplayTag"/> to check against this container.</param>
	/// <returns><see langword="true"/> if <paramref name="tagToCheck"/> is in this container, <see langword="false"/>
	/// if it is not.</returns>
	public readonly bool HasTag(GameplayTag tagToCheck)
	{
		if (!tagToCheck.IsValid)
		{
			return false;
		}

		return GameplayTags.Contains(tagToCheck) || ParentTags.Contains(tagToCheck);
	}

	/// <summary>
	/// Determine if <paramref name="tagToCheck"/> is explicitly present in this container, only allowing exact matches.
	/// </summary>
	/// <remarks>
	/// <para>{"A.1"}.HasTagExact("A") will return <see langword="false"/>.</para>
	/// <para>If <paramref name="tagToCheck"/> is not Valid it will always return <see langword="false"/>.</para>
	/// </remarks>
	/// <param name="tagToCheck"><see cref="GameplayTag"/> to check against this container.</param>
	/// <returns><see langword="true"/> if <paramref name="tagToCheck"/> is in this container, <see langword="false"/>
	/// if it is not.</returns>
	public readonly bool HasTagExact(GameplayTag tagToCheck)
	{
		if (!tagToCheck.IsValid)
		{
			return false;
		}

		return GameplayTags.Contains(tagToCheck);
	}

	/// <summary>
	/// Checks if this container contains ANY of the tags in the specified container, also checks against parent tags.
	/// </summary>
	/// <remarks>
	/// <para>{"A.1"}.HasAny({"A","B"}) will return True, {"A"}.HasAny({"A.1","B"}) will return <see langword="false"/>.
	/// </para>
	/// <para>If <paramref name="containerToCheck"/> is empty/invalid it will always return <see langword="false"/>.
	/// </para>
	/// </remarks>
	/// <param name="containerToCheck"><see cref="GameplayTagContainer"/> to check against this container.</param>
	/// <returns><see langword="true"/> if this container has ANY of the tags of in <paramref name="containerToCheck"/>.
	/// </returns>
	public readonly bool HasAny(GameplayTagContainer containerToCheck)
	{
		if (containerToCheck.IsEmpty)
		{
			return false;
		}

		foreach (var otherTag in containerToCheck)
		{
			if (GameplayTags.Contains(otherTag) || ParentTags.Contains(otherTag))
			{
				return true;
			}
		}

		return false;
	}

	/// <summary>
	/// Checks if this container contains ANY of the tags in the specified container, only allowing exact matches.
	/// </summary>
	/// <remarks>
	/// <para>{"A.1"}.HasAny({"A","B"}) will return <see langword="false"/>.</para>
	/// <para>If <paramref name="containerToCheck"/> is empty/invalid it will always return <see langword="false"/>.
	/// </para>
	/// </remarks>
	/// <param name="containerToCheck"><see cref="GameplayTagContainer"/> to check against this container.</param>
	/// <returns><see langword="true"/> if this container has ANY of the tags of in <paramref name="containerToCheck"/>.
	/// </returns>
	public readonly bool HasAnyExact(GameplayTagContainer containerToCheck)
	{
		if (containerToCheck.IsEmpty)
		{
			return false;
		}

		foreach (var otherTag in containerToCheck)
		{
			if (GameplayTags.Contains(otherTag))
			{
				return true;
			}
		}

		return false;
	}

	/// <summary>
	/// Checks if this container contains ALL of the tags in the specified container, also checks against parent tags.
	/// </summary>
	/// <remarks>
	/// <para>{"A.1","B.1"}.HasAll({"A","B"}) will return <see langword="true"/>, {"A","B"}.HasAll({"A.1","B.1"}) will
	/// return <see langword="false"/>.</para>
	/// <para>If <paramref name="containerToCheck"/> is empty/invalid it will always return <see langword="true"/>,
	/// because there were no failed checks.</para>
	/// </remarks>
	/// <param name="containerToCheck"><see cref="GameplayTagContainer"/> to check against this container.</param>
	/// <returns><see langword="true"/> if this container has ALL of the tags of in <paramref name="containerToCheck"/>,
	/// including if <paramref name="containerToCheck"/> is empty.</returns>
	public readonly bool HasAll(GameplayTagContainer containerToCheck)
	{
		if (containerToCheck.IsEmpty)
		{
			return true;
		}

		foreach (var otherTag in containerToCheck)
		{
			if (!GameplayTags.Contains(otherTag) && !ParentTags.Contains(otherTag))
			{
				return false;
			}
		}

		return true;
	}

	/// <summary>
	/// Checks if this container contains ALL of the tags in the specified container, only allowing exact matches.
	/// </summary>
	/// <remarks>
	/// <para>{"A.1","B.1"}.HasAll({"A","B"}) will return <see langword="false"/>.</para>
	/// <para>If <paramref name="containerToCheck"/> is empty/invalid it will always return <see langword="true"/>,
	/// because there were no failed checks.</para>
	/// </remarks>
	/// <param name="containerToCheck"><see cref="GameplayTagContainer"/> to check against this container.</param>
	/// <returns><see langword="true"/> if this container has ALL of the tags of in <paramref name="containerToCheck"/>,
	/// including if <paramref name="containerToCheck"/> is empty.</returns>
	public readonly bool HasAllExact(GameplayTagContainer containerToCheck)
	{
		if (containerToCheck.IsEmpty)
		{
			return true;
		}

		foreach (var otherTag in containerToCheck)
		{
			if (!GameplayTags.Contains(otherTag))
			{
				return false;
			}
		}

		return true;
	}

	/// <summary>
	/// Returns a filtered version of this container, returns all tags that match against any of the tags in
	/// <paramref name="otherContainer"/>, considering its expanding parents.
	/// </summary>
	/// <param name="otherContainer">The <see cref="GameplayTagContainer"/> to filter against.</param>
	/// <returns>A <see cref="GameplayTagContainer"/> containing the filtered tags.</returns>
	public readonly GameplayTagContainer Filter(GameplayTagContainer otherContainer)
	{
		GameplayTagContainer resultContainer = new ();

		foreach (var tag in GameplayTags)
		{
			if (tag.MatchesAny(otherContainer))
			{
				resultContainer.AddTagFast(tag);
			}
		}

		return resultContainer;
	}

	/// <summary>
	/// Returns a filtered version of this container, returns all tags that match exactly one in
	/// <paramref name="otherContainer"/>.
	/// </summary>
	/// <param name="otherContainer">The <see cref="GameplayTagContainer"/> to filter against.</param>
	/// <returns>A <see cref="GameplayTagContainer"/> containing the filtered tags.</returns>
	public readonly GameplayTagContainer FilterExact(GameplayTagContainer otherContainer)
	{
		GameplayTagContainer resultContainer = new ();

		foreach (var tag in GameplayTags)
		{
			if (tag.MatchesAnyExact(otherContainer))
			{
				resultContainer.AddTagFast(tag);
			}
		}

		return resultContainer;
	}

	/// <summary>
	/// Checks if this container matches the given query.
	/// </summary>
	/// <param name="query"><see cref="GameplayTagQuery"/> we are checking against.</param>
	/// <returns><see langword="true"/> if this container matches the query, <see langword="false"/> otherwise.
	/// </returns>
	public readonly bool MatchesQuery(GameplayTagQuery query)
	{
		return query.Matches(this);
	}

	/// <summary>
	/// Adds all the tags from one container to this container.
	/// </summary>
	/// <remarks>
	/// NOTE: From set theory, this effectively is the union of the container this is called on with
	/// <paramref name="other"/>.</remarks>
	/// <param name="other"><see cref="GameplayTagContainer"/> that has the tags you want to add to this container.
	/// </param>
	public void AppendTags(GameplayTagContainer other)
	{
		GameplayTags.EnsureCapacity(GameplayTags.Count + other.GameplayTags.Count);
		ParentTags.EnsureCapacity(ParentTags.Count + other.ParentTags.Count);

		foreach (var otherTag in other.GameplayTags)
		{
			GameplayTags.Add(otherTag);
		}

		foreach (var otherTag in other.ParentTags)
		{
			ParentTags.Add(otherTag);
		}
	}

	/// <summary>
	/// Adds all the tags that match between the two specified containers to this container. WARNING: This matches any
	/// parent tag in <paramref name="otherA"/>, not just exact matches! So while this should look like the union of the
	/// container this is called on with the intersection of <paramref name="otherA"/> and <paramref name="otherB"/>,
	/// it's not exactly that. Since <paramref name="otherB"/> matches against its parents,
	/// any tag in <paramref name="otherA"/> which has a parent match with a direct tag of <paramref name="otherB"/>
	/// will count. For example, if <paramref name="otherA"/> has 'Color.Green' and <paramref name="otherB"/> has
	/// 'Color', that will count as a match due to the Color parent match.
	/// </summary>
	/// <remarks>
	/// If you want an exact match, you need to call A.FilterExact(B) to get the intersection of A with B.
	/// If you need the disjunctive union (the union of two sets minus their intersection), use AppendTags to create
	/// Union, FilterExact to create Intersection, and then call Union.RemoveTags(Intersection).
	/// </remarks>
	/// <param name="otherA"> that has the matching tags you want to add to this
	/// container, these tags have their parents expanded.</param>
	/// <param name="otherB"><see cref="GameplayTagContainer"/> used to check for matching tags. If the tag matches on
	/// any parent, it counts as a match.</param>
	public void AppendMatchingTags(GameplayTagContainer otherA, GameplayTagContainer otherB)
	{
		foreach (var otherATag in otherA.GameplayTags)
		{
			if (otherATag.MatchesAny(otherB))
			{
				AddTag(otherATag);
			}
		}
	}

	/// <summary>
	/// Efficient network serialize, takes advantage of the dictionary.
	/// TODO: Not really implemented yet.
	/// </summary>
	/// <returns><see langword="true"/> if successfully serialized; <see langword="false"/> otherwise.</returns>
	public readonly bool NetSerialize()
	{
		// 1st bit to indicate empty tag container or not (empty tag containers are frequently replicated). Early out if empty.
		var isEmpty = (GameplayTags.Count == 0) ? (byte)1 : (byte)0;

		// Write isEmpty
		if (isEmpty == 1)
		{
			if (GameplayTags.Count > 0)
			{
				Reset(GameplayTags.Count);
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

		foreach (var tag in GameplayTags)
		{
			//tag.NetSerialize();
		}

		//for (int i = 0; i < numTags; ++i)
		//{
		//	GameplayTag Tag = GameplayTags[i];
		//	Tag.NetSerialize();

		//	//#if !(UE_BUILD_SHIPPING || UE_BUILD_TEST)
		//	//				UGameplayTagsManager::Get().NotifyTagReplicated(Tag, true);
		//	//#endif
		//}

		return true;
	}

	/// <summary>
	/// Efficient network deserialize, takes advantage of the dictionary.
	/// TODO: Not really implemented yet.
	/// </summary>
	/// <returns><see langword="true"/> if successfully deserialized; <see langword="false"/> otherwise.</returns>
	public readonly bool NetDeserialize()
	{
		// No Common Container tags, just replicate this like normal
		byte numTags = 0;

		// Read into numTags
		//Ar.SerializeBits(&NumTags, NumBitsForContainerSize);

		GameplayTags.Clear();
		GameplayTags.EnsureCapacity(numTags);

		foreach (var tag in GameplayTags)
		{
			//tag.NetDeserialize();
		}

		//for (byte i = 0; i < numTags; ++i)
		//{
		//	GameplayTags[i].NetDeserialize();
		//}

		FillParentTags();

		return true;
	}

	/// <summary>
	///  Returns a <see cref="string"/> representation of the <see cref="GameplayTagContainer"/>.
	/// </summary>
	/// <returns>The <see cref="GameplayTagContainer"/> as a <see cref="string"/>.</returns>
	public readonly override string ToString()
	{
		var stringBuilder = new StringBuilder();

		foreach (var tag in GameplayTags)
		{
			stringBuilder.Append($"\"{tag}\"");
			stringBuilder.Append(", ");
		}

		stringBuilder.Remove(GameplayTags.Count - 2, 2);

		return stringBuilder.ToString();
	}

	/// <summary>
	/// Determines wether this instance and another specified <see cref="GameplayTagContainer"/> object have the same
	/// value.
	/// </summary>
	/// <param name="obj">The other <see cref="object"/> to compare against.</param>
	/// <returns><see langword="true"/> if the value of the <paramref name="obj"/> parameter is the same as the value of
	/// this instance; otherwise, <see langword="false"/>. If <paramref name="obj"/> is <see langword="null"/>, the
	/// method returns <see langword="false"/>.</returns>
	public readonly override bool Equals(object? obj)
	{
		if (obj is null)
		{
			return false;
		}

		if (obj is not GameplayTagContainer container)
		{
			return false;
		}

		return GameplayTags == container.GameplayTags;
	}

	/// <summary>
	/// Returns the hash code for this <see cref="GameplayTagContainer"/>.
	/// </summary>
	/// <returns>A 32-bit signed integer hash code.</returns>
	public readonly override int GetHashCode()
	{
		return GameplayTags?.GetHashCode() ?? 0;
	}

	/// <summary>
	/// Returns an enumerator that iterates through a <see cref="GameplayTagContainer"/> object.
	/// </summary>
	/// <returns>A <see cref="HashSet{GameplayTags}.Enumerator"/> object for the <see cref="GameplayTagContainer"/>
	/// object.</returns>
	public IEnumerator<GameplayTag> GetEnumerator()
	{
		return GameplayTags.GetEnumerator();
	}

	/// <summary>
	/// Returns an enumerator that iterates through a <see cref="GameplayTagContainer"/> object.
	/// </summary>
	/// <returns>A <see cref="HashSet{GameplayTags}.Enumerator"/> object for the <see cref="GameplayTagContainer"/>
	/// object.</returns>
	IEnumerator IEnumerable.GetEnumerator()
	{
		return GameplayTags.GetEnumerator();
	}

	/// <summary>
	/// Add the specified <see cref="GameplayTag"/> to the container without validating.
	/// </summary>
	/// <remarks>Useful when building container from another data struct or safe contexts.</remarks>
	/// <param name="tagToAdd"><see cref="GameplayTag"/> to add to the container.</param>
#pragma warning disable T0009 // Internal Styling Rule T0009
	internal void AddTagFast(GameplayTag tagToAdd)
	{
		if (GameplayTags.Add(tagToAdd))
		{
			GameplayTagsManager.Instance.ExtractParentTags(tagToAdd, ParentTags);
		}
	}

	/// <summary>
	/// Returns a new container explicitly containing the tags of this container and all of their parent tags.
	/// </summary>
	/// <returns>A new container explicitly containing the tags of this container and all of their parent tags.
	/// </returns>
	internal GameplayTagContainer GetExplicitGameplayTagParents()
	{
		GameplayTagContainer resultContainer = new (GameplayTags);

		// Add parent tags to explicit tags, the rest got copied over already
		foreach (var tag in ParentTags)
		{
			resultContainer.GameplayTags.Add(tag);
		}

		return resultContainer;
	}

	public static bool operator ==(GameplayTagContainer a, GameplayTagContainer b)
	{
		if (a.GameplayTags.Count != b.GameplayTags.Count)
		{
			return false;
		}

		return a.HasAllExact(b);
	}

	public static bool operator !=(GameplayTagContainer a, GameplayTagContainer b)
	{
		return !(a == b);
	}
}
