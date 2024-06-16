namespace GameplayTags.Runtime;

/// <summary>
/// Interface for things that have tags.
/// </summary>
public interface ITaggable
{
	/// <summary>
	/// Gets or sets the tags for this object.
	/// </summary>
	GameplayTagContainer Tags { get; set; }
}
