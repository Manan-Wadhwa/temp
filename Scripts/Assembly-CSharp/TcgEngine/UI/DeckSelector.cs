using System;
using UnityEngine;
using UnityEngine.Events;

namespace TcgEngine.UI
{
	public class DeckSelector : MonoBehaviour
	{
		public DropdownValue deck_dropdown;

		public UnityAction<string> onChange;

		private void Start()
		{
			DropdownValue dropdownValue = deck_dropdown;
			dropdownValue.onValueChanged = (UnityAction<int, string>)Delegate.Combine(dropdownValue.onValueChanged, new UnityAction<int, string>(OnChange));
		}

		private void Update()
		{
		}

		public void RefreshDeckList()
		{
			deck_dropdown.ClearOptions();
			DeckData[] free_decks = GameplayData.Get().free_decks;
			foreach (DeckData deckData in free_decks)
			{
				deck_dropdown.AddOption(deckData.id, deckData.title);
			}
			UserData userData = Authenticator.Get().UserData;
			if (userData == null)
			{
				return;
			}
			UserDeckData[] decks = userData.decks;
			foreach (UserDeckData userDeckData in decks)
			{
				if (userData.IsDeckValid(userDeckData))
				{
					deck_dropdown.AddOption(userDeckData.tid, userDeckData.title);
				}
			}
		}

		private void SelectDeck(UserDeckData deck)
		{
			if (deck != null)
			{
				deck_dropdown.SetValue(deck.tid);
			}
		}

		private void SelectDeck(DeckData deck)
		{
			if (deck != null)
			{
				deck_dropdown.SetValue(deck.id);
			}
		}

		public void SelectDeck(string deck)
		{
			UserDeckData userDeckData = Authenticator.Get().UserData?.GetDeck(deck);
			if (userDeckData != null)
			{
				SelectDeck(userDeckData);
				return;
			}
			DeckData deckData = DeckData.Get(deck);
			if (deckData != null)
			{
				SelectDeck(deckData);
			}
		}

		public void Lock()
		{
			deck_dropdown.interactable = false;
		}

		public void Unlock()
		{
			deck_dropdown.interactable = true;
		}

		public void SetLocked(bool locked)
		{
			deck_dropdown.interactable = !locked;
		}

		private void OnChange(int i, string val)
		{
			string selectedValue = deck_dropdown.GetSelectedValue();
			onChange?.Invoke(selectedValue);
		}

		public string GetDeckID()
		{
			return deck_dropdown.GetSelectedValue();
		}

		public string GetDeckTitle()
		{
			return deck_dropdown.GetSelectedText();
		}

		public UserDeckData GetDeck()
		{
			UserDeckData deck = Authenticator.Get().UserData.GetDeck(GetDeckID());
			DeckData deckData = DeckData.Get(GetDeckID());
			if (deck != null)
			{
				return deck;
			}
			if (deckData != null)
			{
				return new UserDeckData(deckData);
			}
			return null;
		}
	}
}
