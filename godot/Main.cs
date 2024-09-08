using GameplayTags.Runtime.GameplayEffect;
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

	private GameplayEffectData _effectData;

	public override void _Ready()
	{
		base._Ready();

		_effectData = Effect.GetEffectData();
	}

	public override void _Process(double delta)
	{
		base._Process(delta);

		//GD.Print($"Tags1: [{Tags1.Container}], Tags2: [{Tags2.Container}], {Tags1.Container.HasAllExact(Tags2.Container)}");

		if (Input.IsActionJustPressed("Test"))
		{
			var effect = new GameplayTags.Runtime.GameplayEffect.GameplayEffect(
				_effectData,
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
