using System;
using System.Diagnostics;

namespace GameplayTags.Runtime
{
	public static class ListExtensions
	{
		public static int AddUnique<T>(this List<T> list, T element)
		{
			if (!list.Contains(element))
			{
				list.Add(element);
				return list.Count() - 1;
			}

			return list.IndexOf(element);
		}
	}

	internal enum GameplayTagQueryExpressionType
	{
		Undefined = 0,
		AnyTagsMatch,
		AllTagsMatch,
		NoTagsMatch,
		AnyTagsMatchExact,
		AllTagsMatchExact,
		NoTagsMatchExact,
		AnyExpressionsMatch,
		AllExpressionsMatch,
		NoExpressionsMatch
	};

	public class GameplayTagQueryExpression
	{
		private GameplayTagQueryExpressionType _expressionType;

		/** Expression list, for expression types that need it */
		private List<GameplayTagQueryExpression> _expressionSet = new ();

		/** Tag list, for expression types that need it */
		private List<GameplayTag> _tagSet = new();

		public GameplayTagQueryExpression AnyTagsMatch()
		{
			_expressionType = GameplayTagQueryExpressionType.AnyTagsMatch;
			return this;
		}

		public GameplayTagQueryExpression AllTagsMatch()
		{
			_expressionType = GameplayTagQueryExpressionType.AllTagsMatch;
			return this;
		}

		public GameplayTagQueryExpression NoTagsMatch()
		{
			_expressionType = GameplayTagQueryExpressionType.NoTagsMatch;
			return this;
		}

		public GameplayTagQueryExpression AnyTagsMatchExact()
		{
			_expressionType = GameplayTagQueryExpressionType.AnyTagsMatchExact;
			return this;
		}

		public GameplayTagQueryExpression AllTagsMatchExact()
		{
			_expressionType = GameplayTagQueryExpressionType.AllTagsMatchExact;
			return this;
		}

		public GameplayTagQueryExpression NoTagsMatchExact()
		{
			_expressionType = GameplayTagQueryExpressionType.NoTagsMatchExact;
			return this;
		}

		public GameplayTagQueryExpression AnyExpressionsMatch()
		{
			_expressionType = GameplayTagQueryExpressionType.AnyExpressionsMatch;
			return this;
		}

		public GameplayTagQueryExpression AllExpressionsMatch()
		{
			_expressionType = GameplayTagQueryExpressionType.AllExpressionsMatch;
			return this;
		}

		public GameplayTagQueryExpression NoExpressionsMatch()
		{
			_expressionType = GameplayTagQueryExpressionType.NoExpressionsMatch;
			return this;
		}

		public GameplayTagQueryExpression AddTag(string tagString)
		{
			return AddTag(TagName.FromString(tagString));
		}

		public GameplayTagQueryExpression AddTag(TagName tagName)
		{
			return AddTag(GameplayTag.RequestGameplayTag(tagName));
		}

		public GameplayTagQueryExpression AddTag(GameplayTag tag)
		{
			Debug.Assert(UsesTagSet());
			_tagSet.Add(tag);
			return this;
		}

		public GameplayTagQueryExpression AddTags(GameplayTagContainer tags)
		{
			Debug.Assert(UsesTagSet());
			_tagSet.AddRange(tags.GameplayTags);
			return this;
		}

		public GameplayTagQueryExpression AddExpression(GameplayTagQueryExpression expression)
		{
			Debug.Assert(UsesExpressionSet());
			_expressionSet.Add(expression);
			return this;
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
	
		/** Returns true if this expression uses the expression list data. */
		private bool UsesExpressionSet()
		{
			return (_expressionType == GameplayTagQueryExpressionType.AllExpressionsMatch)
				|| (_expressionType == GameplayTagQueryExpressionType.AnyExpressionsMatch)
				|| (_expressionType == GameplayTagQueryExpressionType.NoExpressionsMatch);
		}

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
					// emit tagset
					byte numTags = (byte)_tagSet.Count;
					tokenStream.Add(numTags);

					foreach (var tag in _tagSet)
					{
						int tagIndex = tagDictionary.AddUnique(tag);
						Debug.Assert(tagIndex <= 254);       // we reserve token 255 for internal use, so 254 is max unique tags
						tokenStream.Add((byte)tagIndex);
					}

					break;

				case GameplayTagQueryExpressionType.AnyExpressionsMatch:
				case GameplayTagQueryExpressionType.AllExpressionsMatch:
				case GameplayTagQueryExpressionType.NoExpressionsMatch:
					// emit tagset
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
	}
}
