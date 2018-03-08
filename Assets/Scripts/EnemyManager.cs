using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

[System.Serializable]
public class EnemyManager
{
	public GameObject enemyPrefab;
	public Transform enemyPanel;

	public List<Enemy> SpawnEnemies(List<EnemyScriptableObject> enemiesSO, BattleManager battleManager, System.Action<Enemy> callback)
	{
		List<Enemy> enemies = new List<Enemy>();

		for (int i = 0; i < enemiesSO.Count; i++)
		{
			enemies.Add(SpawnEnemy(enemiesSO[i], i, battleManager, callback));
		}

		return enemies;
	}

	Enemy SpawnEnemy(EnemyScriptableObject enemySO, int index, BattleManager battleManager, System.Action<Enemy> callback)
	{
		GameObject newEnemyGO = GameObject.Instantiate(enemyPrefab, enemyPanel);
		Enemy newEnemy = newEnemyGO.GetComponent<Enemy>();
		newEnemy.Init(enemySO, index);
		newEnemy.OnEnemyDeath += callback;

		newEnemy.attackSounds = enemySO.attackSounds;
		newEnemy.deathSounds = enemySO.deathSounds;

		newEnemy.targetButton.GetComponent<Button>().onClick.AddListener(delegate { battleManager.SelectTarget(newEnemy); });
		newEnemy.AllowTargetable(false);
		newEnemy.nextAbilityImage.gameObject.SetActive(false);

		return newEnemy;
	}

	public void KillAllEnemies(List<Enemy> enemies)
	{
		for (int i = enemies.Count-1; i > -1; i--)
		{
			Enemy e = enemies[i];
			e.OnEnemyDeath(e);
			enemies.Remove(e);
		}
	}

	public void SetEnemiesTargetable(bool targetable, List<Enemy> enemies)
	{
		foreach(var enemy in enemies)
		{
			enemy.AllowTargetable(targetable);
		}
	}
}