namespace GameplayTags.Runtime;

/// <summary>
/// Simple tree node for <see cref="GameplayTag"/>s, this stores metadata about specific tags.
/// </summary>
public class GameplayTagNode : IComparable<GameplayTagNode>
{
	/// <summary>
	/// Gets the raw name for this tag at current rank in the tree.
	/// </summary>
	public TagName TagName { get; }

	/// <summary>
	/// Gets or sets the net index of this node.
	/// </summary>
	public ushort NetIndex { get; set; }

	/// <summary>
	/// Gets the parent <see cref="GameplayTagNode"/>, if any.
	/// </summary>
	public GameplayTagNode? ParentTagNode { get; }

	/// <summary>
	/// Gets this node's child <see cref="GameplayTagNode"/>s.
	/// </summary>
	public List<GameplayTagNode> ChildTags { get; } = new ();

	/// <summary>
	/// Gets a correctly constructed container with only this tag, useful for doing container queries.
	/// </summary>
	public GameplayTagContainer SingleTagContainer { get; } = new ();

	/// <summary>
	/// Gets the complete tag for the <see cref="GameplayTagNode"/>, including all parent tags, delimited by periods.
	/// </summary>
	public GameplayTag CompleteTag => SingleTagContainer.Count > 0
		? SingleTagContainer.Single()
		: GameplayTag.EmptyTag;

	/// <summary>
	/// Gets the complete <see cref="TagName"/> for the tag represented by this <see cref="GameplayTagNode"/>.
	/// </summary>
	public TagName CompleteTagName => CompleteTag.TagName;

	/// <summary>
	/// Gets a value indicating whether the tag was explicitly added and not only implied by its child tags.
	/// </summary>
	internal bool IsExplicitTag { get; }

	/// <summary>
	/// Initializes a new instance of the <see cref="GameplayTagNode"/> class.
	/// </summary>
	public GameplayTagNode()
	{
		ParentTagNode = null;
	}

	/// <summary>
	/// Initializes a new instance of the <see cref="GameplayTagNode"/> class.
	/// </summary>
	/// <param name="tagName">Short name of tag to insert.</param>
	/// <param name="fullTagName">The full name for this tag, for performance.</param>
	/// <param name="parentTagNode">The parent <see cref="GameplayTagNode"/> of this node, if any.</param>
	/// <param name="isExplicit">Is the tag explicitly defined or is it implied by the existence of a child tag.</param>
	public GameplayTagNode(TagName tagName, TagName fullTagName, GameplayTagNode? parentTagNode, bool isExplicit)
	{
		TagName = tagName;
		ParentTagNode = parentTagNode;
		IsExplicitTag = isExplicit;

		SingleTagContainer.GameplayTags.Add(new GameplayTag(fullTagName));

		if (parentTagNode is null)
		{
			return;
		}

		var parentContainer = parentTagNode.SingleTagContainer;

		if (!parentContainer.IsEmpty)
		{
			SingleTagContainer.ParentTags.Add(parentContainer.Single());
			SingleTagContainer.ParentTags.UnionWith(parentContainer.ParentTags);
		}
	}

	/// <summary>
	/// Compares two specified <see cref="GameplayTagNode?"/> objects and returns an integer that indicates their
	/// relative position in the sort orther.
	/// </summary>
	/// <param name="other">The other <see cref="GameplayTagNode?"/> to compare against.</param>
	/// <returns>A 32-bit signed integer that indicates the lexical relationship between the two comparands.
	/// <para><b>Less than zero</b> - this instance precedes <paramref name="other"/> in the sort order.</para>
	/// <para><b>Zero</b> - this instance is the same position as <paramref name="other"/> in the sort order.</para>
	/// <para><b>Greater than zero</b> - this instance follows <paramref name="other"/> in the sort order.</para>
	/// <para>If <paramref name="other"/> is null, this node is considered to precede <paramref name="other"/>.</para>
	/// </returns>
	public int CompareTo(GameplayTagNode? other)
	{
		if (other is null)
		{
			return -1;
		}

		return CompleteTagName.CompareTo(other.CompleteTagName);
	}
}
