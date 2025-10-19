using System.Collections.Generic;
using UnityEngine;

namespace TcgEngine
{
	[CreateAssetMenu(fileName = "status", menuName = "TcgEngine/StatusData", order = 7)]
	public class StatusData : ScriptableObject
	{
		public StatusType effect;

		[Header("Display")]
		public string title;

		public Sprite icon;

		[TextArea(3, 5)]
		public string desc;

		[Header("FX")]
		public GameObject status_fx;

		[Header("AI")]
		public int hvalue;

		public static List<StatusData> status_list = new List<StatusData>();

		public string GetTitle()
		{
			return title;
		}

		public string GetDesc()
		{
			return GetDesc(1);
		}

		public string GetDesc(int value)
		{
			return desc.Replace("<value>", value.ToString());
		}

		public static void Load(string folder = "")
		{
			if (status_list.Count == 0)
			{
				status_list.AddRange(Resources.LoadAll<StatusData>(folder));
			}
		}

		public static StatusData Get(StatusType effect)
		{
			foreach (StatusData item in GetAll())
			{
				if (item.effect == effect)
				{
					return item;
				}
			}
			return null;
		}

		public static List<StatusData> GetAll()
		{
			return status_list;
		}
	}
}
