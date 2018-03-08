using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "New Ability Step Drain", menuName = "Ability Steps/Drain")]
public class AbilityStep_Drain : AbilityStep_Base
{
	public override OnPlayResult OnPlay(params object[] parameters)
	{
		List<HealthEntity> targets = parameters[0] as List<HealthEntity>;
		OnPlayResult onPlayResult = (OnPlayResult)parameters[1];

		foreach (var t in targets)
		{
			t.TakeHealing(onPlayResult.unshieldedDamage);
		}

		return default(OnPlayResult);
	}
}