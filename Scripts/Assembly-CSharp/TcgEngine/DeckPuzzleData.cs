using UnityEngine;

namespace TcgEngine
{
	[CreateAssetMenu(fileName = "DeckPuzzleData", menuName = "TcgEngine/DeckPuzzleData", order = 7)]
	public class DeckPuzzleData : DeckData
	{
		public DeckCardSlot[] board_cards;

		public int start_cards = 5;

		public int start_mana = 2;

		public int start_hp = 20;

		public bool dont_shuffle_deck;

		public new static DeckPuzzleData Get(string id)
		{
			foreach (DeckData item in DeckData.GetAll())
			{
				if (item.id == id && item is DeckPuzzleData)
				{
					return (DeckPuzzleData)item;
				}
			}
			return null;
		}
	}
}
