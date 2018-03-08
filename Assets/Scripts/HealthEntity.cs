using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class HealthEntity : MonoBehaviour
{
	public new string name;

	public int health;
	public int maxHealth;
	public int armor;
	public StatusEffect statusEffect;
	public Alignment alignment;

	public abstract Enemy.DamageInfo TakeDamage(int amount);
	public abstract void TakeHealing(int amount);
	public abstract void ModifyArmor(int amount);
	public abstract void ResetArmor();
	public abstract void AfflictWithStatusEffect(StatusEffect effect);
	public abstract void OnTurnStart();

	public enum Alignment { None, Good, Evil }
}