using System.Collections.Generic;
using UnityEngine;

namespace TcgEngine
{
	[CreateAssetMenu(fileName = "TraitData", menuName = "TcgEngine/TraitData", order = 1)]
	public class TraitData : ScriptableObject
	{
		public string id;

		public string title;

		public Sprite icon;

		public static List<TraitData> trait_list = new List<TraitData>();

		public string GetTitle()
		{
			return title;
		}

		public static void Load(string folder = "")
		{
			if (trait_list.Count == 0)
			{
				trait_list.AddRange(Resources.LoadAll<TraitData>(folder));
			}
		}

		public static TraitData Get(string id)
		{
			foreach (TraitData item in GetAll())
			{
				if (item.id == id)
				{
					return item;
				}
			}
			return null;
		}

		public static List<TraitData> GetAll()
		{
			return trait_list;
		}
	}
}
