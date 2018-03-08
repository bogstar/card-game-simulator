using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "New Encounter", menuName = "Dungeon/Encounter")]
public class EncounterScriptableObject : ScriptableObject
{
	public EnemyScriptableObject[] enemies;
}