using System.Collections.Generic;
using UnityEngine;

namespace TcgEngine
{
	[CreateAssetMenu(fileName = "VariantData", menuName = "TcgEngine/VariantData", order = 5)]
	public class VariantData : ScriptableObject
	{
		public string id;

		public string title;

		public Sprite frame;

		public Sprite frame_board;

		public Color color = Color.white;

		public int cost_factor = 1;

		public bool is_default;

		public static List<VariantData> variant_list = new List<VariantData>();

		public string GetSuffix()
		{
			return "_" + id;
		}

		public static void Load(string folder = "")
		{
			if (variant_list.Count == 0)
			{
				variant_list.AddRange(Resources.LoadAll<VariantData>(folder));
			}
		}

		public static VariantData GetDefault()
		{
			foreach (VariantData item in GetAll())
			{
				if (item.is_default)
				{
					return item;
				}
			}
			return null;
		}

		public static VariantData GetSpecial()
		{
			foreach (VariantData item in GetAll())
			{
				if (!item.is_default)
				{
					return item;
				}
			}
			return null;
		}

		public static VariantData Get(string id)
		{
			foreach (VariantData item in GetAll())
			{
				if (item.id == id)
				{
					return item;
				}
			}
			return GetDefault();
		}

		public static List<VariantData> GetAll()
		{
			return variant_list;
		}
	}
}
