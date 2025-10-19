using UnityEngine;
using UnityEngine.UI;

namespace TcgEngine.UI
{
	public class ToggleText : MonoBehaviour
	{
		public Color on_color = Color.yellow;

		public Color off_color = Color.white;

		private Toggle toggle;

		private Text toggle_txt;

		private bool previous;

		private void Awake()
		{
			toggle = GetComponent<Toggle>();
			toggle_txt = GetComponentInChildren<Text>();
		}

		private void Start()
		{
			Refresh();
		}

		private void Update()
		{
			if (previous != toggle.isOn)
			{
				Refresh();
			}
		}

		private void Refresh()
		{
			toggle_txt.color = (toggle.isOn ? on_color : off_color);
			previous = toggle.isOn;
		}
	}
}
