namespace GameplayTags.Runtime;

/// <summary>
/// Simple tree node for <see cref="GameplayTag"/>s, this stores metadata about specific tags.
/// </summary>
public class GameplayTagNode
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
	/// Initializes a new instance of the <see cref="GameplayTagNode"/> class.
	/// </summary>
	public GameplayTagNode()
	{
		ParentTagNode = null;
	}

	/// <summary>
	/// Initializes a new instance of the <see cref="GameplayTagNode"/> class.
	/// </summary>
	/// <param name="tagName">The raw name for this tag at current rank in the tree.</param>
	/// <param name="fullTagName">The complete full name for this tag.</param>
	/// <param name="parentTagNode">The parent <see cref="GameplayTagNode"/> of this node.</param>
	public GameplayTagNode(TagName tagName, TagName fullTagName, GameplayTagNode parentTagNode)
	{
		TagName = tagName;
		ParentTagNode = parentTagNode;

		SingleTagContainer.GameplayTags.Add(new GameplayTag(fullTagName));

		var parentContainer = parentTagNode.SingleTagContainer;

		if (!parentContainer.IsEmpty)
		{
			SingleTagContainer.ParentTags.Add(parentContainer.Single());
			SingleTagContainer.ParentTags.UnionWith(parentContainer.ParentTags);
		}
	}
}
