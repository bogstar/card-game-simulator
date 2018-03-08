using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "New Enemy", menuName = "Enemy/Enemy")]
public class EnemyScriptableObject : ScriptableObject
{
	public new string name;
	public Sprite[] pictures;
	public int health;
	public AudioClip[] attackSounds;
	public AudioClip[] deathSounds;

	public AbilitiesWithChance[] abilities;

	[System.Serializable]
	public struct AbilitiesWithChance
	{
		public EnemyAbilityScriptableObject ability;
		public int chance;
	}
}