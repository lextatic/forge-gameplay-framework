namespace GameplayTags.Runtime.GameplayEffect;

/** Saves list of modified attributes, to use for gameplay cues or later processing */
struct GameplayEffectModifiedAttribute
{
	public GameplayTags.Runtime.Attribute.Attribute Attribute;

	/** Total magnitude applied to that attribute */
	public float TotalMagnitude;
}

public class GameplayEffectInstanceUE
{
	public GameplayEffectDataUE GameplayEffectDefinition;

	List<GameplayEffectModifiedAttribute> ModifiedAttributes;

	public float Duration;

	public float Period;
}

public class GameplayEffectDataUE
{
	public GameplayEffectDurationType DurationPolicy;

	public GameplayEffectModifierMagnitude DurationMagnitude;

	public ScalableFloat Period;

	public bool ExecutePeriodicEffectOnApplication;

	public GameplayEffectPeriodInhibitionRemovedPolicy PeriodicInhibitionPolicy;

	public List<GameplayModifierInfo> Modifiers;

	public List<GameplayEffectDataUE> OverflowEffects;

	public bool DenyOverflowApplication;

	public bool ClearStackOnOverflow;

	public bool RequireModifierSuccessToTriggerCues;

	public bool SuppressStackingCues;
	public List<string>  GameplayCues;

	// Components

	//protected List<GameplayEffectComponent> GEComponents;

	//public T FindComponent<T>();
	//public void AddComponent<T>(T component);
	//public void FindOrAddComponent<T>(T component);

	// Stacking
	public GameplayEffectStackingType StackingType;

	public int StackLimitCount;

	public GameplayEffectStackingDurationPolicy StackDurationRefreshPolicy;

	public GameplayEffectStackingPeriodPolicy StackPeriodResetPolicy;

	public GameplayEffectStackingExpirationPolicy StackExpirationPolicy;

	// Cached

	/** Cached copy of all the tags this GE has. Data populated during PostLoad. */
	public GameplayTagContainer CachedAssetTags;

	/** Cached copy of all the tags this GE grants to its target. Data populated during PostLoad. */
	public GameplayTagContainer CachedGrantedTags;

	/** Cached copy of all the tags this GE applies to its target to block Gameplay Abilities. Data populated during PostLoad. */
	public GameplayTagContainer CachedBlockedAbilityTags;

	public GameplayEffectDataUE()
	{
		DurationPolicy = GameplayEffectDurationType.Instant;
		ExecutePeriodicEffectOnApplication = true;
		PeriodicInhibitionPolicy = GameplayEffectPeriodInhibitionRemovedPolicy.NeverReset;
		StackingType = GameplayEffectStackingType.None;
		StackLimitCount = 0;
		StackDurationRefreshPolicy = GameplayEffectStackingDurationPolicy.RefreshOnSuccessfulApplication;
		StackPeriodResetPolicy = GameplayEffectStackingPeriodPolicy.ResetOnSuccessfulApplication;
		RequireModifierSuccessToTriggerCues = true;
	}
}

public struct GameplayEffectModifierMagnitude 
{
	GameplayEffectMagnitudeCalculation MagnitudeCalculationType;

	ScalableFloat ScalableFloatMagnitude;

	/** Magnitude value represented by an attribute-based float
	(Coefficient * (PreMultiplyAdditiveValue + [Eval'd Attribute Value According to Policy])) + PostMultiplyAdditiveValue */
	AttributeBasedFloat AttributeBasedMagnitude;

	/** Magnitude value represented by a custom calculation class */
	CustomCalculationBasedFloat CustomMagnitude;

	/** Magnitude value represented by a SetByCaller magnitude */
	SetByCallerFloat SetByCallerMagnitude;
}

/** 
 * Struct representing a float whose magnitude is dictated by a backing attribute and a calculation policy, follows basic form of:
 * (Coefficient * (PreMultiplyAdditiveValue + [Eval'd Attribute Value According to Policy])) + PostMultiplyAdditiveValue
 */
struct AttributeBasedFloat
{
	/** Coefficient to the attribute calculation */
	ScalableFloat Coefficient;

	/** Additive value to the attribute calculation, added in before the coefficient applies */
	ScalableFloat PreMultiplyAdditiveValue;

	/** Additive value to the attribute calculation, added in after the coefficient applies */
	ScalableFloat PostMultiplyAdditiveValue;

	/** Attribute backing the calculation */
	//GameplayEffectAttributeCaptureDefinition BackingAttribute;

	/** If a curve table entry is specified, the attribute will be used as a lookup into the curve instead of using the attribute directly. */
	Curve AttributeCurve;

	/** Calculation policy in regards to the attribute */
	AttributeBasedFloatCalculationType AttributeCalculationType;

	/** Channel to terminate evaluation on when using AttributeEvaluatedUpToChannel calculation type */
	//GameplayModEvaluationChannel FinalChannel;

	/** Filter to use on source tags; If specified, only modifiers applied with all of these tags will factor into the calculation */
	GameplayTagContainer SourceTagFilter;

	/** Filter to use on target tags; If specified, only modifiers applied with all of these tags will factor into the calculation */
	GameplayTagContainer TargetTagFilter;
}

struct CustomCalculationBasedFloat
{
	//TSubclassOf<UGameplayModMagnitudeCalculation> CalculationClassMagnitude;

	/** Coefficient to the custom calculation */
	ScalableFloat Coefficient;

	/** Additive value to the attribute calculation, added in before the coefficient applies */
	ScalableFloat PreMultiplyAdditiveValue;

	/** Additive value to the attribute calculation, added in after the coefficient applies */
	ScalableFloat PostMultiplyAdditiveValue;

	/** If a curve table entry is specified, the OUTPUT of this custom class magnitude (including the pre and post additive values) lookup into the curve instead of using the attribute directly. */
	Curve FinalLookupCurve;
}

/** Struct for holding SetBytCaller data */
struct SetByCallerFloat
{
	/** The Name the caller (code or blueprint) will use to set this magnitude by. */
	TagName DataName;

	GameplayTag DataTag;
}

public enum GameplayEffectStackingType : byte
{
	/** No stacking. Multiple applications of this GameplayEffect are treated as separate instances. */
	None,
	/** Each caster has its own stack. */
	AggregateBySource,
	/** Each target has its own stack. */
	AggregateByTarget,
};

public enum GameplayModifierOperation
{
	Additive,
	Multiply,
	Override
}

/** Encapsulate require and ignore tags */
public struct GameplayTagRequirements
{
	/** All of these tags must be present */
	GameplayTagContainer RequireTags;

	/** None of these tags may be present */
	GameplayTagContainer IgnoreTags;

	GameplayTagQuery TagQuery;

	/** True if all required tags and no ignore tags found */
	//bool RequirementsMet(const FGameplayTagContainer& Container) const;

	///** True if neither RequireTags or IgnoreTags has any tags */
	//bool IsEmpty() const;

	///** Return debug string */
	//FString ToString() const;

	//bool operator ==(const FGameplayTagRequirements& Other) const;
	//bool operator !=(const FGameplayTagRequirements& Other) const;

	///** Converts the RequireTags and IgnoreTags fields into an equivalent FGameplayTagQuery */
	//GameplayTagQuery ConvertTagFieldsToTagQuery() const;
};

public struct GameplayModifierInfo
{
	/** The Attribute we modify or the GE we modify modifies. */
	public GameplayTags.Runtime.Attribute.Attribute Attribute;

	/** The numeric operation of this modifier: Override, Add, Multiply, etc  */
	public GameplayModifierOperation ModifierOp;// = GameplayModifierOperation.Additive;

	/** Magnitude of the modifier */
	public GameplayEffectModifierMagnitude ModifierMagnitude;

	/** Evaluation channel settings of the modifier */
	//public GameplayModEvaluationChannelSettings EvaluationChannelSettings;

	GameplayTagRequirements SourceTags;

	GameplayTagRequirements TargetTags;

	///** Equality/Inequality operators */
	//bool operator ==(const FGameplayModifierInfo& Other) const;
	//bool operator !=(const FGameplayModifierInfo& Other) const;
}

public struct CurveKey
{
	public float Time { get; set; }
	public float Value { get; set; }

	public CurveKey(float time, float value)
	{
		Time = time;
		Value = value;
	}
}

public class Curve
{
	private List<CurveKey> keys = new List<CurveKey>();

	public void AddKey(float time, float value)
	{
		keys.Add(new CurveKey(time, value));
		keys.Sort((a, b) => a.Time.CompareTo(b.Time));
	}

	public float Evaluate(float time)
	{
		if (keys.Count == 0)
		{
			return 1.0f; // Default scaling factor if no keys are defined
		}

		if (time <= keys[0].Time)
		{
			return keys[0].Value;
		}

		if (time >= keys[keys.Count - 1].Time)
		{
			return keys[keys.Count - 1].Value;
		}

		for (int i = 0; i < keys.Count - 1; i++)
		{
			if (time >= keys[i].Time && time <= keys[i + 1].Time)
			{
				float t = (time - keys[i].Time) / (keys[i + 1].Time - keys[i].Time);
				return keys[i].Value + (t * (keys[i + 1].Value - keys[i].Value));
			}
		}

		return 1.0f; // Fallback
	}
}

public struct ScalableFloat
{
	public float BaseValue { get; set; }
	public Curve ScalingCurve { get; set; }

	public ScalableFloat(float baseValue)
	{
		BaseValue = baseValue;
		ScalingCurve = new Curve();
	}

	public float GetValue(float time)
	{
		float scalingFactor = ScalingCurve.Evaluate(time);
		return BaseValue * scalingFactor;
	}
}
