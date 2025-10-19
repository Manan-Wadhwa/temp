using UnityEngine;
using UnityEngine.UI;

namespace TcgEngine.UI
{
	public class ChatBubble : MonoBehaviour
	{
		public Text msg_txt;

		public Image bubble;

		public CanvasGroup group;

		private float timer;

		private void Start()
		{
		}

		private void Update()
		{
			timer -= Time.deltaTime;
			group.alpha = timer;
			if (timer < 0f)
			{
				Hide();
			}
		}

		public void SetLine(string msg, float duration)
		{
			msg_txt.text = msg;
			timer = duration;
			base.gameObject.SetActive(value: true);
		}

		public void Hide()
		{
			base.gameObject.SetActive(value: false);
		}
	}
}
