using UnityEngine;

namespace TcgEngine.UI
{
	public class DeviceVisibility : MonoBehaviour
	{
		public bool desktop = true;

		public bool mobile = true;

		private void Start()
		{
			bool flag = GameTool.IsMobile();
			if (flag && !mobile)
			{
				base.gameObject.SetActive(value: false);
			}
			else if (!flag && !desktop)
			{
				base.gameObject.SetActive(value: false);
			}
		}
	}
}
