using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class Card
{
	public string name;
	public Sprite picture;
	public string description;
	public CardPlayerScriptableObject.Type type;
	public CardPlayerScriptableObject.Rarity rarity;
	public int manaCost;
	public AbilityStep_Base.OnPlayResult[] onPlayResults;
	public AbilityStepsWithTargetingData_Player[] abilityStepsWithTargetingData;
	public List<int> values;
	public AudioClip[] soundForPlay;

	public Card(CardPlayerScriptableObject cardData)
	{
		this.name = cardData.name;
		this.picture = cardData.picture;
		this.description = cardData.description;
		this.type = cardData.type;
		this.rarity = cardData.rarity;
		this.manaCost = cardData.manaCost;
		this.soundForPlay = cardData.soundsForPlay;
		this.abilityStepsWithTargetingData = cardData.abilityStepsWithTargetingData;
		this.onPlayResults = new AbilityStep_Base.OnPlayResult[abilityStepsWithTargetingData.Length];
		this.values = cardData.values;
	}
}