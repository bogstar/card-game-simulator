using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Player : HealthEntity
{
	public PlayerHand hand;

	public PlayerUI ui;
	public new Transform particleEmitter;

	public int mana;
	public int maxMana;

	public int strength;

	public List<Card> cards = new List<Card>();

	public List<CardGameObject> cardsInHand = new List<CardGameObject>();

	public System.Action RefreshUI;
	public System.Action<bool> AllowTargetable;
	public System.Action<StatusEffect> OnAfflictedWithStatusEffect;


	void Start()
	{
		ui.Init(ref RefreshUI, ref AllowTargetable, ref OnAfflictedWithStatusEffect);
		hand.player = this;
		RefreshUI();
	}

	public void DrawRandomCard(int amount, bool ethereal)
	{
		for (int i = 0; i < amount; i++)
		{
			int randomCardIndex = Random.Range(0, cards.Count);
			Card cardToDraw = cards[randomCardIndex];

			DrawCard(cardToDraw, ethereal);
		}
	}

	void DrawCard(Card card, bool ethereal)
	{
		hand.AddCardToHand(card, ethereal);
	}

	public void DropCard(CardGameObject card, bool stayUp)
	{
		hand.DiscardCard(card.GetComponent<CardInHand>(), stayUp);
	}

	public void DropCard(CardInHand card, bool stayUp)
	{
		hand.DiscardCard(card, stayUp);
	}

	public void DropAllCards()
	{
		hand.DiscardAllCards();
	}

	public override void ModifyArmor(int amount)
	{
		armor += amount;
		armor = Mathf.Clamp(armor, 0, armor);

		RefreshUI();
	}

	public override Enemy.DamageInfo TakeDamage(int amount)
	{
		amount = Mathf.Clamp(amount, 0, amount);
		Enemy.DamageInfo damageInfo = new Enemy.DamageInfo();

		int amountRemaining = amount;

		amountRemaining -= armor;
		ModifyArmor(-amount);

		amountRemaining = Mathf.Clamp(amountRemaining, 0, amountRemaining);

		if(amountRemaining > 0)
		{
			if (amountRemaining > health)
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

		if(health <= 0)
		{
			FindObjectOfType<BattleManager>().Defeat();
		}

		RefreshUI();

		return damageInfo;
	}

	public override void TakeHealing(int amount)
	{
		amount = Mathf.Clamp(amount, 0, amount);

		health += amount;
		health = Mathf.Clamp(health, 0, maxHealth);

		RefreshUI();
	}

	public void ModifyMana(int amount)
	{
		mana += amount;

		RefreshUI();
	}

	public void ResetMaxMana()
	{
		mana = maxMana;

		RefreshUI();
	}

	public override void ResetArmor()
	{
		armor = 0;

		RefreshUI();
	}

	public override void AfflictWithStatusEffect(StatusEffect effect)
	{
		statusEffect = new StatusEffect();
		statusEffect.durationRemaining = effect.durationRemaining;
		statusEffect.damagePerTurn = effect.damagePerTurn;

		OnAfflictedWithStatusEffect(statusEffect);
		RefreshUI();
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
			}
		}

		OnAfflictedWithStatusEffect(statusEffect);
		RefreshUI();
	}
}