namespace GameplayTags.Runtime
{
	public struct GameplayTagStack
	{
		public GameplayTag Tag { get; private set; }
		public int Count { get; private set; }

		public GameplayTagStack(GameplayTag tag, int count)
		{
			Tag = tag;
			Count = count;
		}

		public override string ToString()
		{
			return Tag.ToString();
		}
	}
}
