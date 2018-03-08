using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "New Ability Step Summon", menuName = "Ability Steps/Summon")]
public class AbilityStep_Summon : AbilityStep_Base
{
	public EnemyScriptableObject[] summonables;

	public override OnPlayResult OnPlay(params object[] parameters)
	{
		OnPlayResult onPlayResult = new OnPlayResult();

		HealthEntity summoned = FindObjectOfType<BattleManager>().SummonOnEnemySide(summonables[Random.Range(0, summonables.Length)]);

		onPlayResult.summonedEntity = summoned;

		return onPlayResult;
	}
}