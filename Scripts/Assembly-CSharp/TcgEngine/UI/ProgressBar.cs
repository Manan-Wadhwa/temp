using UnityEngine;
using UnityEngine.UI;

namespace TcgEngine.UI
{
	public class ProgressBar : MonoBehaviour
	{
		public float value;

		public float value_max;

		public Image fill;

		private void Start()
		{
		}

		private void Update()
		{
			float fillAmount = value / Mathf.Max(value_max, 0.01f);
			fill.fillAmount = fillAmount;
		}
	}
}
