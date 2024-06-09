using System.Diagnostics;

namespace GameplayTags.Runtime
{
	public enum GameplayTagQueryStreamVersion
	{
		InitialVersion = 0,

		// -----<new versions can be added before this line>-------------------------------------------------
		// - this needs to be the last line (see note below)
		VersionPlusOne,
		LatestVersion = VersionPlusOne - 1
	}

	public class GameplayTagQuery
	{
		public static GameplayTagQuery EmptyQuery = new();

		private List<GameplayTag> _tagDictionary;
		private GameplayTagQueryStreamVersion _tokenStreamVersion;

		internal List<byte> QueryTokenStream { private set; get; } = new();

		//		private string _userDescription;

		bool IsEmpty => (QueryTokenStream.Count == 0);

		internal GameplayTag GetTagFromIndex(int tagIndex)
		{
			Debug.Assert(_tagDictionary.Count > tagIndex && tagIndex >= 0);
			return _tagDictionary[tagIndex];
		}

		public GameplayTagQuery()
		{
			_tokenStreamVersion = GameplayTagQueryStreamVersion.LatestVersion;
			_tagDictionary = new();
		}

		public static GameplayTagQuery BuildQuery(GameplayTagQueryExpression rootQueryExpression)
		{
			var query = new GameplayTagQuery();
			query.Build(rootQueryExpression);
			return query;
		}

		public static GameplayTagQuery MakeQuery_MatchAnyTags(GameplayTagContainer tags)
		{
			return BuildQuery
			(
				new GameplayTagQueryExpression()
					.AnyTagsMatch()
					.AddTags(tags)
			);
		}

		public static GameplayTagQuery MakeQuery_MatchAllTags(GameplayTagContainer tags)
		{
			return BuildQuery
			(
				new GameplayTagQueryExpression()
					.AllTagsMatch()
					.AddTags(tags)
			);
		}

		public static GameplayTagQuery MakeQuery_MatchNoTags(GameplayTagContainer tags)
		{
			return BuildQuery
			(
				new GameplayTagQueryExpression()
					.NoTagsMatch()
					.AddTags(tags)
			);
		}

		public static GameplayTagQuery MakeQuery_MatchTag(GameplayTag tag)
		{
			return BuildQuery
			(
				new GameplayTagQueryExpression()
					.AllTagsMatch()
					.AddTag(tag)
			);
		}

		public void Build(GameplayTagQueryExpression rootQueryExpression)//, string inUserDescription)
		{
			_tokenStreamVersion = (int)GameplayTagQueryStreamVersion.LatestVersion;
			//UserDescription = InUserDescription;

			// Reserve size here is arbitrary, goal is to minimizing reallocs while being respectful of mem usage
			QueryTokenStream = new(128);
			_tagDictionary.Clear();

			// add stream version first
			QueryTokenStream.Add((byte)GameplayTagQueryStreamVersion.LatestVersion);

			// emit the query
			QueryTokenStream.Add(1);        // true to indicate is has a root expression
			rootQueryExpression.EmitTokens(QueryTokenStream, _tagDictionary);
		}

		public bool Matches(GameplayTagContainer container)
		{
			if (IsEmpty)
			{
				return false;
			}

			QueryEvaluator QE = new(this);
			return QE.Evaluate(container);

			// Implement matching logic here
			// For example, check if all queryTags are in the container
			return _tagDictionary.All(container.HasTag);
		}

		/** Replaces existing tags with passed in tags. Does not modify the tag query expression logic. Useful when you need to cache off and update often used query. Must use same sized tag container! */
		public void ReplaceTagsFast(GameplayTagContainer tags)
		{
			Debug.Assert(tags.Count == _tagDictionary.Count);
			_tagDictionary.Clear();
			_tagDictionary.AddRange(tags.GameplayTags);
		}

		/** Replaces existing tags with passed in tag. Does not modify the tag query expression logic. Useful when you need to cache off and update often used query. */
		public void ReplaceTagFast(GameplayTag tag)
		{
			Debug.Assert(_tagDictionary.Count == 1);
			_tagDictionary.Clear();
			_tagDictionary.Add(tag);
		}
	}
}