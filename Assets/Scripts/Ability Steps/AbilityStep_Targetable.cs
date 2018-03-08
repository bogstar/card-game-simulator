using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "New Ability Step Targetable", menuName = "Ability Steps/Targetable")]
public class AbilityStep_Targetable : AbilityStep_Base
{
	public Type type;
	public int amount;

	public enum Type { Damage, Heal, Block, ModifyMana }

	public override OnPlayResult OnPlay(params object[] parameters)
	{
		List<HealthEntity> targets = parameters[0] as List<HealthEntity>;

		OnPlayResult onPlayResult = new OnPlayResult();

		if(type == Type.Damage)
		{
			foreach (var t in targets)
			{
				Enemy.DamageInfo damageInfo = t.TakeDamage(amount);
				onPlayResult.unshieldedDamage = damageInfo.damageDealt;
				GameObject go = Instantiate(FindObjectOfType<BattleManager>().particlePrefab);

				if (t is Player)
				{
					go.transform.SetParent(((Player)t).particleEmitter);
				}
				else
				{
					go.transform.SetParent(t.transform);
				}

				go.GetComponent<ParticlesEffect>().Init(damageInfo.damageDealt);
				go.transform.localScale = Vector3.one;
				go.transform.localPosition = new Vector3(0, -20, 0);
			}
		}
		else if(type == Type.Heal)
		{
			foreach (var t in targets)
			{
				t.TakeHealing(amount);
			}
		}
		else if(type == Type.Block)
		{
			foreach (var t in targets)
			{
				t.ModifyArmor(amount);
			}
		}
		else
		{
			foreach (var t in targets)
			{
				((Player)t).ModifyMana(amount);
			}
		}

		return onPlayResult;
	}
}