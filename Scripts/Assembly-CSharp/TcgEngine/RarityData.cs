using System.Collections.Generic;
using UnityEngine;

namespace TcgEngine
{
	[CreateAssetMenu(fileName = "RarityData", menuName = "TcgEngine/RarityData", order = 1)]
	public class RarityData : ScriptableObject
	{
		public string id;

		public string title;

		public Sprite icon;

		public int rank;

		public static List<RarityData> rarity_list = new List<RarityData>();

		public static void Load(string folder = "")
		{
			if (rarity_list.Count == 0)
			{
				rarity_list.AddRange(Resources.LoadAll<RarityData>(folder));
			}
		}

		public static RarityData GetFirst()
		{
			int num = 99999;
			RarityData result = null;
			foreach (RarityData item in GetAll())
			{
				if (item.rank < num)
				{
					result = item;
					num = item.rank;
				}
			}
			return result;
		}

		public static RarityData Get(string id)
		{
			foreach (RarityData item in GetAll())
			{
				if (item.id == id)
				{
					return item;
				}
			}
			return null;
		}

		public static List<RarityData> GetAll()
		{
			return rarity_list;
		}
	}
}
