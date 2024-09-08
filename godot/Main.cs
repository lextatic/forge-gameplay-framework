using Godot;

public partial class Main : Node
{
	[Export]
	public Tags Tags1;

	[Export]
	public Tags Tags2;

	[Export]
	public Player Player;

	[Export]
	public GameplayEffect Effect;

	public override void _Process(double delta)
	{
		base._Process(delta);

		//GD.Print($"Tags1: [{Tags1.Container}], Tags2: [{Tags2.Container}], {Tags1.Container.HasAllExact(Tags2.Container)}");

		if (Input.IsActionJustPressed("Test"))
		{
			var effectData = Effect.GetEffectData();

			var effect = new GameplayTags.Runtime.GameplayEffect.GameplayEffect(
				effectData,
				1,
				new GameplayTags.Runtime.GameplayEffect.GameplayEffectContext
				{
					EffectCauser = Player,
					Instigator = Player,
				});

			Player.GameplaySystem.GameplayEffectsManager.ApplyEffect(effect);
			//Tags1.Container.AppendTags(Tags2.Container);
		}
	}
}
