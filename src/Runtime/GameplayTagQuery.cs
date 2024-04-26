namespace GameplayTags.Runtime
{

	public class GameplayTagQuery
	{
		private HashSet<GameplayTag> _queryTags;

		public GameplayTagQuery(IEnumerable<GameplayTag> tags)
		{
			_queryTags = new HashSet<GameplayTag>(tags);
		}

		public bool Matches(GameplayTagContainer container)
		{
			// Implement matching logic here
			// For example, check if all queryTags are in the container
			return _queryTags.All(container.HasTag);

		}
	}
}
