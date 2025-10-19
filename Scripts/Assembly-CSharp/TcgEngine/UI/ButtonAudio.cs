using UnityEngine;
using UnityEngine.UI;

namespace TcgEngine.UI
{
	public class ButtonAudio : MonoBehaviour
	{
		public AudioClip click_audio;

		private void Start()
		{
			Button component = GetComponent<Button>();
			if (component != null)
			{
				component.onClick.AddListener(OnClick);
			}
		}

		private void OnClick()
		{
			AudioTool.Get().PlaySFX("ui", click_audio);
		}
	}
}
