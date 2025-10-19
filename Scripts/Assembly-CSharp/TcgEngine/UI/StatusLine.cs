using UnityEngine;
using UnityEngine.UI;

namespace TcgEngine.UI
{
	public class StatusLine : MonoBehaviour
	{
		public Text title;

		public Text desc;

		private float timer;

		private void Start()
		{
			base.gameObject.SetActive(value: false);
		}

		private void Update()
		{
			timer += Time.deltaTime;
		}

		public void SetLine(CardData icard, AbilityData ability)
		{
			if (!string.IsNullOrWhiteSpace(ability.desc))
			{
				title.text = ability.GetTitle();
				desc.text = ability.GetDesc(icard);
				base.gameObject.SetActive(value: true);
				timer = 0f;
			}
		}

		public void SetLine(StatusType effect, int value)
		{
			StatusData statusData = StatusData.Get(effect);
			if (statusData != null)
			{
				SetLine(statusData, value);
			}
		}

		public void SetLine(StatusData effect, int value)
		{
			if (!string.IsNullOrWhiteSpace(effect.desc))
			{
				title.text = effect.GetTitle();
				desc.text = effect.GetDesc(value);
				base.gameObject.SetActive(value: true);
				timer = 0f;
			}
		}

		public void Hide()
		{
			if (timer > 0.05f)
			{
				base.gameObject.SetActive(value: false);
			}
		}
	}
}
