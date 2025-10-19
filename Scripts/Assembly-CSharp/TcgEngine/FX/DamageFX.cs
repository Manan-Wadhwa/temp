using UnityEngine;
using UnityEngine.UI;

namespace TcgEngine.FX
{
	public class DamageFX : MonoBehaviour
	{
		public Text text_value;

		private void Start()
		{
		}

		private void Update()
		{
		}

		public void SetValue(int value)
		{
			if (text_value != null)
			{
				text_value.text = value.ToString();
			}
		}

		public void SetValue(string value)
		{
			if (text_value != null)
			{
				text_value.text = value;
			}
		}
	}
}
