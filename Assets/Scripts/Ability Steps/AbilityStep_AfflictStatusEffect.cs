using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "New Ability Step Afflict Satus Effect", menuName = "Ability Steps/Afflict Status Effect")]
public class AbilityStep_AfflictStatusEffect : AbilityStep_Base
{
	public StatusEffect statusEffect;

	public override OnPlayResult OnPlay(params object[] parameters)
	{
		List<HealthEntity> targets = parameters[0] as List<HealthEntity>;

		OnPlayResult onPlayResult = new OnPlayResult();

		foreach (var t in targets)
		{
			t.AfflictWithStatusEffect(statusEffect);
		}

		return onPlayResult;
	}
}