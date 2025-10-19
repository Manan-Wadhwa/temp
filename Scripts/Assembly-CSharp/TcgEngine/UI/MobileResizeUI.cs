using UnityEngine;

namespace TcgEngine.UI
{
	public class MobileResizeUI : MonoBehaviour
	{
		public Vector2 position_offset;

		public float size = 1f;

		private void Start()
		{
			if (GameTool.IsMobile())
			{
				GetComponent<RectTransform>().anchoredPosition += position_offset;
				base.transform.localScale = base.transform.localScale * size;
			}
		}
	}
}
