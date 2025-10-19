using UnityEngine;
using UnityEngine.UI;

namespace TcgEngine.UI
{
	public class CardGrid : MonoBehaviour
	{
		private GridLayoutGroup grid;

		private RectTransform rect;

		private void Awake()
		{
			grid = GetComponent<GridLayoutGroup>();
			rect = GetComponent<RectTransform>();
		}

		public void GetColumnAndRow(out int rows, out int columns)
		{
			rows = 0;
			columns = 0;
			if (grid.transform.childCount == 0)
			{
				return;
			}
			Vector2 anchoredPosition = grid.transform.GetChild(0).GetComponent<RectTransform>().anchoredPosition;
			bool flag = false;
			if (anchoredPosition.x == 0f && anchoredPosition.y == 0f)
			{
				return;
			}
			rows = 1;
			columns = 1;
			for (int i = 1; i < grid.transform.childCount; i++)
			{
				Vector2 anchoredPosition2 = grid.transform.GetChild(i).GetComponent<RectTransform>().anchoredPosition;
				if (Mathf.Abs(anchoredPosition.x - anchoredPosition2.x) < 0.1f)
				{
					rows++;
					flag = true;
				}
				else if (!flag)
				{
					columns++;
				}
			}
		}

		public GridLayoutGroup GetGrid()
		{
			return grid;
		}

		public RectTransform GetRect()
		{
			return rect;
		}
	}
}
