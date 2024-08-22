using GameplayTags.Runtime.Attribute;

namespace GameplayTags.Runtime.GameplayEffect;
#pragma warning disable SA1600

public interface IGameplaySystem
{
	public GameplaySystem GameplaySystem { get; }
}

public class GameplaySystem
{
	public GameplayEffectsManager GameplayEffectsManager { get; }

	public List<AttributeSet> AttributeSets { get; }

	public Dictionary<TagName, Attribute.Attribute> Attributes { get; }

	public GameplaySystem()
	{
		AttributeSets = new List<AttributeSet>();
		Attributes = new Dictionary<TagName, Attribute.Attribute>();
		GameplayEffectsManager = new GameplayEffectsManager(this);
	}

	public void AddAttributeSet(AttributeSet attributeSet)
	{
		AttributeSets.Add(attributeSet);

		foreach (var attribute in attributeSet.AttributesMap)
		{
			Attributes.Add(attribute.Key, attribute.Value);
		}
	}
}
