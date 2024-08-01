using GameplayTags.Runtime;
using Godot;
using System.Linq;

public partial class GameplayTagsStartup : Node
{
	public override void _Ready()
	{
		var registeredTags = ResourceLoader.Load<RegisteredTags>("res://addons/gameplay_tags/RegisteredTags.tres");
		GameplayTagsManager.Instance.ConstructGameplayTagTreeFromList(registeredTags.Tags.ToList());
		GD.Print("Initialize Tag Tree");
	}
}
