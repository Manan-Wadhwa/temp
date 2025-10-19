using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

namespace TcgEngine.UI
{
	public class CardbackUI : MonoBehaviour
	{
		public UnityAction<CardbackData> onClick;

		private Image cardback_img;

		private Button cardback_button;

		private Sprite default_icon;

		private CardbackData cardback;

		private void Awake()
		{
			cardback_img = GetComponent<Image>();
			cardback_button = GetComponent<Button>();
			default_icon = cardback_img.sprite;
			if (cardback_button != null)
			{
				cardback_button.onClick.AddListener(OnClick);
			}
		}

		public void SetCardback(CardbackData cardback)
		{
			this.cardback = cardback;
			cardback_img.enabled = true;
			cardback_img.sprite = default_icon;
			if (cardback != null)
			{
				cardback_img.sprite = cardback.cardback;
			}
		}

		public void SetDefaultCardback()
		{
			cardback = null;
			cardback_img.enabled = true;
			cardback_img.sprite = default_icon;
		}

		public void Hide()
		{
			cardback = null;
			cardback_img.enabled = false;
		}

		public CardbackData GetCardback()
		{
			return cardback;
		}

		private void OnClick()
		{
			if (cardback != null)
			{
				onClick?.Invoke(cardback);
			}
		}
	}
}
