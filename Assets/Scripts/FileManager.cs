using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FileManager
{
	static char m_directorySeparatorChar = System.IO.Path.DirectorySeparatorChar;
	public static char DirectorySeparatorChar { get { return m_directorySeparatorChar; } }

	static string m_databasePath = "Database";
	public static string DatabasePath { get { return m_databasePath; } }

	static string m_cardsPath = m_databasePath + m_directorySeparatorChar + "Cards";
	public static string CardsPath { get { return m_cardsPath; } }

	static string m_charactersPath = m_databasePath + m_directorySeparatorChar + "Characters";
	public static string CharactersPath { get { return m_charactersPath; } }

	static string m_dungeonsPath = m_databasePath + m_directorySeparatorChar + "Dungeons";
	public static string DungeonsPath { get { return m_dungeonsPath; } }

	static char m_separatorChar = '$';
	public static char SeparatorChar { get { return m_separatorChar; } }

}