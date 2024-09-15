namespace GameplayTags.Runtime;

/// <summary>
/// Types of <see cref="GameplayTagQueryExpression"/>s.
/// </summary>
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
	NoExpressionsMatch,
}
