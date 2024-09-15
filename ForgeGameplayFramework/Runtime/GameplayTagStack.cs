namespace GameplayTags.Runtime;

/// <summary>
/// An implementation of the <see cref="GameplayTag"/> that supports stacks.
/// </summary>
public struct GameplayTagStack
{
	/// <summary>
	/// Gets the <see cref="GameplayTag"/> for this tag stack.
	/// </summary>
	public GameplayTag Tag { get; }

	/// <summary>
	/// Gets the number of stacks for this <see cref="GameplayTagStack"/>.
	/// </summary>
	public int Count { get; internal set; }

	/// <summary>
	/// Initializes a new instance of the <see cref="GameplayTagStack"/> struct.
	/// </summary>
	/// <param name="tag">The <see cref="GameplayTag"/> for this stack.</param>
	/// <param name="count">The number of initial stacks.</param>
	public GameplayTagStack(GameplayTag tag, int count = 1)
	{
		Tag = tag;
		Count = count;
	}

	/// <summary>
	/// Returns a <see cref="string"/> representation of the <see cref="GameplayTagStack"/>.
	/// </summary>
	/// <returns>The <see cref="GameplayTagStack"/> as a <see cref="string"/>.</returns>
	public override string ToString()
	{
		return $"{Tag}:{Count}";
	}
}
