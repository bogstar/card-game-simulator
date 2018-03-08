using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class DisplayCard : MonoBehaviour
{
	public Text nameText;
	public Text descText;
	public Text manaCostText;
	public Text typeText;
	public Image image;

	public void Display(CardGameObject card)
	{
		if (card != null && card.ethereal)
		{
			nameText.text = "ETHEREAL " + card.card.name;
		}
		else
		{
			nameText.text = card.card.name;
		}
		
		descText.text = Utility.GetString(card.card.description, card.card.values);
		manaCostText.text = card.card.manaCost.ToString();
		typeText.text = card.card.type.ToString();
		image.sprite = card.card.picture;
	}

	public void Display(Card card)
	{
		nameText.text = card.name;
		descText.text = Utility.GetString(card.description, card.values);
		manaCostText.text = card.manaCost.ToString();
		typeText.text = card.type.ToString();
		image.sprite = card.picture;
	}
}