using System.Collections.Generic;
using UnityEngine;

namespace TcgEngine
{
	public class DataLoader : MonoBehaviour
	{
		public GameplayData data;

		public AssetData assets;

		private HashSet<string> card_ids = new HashSet<string>();

		private HashSet<string> ability_ids = new HashSet<string>();

		private HashSet<string> deck_ids = new HashSet<string>();

		private static DataLoader instance;

		private void Awake()
		{
			instance = this;
			LoadData();
		}

		public void LoadData()
		{
			CardData.Load();
			TeamData.Load();
			RarityData.Load();
			TraitData.Load();
			VariantData.Load();
			PackData.Load();
			LevelData.Load();
			DeckData.Load();
			AbilityData.Load();
			StatusData.Load();
			AvatarData.Load();
			CardbackData.Load();
			RewardData.Load();
			CheckCardData();
			CheckAbilityData();
			CheckDeckData();
			CheckVariantData();
		}

		private void CheckCardData()
		{
			card_ids.Clear();
			foreach (CardData item in CardData.GetAll())
			{
				if (string.IsNullOrEmpty(item.id))
				{
					Debug.LogError(item.name + " id is empty");
				}
				if (card_ids.Contains(item.id))
				{
					Debug.LogError("Dupplicate Card ID: " + item.id);
				}
				if (item.team == null)
				{
					Debug.LogError(item.id + " team is null");
				}
				if (item.rarity == null)
				{
					Debug.LogError(item.id + " rarity is null");
				}
				TraitData[] traits = item.traits;
				for (int i = 0; i < traits.Length; i++)
				{
					if (traits[i] == null)
					{
						Debug.LogError(item.id + " has null trait");
					}
				}
				if (item.stats != null)
				{
					TraitStat[] stats = item.stats;
					for (int i = 0; i < stats.Length; i++)
					{
						if (stats[i].trait == null)
						{
							Debug.LogError(item.id + " has null stat trait");
						}
					}
				}
				AbilityData[] abilities = item.abilities;
				for (int i = 0; i < abilities.Length; i++)
				{
					if (abilities[i] == null)
					{
						Debug.LogError(item.id + " has null ability");
					}
				}
				card_ids.Add(item.id);
			}
		}

		private void CheckAbilityData()
		{
			ability_ids.Clear();
			foreach (AbilityData item in AbilityData.GetAll())
			{
				if (string.IsNullOrEmpty(item.id))
				{
					Debug.LogError(item.name + " id is empty");
				}
				if (ability_ids.Contains(item.id))
				{
					Debug.LogError("Dupplicate Ability ID: " + item.id);
				}
				AbilityData[] chain_abilities = item.chain_abilities;
				for (int i = 0; i < chain_abilities.Length; i++)
				{
					if (chain_abilities[i] == null)
					{
						Debug.LogError(item.id + " has null chain ability");
					}
				}
				ability_ids.Add(item.id);
			}
		}

		private void CheckDeckData()
		{
			GameplayData gameplayData = GameplayData.Get();
			CheckDeckArray(gameplayData.ai_decks);
			CheckDeckArray(gameplayData.free_decks);
			CheckDeckArray(gameplayData.starter_decks);
			if (gameplayData.test_deck == null || gameplayData.test_deck_ai == null)
			{
				Debug.Log("Deck is null in Resources/GameplayData");
			}
			deck_ids.Clear();
			foreach (DeckData item in DeckData.GetAll())
			{
				if (string.IsNullOrEmpty(item.id))
				{
					Debug.LogError(item.name + " id is empty");
				}
				if (deck_ids.Contains(item.id))
				{
					Debug.LogError("Dupplicate Deck ID: " + item.id);
				}
				CardData[] cards = item.cards;
				for (int i = 0; i < cards.Length; i++)
				{
					if (cards[i] == null)
					{
						Debug.LogError(item.id + " has null card");
					}
				}
				deck_ids.Add(item.id);
			}
		}

		private void CheckDeckArray(DeckData[] decks)
		{
			for (int i = 0; i < decks.Length; i++)
			{
				if (decks[i] == null)
				{
					Debug.Log("Deck is null in Resources/GameplayData");
				}
			}
		}

		private void CheckVariantData()
		{
			if (VariantData.GetDefault() == null)
			{
				Debug.LogError("No default variant data found, make sure you have a default VariantData");
			}
		}

		public static DataLoader Get()
		{
			return instance;
		}
	}
}
