using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace TcgEngine.UI
{
	public class DeckDisplay : MonoBehaviour
	{
		public Text deck_title;

		public Text card_count;

		public CardUI[] ui_cards;

		private string deck_id;

		private void Awake()
		{
			Clear();
		}

		private void Update()
		{
		}

		public void Clear()
		{
			if (deck_title != null)
			{
				deck_title.text = "";
			}
			if (card_count != null)
			{
				card_count.text = "";
			}
			CardUI[] array = ui_cards;
			for (int i = 0; i < array.Length; i++)
			{
				array[i].Hide();
			}
		}

		public void SetDeck(string tid)
		{
			UserDeckData deck = Authenticator.Get().UserData.GetDeck(tid);
			DeckData deckData = DeckData.Get(tid);
			if (deck != null)
			{
				SetDeck(deck);
			}
			else if (deckData != null)
			{
				SetDeck(deckData);
			}
			else
			{
				Clear();
			}
		}

		public void SetDeck(UserDeckData deck)
		{
			Clear();
			if (deck != null)
			{
				deck_id = deck.tid;
				if (deck_title != null)
				{
					deck_title.text = deck.title;
				}
				if (card_count != null)
				{
					card_count.text = deck.GetQuantity() + " / " + GameplayData.Get().deck_size;
					card_count.color = ((deck.GetQuantity() >= GameplayData.Get().deck_size) ? Color.white : Color.red);
				}
				List<CardDataQ> list = new List<CardDataQ>();
				UserCardData[] cards = deck.cards;
				foreach (UserCardData userCardData in cards)
				{
					CardDataQ item = new CardDataQ
					{
						card = CardData.Get(userCardData.tid),
						variant = VariantData.Get(userCardData.variant),
						quantity = userCardData.quantity
					};
					if (item.card != null)
					{
						list.Add(item);
					}
				}
				ShowCards(list);
			}
			base.gameObject.SetActive(deck != null);
		}

		public void SetDeck(DeckData deck)
		{
			Clear();
			if (deck != null)
			{
				deck_id = deck.id;
				if (deck_title != null)
				{
					deck_title.text = deck.title;
				}
				if (card_count != null)
				{
					card_count.text = deck.GetQuantity() + " / " + GameplayData.Get().deck_size;
					card_count.color = ((deck.GetQuantity() >= GameplayData.Get().deck_size) ? Color.white : Color.red);
				}
				List<CardDataQ> list = new List<CardDataQ>();
				VariantData variant = VariantData.GetDefault();
				CardData[] cards = deck.cards;
				foreach (CardData cardData in cards)
				{
					if (cardData != null)
					{
						list.Add(new CardDataQ
						{
							card = cardData,
							variant = variant,
							quantity = 1
						});
					}
				}
				if (deck is DeckPuzzleData)
				{
					DeckCardSlot[] board_cards = ((DeckPuzzleData)deck).board_cards;
					foreach (DeckCardSlot deckCardSlot in board_cards)
					{
						if (deckCardSlot.card != null)
						{
							list.Add(new CardDataQ
							{
								card = deckCardSlot.card,
								variant = variant,
								quantity = 1
							});
						}
					}
				}
				ShowCards(list);
			}
			base.gameObject.SetActive(deck != null);
		}

		public void ShowCards(List<CardDataQ> cards)
		{
			cards.Sort((CardDataQ a, CardDataQ b) => b.card.mana.CompareTo(a.card.mana));
			int num = 0;
			foreach (CardDataQ card in cards)
			{
				for (int num2 = 0; num2 < card.quantity; num2++)
				{
					if (num < ui_cards.Length)
					{
						ui_cards[num].SetCard(card.card, card.variant);
						num++;
					}
				}
			}
		}

		public void Hide()
		{
			base.gameObject.SetActive(value: false);
		}

		public string GetDeck()
		{
			return deck_id;
		}
	}
}
