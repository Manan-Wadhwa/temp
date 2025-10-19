using UnityEngine;

namespace TcgEngine.FX
{
	public class HoverFX : MonoBehaviour
	{
		public GameObject fx;

		private bool hover;

		private void Start()
		{
		}

		private void Update()
		{
			if (hover != fx.activeSelf)
			{
				fx.SetActive(hover);
			}
		}

		public void PointerEnter()
		{
			hover = true;
		}

		public void PointerExit()
		{
			hover = false;
		}
	}
}
