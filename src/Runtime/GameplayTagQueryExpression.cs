using System.Diagnostics;

namespace GameplayTags.Runtime;

/// <summary>
/// Used for a fluid syntax approach for setting the type and parameters to be used on a <see cref="GameplayTagQuery"/>.
/// </summary>
public class GameplayTagQueryExpression
{
	private readonly List<GameplayTagQueryExpression> _expressionSet = new ();

	private readonly List<GameplayTag> _tagSet = new ();

	private GameplayTagQueryExpressionType _expressionType;

	/// <summary>
	/// Sets this query's expression type to <see cref="GameplayTagQueryExpressionType.AnyTagsMatch"/>.
	/// </summary>
	/// <returns>This <see cref="GameplayTagQueryExpression"/> itself.</returns>
	public GameplayTagQueryExpression AnyTagsMatch()
	{
		_expressionType = GameplayTagQueryExpressionType.AnyTagsMatch;
		return this;
	}

	/// <summary>
	/// Sets this query's expression type to <see cref="GameplayTagQueryExpressionType.AllTagsMatch"/>.
	/// </summary>
	/// <returns>This <see cref="GameplayTagQueryExpression"/> itself.</returns>
	public GameplayTagQueryExpression AllTagsMatch()
	{
		_expressionType = GameplayTagQueryExpressionType.AllTagsMatch;
		return this;
	}

	/// <summary>
	/// Sets this query's expression type to <see cref="GameplayTagQueryExpressionType.NoTagsMatch"/>.
	/// </summary>
	/// <returns>This <see cref="GameplayTagQueryExpression"/> itself.</returns>
	public GameplayTagQueryExpression NoTagsMatch()
	{
		_expressionType = GameplayTagQueryExpressionType.NoTagsMatch;
		return this;
	}

	/// <summary>
	/// Sets this query's expression type to <see cref="GameplayTagQueryExpressionType.AnyTagsMatchExact"/>.
	/// </summary>
	/// <returns>This <see cref="GameplayTagQueryExpression"/> itself.</returns>
	public GameplayTagQueryExpression AnyTagsMatchExact()
	{
		_expressionType = GameplayTagQueryExpressionType.AnyTagsMatchExact;
		return this;
	}

	/// <summary>
	/// Sets this query's expression type to <see cref="GameplayTagQueryExpressionType.AllTagsMatchExact"/>.
	/// </summary>
	/// <returns>This <see cref="GameplayTagQueryExpression"/> itself.</returns>
	public GameplayTagQueryExpression AllTagsMatchExact()
	{
		_expressionType = GameplayTagQueryExpressionType.AllTagsMatchExact;
		return this;
	}

	/// <summary>
	/// Sets this query's expression type to <see cref="GameplayTagQueryExpressionType.NoTagsMatchExact"/>.
	/// </summary>
	/// <returns>This <see cref="GameplayTagQueryExpression"/> itself.</returns>
	public GameplayTagQueryExpression NoTagsMatchExact()
	{
		_expressionType = GameplayTagQueryExpressionType.NoTagsMatchExact;
		return this;
	}

	/// <summary>
	/// Sets this query's expression type to <see cref="GameplayTagQueryExpressionType.AnyExpressionsMatch"/>.
	/// </summary>
	/// <returns>This <see cref="GameplayTagQueryExpression"/> itself.</returns>
	public GameplayTagQueryExpression AnyExpressionsMatch()
	{
		_expressionType = GameplayTagQueryExpressionType.AnyExpressionsMatch;
		return this;
	}

	/// <summary>
	/// Sets this query's expression type to <see cref="GameplayTagQueryExpressionType.AllExpressionsMatch"/>.
	/// </summary>
	/// <returns>This <see cref="GameplayTagQueryExpression"/> itself.</returns>
	public GameplayTagQueryExpression AllExpressionsMatch()
	{
		_expressionType = GameplayTagQueryExpressionType.AllExpressionsMatch;
		return this;
	}

	/// <summary>
	/// Sets this query's expression type to <see cref="GameplayTagQueryExpressionType.NoExpressionsMatch"/>.
	/// </summary>
	/// <returns>This <see cref="GameplayTagQueryExpression"/> itself.</returns>
	public GameplayTagQueryExpression NoExpressionsMatch()
	{
		_expressionType = GameplayTagQueryExpressionType.NoExpressionsMatch;
		return this;
	}

	/// <summary>
	/// Adds a tag this expression.
	/// </summary>
	/// <param name="tagString"><see cref="string"/> that represents the tag to be added.</param>
	/// <returns>This <see cref="GameplayTagQueryExpression"/> itself.</returns>
	public GameplayTagQueryExpression AddTag(string tagString)
	{
		return AddTag(TagName.FromString(tagString));
	}

	/// <summary>
	/// Adds a tag this expression.
	/// </summary>
	/// <param name="tagName"><see cref="TagName"/> that represents the tag to be added.</param>
	/// <returns>This <see cref="GameplayTagQueryExpression"/> itself.</returns>
	public GameplayTagQueryExpression AddTag(TagName tagName)
	{
		return AddTag(GameplayTag.RequestGameplayTag(tagName));
	}

	/// <summary>
	/// Adds a tag this expression.
	/// </summary>
	/// <param name="tag"><see cref="GameplayTag"/> to be added.</param>
	/// <returns>This <see cref="GameplayTagQueryExpression"/> itself.</returns>
	public GameplayTagQueryExpression AddTag(GameplayTag tag)
	{
		Debug.Assert(UsesTagSet(), "Shouldn't be adding tags to a query of ExpressionSet type.");
		_tagSet.Add(tag);
		return this;
	}

	/// <summary>
	/// Adds multiple tags this expression.
	/// </summary>
	/// <param name="tags"><see cref="GameplayTagContainer"/> containing the tags to be added.</param>
	/// <returns>This <see cref="GameplayTagContainer"/> itself.</returns>
	public GameplayTagQueryExpression AddTags(GameplayTagContainer tags)
	{
		Debug.Assert(UsesTagSet(), "Shouldn't be adding tags to a query of ExpressionSet type.");
		_tagSet.AddRange(tags.GameplayTags);
		return this;
	}

	/// <summary>
	/// Adds a expression to this expression.
	/// </summary>
	/// <param name="expression"><see cref="GameplayTagQueryExpression"/> to be added.</param>
	/// <returns>This <see cref="GameplayTagQueryExpression"/> itself.</returns>
	public GameplayTagQueryExpression AddExpression(GameplayTagQueryExpression expression)
	{
		Debug.Assert(UsesExpressionSet(), "Shouldn't be adding expressions to a query of TagSet type.");
		_expressionSet.Add(expression);
		return this;
	}

	/// <summary>
	/// Writes this expression to the given token stream.
	/// </summary>
	/// <param name="tokenStream">A <see cref="List{byte}"/> to write this expression on.</param>
	/// <param name="tagDictionary">A 'dictionary' containing the <see cref="GameplayTag"/>s for this query.</param>
	internal void EmitTokens(List<byte> tokenStream, List<GameplayTag> tagDictionary)
	{
		// emit exprtype
		tokenStream.Add((byte)_expressionType);

		// emit exprdata
		switch (_expressionType)
		{
			case GameplayTagQueryExpressionType.AnyTagsMatch:
			case GameplayTagQueryExpressionType.AllTagsMatch:
			case GameplayTagQueryExpressionType.NoTagsMatch:
			case GameplayTagQueryExpressionType.AnyTagsMatchExact:
			case GameplayTagQueryExpressionType.AllTagsMatchExact:
			case GameplayTagQueryExpressionType.NoTagsMatchExact:
				// Emit tag set.
				byte numTags = (byte)_tagSet.Count;
				tokenStream.Add(numTags);

				foreach (var tag in _tagSet)
				{
					int tagIndex = tagDictionary.AddUnique(tag);

					// We reserve token 255 for internal use, so 254 is max unique tags.
					Debug.Assert(tagIndex <= 254, "Stream can't hold more than 254 tags.");
					tokenStream.Add((byte)tagIndex);
				}

				break;

			case GameplayTagQueryExpressionType.AnyExpressionsMatch:
			case GameplayTagQueryExpressionType.AllExpressionsMatch:
			case GameplayTagQueryExpressionType.NoExpressionsMatch:
				// Emit expression set.
				byte numExpressions = (byte)_expressionSet.Count;
				tokenStream.Add(numExpressions);

				foreach (var expression in _expressionSet)
				{
					expression.EmitTokens(tokenStream, tagDictionary);
				}

				break;

			default:
				break;
		}
	}

	private bool UsesTagSet()
	{
		return (_expressionType == GameplayTagQueryExpressionType.AllTagsMatch)
			|| (_expressionType == GameplayTagQueryExpressionType.AnyTagsMatch)
			|| (_expressionType == GameplayTagQueryExpressionType.NoTagsMatch)
			|| (_expressionType == GameplayTagQueryExpressionType.AllTagsMatchExact)
			|| (_expressionType == GameplayTagQueryExpressionType.AnyTagsMatchExact)
			|| (_expressionType == GameplayTagQueryExpressionType.NoTagsMatchExact);
	}

	private bool UsesExpressionSet()
	{
		return (_expressionType == GameplayTagQueryExpressionType.AllExpressionsMatch)
			|| (_expressionType == GameplayTagQueryExpressionType.AnyExpressionsMatch)
			|| (_expressionType == GameplayTagQueryExpressionType.NoExpressionsMatch);
	}
}
