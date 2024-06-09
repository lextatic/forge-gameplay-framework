using GameplayTags.Runtime;
using System.Diagnostics;

namespace GameplayTags.Runtime
{ 
	public class QueryEvaluator
	{
		private GameplayTagQuery _query;
		private int _curStreamIdx;
		private bool _readError;

		public QueryEvaluator(GameplayTagQuery query)
		{
			_query = query;
		}

		private byte GetToken()
		{
			if (_query.QueryTokenStream.Count > _curStreamIdx)
			{
				return _query.QueryTokenStream[_curStreamIdx++];
			}

			// "Error parsing GameplayTagQuery!"
			_readError = true;
			return 0;
		}

		public bool Evaluate(GameplayTagContainer tags	)
		{
			_curStreamIdx = 0;

			// start parsing the set
			var version = GetToken();
			
			if (_readError)
			{
				return false;
			}

			bool returnValue = false;

			byte _hasRootExpression = GetToken();
			if (!_readError && _hasRootExpression != 0)
			{
				returnValue = EvaluateExpression(tags);
			}

			Debug.Assert(_curStreamIdx == _query.QueryTokenStream.Count);
			return returnValue;
		}

		public bool EvaluateExpression(GameplayTagContainer tags, bool skip = false)
		{
			GameplayTagQueryExpressionType expressionType = (GameplayTagQueryExpressionType)GetToken();

			// emit exprdata
			switch (expressionType)
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

			Debug.Assert(false);
			return false;
		}

		private bool EvaluateAnyTagsMatch(GameplayTagContainer tags, bool exactMatch, bool skip)
		{
			bool shortCircuit = skip;
			bool result = false;

				// parse tagset
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
					GameplayTag tag = _query.GetTagFromIndex(tagIndex);

					bool hasTag = HasTag(tags, tag, exactMatch);

					if (hasTag)
					{
						// one match is sufficient for a true result!
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

			// assume true until proven otherwise
			bool result = true;

			// parse tagset
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
					GameplayTag tag = _query.GetTagFromIndex(tagIndex);

					bool hasTag = HasTag(tags, tag, exactMatch);

					if (!hasTag)
					{
						// one failed match is sufficient for a false result
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

			// assume true until proven otherwise
			bool result = true;

			// parse tagset
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

				if (shortCircuit == false)
				{
					GameplayTag tag = _query.GetTagFromIndex(tagIndex);

					bool hasTag = HasTag(tags, tag, exactMatch);

					if (hasTag == true)
					{
						// one match is sufficient for a false result
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

			// assume false until proven otherwise
			bool result = false;

			// parse exprset
			int numExpressions = GetToken();
			if (_readError)
			{
				return false;
			}

			for (int i = 0; i < numExpressions; ++i)
			{
				bool expressionResult = EvaluateExpression(tags, shortCircuit);
				if (shortCircuit == false)
				{
					if (expressionResult == true)
					{
						// one match is sufficient for true result
						result = true;
						shortCircuit = true;
					}
				}
			}

			return result;
		}

		private bool EvaluateAllExpressionsMatch(GameplayTagContainer tags, bool skip)
		{
			bool shortCircuit = skip;

			// assume true until proven otherwise
			bool result = true;

			// parse exprset
			int numExpressions = GetToken();
			if (_readError)
			{
				return false;
			}

			for (int i = 0; i < numExpressions; ++i)
			{
				bool expressionResult = EvaluateExpression(tags, shortCircuit);
				if (shortCircuit == false)
				{
					if (expressionResult == false)
					{
						// one fail is sufficient for false result
						result = false;
						shortCircuit = true;
					}
				}
			}

			return result;
		}

		private bool EvaluateNoExpressionsMatch(GameplayTagContainer tags, bool skip)
		{
			bool shortCircuit = skip;

			// assume true until proven otherwise
			bool result = true;

			// parse exprset
			int numExpressions = GetToken();
			if (_readError)
			{
				return false;
			}

			for (int i = 0; i<numExpressions; ++i)
			{
				bool expressionResult = EvaluateExpression(tags, shortCircuit);
				if (shortCircuit == false)
				{
					if (expressionResult == true)
					{
						// one match is sufficient for fail result
						result = false;
						shortCircuit = true;
					}
				}
			}

			return result;
		}
	}
}
