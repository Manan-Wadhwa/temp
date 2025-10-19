using System.Collections.Generic;
using UnityEngine;

namespace TcgEngine
{
	[CreateAssetMenu(fileName = "RewardData", menuName = "TcgEngine/RewardData", order = 5)]
	public class RewardData : ScriptableObject
	{
		public string id;

		public string group;

		public int coins;

		public int xp;

		public PackData[] packs;

		public CardData[] cards;

		public DeckData[] decks;

		public bool repeat = true;

		public static List<RewardData> reward_list = new List<RewardData>();

		public static void Load(string folder = "")
		{
			if (reward_list.Count == 0)
			{
				reward_list.AddRange(Resources.LoadAll<RewardData>(folder));
			}
		}

		public static RewardData Get(string id)
		{
			foreach (RewardData item in GetAll())
			{
				if (item.id == id)
				{
					return item;
				}
			}
			return null;
		}

		public static List<RewardData> GetAll()
		{
			return reward_list;
		}
	}
}
