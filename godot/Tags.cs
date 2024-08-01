using GameplayTags.Runtime;
using Godot;
using Godot.Collections;

// Without this, InspectorPlugin can't detect this type
[Tool]
public partial class Tags : Node
{
	[Export]
	public Array<string> ContainerTags;

	public GameplayTagContainer Container;

	public override void _Ready()
	{
		base._Ready();

		Container = new GameplayTagContainer();

		foreach (var tag in ContainerTags)
		{
			Container.AddTag(GameplayTag.RequestGameplayTag(TagName.FromString(tag)));
		}
	}

	public void ParseBegin(TagsInspectorPlugin inspectorPlugin)
	{

	}
}
