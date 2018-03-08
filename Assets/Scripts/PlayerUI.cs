using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PlayerUI : MonoBehaviour
{
	public Player player;
	public PlayerHand hand;
	public Text nameBar;
	public Button targetButton;
	public GameObject armorImage;
	public Text armorText;
	public Image healthBar;
	public Text healthTextual;
	public float healthBarWidth = 165;
	public RectTransform manaBar;
	public Text manaTextual;
	public float manaBarWidth = 35;
	public Color armoredColorHealthBar;
	public Color healthBarColor;
	public StatusEffectBar statusEffectBar;

	public void UpdateUI()
	{
		if (player.armor > 0)
		{
			armorImage.SetActive(true);
			healthBar.color = armoredColorHealthBar;
			armorText.text = player.armor.ToString();
		}
		else
		{
			armorImage.SetActive(false);
			healthBar.color = healthBarColor;
		}

		float healthPercentage = (player.health / (float)player.maxHealth) * healthBarWidth;
		healthBar.rectTransform.sizeDelta = new Vector2(healthPercentage, healthBar.rectTransform.sizeDelta.y);
		healthTextual.text = player.health + "/" + player.maxHealth;
		float manaPercentage = (player.mana / (float)player.maxMana) * manaBarWidth;
		manaPercentage = Mathf.Clamp(manaPercentage, 0, manaBarWidth);
		manaBar.sizeDelta = new Vector2(manaPercentage, manaBar.sizeDelta.y);
		manaTextual.text = player.mana + "/" + player.maxMana;
		nameBar.text = player.name;
		statusEffectBar.UpdateUI();
	}

	public void Init(ref System.Action RefreshUICallback, ref System.Action<bool> AllowTargetableCallback, ref System.Action<StatusEffect> OnStatusEffectAfflictedCallback)
	{
		RefreshUICallback += UpdateUI;
		AllowTargetableCallback += AllowTargetable;
		OnStatusEffectAfflictedCallback += AfflictWithStatusEffect;
	}

	void AfflictWithStatusEffect(StatusEffect effect)
	{
		statusEffectBar.statusEffect = effect;
		statusEffectBar.GetComponent<Image>().enabled = (effect == null) ? false : true;
		if (effect == null)
			return;
		statusEffectBar.durationText.text = effect.durationRemaining.ToString();
	}

	public void AllowTargetable(bool allow)
	{
		targetButton.gameObject.SetActive(allow);
	}
}