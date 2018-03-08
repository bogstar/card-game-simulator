using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class CardGameObject : MonoBehaviour
{
	public Card card;
	public Button cardButton;
	public bool ethereal;

	/*
	string GetString(string text, Dictionary<int, int> values)
	{
		string finalString = "";

		if (text == null)
		{
			return "";
		}

		string[] s = text.Split('$');

		if (s.Length == 0 || s.Length % 2 == 0)
		{
			return text;
		}

		for (int i = 0; i < s.Length; i++)
		{
			if (i % 2 == 0)
			{
				finalString += s[i];
			}
			else
			{
				finalString += '5';
				//finalString += values[int.Parse(s[i])];
			}
		}

		return finalString;
	}*/
}