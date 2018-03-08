using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class Utility
{
	public static string GetString(string text, List<int> values)
	{
		string[] splits = text.Split('$');
		string finalString = "";

		int i = -1;
		int j = 0;
		foreach (var split in splits)
		{
			i++;
			if (i % 2 == 0)
			{
				finalString += split;
				continue;
			}

			finalString += values[j].ToString();

			j++;
		}

		return finalString;
	}
}