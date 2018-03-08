using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "New Character", menuName = "Characters/Character")]
public class CharacterScriptableObject : ScriptableObject
{
	public new string name;
	public int health;
	public int mana;

	public CardPlayerScriptableObject[] startingCards;
}