using Godot;
using Godot.Collections;

[Tool]
[GlobalClass]
public partial class RegisteredTags : Resource
{
	[Export]
	public Array<string> Tags;
}
