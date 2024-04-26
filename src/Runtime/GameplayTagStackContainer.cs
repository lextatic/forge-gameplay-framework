namespace GameplayTags.Runtime
{
	public struct GameplayTagStackContainer
	{
		private HashSet<GameplayTagStack> _stacks = new();
		private Dictionary<GameplayTag, int> _tagCountMap = new();

		public GameplayTagStackContainer()
		{
		}

		public void AddStackCount(GameplayTag tag, int count)
		{

		}

		public void RemoveStackCount(GameplayTag tag, int count)
		{

		}

		public bool HasTag(GameplayTag tag)
		{
			return _tagCountMap.ContainsKey(tag);
		}

		public int GetStackCount(GameplayTag tag)
		{
			return _tagCountMap[tag];
		}
	}
}
