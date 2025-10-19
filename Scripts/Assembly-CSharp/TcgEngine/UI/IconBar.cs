using UnityEngine;
using UnityEngine.UI;

namespace TcgEngine.UI
{
	public class IconBar : MonoBehaviour
	{
		public int value;

		public int max_value = 4;

		public bool auto_refresh = true;

		public Image[] icons;

		public Sprite sprite_full;

		public Sprite sprite_empty;

		private void Awake()
		{
		}

		private void Update()
		{
			if (auto_refresh)
			{
				Refresh();
			}
		}

		public void Refresh()
		{
			int num = 0;
			Image[] array = icons;
			foreach (Image obj in array)
			{
				obj.gameObject.SetActive(num < value || num < max_value);
				obj.sprite = ((num < value) ? sprite_full : sprite_empty);
				num++;
			}
		}

		public void SetMat(Material mat)
		{
			Image[] array = icons;
			for (int i = 0; i < array.Length; i++)
			{
				array[i].material = mat;
			}
		}
	}
}
