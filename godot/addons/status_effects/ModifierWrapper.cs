using GameplayTags.Runtime;
using GameplayTags.Runtime.GameplayEffect;
using Godot;

[Tool]
[GlobalClass]
public partial class ModifierWrapper : Resource
{
	[Export]
	public string Attribute { get; set; }

	[Export]
	public ModifierOperation Operation { get; set; }

	[Export]
	public float Magnitude { get; set; }

	[Export]
	public int Channel { get; set; }

	public Modifier GetModifier()
	{
		return new Modifier
		{
			Attribute = TagName.FromString(Attribute),
			Operation = Operation,
			Magnitude = new ModifierMagnitude
			{
				MagnitudeCalculationType = MagnitudeCalculationType.ScalableFloat,
				ScalableFloatMagnitude = new ScalableFloat(Magnitude),
			},
			Channel = Channel,
		};
	}
}
