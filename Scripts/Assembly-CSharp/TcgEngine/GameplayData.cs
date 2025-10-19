using TcgEngine.AI;
using UnityEngine;

namespace TcgEngine
{
	[CreateAssetMenu(fileName = "GameplayData", menuName = "TcgEngine/GameplayData", order = 0)]
	public class GameplayData : ScriptableObject
	{
		[Header("Gameplay")]
		public int hp_start = 20;

		public int mana_start = 1;

		public int mana_per_turn = 1;

		public int mana_max = 10;

		public int cards_start = 5;

		public int cards_per_turn = 1;

		public int cards_max = 10;

		public float turn_duration = 30f;

		public CardData second_bonus;

		[Header("Deckbuilding")]
		public int deck_size = 30;

		public int deck_duplicate_max = 2;

		[Header("Buy/Sell")]
		public float sell_ratio = 0.8f;

		[Header("AI")]
		public AIType ai_type;

		public int ai_level = 10;

		[Header("Decks")]
		public DeckData[] free_decks;

		public DeckData[] starter_decks;

		public DeckData[] ai_decks;

		[Header("Scenes")]
		public string[] arena_list;

		[Header("Test")]
		public DeckData test_deck;

		public DeckData test_deck_ai;

		public bool ai_vs_ai;

		public int GetPlayerLevel(int xp)
		{
			return Mathf.FloorToInt((float)xp / 1000f) + 1;
		}

		public string GetRandomArena()
		{
			if (arena_list.Length != 0)
			{
				return arena_list[Random.Range(0, arena_list.Length)];
			}
			return "Game";
		}

		public string GetRandomAIDeck()
		{
			if (ai_decks.Length != 0)
			{
				return ai_decks[Random.Range(0, ai_decks.Length)].id;
			}
			return "";
		}

		public static GameplayData Get()
		{
			return DataLoader.Get().data;
		}
	}
}
