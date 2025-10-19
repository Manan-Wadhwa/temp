using System.Collections.Generic;
using UnityEngine;

namespace TcgEngine.Client
{
	public class BoardRef : MonoBehaviour
	{
		public BoardRefType type;

		public int index;

		public bool opponent;

		private static List<BoardRef> ref_list = new List<BoardRef>();

		private void Awake()
		{
			ref_list.Add(this);
		}

		private void OnDestroy()
		{
			ref_list.Remove(this);
		}

		public static BoardRef Get(BoardRefType type, bool opponent)
		{
			foreach (BoardRef item in ref_list)
			{
				if (item.type == type && item.opponent == opponent)
				{
					return item;
				}
			}
			return null;
		}

		public static BoardRef Get(BoardRefType type, int index)
		{
			foreach (BoardRef item in ref_list)
			{
				if (item.type == type && item.index == index)
				{
					return item;
				}
			}
			return null;
		}
	}
}
