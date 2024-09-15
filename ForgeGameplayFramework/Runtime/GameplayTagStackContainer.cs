using System;

namespace GameplayTags.Runtime;

/// <summary>
/// A <see cref="GameplayTagStackContainer"/> holds a collection of <see cref="GameplayTagStack"/>s.
/// </summary>
public readonly struct GameplayTagStackContainer
{
	private readonly HashSet<GameplayTagStack> _stacks = new ();

	private readonly Dictionary<GameplayTag, int> _tagCountMap = new ();

	/// <summary>
	/// Initializes a new instance of the <see cref="GameplayTagStackContainer"/> struct.
	/// </summary>
	public GameplayTagStackContainer()
	{
	}

	/// <summary>
	/// Adds a certain ammount of stacks from the given tag.
	/// </summary>
	/// <param name="tag">The tag to remove stacks from.</param>
	/// <param name="count">The amount of stacks to remove.</param>
	public void AddStackCount(GameplayTag tag, int count)
	{
		if (count > 0)
		{
			var modifiedStack = _stacks.Single(x => x.Tag == tag);

			// What if it doesn't exist in the _stacks yet?
			modifiedStack.Count += count;
			_tagCountMap[tag] = modifiedStack.Count;
		}
	}

	/// <summary>
	/// Removes a certain ammount of stacks from the given tag.
	/// </summary>
	/// <param name="tag">The tag to remove stacks from.</param>
	/// <param name="count">The amount of stacks to remove.</param>
	public void RemoveStackCount(GameplayTag tag, int count)
	{
		// Should check if contains first?
		if (count > 0)
		{
			var modifiedStack = _stacks.Single(x => x.Tag == tag);

			if (modifiedStack.Count <= count)
			{
				_stacks.Remove(modifiedStack);
				_tagCountMap.Remove(tag);
			}
			else
			{
				modifiedStack.Count -= count;
				_tagCountMap[tag] = modifiedStack.Count;
			}
		}
	}

	/// <summary>
	/// Check whether this StackContainer has a given tag.
	/// </summary>
	/// <param name="tag">Tag to check.</param>
	/// <returns>True if contains, false otherwise.</returns>
	public readonly bool HasTag(GameplayTag tag)
	{
		return _tagCountMap.ContainsKey(tag);
	}

	/// <summary>
	/// Gets the stack count for a given tag.
	/// </summary>
	/// <param name="tag">Tag to check.</param>
	/// <returns>Number of stacks for the given tag.</returns>
	public readonly int GetStackCount(GameplayTag tag)
	{
		// What if it doesn't contain the tag?
		return _tagCountMap[tag];
	}
}
