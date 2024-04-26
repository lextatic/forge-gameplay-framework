namespace GameplayTags.Runtime
{
	public class GameplayTagNode
	{
		private TagName _tag;

		private List<GameplayTagNode> _childTags = new();
		public GameplayTagNode? ParentTagNode { get; private set; }
		public GameplayTagContainer SingleTagContainer { get; private set; }

		public List<GameplayTagNode> ChildTags => _childTags;
		public GameplayTag CompleteTag => SingleTagContainer.Count > 0 ? SingleTagContainer[0] : GameplayTag.EmptyTag;
		public TagName CompleteTagName => CompleteTag.TagName;
		public TagName TagName => _tag;

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
				SingleTagContainer.ParentTags.Add(parentContainer[0]);
				SingleTagContainer.ParentTags.AddRange(parentContainer.ParentTags);
			}
		}
	}
}
