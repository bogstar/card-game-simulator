using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "New Card", menuName = "Player/Card")]
public class CardPlayerScriptableObject : ScriptableObject
{
	public new string name;
	public Sprite picture;
	public string description;
	public Type type;
	public Rarity rarity;
	public int manaCost;
	public AudioClip[] soundsForPlay;

	public List<int> values;

	public enum Type { Attack, Skill, Power }
	public enum Rarity { Common }

	public AbilityStepsWithTargetingData_Player[] abilityStepsWithTargetingData;
}

[System.Serializable]
public class AbilityStepsWithTargetingData_Player : AbilityStepWithTargetingData_Base
{
	public new TargetingData_Player targetingData;
}

[System.Serializable]
public class TargetingData_Player : TargetingData_Base
{
	public string textForChoosing;
}