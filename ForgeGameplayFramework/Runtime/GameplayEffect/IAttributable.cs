using GameplayTags.Runtime.Attribute;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Diagnostics;

namespace GameplayTags.Runtime.GameplayEffect;
#pragma warning disable SA1600

public interface IForgeEntity
{
	public GameplayEffectsManager GameplayEffectsManager { get; }

	// TODO: Convert AttributeSets into a container class that also keeps the Attributes dictionary.
	public List<AttributeSet> AttributeSets { get; }

	public Dictionary<TagName, Attribute.Attribute> Attributes { get; }

	public GameplayTagContainer GameplayTags { get; }

	//void AddAttributeSet(AttributeSet attributeSet);

	public void AddAttributeSet(AttributeSet attributeSet)
	{
		Debug.Assert(attributeSet is not null, "AttributeSets is not initialized.");

		AttributeSets.Add(attributeSet);

		foreach (var attribute in attributeSet.AttributesMap)
		{
			Attributes.Add(attribute.Key, attribute.Value);
		}
	}
}
