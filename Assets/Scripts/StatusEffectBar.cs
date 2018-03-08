using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class StatusEffectBar : MonoBehaviour
{
	public StatusEffect statusEffect;
	public Text durationText;

	public void UpdateUI()
	{
		if(statusEffect != null)
		{
			GetComponent<Image>().enabled = true;
			durationText.text = statusEffect.durationRemaining.ToString();
		}
		else
		{
			GetComponent<Image>().enabled = false;
			durationText.text = "";
		}
	}
}