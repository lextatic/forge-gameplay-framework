using System.Diagnostics;

namespace GameplayTags.Runtime;

/// <summary>
/// The possible stream versions for the <see cref="GameplayTagQuery"/>.
/// </summary>
public enum GameplayTagQueryStreamVersion
{
	InitialVersion = 0,

	// -----<new versions can be added before this line>-------------------------------------------------
	// - this needs to be the last line (see code below)
	VersionPlusOne,
	LatestVersion = VersionPlusOne - 1,
}

/// <summary>
/// <para>An <see cref="GameplayTagQuery"/> is a logical query that can be run against an
/// <see cref="GameplayTagContainer"/>.</para>
/// <para>A query that succeeds is said to "match".</para>
/// </summary>
/// <remarks>
/// <para>Queries are logical expressions that can test the intersection properties of another tag container (all, any,
/// or none), or the matching state of a set of sub-expressions (all, any, or none). This allows queries to be
/// arbitrarily recursive and very expressive. For instance, if you wanted to test if a given
/// <see cref="GameplayTagContainer"/> contained tags ((A AND B) OR (C)) AND (!D), you would construct your query in the
/// form ALL( ANY( ALL(A,B), ALL(C) ), NONE(D) ).</para>
/// <para>You can construct queries natively in code. </para>
/// <para>
/// Example of how to build a query via code:
/// </para>
/// <code>
/// GameplayTagQuery query;
/// query.BuildQuery(
///   new GameplayTagQueryExpression()
///     .AllTagsMatch()
///       .AddTag(GameplayTag.RequestGameplayTag(TagName.FromString("Animal.Mammal.Dog.Corgi")))
///       .AddTag(GameplayTag.RequestGameplayTag(TagName.FromString("Plant.Tree.Spruce")))
/// );
/// </code>
/// <para>Queries are internally represented as a byte stream that is memory-efficient and can be evaluated quickly at
/// runtime.</para>
/// </remarks>
public class GameplayTagQuery
{
	private readonly List<GameplayTag> _tagDictionary = new ();

#pragma warning disable S4487 // Unread "private" fields should be removed
	private GameplayTagQueryStreamVersion _tokenStreamVersion;
#pragma warning restore S4487 // Unread "private" fields should be removed

	/// <summary>
	/// Gets a static representation of an empty <see cref="GameplayTagQuery"/>.
	/// </summary>
	public static GameplayTagQuery EmptyQuery { get; } = new ();

	/// <summary>
	/// Gets the <see cref="List{byte}"/> stream that represents this query.
	/// </summary>
	internal List<byte> QueryTokenStream { get; private set; } = new ();

	private bool IsEmpty => QueryTokenStream.Count == 0;

	/// <summary>
	/// Initializes a new instance of the <see cref="GameplayTagQuery"/> class.
	/// </summary>
	public GameplayTagQuery()
	{
		_tokenStreamVersion = GameplayTagQueryStreamVersion.LatestVersion;
	}

	/// <summary>
	/// Initializes a new instance of the <see cref="GameplayTagQuery"/> class with a specified
	/// <see cref="GameplayTagQueryStreamVersion"/>.
	/// </summary>
	/// <remarks>StreamVersion is never used (yet).</remarks>
	/// <param name="tokenStreamVersion">The specified <see cref="GameplayTagQueryStreamVersion"/> for this query.
	/// </param>
	public GameplayTagQuery(GameplayTagQueryStreamVersion tokenStreamVersion)
	{
		_tokenStreamVersion = tokenStreamVersion;
	}

	/// <summary>
	/// Static function to assemble and return a query.
	/// </summary>
	/// <param name="rootQueryExpression">The <see cref="GameplayTagQueryExpression"/> for this query.</param>
	/// <returns>The created <see cref="GameplayTagQuery"/>.</returns>
	public static GameplayTagQuery BuildQuery(GameplayTagQueryExpression rootQueryExpression)
	{
		var query = new GameplayTagQuery();
		query.Build(rootQueryExpression);
		return query;
	}

	/// <summary>
	/// Creates a tag query that will match if there are any common tags between the given tags and the tags being
	/// queried against.
	/// </summary>
	/// <param name="tags"><see cref="GameplayTagContainer"/> containing the tags being queried against.</param>
	/// <returns>A constructed <see cref="GameplayTagQuery"/> for matching any tags.</returns>
	public static GameplayTagQuery MakeQuery_MatchAnyTags(GameplayTagContainer tags)
	{
		return BuildQuery(
			new GameplayTagQueryExpression()
				.AnyTagsMatch()
				.AddTags(tags));
	}

	/// <summary>
	/// Creates a tag query that will match if there are any exactly common tags between the given tags and the tags
	/// being queried against.
	/// </summary>
	/// <param name="tags"><see cref="GameplayTagContainer"/> containing the tags being queried against.</param>
	/// <returns>A constructed <see cref="GameplayTagQuery"/> for matching any tags exact.</returns>
	public static GameplayTagQuery MakeQuery_MatchAnyTagsExact(GameplayTagContainer tags)
	{
		return BuildQuery(
			new GameplayTagQueryExpression()
				.AnyTagsMatchExact()
				.AddTags(tags));
	}

	/// <summary>
	/// Creates a tag query that will match if all tags match between the given tags and the tags being queried against.
	/// </summary>
	/// <param name="tags"><see cref="GameplayTagContainer"/> containing the tags being queried against.</param>
	/// <returns>A constructed <see cref="GameplayTagQuery"/> for matching all tags.</returns>
	public static GameplayTagQuery MakeQuery_MatchAllTags(GameplayTagContainer tags)
	{
		return BuildQuery(
			new GameplayTagQueryExpression()
				.AllTagsMatch()
				.AddTags(tags));
	}

	/// <summary>
	/// Creates a tag query that will match if all tags match exactly between the given tags and the tags being queried
	/// against.
	/// </summary>
	/// <param name="tags"><see cref="GameplayTagContainer"/> containing the tags being queried against.</param>
	/// <returns>A constructed <see cref="GameplayTagQuery"/> for matching all tags exact.</returns>
	public static GameplayTagQuery MakeQuery_MatchAllTagsExact(GameplayTagContainer tags)
	{
		return BuildQuery(
			new GameplayTagQueryExpression()
				.AllTagsMatchExact()
				.AddTags(tags));
	}

	/// <summary>
	/// Creates a tag query that will match if no tags match between the given tags and the tags being queried against.
	/// </summary>
	/// <param name="tags"><see cref="GameplayTagContainer"/> containing the tags being queried against.</param>
	/// <returns>A constructed <see cref="GameplayTagQuery"/> for matching no tags.</returns>
	public static GameplayTagQuery MakeQuery_MatchNoTags(GameplayTagContainer tags)
	{
		return BuildQuery(
			new GameplayTagQueryExpression()
				.NoTagsMatch()
				.AddTags(tags));
	}

	/// <summary>
	/// Creates a tag query that will match if no tags match exactly between the given tags and the tags being queried
	/// against.
	/// </summary>
	/// <param name="tags"><see cref="GameplayTagContainer"/> containing the tags being queried against.</param>
	/// <returns>A constructed <see cref="GameplayTagQuery"/> for matching no tags exactly.</returns>
	public static GameplayTagQuery MakeQuery_MatchNoTagsExact(GameplayTagContainer tags)
	{
		return BuildQuery(
			new GameplayTagQueryExpression()
				.NoTagsMatchExact()
				.AddTags(tags));
	}

	/// <summary>
	/// Creates a tag query that will match if there are any common tags between the given tags and a single tag being
	/// queried against.
	/// </summary>
	/// <param name="tag">The <see cref="GameplayTag"/> being queried against.</param>
	/// <returns>A constructed <see cref="GameplayTagQuery"/> for matching a single tag.</returns>
	public static GameplayTagQuery MakeQuery_MatchTag(GameplayTag tag)
	{
		return BuildQuery(
			new GameplayTagQueryExpression()
				.AllTagsMatch()
				.AddTag(tag));
	}

	/// <summary>
	/// Creates a tag query that will match if there are any exactly common tags between the given tags and a single tag
	/// being queried against.
	/// </summary>
	/// <param name="tag">The <see cref="GameplayTag"/> being queried against.</param>
	/// <returns>A constructed <see cref="GameplayTagQuery"/> for matching a single tag exact.</returns>
	public static GameplayTagQuery MakeQuery_MatchTagExact(GameplayTag tag)
	{
		return BuildQuery(
			new GameplayTagQueryExpression()
				.AllTagsMatchExact()
				.AddTag(tag));
	}

	/// <summary>
	/// Creates this query with the given root expression.
	/// </summary>
	/// <param name="rootQueryExpression">The <see cref="GameplayTagQueryExpression"/> for this query.</param>
	public void Build(GameplayTagQueryExpression rootQueryExpression)
	{
		_tokenStreamVersion = (int)GameplayTagQueryStreamVersion.LatestVersion;

		// Reserve size here is arbitrary, goal is to minimizing reallocs while being respectful of mem usage.
		QueryTokenStream = new(128);
		_tagDictionary.Clear();

		// Add stream version first.
		QueryTokenStream.Add((byte)GameplayTagQueryStreamVersion.LatestVersion);

		// Emit the query
		// 1 = true to indicate is has a root expression.
		QueryTokenStream.Add(1);
		rootQueryExpression.EmitTokens(QueryTokenStream, _tagDictionary);
	}

	/// <summary>
	/// Returns <see langword="true"/> if the given tags match this query, or <see langword="false"/> otherwise.
	/// </summary>
	/// <param name="container">The <see cref="GameplayTagContainer"/> to be tested against this query.</param>
	/// <returns><see langword="true"/> if the given tags match this query, or <see langword="false"/> otherwise.
	/// </returns>
	public bool Matches(GameplayTagContainer container)
	{
		if (IsEmpty)
		{
			return false;
		}

		QueryEvaluator queryEvaluator = new (this);
		return queryEvaluator.Evaluate(container);
	}

	/// <summary>
	/// Replaces existing tags with passed in tags. Does not modify the tag query expression logic.
	/// </summary>
	/// <remarks>
	/// Useful when you need to cache off and update often used query. Must use same sized tag container.
	/// </remarks>
	/// <param name="container">The <see cref="GameplayTagContainer"/> with the new tags being replaced.</param>
	public void ReplaceTagsFast(GameplayTagContainer container)
	{
		Debug.Assert(container.Count == _tagDictionary.Count, "Must use containers with the same size.");
		_tagDictionary.Clear();
		_tagDictionary.AddRange(container.GameplayTags);
	}

	/// <summary>
	/// Replaces existing tags with passed in tag. Does not modify the tag query expression logic.
	/// </summary>
	/// <remarks>
	/// Useful when you need to cache off and update often used query.
	/// </remarks>
	/// <param name="tag">The new <see cref="GameplayTag"/> being replaced.</param>
	public void ReplaceTagFast(GameplayTag tag)
	{
		Debug.Assert(_tagDictionary.Count == 1, "Must use single containers.");
		_tagDictionary.Clear();
		_tagDictionary.Add(tag);
	}

	/// <summary>
	/// Returns a <see cref="GameplayTag"/> from the tag dictionay.
	/// </summary>
	/// <param name="tagIndex">The index to get the tag at.</param>
	/// <returns>A <see cref="GameplayTag"/> from the tag dictionay.</returns>
	internal GameplayTag GetTagFromIndex(int tagIndex)
	{
		Debug.Assert(
			_tagDictionary.Count > tagIndex && tagIndex >= 0,
			"tagIndex should be a valid _tagDictionary index.");
		return _tagDictionary[tagIndex];
	}
}
