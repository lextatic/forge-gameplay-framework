namespace GameplayTags.Runtime.GameplayEffect;

public struct ModifierEvaluatedData
{
	public Attribute.Attribute Attribute;
	public ModifierOperation ModifierOperation;
	public float Magnitude;
	public int Channel;
	public bool IsValid; // remove if not used
}

public struct GameplayEffectEvaluatedData
{
	public GameplayEffect GameplayEffect;
	public List<ModifierEvaluatedData> ModifiersEvaluatedData;
	public int Level;
	public int Stack;
	public float Duration;
	public float Period;
	public GameplaySystem Target;
}
