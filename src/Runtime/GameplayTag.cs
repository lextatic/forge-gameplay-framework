using System.Text;

namespace GameplayTags.Runtime;

/// <summary>
/// A single gameplay tag, which represents a hierarchical name of the form x.y.z that is registered in the
/// <see cref="GameplayTagsManager"/>.
/// </summary>
public readonly struct GameplayTag : IEquatable<GameplayTag>
{
	/// <summary>
	/// Gets a static representation of an empty <see cref="GameplayTag"/>.
	/// </summary>
	/// <remarks>
	/// Generally used for tag validation.
	/// </remarks>
	public static GameplayTag EmptyTag => default;

	/// <summary>
	/// Gets the <see cref="GameplayTags.Runtime.TagName"/> representing this tag.
	/// </summary>
	public readonly TagName TagName { get; }

	/// <summary>
	/// Gets a value indicating whether this tag is a valid tag.
	/// </summary>
	public bool IsValid => this != EmptyTag;

	/// <summary>
	/// Initializes a new instance of the <see cref="GameplayTag"/> struct.
	/// </summary>
	/// <param name="tagName"><see cref="GameplayTags.Runtime.TagName"/> that's going to represents this tag.</param>
	internal GameplayTag(TagName tagName)
	{
		TagName = tagName;
	}

	/// <summary>
	/// Gets the <see cref="GameplayTag"/> that corresponds to the <see cref="GameplayTags.Runtime.TagName"/>.
	/// </summary>
	/// <param name="tagName">The name of the tag to search for.</param>
	/// <param name="errorIfNotFound">Throws an exception in case the <see cref="GameplayTag"/> doesn't exist.</param>
	/// <returns>Will return the corresponding <see cref="GameplayTag"/> or an <see cref="EmptyTag"/> if not found.
	/// </returns>
	public static GameplayTag RequestGameplayTag(TagName tagName, bool errorIfNotFound = true)
	{
		return GameplayTagsManager.Instance.RequestGameplayTag(tagName, errorIfNotFound);
	}

	/// <summary>
	/// Gets a <see cref="GameplayTagContainer"/> containing only this <see cref="GameplayTag"/>.
	/// </summary>
	/// <returns>A <see cref="GameplayTagContainer"/> containing only this <see cref="GameplayTag"/>.</returns>
	public GameplayTagContainer GetSingleTagContainer()
	{
		var tagNode = GameplayTagsManager.Instance.FindTagNode(this);

		if (tagNode is not null)
		{
			return tagNode.SingleTagContainer;
		}

#if DEBUG
		System.Diagnostics.Debug.Assert(
			!IsValid,
			$"{nameof(GameplayTag)}:{TagName} isn't properly registred in the {nameof(GameplayTagsManager)}.");
#endif

		return GameplayTagContainer.EmptyContainer;
	}

	/// <summary>
	/// Returns direct parent <see cref="GameplayTag"/> of this <see cref="GameplayTag"/>.
	/// </summary>
	/// <remarks>
	/// For example, calling on x.y.z will return x.y.
	/// </remarks>
	/// <returns>This <see cref="GameplayTag"/>'s direct parent.</returns>
	public GameplayTag RequestDirectParent()
	{
		return GameplayTagsManager.Instance.RequestGameplayTagDirectParent(this);
	}

	/// <summary>
	/// Returns a new tag <see cref="GameplayTagContainer"/> that includes this <see cref="GameplayTag"/> and all
	/// parent <see cref="GameplayTag"/> as explicitly added tags.
	/// </summary>
	/// <remarks>
	/// For example, calling this on x.y.z would return a <see cref="GameplayTagContainer"/> with x.y.z, x.y, and x.
	/// </remarks>
	/// <returns>A <see cref="GameplayTagContainer"/> containing all exlicitly added parent <see cref="GameplayTag"/>s.
	/// </returns>
	public GameplayTagContainer GetGameplayTagParents()
	{
		return GameplayTagsManager.Instance.RequestGameplayTagDirectParents(this);
	}

#if DEBUG
	/// <summary>
	/// Parses the tag name and returns a list with raw parent tags, without validating with the
	/// <see cref="GameplayTagsManager"/>.
	/// </summary>
	/// <remarks>
	/// For example, calling this on x.y.z would add x.y and x to the returned array.
	/// </remarks>
	/// <returns>A list containing all raw parent tags for this tag.</returns>
	public List<GameplayTag> ParseParentTags()
	{
		var uniqueParentTags = new List<GameplayTag>();

		// This needs to be in the same order as the gameplay tag node ParentTags, which is immediate parent first.
		var rawTag = TagName;

		int dotIndex = rawTag.ToString().LastIndexOf('.');

		while (dotIndex != -1)
		{
			// Remove everything starting with the last dot.
			var parent = rawTag.ToString().Substring(dotIndex);

			dotIndex = parent.LastIndexOf('.');

			var parentTag = new GameplayTag(TagName.FromString(parent));
			uniqueParentTags.AddUnique(parentTag);
		}

		return uniqueParentTags;
	}
#endif

	/// <summary>
	/// Determine if this tag matches <paramref name="tagToCheck"/>, expanding our parent tags.
	/// </summary>
	/// <remarks>
	/// <para>"A.1".MatchesTag("A") will return <see langword="true"/>, "A".MatchesTag("A.1") will return
	/// <see langword="false"/>.</para>
	/// <para>If <paramref name="tagToCheck"/> is not Valid it will always return <see langword="false"/>.</para>
	/// </remarks>
	/// <param name="tagToCheck"><see cref="GameplayTag"/> to check against this tag.</param>
	/// <returns><see langword="true"/> if this <see cref="GameplayTag"/> matches <paramref name="tagToCheck"/>.</returns>
	public bool MatchesTag(GameplayTag tagToCheck)
	{
		var tagNode = GameplayTagsManager.Instance.FindTagNode(this);

		if (tagNode is not null)
		{
			return tagNode.SingleTagContainer.HasTag(tagToCheck);
		}

		return false;
	}

	/// <summary>
	/// Determine if <paramref name="tagToCheck"/> is valid and exactly matches this tag.
	/// </summary>
	/// <remarks>
	/// <para>"A.1".MatchesTagExact("A") will return <see langword="false"/>.</para>
	/// <para>If <paramref name="tagToCheck"/> is not Valid it will always return <see langword="false"/>.</para>
	/// </remarks>
	/// <param name="tagToCheck"><see cref="GameplayTag"/> to check against this tag.</param>
	/// <returns><see langword="true"/> if <paramref name="tagToCheck"/> is Valid and is exactly this tag.</returns>
	public bool MatchesTagExact(GameplayTag tagToCheck)
	{
		if (!tagToCheck.IsValid)
		{
			return false;
		}

		return TagName == tagToCheck.TagName;
	}

	/// <summary>
	/// Checks if this tag matches ANY of the tags in the specified <see cref="GameplayTagContainer"/>, also checks
	/// against our parent tags.
	/// </summary>
	/// <remarks>
	/// <para>"A.1".MatchesAny({"A","B"}) will return <see langword="true"/>, "A".MatchesAny({"A.1","B"}) will return
	/// <see langword="false"/>.</para>
	/// <para>If <paramref name="containerToCheck"/> is empty/invalid it will always return <see langword="false"/>.
	/// </para>
	/// </remarks>
	/// <param name="containerToCheck"><see cref="GameplayTagContainer"/> to check against this tag.</param>
	/// <returns><see langword="true"/> if this tag matches ANY of the tags of in <paramref name="containerToCheck"/>.
	/// </returns>
	public bool MatchesAny(GameplayTagContainer containerToCheck)
	{
		var tagNode = GameplayTagsManager.Instance.FindTagNode(this);

		if (tagNode is not null)
		{
			return tagNode.SingleTagContainer.HasAny(containerToCheck);
		}

		return false;
	}

	/// <summary>
	/// Checks if this tag matches ANY of the tags in the specified <see cref="GameplayTagContainer"/>, only allowing
	/// exact matches.
	/// </summary>
	/// <remarks>
	/// <para>"A.1".MatchesAny({"A","B"}) will return <see langword="false"/>.</para>
	/// <para>If <paramref name="containerToCheck"/> is empty/invalid it will always return <see langword="false"/>.
	/// </para>
	/// </remarks>
	/// <param name="containerToCheck"><see cref="GameplayTagContainer"/> to check against this tag.</param>
	/// <returns><see langword="true"/> if this tag matches ANY of the tags of in <paramref name="containerToCheck"/>
	/// exactly.</returns>
	public bool MatchesAnyExact(GameplayTagContainer containerToCheck)
	{
		if (containerToCheck.IsEmpty)
		{
			return false;
		}

		return containerToCheck.GameplayTags.Contains(this);
	}

	/// <summary>
	/// Check to see how closely two <see cref="GameplayTag"/>s match. Higher values indicate more matching terms in the
	/// tags.
	/// </summary>
	/// <param name="tagToCheck"><see cref="GameplayTag"/> to match against.</param>
	/// <returns>The depth of the match, higher means they are closer to an exact match.</returns>
	public int MatchesTagDepth(GameplayTag tagToCheck)
	{
		return GameplayTagsManager.Instance.GameplayTagsMatchDepth(this, tagToCheck);
	}

	/// <summary>
	/// Serializes this <see cref="GameplayTag"/> into a <see cref="ushort"/> netIndex.
	/// </summary>
	/// <returns><see langword="true"/> if serialized successfully.</returns>
	public bool NetSerialize()
	{
		// Need to actually serialize and write it a stream.
		var netIndex = GameplayTagsManager.Instance.GetNetIndexFromTag(this);
		return true;
	}

	/// <summary>
	/// Deserializes this <see cref="GameplayTag"/> from a <see cref="ushort"/> netIndex value.
	/// </summary>
	/// <returns><see langword="true"/> if deserialized successfully.</returns>
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

	/// <summary>
	/// Returns a <see cref="string"/> representation of the <see cref="GameplayTag"/>.
	/// </summary>
	/// <returns>The <see cref="GameplayTag"/> as a <see cref="string"/>.</returns>
	public override string ToString()
	{
		return TagName.ToString();
	}

	/// <summary>
	/// Determines wether this instance and another specified <see cref="GameplayTag"/> object have the same value.
	/// </summary>
	/// <param name="obj">The other <see cref="object"/> to compare against.</param>
	/// <returns><see langword="true"/> if the value of the <paramref name="obj"/> parameter is the same as the value of
	/// this instance; otherwise, <see langword="false"/>. If <paramref name="obj"/> is <see langword="null"/>, the
	/// method returns <see langword="false"/>.</returns>
	/// <exception cref="ArgumentException">Compared object must be of type <see cref="GameplayTag"/>.</exception>
	public override bool Equals(object? obj)
	{
		if (obj is null)
		{
			return false;
		}

		if (obj is not GameplayTag gameplayTag)
		{
			return false;
		}

		return TagName.Equals(gameplayTag.TagName);
	}

	/// <summary>
	/// Determines wether this instance and another specified <see cref="GameplayTag"/> object have the same value.
	/// </summary>
	/// <param name="other">The other <see cref="GameplayTag"/> to compare against.</param>
	/// <returns><see langword="true"/> if the value of the <paramref name="other"/> parameter is the same as the value
	/// of this instance; otherwise, <see langword="false"/>. If <paramref name="other"/> is <see langword="null"/>, the
	/// method returns <see langword="false"/>.</returns>
	public bool Equals(GameplayTag other)
	{
		return TagName.Equals(other.TagName);
	}

	/// <summary>
	/// Returns the hash code for this <see cref="GameplayTag"/>.
	/// </summary>
	/// <returns>A 32-bit signed integer hash code.</returns>
	public override int GetHashCode()
	{
		return TagName.GetHashCode();
	}

	public static bool operator ==(GameplayTag a, GameplayTag b) { return a.Equals(b); }

	public static bool operator !=(GameplayTag a, GameplayTag b) { return !a.Equals(b); }
}
