namespace GameplayTags.Runtime;

/// <summary>
/// Exception for then a <see cref="GameplayTagNode"/> for a given <see cref="TagName"/> is not found on the tag map.
/// </summary>
public class GameplayTagNotFoundException : Exception
{
	/// <summary>
	/// Initializes a new instance of the <see cref="GameplayTagNotFoundException"/> class.
	/// </summary>
	/// <param name="tagName"><see cref="TagName"/> to which <see cref="GameplayTagNode"/> was not found.</param>
	public GameplayTagNotFoundException(TagName tagName)
		: base($"GameplayTag for TagName: '{tagName}' could not be found within the tags tree.")
	{
	}

	/// <summary>
	/// Initializes a new instance of the <see cref="GameplayTagNotFoundException"/> class.
	/// </summary>
	public GameplayTagNotFoundException()
		: base()
	{
	}

	/// <summary>
	/// Initializes a new instance of the <see cref="GameplayTagNotFoundException"/> class.
	/// </summary>
	/// <param name="message">A custom exception message.</param>
	public GameplayTagNotFoundException(string? message)
		: base(message)
	{
	}

	/// <summary>
	/// Initializes a new instance of the <see cref="GameplayTagNotFoundException"/> class.
	/// </summary>
	/// <param name="message">A custom exception message.</param>
	/// <param name="innerException">InnerException for propragation.</param>
	public GameplayTagNotFoundException(string? message, Exception? innerException)
		: base(message, innerException)
	{
	}
}
