using GameplayTags.Runtime.Attribute;
using GameplayTags.Runtime.GameplayEffect;
using Godot;
using System;
using Attribute = GameplayTags.Runtime.Attribute.Attribute;

public partial class Player : Node, IGameplaySystem
{
	[Export]
	public int Health;
	[Export]
	public int Mana;
	[Export]
	public int Strength;
	[Export]
	public int Agility;
	[Export]
	public int Intelligence;

	public GameplaySystem GameplaySystem { get; set; }

	public PlayerAttributeSet PlayerAttributeSet { get; set; }

	public override void _Ready()
	{
		base._Ready();
		
		GameplaySystem = new GameplaySystem();
		PlayerAttributeSet = new PlayerAttributeSet(Health, Mana, Strength, Agility, Intelligence);
		GameplaySystem.AddAttributeSet(PlayerAttributeSet);
	}

	public override void _Process(double delta)
	{
		base._Process(delta);

		GameplaySystem.GameplayEffectsManager.UpdateEffects(delta);
	}
}

public class PlayerAttributeSet : AttributeSet
{
	public readonly Attribute Health;

	public readonly Attribute Mana;

	public readonly Attribute Strength;

	public readonly Attribute Agility;

	public readonly Attribute Intelligence;

	private readonly int _defaultHealth;
	private readonly int _defaultMana;
	private readonly int _defaultStrength;
	private readonly int _defaultAgility;
	private readonly int _defaultIntelligence;

	public PlayerAttributeSet(int health, int mana, int strength, int agility, int intelligence)
	{
		_defaultHealth = health;
		_defaultMana = mana;
		_defaultStrength = strength;
		_defaultAgility = agility;
		_defaultIntelligence = intelligence;

		InitializeAttributes();
	}

	protected override void InitializeAttributes()
	{
		InitializeAttribute(Health, _defaultHealth, 0, 1000);
		InitializeAttribute(Mana, _defaultMana, 0, 1000);
		InitializeAttribute(Strength, _defaultStrength, 0, 99);
		InitializeAttribute(Agility, _defaultAgility, 0, 99);
		InitializeAttribute(Intelligence, _defaultIntelligence, 0, 99);
	}
}
