namespace GameplayTags.Runtime.GameplayEffect;

public struct GameplayEffectContext
{
	public IGameplaySystem Instigator; // Entity responsible for causing the action or event (eg. Character, NPC, Environment)
	public IGameplaySystem EffectCauser; // The actual entity that caused the effect (eg. Weapon, Projectile, Trap)
}

public class GameplayEffect
{
	private int _level;

	public event Action<int> OnLevelChanged;

	public GameplayEffectData EffectData { get; }

	// Maybe this should be a method since it fires an event
	public int Level
	{
		get
		{
			return _level;
		}

		set
		{
			_level = value;
			OnLevelChanged?.Invoke(value);
		}
	}

	public GameplayEffectContext Context { get; }

	public GameplayEffect(GameplayEffectData effectData, int level, GameplayEffectContext context)
	{
		EffectData = effectData;
		_level = level;
		Context = context;
	}

	public void LevelUp()
	{
		Level++;
	}

	internal void Execute(GameplayEffectEvaluatedData effectEvaluatedData)
	{
		foreach(var modifier in effectEvaluatedData.ModifiersEvaluatedData)
		{
			switch (modifier.ModifierOperation)
			{
				case ModifierOperation.Flat:
					modifier.Attribute.ExecuteFlatModifier((int)modifier.Magnitude);
					break;

				case ModifierOperation.Percent:
					modifier.Attribute.ExecutePercentModifier(modifier.Magnitude);
					break;

				case ModifierOperation.Override:
					modifier.Attribute.ExecuteOverride((int)modifier.Magnitude);
					break;
			}
		}
	}
}
