using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class PlayerHand : MonoBehaviour
{
	public GameObject cardPrefab;
	public DisplayCard displayCard;

	public Player player;

	public RectTransform drawPile;
	public RectTransform discardPile;
	public RectTransform selectPosition;

	public float intersectionAmount;

	public float cardWidth;

	RectTransform rectTransform;

	public List<CardInHand> cardsInHand = new List<CardInHand>();

	CardInHand currentHover;
	CardInHand currentSelect;
	int currentSelectIndex = -1;
	int currentHoverIndex = -1;

	CardPlayerScriptableObject[] cards;


	void Awake()
	{
		rectTransform = (RectTransform)transform;
	}

	List<CardInHand> GetAllCards()
	{
		PointerEventData pointer = new PointerEventData(EventSystem.current);
		pointer.position = Input.mousePosition;

		List<CardInHand> cards = new List<CardInHand>();

		List<RaycastResult> raycastResults = new List<RaycastResult>();
		EventSystem.current.RaycastAll(pointer, raycastResults);

		foreach (var r in raycastResults)
		{
			if (r.gameObject.GetComponent<CardInHand>() != null)
			{
				if (!r.gameObject.GetComponent<CardInHand>().hoverable || !r.gameObject.GetComponent<CardInHand>().active)
					continue;
				cards.Add(r.gameObject.GetComponent<CardInHand>());
			}
		}

		return cards;
	}

	CardInHand GetCardWithHighestOrder(List<CardInHand> allCards)
	{
		CardInHand card = null;
		/*
		if (currentIndex != -1)
		{
			foreach (var c in allCards)
			{
				if(c == currentSelection)
				{
					return c;
				}
			}
		}*/

		int currentHighest = -1;

		foreach (var c in allCards)
		{
			if(c.orderInHand > currentHighest)
			{
				currentHighest = c.orderInHand;
				card = c;
			}
		}

		return card;
	}

	void UnhoverOverCard()
	{
		if(currentHover != null)
		{
			currentHover = null;
			currentHoverIndex = -1;
		}
		displayCard.gameObject.SetActive(false);
		RecalculateCardsPosition();
	}

	void HoverOverCard(CardInHand card)
	{
		UnhoverOverCard();
		currentHover = card;
		currentHoverIndex = card.orderInHand - 1;
		displayCard.gameObject.SetActive(true);
		displayCard.Display(card.GetComponent<CardGameObject>());
		RecalculateCardsPosition();
	}

	public void UnselectCard()
	{
		if(currentSelect != null)
		{
			currentSelect.transform.SetSiblingIndex(currentSelect.orderInHand - 1);
			currentSelect.hoverable = true;
			currentSelect = null;
			currentSelectIndex = -1;
		}
	}

	public void SelectCard(CardInHand card)
	{
		UnselectCard();

		currentSelectIndex = card.orderInHand - 1;
		currentSelect = card;
		currentSelect.transform.SetSiblingIndex(100);
		currentSelect.hoverable = false;
	}

	void Start()
	{
		UnhoverOverCard();
	}

	void Update()
	{
		List<CardInHand> allCardsUnderMouse = GetAllCards();

		if (allCardsUnderMouse.Count < 1)
		{
			UnhoverOverCard();
			return;
		}

		CardInHand selectedCardOrHighestIndex = GetCardWithHighestOrder(allCardsUnderMouse);

		if(selectedCardOrHighestIndex.hand != this)
		{
			return;
		}

		if (selectedCardOrHighestIndex.orderInHand - 1 != currentHoverIndex)
		{
			HoverOverCard(selectedCardOrHighestIndex);
		}
	}

	float GetAngleForLinearLength(float length, float radius)
	{
		return Mathf.Asin((length / 2) / radius) * Mathf.Rad2Deg * 2;
	}

	public void RecalculateCardsPosition()
	{
		float angleBetweenCards = 0;
		int cardNum = cardsInHand.Count;
		float chordLength = rectTransform.sizeDelta.x - cardWidth;
		float radius = chordLength * 2;
		bool moreCardsThanCanFit = false;

		float lengthForEachCard = cardWidth - intersectionAmount;

		if ((cardWidth - intersectionAmount) * cardNum > chordLength)
		{
			moreCardsThanCanFit = true;
			lengthForEachCard = chordLength / cardNum;
		}

		angleBetweenCards = GetAngleForLinearLength(lengthForEachCard, radius);
		float angleForOneFullCard = GetAngleForLinearLength(cardWidth - intersectionAmount, radius);

		int j = -cardsInHand.Count / 2;
		float offset = 0;

		if (cardsInHand.Count % 2 == 0)
		{
			offset = angleBetweenCards / 2;
		}

		for (int i = 0; i < cardsInHand.Count; i++)
		{
			float offset2 = 0;

			if (currentHoverIndex > -1)
			{
				float finalAngle = (angleForOneFullCard - angleBetweenCards);

				if (chordLength / cardWidth < 2)
				{
					finalAngle = angleBetweenCards;
				}

				if (i > currentHoverIndex)
				{
					offset2 += finalAngle;
				}
			}

			if (!moreCardsThanCanFit)
			{
				offset2 = 0;
			}

			float yOffset = radius;
			
			if(currentHoverIndex > -1)
			{
				if(currentHoverIndex == i)
				{
					yOffset += 20;
				}
			}

			float angle = -(j * angleBetweenCards + offset + offset2);
			float yPos = 0;
			float xPos = 0;

			yPos = Mathf.Sin((angle + 90) * Mathf.Deg2Rad) * yOffset - radius;
			xPos = Mathf.Cos((angle + 90) * Mathf.Deg2Rad) * yOffset;

			if(i != currentSelectIndex)
			{
				cardsInHand[i].StartChanging(new Vector3(xPos, yPos, 0), angle, 1f);
			}
			else
			{
				cardsInHand[i].StartChanging(selectPosition.localPosition, 0, 1.5f);
			}

			j++;
		}
	}

	public void AddCardToHand(Card card, bool ethereal)
	{
		Canvas canvas = FindObjectOfType<Canvas>();
		CardInHand newCard = GameObject.Instantiate(cardPrefab, canvas.transform).GetComponent<CardInHand>();

		newCard.card = card;
		newCard.active = true;
		
		newCard.gameObject.name = "Card " + card.name;
		newCard.transform.localPosition = drawPile.localPosition;
		newCard.transform.SetParent(rectTransform);
		newCard.hand = this;
		newCard.hoverable = true;
		CardGameObject cardGO = newCard.GetComponent<CardGameObject>();

		if (ethereal)
		{
			cardGO.ethereal = true;
		}
		
		cardGO.card = card;
		cardGO.cardButton.onClick.AddListener(delegate { FindObjectOfType<BattleManager>().SelectCard(cardGO, player); });

		cardsInHand.Add(newCard);

		newCard.GetComponent<CardInHand>().orderInHand = rectTransform.childCount;

		newCard.GetComponent<DisplayCard>().Display(cardGO);

		RecalculateCardsPosition();
	}

	public void DiscardAllCards()
	{
		for (int i = cardsInHand.Count - 1; i > -1; i--) 
		{
			DiscardCard(cardsInHand[i], false);
		}
	}

	void RecalculateCardPositions()
	{
		for (int i = 0; i < cardsInHand.Count; i++)
		{
			cardsInHand[i].orderInHand = i + 1;
		}
	}

	public void DiscardCard(CardInHand card, bool animate)
	{
		UnselectCard();
		card.active = false;
		card.GetComponentInChildren<Button>().interactable = false;
		cardsInHand.Remove(card);

		if (animate)
		{
			StartCoroutine(ThrowToDiscard(card));
		}
		else
		{
			card.StartChanging(discardPile.localPosition, 0, 0);
			card.transform.SetParent(FindObjectOfType<Canvas>().transform);
			StartCoroutine(DestroyCard(card.gameObject));
		}

		RecalculateCardPositions();
	}

	IEnumerator ThrowToDiscard(CardInHand card)
	{
		GameObject cardGO = card.gameObject;
		yield return new WaitForSeconds(1f);
		card.StartChanging(discardPile.localPosition, 0, 0);
		card.transform.SetParent(FindObjectOfType<Canvas>().transform);
		StartCoroutine(DestroyCard(cardGO));
	}

	IEnumerator DestroyCard(GameObject card)
	{
		yield return new WaitForSeconds(1f);
		GameObject.Destroy(card);
	}
}