using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class AbilityStep_Base : ScriptableObject
{
	public abstract OnPlayResult OnPlay(params object[] parameters);
	
	public struct OnPlayResult
	{
		public int unshieldedDamage;
		public HealthEntity summonedEntity;
	}
}