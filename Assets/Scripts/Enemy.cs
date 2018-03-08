using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Enemy : HealthEntity
{
	public Sprite damagingAbilitySprite;
	public Sprite blockingAbilitySprite;
	public Sprite healingAbilitySprite;
	public Sprite buffingAbilitySprite;

	public EnemyScriptableObject enemyScriptableObject;

	public Text nameText;
	public Text targettingText;
	public Image picture;
	public EnemyScriptableObject.AbilitiesWithChance[] abilities;
	public EnemyAbilityScriptableObject nextAbility;
	public List<List<HealthEntity>> targetsPerAbilityStep;

	public GameObject targetButton;
	public Image healthBar;
	public Text healthTextual;
	public float healthBarWidth = 70;
	public Color armoredColorHealthBar;
	public Color healthBarColor;
	public GameObject armorImage;
	public Text armorText;
	public Image nextAbilityImage;
	public Text nextAbilityDamage;

	public StatusEffectBar statusEffectBar;

	public AudioClip[] attackSounds;
	public AudioClip[] deathSounds;

	public System.Action<Enemy> OnEnemyDeath;

	public int enemyIndex;

	bool doingTurn;
	bool doneTurn;


	public void Init(EnemyScriptableObject data, int index)
	{
		maxHealth = health = (int) (data.health * Random.Range(.85f, 1.15f));
		picture.sprite = data.pictures[Random.Range(0, data.pictures.Length)];
		name = nameText.text = data.name;
		abilities = data.abilities;
		targettingText.text = "";
		enemyIndex = index;
		alignment = Alignment.Evil;
		statusEffectBar.statusEffect = null;
		TakeDamage(0);
	}

	public void AllowTargetable(bool allow)
	{
		targetButton.GetComponent<Button>().interactable = allow;
		if(allow)
			targetButton.GetComponent<Image>().color = Color.green;
		else
			targetButton.GetComponent<Image>().color = Color.white;
	}
	
	public void DoTurn(ref bool doing)
	{
		if (doingTurn == false && doneTurn == false)
		{
			doingTurn = doing = true;
			EnemyAbilityScriptableObject nextAbilityTargetable = (EnemyAbilityScriptableObject)nextAbility;

			StartCoroutine(DoAbility());
		}
		else if(doingTurn == false && doneTurn == true)
		{
			doing = false;
			doneTurn = false;
		}
	}
	
	public void ChooseNextAbility()
	{
		nextAbilityImage.gameObject.SetActive(true);

		int totalChance = 0;
		foreach (var a in abilities)
		{
			totalChance += a.chance;
		}

		int randomIndex = Random.Range(0, totalChance);

		int anotherChance = -1;
		foreach (var a in abilities)
		{
			anotherChance += a.chance;
			if (anotherChance >= randomIndex)
			{
				nextAbility = a.ability;
				break;
			}
		}

		EnemyAbilityScriptableObject nextAbilityTargetable = nextAbility;
		if (nextAbilityTargetable.type == EnemyAbilityScriptableObject.Type.Offensive)
		{
			nextAbilityImage.sprite = damagingAbilitySprite;
			AbilityStep_Base att = nextAbilityTargetable.abilityStepsWithTargetingData[0].abilityStep;
			nextAbilityDamage.text = ((AbilityStep_Targetable)att).amount.ToString();
		}
		else if(nextAbilityTargetable.type == EnemyAbilityScriptableObject.Type.Blocking)
		{
			nextAbilityImage.sprite = blockingAbilitySprite;
			nextAbilityDamage.text = "";
		}
		else if (nextAbilityTargetable.type == EnemyAbilityScriptableObject.Type.Healing)
		{
			nextAbilityImage.sprite = healingAbilitySprite;
			nextAbilityDamage.text = "";
		}
		else if (nextAbilityTargetable.type == EnemyAbilityScriptableObject.Type.Buffing)
		{
			nextAbilityImage.sprite = buffingAbilitySprite;
			nextAbilityDamage.text = "";
		}
	}

	IEnumerator DoAbility()
	{
		string targetString = "";
		string actionString = "";
		string amount = ".";

		for (int i = 0; i < targetsPerAbilityStep.Count; i++)
		{
			AbilityStep_Base.OnPlayResult result = nextAbility.abilityStepsWithTargetingData[i].abilityStep.OnPlay(targetsPerAbilityStep[i]);

			if (nextAbility.abilityStepsWithTargetingData[i].abilityStep is AbilityStep_Summon)
			{
				actionString = "Summoning";
				targetString = result.summonedEntity.name;
			}
			else
			{
				if (nextAbility.abilityStepsWithTargetingData[i].targetingData.onlySelf)
				{
					targetString = "self";
				}
				else if (nextAbility.abilityStepsWithTargetingData[i].targetingData.targets == TargetingData_Base.Target.All)
				{
					targetString = "everyone";
				}
				else if (targetsPerAbilityStep[i].Count == 1)
				{
					if (targetsPerAbilityStep[i][0] == this)
						targetString = "self";
					else
						targetString = targetsPerAbilityStep[i][0].name;
				}

				if (nextAbility.type == EnemyAbilityScriptableObject.Type.Offensive)
				{
					actionString = "Attacking";
				}
				else if (nextAbility.type == EnemyAbilityScriptableObject.Type.Healing)
				{
					actionString = "Healing";
				}
				else if (nextAbility.type == EnemyAbilityScriptableObject.Type.Blocking)
				{
					actionString = "Blocking";
				}

				amount = " for " + ((AbilityStep_Targetable)nextAbility.abilityStepsWithTargetingData[i].abilityStep).amount + " dmg.";
			}

			targettingText.text = actionString + ":\n" + targetString + amount;
		}

		FindObjectOfType<AudioManager>().PlayAudio(attackSounds);
		

		yield return new WaitForSeconds(1.5f);
		nextAbilityImage.gameObject.SetActive(false);
		targettingText.text = "";
		doingTurn = false;
		doneTurn = true;
	}

	public override DamageInfo TakeDamage(int amount)
	{
		amount = Mathf.Clamp(amount, 0, amount);

		DamageInfo damageInfo = new DamageInfo();

		int amountRemaining = amount;

		amountRemaining -= armor;
		ModifyArmor(-amount);

		amountRemaining = Mathf.Clamp(amountRemaining, 0, amountRemaining);

		if (amountRemaining > 0)
		{
			if(amountRemaining > health)
			{
				damageInfo.damageDealt = health;
			}
			else
			{
				damageInfo.damageDealt = amountRemaining;
			}

			health -= amountRemaining;
			health = Mathf.Clamp(health, 0, maxHealth);
		}

		UpdateUI();

		if (health <= 0)
		{
			OnEnemyDeath(this);
		}

		return damageInfo;
	}

	public struct DamageInfo
	{
		public int damageDealt;
	}

	public override void TakeHealing(int amount)
	{
		amount = Mathf.Clamp(amount, 0, amount);

		health += amount;
		health = Mathf.Clamp(health, 0, maxHealth);

		UpdateUI();
	}

	public override void ModifyArmor(int amount)
	{
		armor += amount;
		armor = Mathf.Clamp(armor, 0, armor);

		UpdateUI();
	}

	void UpdateUI()
	{
		if (armor > 0)
		{
			armorImage.SetActive(true);
			healthBar.color = armoredColorHealthBar;
			armorText.text = armor.ToString();
		}
		else
		{
			armorImage.SetActive(false);
			healthBar.color = healthBarColor;
		}

		float healthPercentage = (health / (float)maxHealth) * healthBarWidth;
		healthBar.rectTransform.sizeDelta = new Vector2(healthPercentage, healthBar.rectTransform.sizeDelta.y);
		healthTextual.text = health + "/" + maxHealth;
		statusEffectBar.UpdateUI();
	}

	public override void ResetArmor()
	{
		armor = 0;

		UpdateUI();
	}

	public override void AfflictWithStatusEffect(StatusEffect effect)
	{
		statusEffect = new StatusEffect();
		statusEffect.durationRemaining = effect.durationRemaining;
		statusEffect.damagePerTurn = effect.damagePerTurn;
		statusEffectBar.statusEffect = statusEffect;
		statusEffectBar.UpdateUI();
	}

	public override void OnTurnStart()
	{
		if (statusEffect != null)
		{
			TakeDamage(statusEffect.damagePerTurn);
			statusEffect.durationRemaining--;
			if (statusEffect.durationRemaining < 1)
			{
				statusEffect = null;
				statusEffectBar.statusEffect = null;
			}
		}
		statusEffectBar.UpdateUI();
	}
}