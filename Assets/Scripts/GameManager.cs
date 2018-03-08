using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class GameManager : MonoBehaviour
{
	[Header("Prefabs")]
	public GameObject overworldCardPrefab;

	[Header("References")]
	public AudioClip VictorySound;

	// Temporary references here. Later will be decoupled to a
	// UI manager of some sorts.
	public GameObject battlePanel;
	public GameObject overworldPanel;

	public GameObject player1ScrollArea;
	public Text overworldPlayer1Name;
	public Text overworldPlayer1Mana;
	public Text overworldPlayer1Health;
	public Image overworldPlayer1HealthBar;
	public float overworldPlayer1HealthBarWitdh;

	public GameObject player2ScrollArea;
	public Text overworldPlayer2Name;
	public Text overworldPlayer2Mana;
	public Text overworldPlayer2Health;
	public Image overworldPlayer2HealthBar;
	public float overworldPlayer2HealthBarWitdh;

	public Text goldText;

	public GameObject startingPanel;
	public Player player1;
	public Player player2;
	public Dropdown player1Dropdown;
	public Dropdown player2Dropdown;
	public Dropdown dungeonDropdown;

	[Header("Public values")]
	public int maxCardsForPlayers;
	public int gold;

	DungeonScriptableObject[] currentDungeons;
	CharacterScriptableObject[] currentCharacters;

	BattleManager battleManager;


	void Awake()
	{
		// Find references. Later, these will be singletons.
		battleManager = FindObjectOfType<BattleManager>();
		battleManager.OnCombatEnd += OnBattleEnd;
	}

	void Start()
	{
		// Let's load all characters and allow the player to choose from the selection.
		currentCharacters = Resources.LoadAll<CharacterScriptableObject>(FileManager.CharactersPath);

		List<string> player1DropdownOptions = new List<string>();
		player1DropdownOptions.Add("Choose a character");
		List<string> player2DropdownOptions = new List<string>();
		player2DropdownOptions.Add("Choose a character");

		foreach (var character in currentCharacters)
		{
			player1DropdownOptions.Add(character.name);
			player2DropdownOptions.Add(character.name);
		}

		// Let's load all dungeons as well.
		currentDungeons = Resources.LoadAll<DungeonScriptableObject>(FileManager.DungeonsPath);
		List<string> dungeonsList = new List<string>();

		foreach(var dungeon in currentDungeons)
		{
			dungeonsList.Add(dungeon.name);
		}

		dungeonDropdown.AddOptions(dungeonsList);
		player1Dropdown.AddOptions(player1DropdownOptions);
		player2Dropdown.AddOptions(player2DropdownOptions);

		player1Dropdown.value = 1;
		player2Dropdown.value = 2;

		//Input_StartGame();

		// Change the state to "starting area".
		ChangeGameState(GameState.StartingArea);

		// Now we are awaiting for user input - on Input_StartGame().
	}

	public void Input_StartGame()
	{
		if (player1Dropdown.value == player2Dropdown.value || player1Dropdown.value == 0 || player2Dropdown.value == 0)
		{
			return;
		}

		CharacterScriptableObject player1Data = currentCharacters[player1Dropdown.value - 1];
		CharacterScriptableObject player2Data = currentCharacters[player2Dropdown.value - 1];

		player1.alignment = HealthEntity.Alignment.Good;
		foreach (var cardSO in player1Data.startingCards)
		{
			Card newCard = new Card(cardSO);
			player1.cards.Add(newCard);
		}
		player1.name = player1Data.name;
		player1.maxHealth = player1Data.health;
		player1.maxMana = player1Data.mana;

		player2.alignment = HealthEntity.Alignment.Good;
		foreach (var cardSO in player2Data.startingCards)
		{
			Card newCard = new Card(cardSO);
			player2.cards.Add(newCard);
		}
		player2.name = player2Data.name;
		player2.maxHealth = player2Data.health;
		player2.maxMana = player2Data.mana;

		StartGame();
	}

	void RefreshUI()
	{
		foreach (Transform child in player1ScrollArea.transform)
		{
			GameObject.Destroy(child.gameObject);
		}
		foreach (Transform child in player2ScrollArea.transform)
		{
			GameObject.Destroy(child.gameObject);
		}

		overworldPlayer1Name.text = player1.name;
		overworldPlayer2Name.text = player2.name;
		overworldPlayer1Health.text = player1.health + "/" + player1.maxHealth;
		float percentagePlayer1 = (player1.health / (float)player1.maxHealth) * overworldPlayer1HealthBarWitdh;
		overworldPlayer1HealthBar.rectTransform.sizeDelta = new Vector2(percentagePlayer1, overworldPlayer1HealthBar.rectTransform.sizeDelta.y);
		overworldPlayer2Health.text = player2.health + "/" + player2.maxHealth;
		float percentagePlayer2 = (player2.health / (float)player2.maxHealth) * overworldPlayer2HealthBarWitdh;
		overworldPlayer2HealthBar.rectTransform.sizeDelta = new Vector2(percentagePlayer2, overworldPlayer2HealthBar.rectTransform.sizeDelta.y);
		overworldPlayer1Mana.text = "Mana: " + player1.maxMana.ToString();
		overworldPlayer2Mana.text = "Mana: " + player2.maxMana.ToString();

		foreach (var card in player1.cards)
		{
			GameObject go = Instantiate(overworldCardPrefab, player1ScrollArea.transform) as GameObject;
			DisplayCard displayCard = go.GetComponentInChildren<DisplayCard>();
			displayCard.Display(card);
		}
		((RectTransform)player1ScrollArea.transform).sizeDelta = new Vector2(243, (Mathf.Clamp(player1.cards.Count - 1, 0, player1.cards.Count) / 5) * 65.454f + 65.454f);
		foreach (var card in player2.cards)
		{
			GameObject go = Instantiate(overworldCardPrefab, player2ScrollArea.transform) as GameObject;
			DisplayCard displayCard = go.GetComponentInChildren<DisplayCard>();
			displayCard.Display(card);
		}
		((RectTransform)player2ScrollArea.transform).sizeDelta = new Vector2(243, (Mathf.Clamp(player2.cards.Count - 1, 0, player2.cards.Count) / 5) * 65.454f + 65.454f);

		goldText.text = "Gold: " + gold.ToString();
	}

	void StartGame()
	{
		// Change to overworld.
		ChangeGameState(GameState.Overworld);
		player1.health = player1.maxHealth;
		player2.health = player2.maxHealth;

		RefreshUI();

		// Now we are awaiting for user input - on Input_StartBattle()
	}

	public void Input_StartBattle()
	{
		DungeonScriptableObject dungeon = currentDungeons[dungeonDropdown.value];

		if (dungeon.encounters.Length > 0)
		{
			ChangeGameState(GameState.Battle);
			List<Player> players = new List<Player>();
			players.Add(player1);
			players.Add(player2);
			battleManager.StartBattle(dungeon.encounters[Random.Range(0, dungeon.encounters.Length)], players, maxCardsForPlayers);
		}
		else
			Debug.LogError("Not a single encounter in the list.");
	}

	void OnBattleEnd(int goldWon, CardPlayerScriptableObject player1Win, CardPlayerScriptableObject player2Win)
	{
		FindObjectOfType<AudioManager>().PlayAudio(VictorySound);
		StartCoroutine(EndBattle());
	}

	IEnumerator EndBattle()
	{
		yield return new WaitForSeconds(1f);
		ChangeGameState(GameState.Overworld);

		player1.DropAllCards();
		player2.DropAllCards();

		//player1.cards.Add(new Card(player1Win));
		//player2.cards.Add(new Card(player2Win));

		//gold += goldWon;

		RefreshUI();
	}

	// Game state changer.
	void ChangeGameState(GameState newState)
	{
		overworldPanel.SetActive(false);
		battlePanel.SetActive(false);
		startingPanel.SetActive(false);
		switch (newState)
		{
			case GameState.Battle:
				battlePanel.SetActive(true);
				break;
			case GameState.Overworld:
				overworldPanel.SetActive(true);
				break;
			case GameState.StartingArea:
				startingPanel.SetActive(true);
				break;
		}
	}

	public enum GameState { StartingArea, Overworld, Battle }
}