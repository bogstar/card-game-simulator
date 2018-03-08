using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "New Ability", menuName = "Enemy/Ability")]
public class EnemyAbilityScriptableObject : ScriptableObject
{
	public Type type;

	public enum Type { None, Offensive, Blocking, Healing, Buffing }

	public AbilityStepsWithTargetingData_Enemy[] abilityStepsWithTargetingData;
}

[System.Serializable]
public class AbilityStepsWithTargetingData_Enemy : AbilityStepWithTargetingData_Base
{
	public new TargetingData_Enemy targetingData;
}

[System.Serializable]
public class TargetingData_Enemy : TargetingData_Base
{
	
}