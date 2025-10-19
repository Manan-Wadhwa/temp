using System.Collections.Generic;
using UnityEngine;

namespace TcgEngine
{
	[CreateAssetMenu(fileName = "PackData", menuName = "TcgEngine/PackData", order = 5)]
	public class PackData : ScriptableObject
	{
		public string id;

		[Header("Content")]
		public PackType type;

		public int cards = 5;

		public PackRarity[] rarities_1st;

		public PackRarity[] rarities;

		public PackVariant[] variants;

		[Header("Display")]
		public string title;

		public Sprite pack_img;

		public Sprite cardback_img;

		[TextArea(5, 10)]
		public string desc;

		public int sort_order;

		[Header("Availability")]
		public bool available = true;

		public int cost = 100;

		public static List<PackData> pack_list = new List<PackData>();

		public static void Load(string folder = "")
		{
			if (pack_list.Count == 0)
			{
				pack_list.AddRange(Resources.LoadAll<PackData>(folder));
			}
			pack_list.Sort((PackData a, PackData b) => (a.sort_order == b.sort_order) ? a.id.CompareTo(b.id) : a.sort_order.CompareTo(b.sort_order));
		}

		public string GetTitle()
		{
			return title;
		}

		public string GetDesc()
		{
			return desc;
		}

		public static PackData Get(string id)
		{
			foreach (PackData item in GetAll())
			{
				if (item.id == id)
				{
					return item;
				}
			}
			return null;
		}

		public static List<PackData> GetAllAvailable()
		{
			List<PackData> list = new List<PackData>();
			foreach (PackData item in GetAll())
			{
				if (item.available)
				{
					list.Add(item);
				}
			}
			return list;
		}

		public static List<PackData> GetAll()
		{
			return pack_list;
		}
	}
}
