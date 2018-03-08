using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "New Dungeon", menuName = "Dungeon/Dungeon")]
public class DungeonScriptableObject : ScriptableObject
{
	public new string name;

	public EncounterScriptableObject[] encounters;
}