using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class BattleManager : MonoBehaviour
{
	[Header("References")]
	public Text infoText;
	public EnemyManager enemyManager;
	public GameObject particlePrefab;

	[Header("Private Values Publicized")]
	public List<CardPlayerScriptableObject> cardsPerWin;
	public int goldPerWin;

	List<Player> players;
	List<Enemy> enemies;

	Player currentCardOwner;
	CardGameObject currentSelectedCard;

	Encounter currentEncounter;

	TurnSelection turn;

	public System.Action<int, CardPlayerScriptableObject, CardPlayerScriptableObject> OnCombatEnd;

	List<Coroutine> coroutines = new List<Coroutine>();

	int maxCardForPlayers;

	bool targetFound;
	List<HealthEntity> chosenTargets;
	List<List<HealthEntity>> targetsPerAbilityStep;
	List<HealthEntity> entitiesToAddThisTurn = new List<HealthEntity>();

	bool drawNewSetOfCards;


	void Start()
	{
		infoText.text = "";
	}

	public HealthEntity SummonOnEnemySide(EnemyScriptableObject enemySO)
	{
		// This looks sloppy. Rework.
		List<EnemyScriptableObject> enemyList = new List<EnemyScriptableObject>();
		enemyList.Add(enemySO);
		Enemy enemy = enemyManager.SpawnEnemies(enemyList, this, OnEnemyDeath)[0];

		entitiesToAddThisTurn.Add(enemy);

		return enemy;
	}

	public void StartBattle(EncounterScriptableObject encounterSO, List<Player> players, int maxCardForPlayers)
	{
		currentEncounter = new Encounter(encounterSO);

		List<EnemyScriptableObject> enemyList = new List<EnemyScriptableObject>();
		foreach (var enemy in currentEncounter.enemies)
		{
			enemyList.Add(enemy);
		}

		this.players = players;
		this.enemies = enemyManager.SpawnEnemies(enemyList, this, OnEnemyDeath);
		this.maxCardForPlayers = maxCardForPlayers;

		StartCoroutine(DrawFullHand(maxCardForPlayers));

		foreach (var p in players)
		{
			p.RefreshUI();
		}

		HideTargettingButtons();
		turn = TurnSelection.Players;

		StartTurn();
		//FindObjectOfType<AudioManager>().PlayMusic();
	}

	void StartTurn()
	{
		switch (turn)
		{
			case TurnSelection.Players:
				if (drawNewSetOfCards)
				{
					StartCoroutine(DrawFullHand(maxCardForPlayers));
				}
				foreach (var p in players)
				{
					p.OnTurnStart();
					p.ResetArmor();
					p.ResetMaxMana();
				}
				foreach (var e in enemies)
				{
					e.ChooseNextAbility();
				}
				break;
			case TurnSelection.Enemies:
				for (int i = enemies.Count-1; i > -1; i--)
				{
					enemies[i].ResetArmor();
					enemies[i].OnTurnStart();
				}
				StartCoroutine(DoEnemyTurn());
				break;
		}
	}

	IEnumerator DoEnemyTurn()
	{
		foreach (var e in enemies)
		{
			bool doing = false;
			e.targetsPerAbilityStep = SelectAbilityTargetEnemy(e);
			do
			{
				e.DoTurn(ref doing);
				yield return null;
			}
			while (doing == true);
		}

		EndTurn();
		yield return null;
	}

	void EndTurn()
	{
		switch (turn)
		{
			case TurnSelection.Players:
				turn = TurnSelection.Enemies;
				break;
			case TurnSelection.Enemies:
				turn = TurnSelection.Players;
				break;
		}

		foreach (var entity in entitiesToAddThisTurn)
		{
			enemies.Add(entity as Enemy);
		}
		entitiesToAddThisTurn.Clear();

		StartTurn();
	}

	void DoCardTrick()
	{
		for (int i = 0; i < currentSelectedCard.card.abilityStepsWithTargetingData.Length; i++)
		{
			if (currentSelectedCard.card.abilityStepsWithTargetingData[i].abilityStep is AbilityStep_Drain)
			{
				AbilityStep_Drain card = currentSelectedCard.card.abilityStepsWithTargetingData[i].abilityStep as AbilityStep_Drain;

				if (i != 0)
				{
					card.OnPlay(targetsPerAbilityStep[i], currentSelectedCard.card.onPlayResults[i - i]);
				}
			}
			else
			{
				AbilityStep_Base card = currentSelectedCard.card.abilityStepsWithTargetingData[i].abilityStep;

				currentSelectedCard.card.onPlayResults[i] = card.OnPlay(targetsPerAbilityStep[i]);
			}
		}
		
		FindObjectOfType<AudioManager>().PlayAudioDeath(currentSelectedCard.card.soundForPlay);

		currentCardOwner.ModifyMana(-currentSelectedCard.card.manaCost);
		currentCardOwner.DropCard(currentSelectedCard, stayUp);

		infoText.text = "";

		if (!currentSelectedCard.ethereal)
		{
			foreach (var ally in GetAllRelativeAllies(currentCardOwner, false))
			{
				Player a = (Player)ally;
				a.DrawRandomCard(1, false);
			}
		}

		HideTargettingButtons();
	}

	bool stayUp;

	public void SelectTarget(HealthEntity entity)
	{
		// Break if there is no selected card.
		if (currentSelectedCard == null)
		{
			return;
		}

		HideTargettingButtons();
		chosenTargets.Add(entity);
		targetFound = true;
	}

	public struct TargetData
	{
		public bool targetDecided;
		public List<HealthEntity> entities;
		public int previousStepTargets;
	}

	TargetData GetValidTargets(HealthEntity caster, TargetingData_Base targetingData)
	{
		TargetData targetData = new TargetData();
		targetData.previousStepTargets = -1;
		List<HealthEntity> targets = new List<HealthEntity>();

		if (targetingData.targetingType == TargetingData_Base.TargetingType.NoTargeting)
		{
			// There is no targeting. Don't bother.
		}
		// We will use other step's targets.
		else if(targetingData.targetingType == TargetingData_Base.TargetingType.PreviousStep)
		{
			targetData.previousStepTargets = targetingData.useTargetsFromStepNumber;
		}
		// If the target is only self, do not check for anything else.
		else if (targetingData.onlySelf)
		{
			targets.Add(caster);
			targetData.targetDecided = true;
		}
		else
		{
			// Can we add self?
			bool selfIncluded = targetingData.selfIncluded;

			// Let's skip the check if we can only target ourselves.
			if (targetingData.onlySelf)
			{
				targets.Add(caster);
			}
			// We can target more than ourselves. Let's first check if we can target at all.
			else if (targetingData.targets != TargetingData_Base.Target.All)
			{
				switch (targetingData.targetAlignment)
				{
					case TargetingData_Base.TargetAlignment.Both:
						switch (targetingData.targets)
						{
							case TargetingData_Base.Target.One:
								// Select one from friendlies or from enemies.

								// Select one from friendlies taking into account whether to add self.
								foreach (var f in GetAllRelativeAllies(caster, selfIncluded))
								{
									targets.Add(f);
								}

								// Select one from enemies.
								foreach (var e in GetAllRelativeEnemies(caster))
								{
									targets.Add(e);
								}
								break;
							case TargetingData_Base.Target.MultipleSpecific:
								// Select multiple specific from friendlies or enemies.

								// Add ourselves to the list.
								if (targetingData.selfIncluded)
								{

								}
								break;
						}
						break;
					case TargetingData_Base.TargetAlignment.Friendly:
						switch (targetingData.targets)
						{
							case TargetingData_Base.Target.One:
								// Select one from friendlies taking into account whether to add self.
								foreach (var f in GetAllRelativeAllies(caster, selfIncluded))
								{
									targets.Add(f);
								}
								break;
							case TargetingData_Base.Target.MultipleSpecific:
								// Select multiple specific friendlies taking into account whether to add self.
								break;
							case TargetingData_Base.Target.Multiple:
								// Select multiple friendlies taking into account whether to add self.
								foreach (var f in GetAllRelativeAllies(caster, selfIncluded))
								{
									targets.Add(f);
								}
								break;
						}
						break;
					case TargetingData_Base.TargetAlignment.Hostile:
						switch (targetingData.targets)
						{
							case TargetingData_Base.Target.One:
								// Select one from enemies.
								foreach (var e in GetAllRelativeEnemies(caster))
								{
									targets.Add(e);
								}
								break;
							case TargetingData_Base.Target.MultipleSpecific:
								// Select multiple specific from enemies.
								break;
							case TargetingData_Base.Target.Multiple:
								// Select multiple from enemies.
								foreach (var e in GetAllRelativeEnemies(caster))
								{
									targets.Add(e);
								}
								break;
						}
						break;
				}
			}
			// We cannot target so we choose all.
			else
			{
				switch (targetingData.targetAlignment)
				{
					case TargetingData_Base.TargetAlignment.Both:
						// Select all targets.

						// Select all allies taking into account if self included
						foreach (var a in GetAllRelativeAllies(caster, selfIncluded))
						{
							targets.Add(a);
						}

						// Select all enemies.
						foreach (var e in GetAllRelativeEnemies(caster))
						{
							targets.Add(e);
						}
						break;
					case TargetingData_Base.TargetAlignment.Friendly:
						// Select all allies taking into account if self included
						foreach (var a in GetAllRelativeAllies(caster, selfIncluded))
						{
							targets.Add(a);
						}
						break;
					case TargetingData_Base.TargetAlignment.Hostile:
						// Select all enemies.
						foreach (var e in GetAllRelativeEnemies(caster))
						{
							targets.Add(e);
						}
						break;
				}
				targetData.targetDecided = true;
			}
		}

		targetData.entities = targets;

		return targetData;
	}

	void ShowTargetingButton(List<HealthEntity> entities)
	{
		foreach (var e in entities)
		{
			if(e is Player)
				((Player)e).AllowTargetable(true);
			if(e is Enemy)
				((Enemy)e).AllowTargetable(true);
		}
	}

	List<List<HealthEntity>> SelectAbilityTargetEnemy(Enemy enemy)
	{
		List<List<HealthEntity>> targetsPerAbilityStep = new List<List<HealthEntity>>();

		AbilityStepsWithTargetingData_Enemy[] abilityStepsWithTargetingData = enemy.nextAbility.abilityStepsWithTargetingData;

		// Let's cycle through all ability steps in order to determine targets.
		foreach (var abilityStepWithTargetingData in abilityStepsWithTargetingData)
		{
			List<HealthEntity> selectedTargets = new List<HealthEntity>();

			TargetData targetData = GetValidTargets(enemy, abilityStepWithTargetingData.targetingData);

			if(targetData.previousStepTargets > -1)
			{
				foreach(var entity in targetsPerAbilityStep[targetData.previousStepTargets])
				{
					selectedTargets.Add(entity);
				}
			}
			else if (targetData.targetDecided)
			{
				foreach (var e in targetData.entities)
				{
					selectedTargets.Add(e);
				}
			}
			else
			{
				switch (abilityStepWithTargetingData.targetingData.targets)
				{
					case TargetingData_Base.Target.One:
						// Choose a random target for now
						selectedTargets.Add(targetData.entities[Random.Range(0, targetData.entities.Count)]);
						break;
					case TargetingData_Base.Target.Multiple:
						// Choose a random target for now
						selectedTargets.Add(targetData.entities[Random.Range(0, targetData.entities.Count)]);
						break;
					case TargetingData_Base.Target.MultipleSpecific:
						// Choose a random target for now

						// If it happens that there are less available targets than our ability wants us to target,
						// add them all.
						if (abilityStepWithTargetingData.targetingData.numberOfTargets < targetData.entities.Count)
						{
							foreach (var entity in targetData.entities)
							{
								selectedTargets.Add(entity);
							}
						}
						else
						{
							List<HealthEntity> st = new List<HealthEntity>(selectedTargets);
							for (int i = 0; i < abilityStepWithTargetingData.targetingData.numberOfTargets; i++)
							{
								HealthEntity entity = targetData.entities[Random.Range(0, targetData.entities.Count)];
								selectedTargets.Add(entity);
								st.Remove(entity);
							}
						}
						break;
				}
			}

			targetsPerAbilityStep.Add(selectedTargets);
		}

		// We have all chosen targets.
		// We now have to check whether any of those are multiple hits.
		for (int i = 0; i < targetsPerAbilityStep.Count; i++)
		{
			TargetingData_Base targetingData = enemy.nextAbility.abilityStepsWithTargetingData[i].targetingData;

			if (targetingData.targetAlignment == TargetingData_Base.TargetAlignment.Friendly)
			{
				if (targetingData.targets == TargetingData_Base.Target.Multiple)
				{
					Enemy chosenTarget = (Enemy)targetsPerAbilityStep[i][0];
					List<Enemy> nearbyEnemies = GetNearbyEnemies(chosenTarget, targetingData.numberOfTargets);
					foreach (var e in nearbyEnemies)
					{
						targetsPerAbilityStep[i].Add(e);
					}
				}
			}
		}

		return targetsPerAbilityStep;
	}

	IEnumerator SelectCardCoro(CardGameObject cardGO)
	{
		Player playerOnTurn = currentCardOwner;
		HealthEntity caster = playerOnTurn;

		targetsPerAbilityStep = new List<List<HealthEntity>>();

		AbilityStepsWithTargetingData_Player[] abilities = cardGO.card.abilityStepsWithTargetingData;

		// Let's cycle through all ability steps in order to determine targets.
		foreach (var abilityStepWithTargetingData in abilities)
		{
			chosenTargets = new List<HealthEntity>();
			targetFound = false;

			TargetData targetData = GetValidTargets(caster, abilityStepWithTargetingData.targetingData);

			if (targetData.previousStepTargets > -1)
			{
				targetFound = true;
				foreach (var entity in targetsPerAbilityStep[targetData.previousStepTargets])
				{
					chosenTargets.Add(entity);
				}
			}
			else if (targetData.targetDecided)
			{
				targetFound = true;
				foreach (var e in targetData.entities)
				{
					chosenTargets.Add(e);
				}
			}
			else
			{
				if(targetData.entities.Count == 1 && targetData.entities[0] is Player)
				{
					targetFound = true;
					chosenTargets.Add(targetData.entities[0]);
				}
				else
				{
					infoText.text = abilityStepWithTargetingData.targetingData.textForChoosing;
					ShowTargetingButton(targetData.entities);
				}
			}

			while (targetFound == false)
			{
				stayUp = false;
				yield return null;
			}

			targetsPerAbilityStep.Add(chosenTargets);
		}

		// We have all chosen targets.
		// We now have to check whether any of those are multiple hits.

		for (int i = 0; i < targetsPerAbilityStep.Count; i++)
		{
			TargetingData_Base targetingData = cardGO.card.abilityStepsWithTargetingData[i].targetingData;

			if (targetingData.targetAlignment == TargetingData_Base.TargetAlignment.Hostile)
			{
				if (targetingData.targets == TargetingData_Base.Target.Multiple)
				{
					Enemy chosenTarget = (Enemy)targetsPerAbilityStep[i][0];
					List<Enemy> nearbyEnemies = GetNearbyEnemies(chosenTarget, targetingData.numberOfTargets);
					foreach (var e in nearbyEnemies)
					{
						targetsPerAbilityStep[i].Add(e);
					}
				}
			}
		}

		// Now we have full lists of all targets for each attribute.
		// Let's do the card trick.
	
		DoCardTrick();

		yield return null;
	}

	public List<HealthEntity> GetAllRelativeEnemies(HealthEntity entity)
	{
		List<HealthEntity> entities = new List<HealthEntity>();
		if (entity.alignment == HealthEntity.Alignment.Evil)
		{
			foreach (var p in players)
			{
				entities.Add(p);
			}
		}
		if (entity.alignment == HealthEntity.Alignment.Good)
		{
			foreach (var p in enemies)
			{
				entities.Add(p);
			}
		}
		return entities;
	}

	public List<HealthEntity> GetAllRelativeAllies(HealthEntity entity, bool selfIncluded)
	{
		List<HealthEntity> entities = new List<HealthEntity>();
		if (entity.alignment == HealthEntity.Alignment.Evil)
		{
			foreach (var p in enemies)
			{
				entities.Add(p);
			}
		}
		if (entity.alignment == HealthEntity.Alignment.Good)
		{
			foreach (var p in players)
			{
				if ((entity as Player) == p)
				{
					if (selfIncluded)
					{
						entities.Add(p);
					}
				}
				else
				{
					entities.Add(p);
				}
			}
		}
		return entities;
	}

	public List<Enemy> GetNearbyEnemies(Enemy enemy, int enemyNumber)
	{
		if (enemyNumber % 2 == 0)
		{
			Debug.LogError("Only odd numbers for now.");
			return null;
		}
		if (enemyNumber < 3)
		{
			Debug.LogError("No point calculating this for less than 3 enemies.");
			return null;
		}

		List<Enemy> nearbyEnemies = new List<Enemy>();
		List<Enemy> enemiesAsEnemy = new List<Enemy>();

		foreach (var entity in enemies)
		{
			enemiesAsEnemy.Add(entity as Enemy);
		}

		int enemiesRemaining = enemies.Count;
		int enemyIndex = enemy.enemyIndex;

		if (enemiesRemaining <= enemyNumber)
		{
			nearbyEnemies = new List<Enemy>(enemiesAsEnemy);
		}
		else
		{
			if (enemyIndex < enemyNumber / 2 + 1)
			{
				for (int i = 0; i < enemyNumber; i++)
				{
					nearbyEnemies.Add(enemiesAsEnemy[i]);
				}
			}
			else if (enemyIndex > (enemiesRemaining - enemyNumber))
			{
				for (int i = enemiesRemaining - enemyNumber; i < enemiesRemaining; i++)
				{
					nearbyEnemies.Add(enemiesAsEnemy[i]);
				}
			}
			else
			{
				int startingIndex = enemyNumber / 2;
				for (int i = -startingIndex; i <= startingIndex; i++)
				{
					nearbyEnemies.Add(enemiesAsEnemy[enemyIndex + i]);
				}
			}
		}

		nearbyEnemies.Remove(enemy);
		return nearbyEnemies;
	}

	public void SelectCard(CardGameObject cardGO, Player player)
	{
		// Player has selected a card from the hand.

		// If it is not player's turn, do nothing.
		if (turn != TurnSelection.Players)
		{
			return;
		}

		// From here on, it's player's turn.

		// Ignore if the player does not have enough mana to play the card.
		if (cardGO.card.manaCost > player.mana)
		{
			return;
		}

		// Hide buttons for targetting.
		HideTargettingButtons();

		stayUp = true;
		currentCardOwner = player;
		currentSelectedCard = cardGO;

		foreach (var c in coroutines)
		{
			StopCoroutine(c);
		}

		player.hand.SelectCard(cardGO.GetComponent<CardInHand>());
		player.hand.RecalculateCardsPosition();
		coroutines.Add(StartCoroutine(SelectCardCoro(cardGO)));
	}

	public void PlayerEndTurn(bool discard)
	{
		if (turn != TurnSelection.Players)
		{
			return;
		}

		foreach (var player in players)
		{
			player.hand.UnselectCard();
		}

		if (discard)
		{
			foreach (var player in players)
			{
				player.DropAllCards();
				drawNewSetOfCards = true;
			}
		}
		else
		{
			foreach (var player in players)
			{
				for (int i = player.hand.cardsInHand.Count-1; i > -1; i--)
				{
					if (player.hand.cardsInHand[i].GetComponent<CardGameObject>().ethereal)
					{
						player.DropCard(player.hand.cardsInHand[i], false);
					}
				}
			}
		}

		HideTargettingButtons();

		EndTurn();
	}

	float timebetweencards = .15f;

	IEnumerator DrawFullHand(int maxCardForPlayers)
	{
		drawNewSetOfCards = false;
		int j = Random.Range(0, 2);
		for (int i = 0; i < maxCardForPlayers; i++)
		{
			players[j].DrawRandomCard(1, false);
			j = (j + 1) % 2;
			yield return new WaitForSeconds(timebetweencards);
		}
	}

	public void EndBattle()
	{
		enemyManager.KillAllEnemies(enemies);

		HideTargettingButtons();
	}

	void OnEnemyDeath(Enemy enemy)
	{
		FindObjectOfType<AudioManager>().PlayAudio(enemy.deathSounds);

		enemies.Remove(enemy);
		int i = 0;
		foreach (var e in enemies)
		{
			e.enemyIndex = i;
			i++;
		}
		GameObject.Destroy(enemy.gameObject);
		if (enemies.Count == 0)
		{
			EndBattle();
			OnCombatEnd(goldPerWin, null, null);
		}
	}

	public void Defeat()
	{
		UnityEngine.SceneManagement.SceneManager.LoadScene(UnityEngine.SceneManagement.SceneManager.GetActiveScene().name);
	}

	public void ManualInput_Victory()
	{
		enemyManager.KillAllEnemies(enemies);
	}

	void HideTargettingButtons()
	{
		foreach (var p in players)
		{
			p.AllowTargetable(false);
		}
		enemyManager.SetEnemiesTargetable(false, enemies);
	}

	public enum BattleState { None, Choosing, Targeting }
	public enum TurnSelection { None, Players, Enemies }
}
