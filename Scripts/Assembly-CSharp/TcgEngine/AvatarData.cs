using System.Collections.Generic;
using UnityEngine;

namespace TcgEngine
{
	[CreateAssetMenu(fileName = "Avatar", menuName = "TcgEngine/Avatar", order = 10)]
	public class AvatarData : ScriptableObject
	{
		public string id;

		public Sprite avatar;

		public int sort_order;

		public static List<AvatarData> avatar_list = new List<AvatarData>();

		public static void Load(string folder = "")
		{
			if (avatar_list.Count == 0)
			{
				avatar_list.AddRange(Resources.LoadAll<AvatarData>(folder));
			}
			avatar_list.Sort((AvatarData a, AvatarData b) => (a.sort_order == b.sort_order) ? a.id.CompareTo(b.id) : a.sort_order.CompareTo(b.sort_order));
		}

		public static AvatarData Get(string id)
		{
			foreach (AvatarData item in GetAll())
			{
				if (item.id == id)
				{
					return item;
				}
			}
			return null;
		}

		public static List<AvatarData> GetAll()
		{
			return avatar_list;
		}
	}
}
