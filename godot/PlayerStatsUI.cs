using Godot;
using System;

public partial class PlayerStatsUI : Node
{
	[Export]
	public Player Player { get; set; }

	[Export]
	public Label HealthLabel { get; set; }

	[Export]
	public Label ManaLabel { get; set; }

	[Export]
	public Label StrengthLabel { get; set; }

	[Export]
	public Label AgilityLabel { get; set; }

	[Export]
	public Label IntelligenceLabel { get; set; }

	public override void _Ready()
	{
		base._Ready();

		Player.PlayerAttributeSet.Health.OnValueChanged += Health_OnValueChanged;
		Player.PlayerAttributeSet.Mana.OnValueChanged += Mana_OnValueChanged;
		Player.PlayerAttributeSet.Strength.OnValueChanged += Strength_OnValueChanged;
		Player.PlayerAttributeSet.Agility.OnValueChanged += Agility_OnValueChanged;
		Player.PlayerAttributeSet.Intelligence.OnValueChanged += Intelligence_OnValueChanged;

		HealthLabel.Text = $"Health: {Player.PlayerAttributeSet.Health.TotalValue} = {Player.PlayerAttributeSet.Health.BaseValue} + {Player.PlayerAttributeSet.Health.Modifier}";
		ManaLabel.Text = $"Mana: {Player.PlayerAttributeSet.Mana.TotalValue} = {Player.PlayerAttributeSet.Mana.BaseValue} + {Player.PlayerAttributeSet.Mana.Modifier}";
		StrengthLabel.Text = $"Strength: {Player.PlayerAttributeSet.Strength.TotalValue} = {Player.PlayerAttributeSet.Strength.BaseValue} + {Player.PlayerAttributeSet.Strength.Modifier}";
		AgilityLabel.Text = $"Agility: {Player.PlayerAttributeSet.Agility.TotalValue} = {Player.PlayerAttributeSet.Agility.BaseValue} + {Player.PlayerAttributeSet.Agility.Modifier}";
		IntelligenceLabel.Text = $"Intelligence: {Player.PlayerAttributeSet.Intelligence.TotalValue} = {Player.PlayerAttributeSet.Intelligence.BaseValue} + {Player.PlayerAttributeSet.Intelligence.Modifier}";
	}

	private void Health_OnValueChanged(GameplayTags.Runtime.Attribute.Attribute health, int change)
	{
		HealthLabel.Text = $"Health: {Player.PlayerAttributeSet.Health.TotalValue} = {Player.PlayerAttributeSet.Health.BaseValue} + {Player.PlayerAttributeSet.Health.Modifier}";
	}

	private void Mana_OnValueChanged(GameplayTags.Runtime.Attribute.Attribute mana, int change)
	{
		ManaLabel.Text = $"Mana: {Player.PlayerAttributeSet.Mana.TotalValue} = {Player.PlayerAttributeSet.Mana.BaseValue} + {Player.PlayerAttributeSet.Mana.Modifier}";
	}

	private void Strength_OnValueChanged(GameplayTags.Runtime.Attribute.Attribute strength, int change)
	{
		StrengthLabel.Text = $"Strength: {Player.PlayerAttributeSet.Strength.TotalValue} = {Player.PlayerAttributeSet.Strength.BaseValue} + {Player.PlayerAttributeSet.Strength.Modifier}";
	}

	private void Agility_OnValueChanged(GameplayTags.Runtime.Attribute.Attribute agility, int change)
	{
		AgilityLabel.Text = $"Agility: {Player.PlayerAttributeSet.Agility.TotalValue} = {Player.PlayerAttributeSet.Agility.BaseValue} + {Player.PlayerAttributeSet.Agility.Modifier}";
	}

	private void Intelligence_OnValueChanged(GameplayTags.Runtime.Attribute.Attribute ingelligence, int change)
	{
		IntelligenceLabel.Text = $"Intelligence: {Player.PlayerAttributeSet.Intelligence.TotalValue} = {Player.PlayerAttributeSet.Intelligence.BaseValue} + {Player.PlayerAttributeSet.Intelligence.Modifier}";
	}

}
