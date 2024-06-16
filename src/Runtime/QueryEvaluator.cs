using System.Diagnostics;

namespace GameplayTags.Runtime;

/// <summary>
/// Helper class to parse/eval query token streams.
/// </summary>
public class QueryEvaluator
{
	private readonly GameplayTagQuery _query;

	private int _curStreamIdx;
	private bool _readError;

	/// <summary>
	/// Initializes a new instance of the <see cref="QueryEvaluator"/> class.
	/// </summary>
	/// <param name="query">The <see cref="QueryEvaluator"/> to be used by this evaluator.</param>
	public QueryEvaluator(GameplayTagQuery query)
	{
		_query = query;
	}

	/// <summary>
	/// Evaluates the query against the given tag container and returns the result (<see langword="true"/> if matching,
	/// <see langword="false"/> otherwise).
	/// </summary>
	/// <param name="container">The <see cref="GameplayTagContainer"/> with the tags being evaluated against.</param>
	/// <returns><see langword="true"/> if matching, <see langword="false"/> otherwise.</returns>
	public bool Evaluate(GameplayTagContainer container)
	{
		_curStreamIdx = 0;

		// start parsing the set
		var version = GetToken();

		if (_readError)
		{
			return false;
		}

		bool returnValue = false;

		byte hasRootExpression = GetToken();
		if (!_readError && hasRootExpression != 0)
		{
			returnValue = EvaluateExpression(container);
		}

		Debug.Assert(
			_curStreamIdx == _query.QueryTokenStream.Count,
			"There shouldn't be remaining tokens into the stream.");

		return returnValue;
	}

	private bool EvaluateExpression(GameplayTagContainer tags, bool skip = false)
	{
		// Emit expression data.
		switch ((GameplayTagQueryExpressionType)GetToken())
		{
			case GameplayTagQueryExpressionType.AnyTagsMatch:
				return EvaluateAnyTagsMatch(tags, false, skip);
			case GameplayTagQueryExpressionType.AllTagsMatch:
				return EvaluateAllTagsMatch(tags, false, skip);
			case GameplayTagQueryExpressionType.NoTagsMatch:
				return EvaluateNoTagsMatch(tags, false, skip);

			case GameplayTagQueryExpressionType.AnyTagsMatchExact:
				return EvaluateAnyTagsMatch(tags, true, skip);
			case GameplayTagQueryExpressionType.AllTagsMatchExact:
				return EvaluateAllTagsMatch(tags, true, skip);
			case GameplayTagQueryExpressionType.NoTagsMatchExact:
				return EvaluateNoTagsMatch(tags, true, skip);

			case GameplayTagQueryExpressionType.AnyExpressionsMatch:
				return EvaluateAnyExpressionsMatch(tags, skip);
			case GameplayTagQueryExpressionType.AllExpressionsMatch:
				return EvaluateAllExpressionsMatch(tags, skip);
			case GameplayTagQueryExpressionType.NoExpressionsMatch:
				return EvaluateNoExpressionsMatch(tags, skip);
		}

		Debug.Fail("Code should not reach this point.");

		return false;
	}

	private bool EvaluateAnyTagsMatch(GameplayTagContainer tags, bool exactMatch, bool skip)
	{
		bool shortCircuit = skip;
		bool result = false;

		// Parse tag set.
		int numTags = GetToken();
		if (_readError)
		{
			return false;
		}

		for (int i = 0; i < numTags; ++i)
		{
			int tagIndex = GetToken();
			if (_readError)
			{
				return false;
			}

			if (!shortCircuit)
			{
				var tag = _query.GetTagFromIndex(tagIndex);

				bool hasTag = HasTag(tags, tag, exactMatch);

				if (hasTag)
				{
					// One match is sufficient for a true result.
					shortCircuit = true;
					result = true;
				}
			}
		}

		return result;
	}

	private bool EvaluateAllTagsMatch(GameplayTagContainer tags, bool exactMatch, bool skip)
	{
		bool shortCircuit = skip;

		// Assume true until proven otherwise.
		bool result = true;

		// Parse tag set.
		int numTags = GetToken();
		if (_readError)
		{
			return false;
		}

		for (int i = 0; i < numTags; ++i)
		{
			int tagIndex = GetToken();
			if (_readError)
			{
				return false;
			}

			if (!shortCircuit)
			{
				var tag = _query.GetTagFromIndex(tagIndex);

				bool hasTag = HasTag(tags, tag, exactMatch);

				if (!hasTag)
				{
					// One failed match is sufficient for a false result.
					shortCircuit = true;
					result = false;
				}
			}
		}

		return result;
	}

	private bool EvaluateNoTagsMatch(GameplayTagContainer tags, bool exactMatch, bool skip)
	{
		bool shortCircuit = skip;

		// Assume true until proven otherwise.
		bool result = true;

		// Parse tag set.
		int numTags = GetToken();
		if (_readError)
		{
			return false;
		}

		for (int i = 0; i < numTags; ++i)
		{
			int tagIndex = GetToken();
			if (_readError)
			{
				return false;
			}

			if (!shortCircuit)
			{
				var tag = _query.GetTagFromIndex(tagIndex);

				bool hasTag = HasTag(tags, tag, exactMatch);

				if (hasTag)
				{
					// One match is sufficient for a false result.
					shortCircuit = true;
					result = false;
				}
			}
		}

		return result;
	}

	private bool HasTag(GameplayTagContainer tags, GameplayTag tag, bool exactMatch)
	{
		if (exactMatch)
		{
			return tags.HasTagExact(tag);
		}

		return tags.HasTag(tag);
	}

	private bool EvaluateAnyExpressionsMatch(GameplayTagContainer tags, bool skip)
	{
		bool shortCircuit = skip;

		// Assume false until proven otherwise.
		bool result = false;

		// Parse expression set.
		int numExpressions = GetToken();
		if (_readError)
		{
			return false;
		}

		for (int i = 0; i < numExpressions; ++i)
		{
			bool expressionResult = EvaluateExpression(tags, shortCircuit);

			if (!shortCircuit && expressionResult)
			{
				// One match is sufficient for true result.
				result = true;
				shortCircuit = true;
			}
		}

		return result;
	}

	private bool EvaluateAllExpressionsMatch(GameplayTagContainer tags, bool skip)
	{
		bool shortCircuit = skip;

		// Assume true until proven otherwise.
		bool result = true;

		// Parse expression set.
		int numExpressions = GetToken();
		if (_readError)
		{
			return false;
		}

		for (int i = 0; i < numExpressions; ++i)
		{
			bool expressionResult = EvaluateExpression(tags, shortCircuit);

			if (!shortCircuit && !expressionResult)
			{
				// One fail is sufficient for false result.
				result = false;
				shortCircuit = true;
			}
		}

		return result;
	}

	private bool EvaluateNoExpressionsMatch(GameplayTagContainer tags, bool skip)
	{
		bool shortCircuit = skip;

		// Assume true until proven otherwise.
		bool result = true;

		// Parse expression set.
		int numExpressions = GetToken();
		if (_readError)
		{
			return false;
		}

		for (int i = 0; i < numExpressions; ++i)
		{
			bool expressionResult = EvaluateExpression(tags, shortCircuit);

			if (!shortCircuit && expressionResult)
			{
				// One match is sufficient for fail result.
				result = false;
				shortCircuit = true;
			}
		}

		return result;
	}

	private byte GetToken()
	{
		if (_query.QueryTokenStream.Count > _curStreamIdx)
		{
			return _query.QueryTokenStream[_curStreamIdx++];
		}

		_readError = true;
		Debug.Fail("Error parsing GameplayTagQuery! Code should not reach this point.");

		return 0;
	}
}
