using System.Collections.Generic;
using UnityEngine;

namespace TcgEngine
{
	[CreateAssetMenu(fileName = "Cardback", menuName = "TcgEngine/Cardback", order = 10)]
	public class CardbackData : ScriptableObject
	{
		public string id;

		public Sprite cardback;

		public Sprite deck;

		public int sort_order;

		public static List<CardbackData> cardback_list = new List<CardbackData>();

		public static void Load(string folder = "")
		{
			if (cardback_list.Count == 0)
			{
				cardback_list.AddRange(Resources.LoadAll<CardbackData>(folder));
			}
			cardback_list.Sort((CardbackData a, CardbackData b) => (a.sort_order == b.sort_order) ? a.id.CompareTo(b.id) : a.sort_order.CompareTo(b.sort_order));
		}

		public static CardbackData Get(string id)
		{
			foreach (CardbackData item in GetAll())
			{
				if (item.id == id)
				{
					return item;
				}
			}
			return null;
		}

		public static List<CardbackData> GetAll()
		{
			return cardback_list;
		}
	}
}
