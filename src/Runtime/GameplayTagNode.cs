namespace GameplayTags.Runtime
{
	public class GameplayTagNode
	{
		private TagName _tag;

		private List<GameplayTagNode> _childTags = new();
		public GameplayTagNode? ParentTagNode { get; private set; }
		public GameplayTagContainer SingleTagContainer { get; private set; }

		public List<GameplayTagNode> ChildTags => _childTags;
		public GameplayTag CompleteTag => SingleTagContainer.Count > 0 ? SingleTagContainer.Single() : GameplayTag.EmptyTag;

		public TagName CompleteTagName => CompleteTag.TagName;
		public TagName TagName => _tag;

		public ushort NetIndex;

		public GameplayTagNode()
		{
			SingleTagContainer = new GameplayTagContainer();
		}

		public GameplayTagNode(TagName tagName, TagName fullTagName, GameplayTagNode parentTagNode)
		{
			_tag = tagName;
			ParentTagNode = parentTagNode;

			SingleTagContainer = new GameplayTagContainer();
			SingleTagContainer.GameplayTags.Add(new GameplayTag(fullTagName));

			var parentContainer = parentTagNode.SingleTagContainer;

			if (!parentContainer.IsEmpty)
			{
				SingleTagContainer.ParentTags.Add(parentContainer.Single());
				SingleTagContainer.ParentTags.UnionWith(parentContainer.ParentTags);
			}
		}
	}
}
