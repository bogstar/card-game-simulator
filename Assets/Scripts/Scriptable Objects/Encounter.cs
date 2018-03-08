using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class Encounter
{
	public EnemyScriptableObject[] enemies;

	public Encounter(EncounterScriptableObject encounterData)
	{
		enemies = encounterData.enemies;
	}
}