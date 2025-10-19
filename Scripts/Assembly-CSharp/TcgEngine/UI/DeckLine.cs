using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.Events;
using UnityEngine.UI;

namespace TcgEngine.UI
{
	public class DeckLine : MonoBehaviour, IPointerClickHandler, IEventSystemHandler, IPointerEnterHandler, IPointerExitHandler
	{
		public Image image;

		public Image frame;

		public Text title;

		public Text value;

		public IconValue cost;

		public UIPanel delete_btn;

		public AudioClip click_audio;

		public Material disabled_mat;

		public Material default_mat;

		public UnityAction<DeckLine> onClick;

		public UnityAction<DeckLine> onClickRight;

		public UnityAction<DeckLine> onClickDelete;

		private CardData card;

		private VariantData variant;

		private DeckData deck;

		private UserDeckData udeck;

		private bool hidden;

		private bool hover;

		private void Awake()
		{
		}

		private void Update()
		{
			if (delete_btn != null)
			{
				bool flag = hover || GameTool.IsMobile();
				delete_btn.SetVisible(flag && !hidden && udeck != null);
			}
		}

		public void SetLine(CardData card, VariantData variant, int quantity, bool invalid = false)
		{
			this.card = card;
			this.variant = variant;
			deck = null;
			udeck = null;
			hidden = false;
			if (title != null)
			{
				title.text = card.title;
			}
			if (title != null)
			{
				title.color = variant.color;
			}
			if (value != null)
			{
				value.text = quantity.ToString();
			}
			if (value != null)
			{
				value.enabled = quantity > 1;
			}
			if (cost != null)
			{
				cost.value = card.mana;
			}
			if (value != null)
			{
				value.color = (invalid ? Color.red : Color.white);
			}
			if (invalid)
			{
				title.color = Color.gray;
			}
			if (image != null)
			{
				image.sprite = card.GetFullArt(variant);
				image.enabled = true;
				image.material = (invalid ? disabled_mat : default_mat);
			}
			if (frame != null)
			{
				frame.sprite = variant.frame;
				frame.enabled = true;
				frame.material = (invalid ? disabled_mat : default_mat);
			}
			base.gameObject.SetActive(value: true);
		}

		public void SetLine(DeckData deck)
		{
			card = null;
			this.deck = deck;
			udeck = null;
			hidden = false;
			if (title != null)
			{
				title.text = deck.title;
			}
			if (title != null)
			{
				title.color = Color.white;
			}
			if (value != null)
			{
				value.text = deck.GetQuantity().ToString();
			}
			if (value != null)
			{
				value.enabled = deck.GetQuantity() > 0;
			}
			base.gameObject.SetActive(value: true);
		}

		public void SetLine(UserData udata, UserDeckData deck)
		{
			card = null;
			this.deck = null;
			udeck = deck;
			hidden = false;
			if (title != null)
			{
				title.text = deck.title;
			}
			if (title != null)
			{
				title.color = Color.white;
			}
			if (value != null)
			{
				value.text = deck.GetQuantity() + "/" + GameplayData.Get().deck_size;
			}
			if (value != null)
			{
				value.enabled = deck.GetQuantity() > 0;
			}
			if (value != null)
			{
				value.color = (udata.IsDeckValid(deck) ? Color.white : Color.red);
			}
			base.gameObject.SetActive(value: true);
		}

		public void SetLine(string title)
		{
			card = null;
			deck = null;
			udeck = null;
			hidden = false;
			if (this.title != null)
			{
				this.title.text = title;
			}
			if (this.title != null)
			{
				this.title.color = Color.white;
			}
			if (value != null)
			{
				value.enabled = false;
			}
			base.gameObject.SetActive(value: true);
		}

		public void Hide()
		{
			card = null;
			deck = null;
			udeck = null;
			hidden = true;
			hover = false;
			if (title != null)
			{
				title.text = "";
			}
			if (title != null)
			{
				title.color = Color.white;
			}
			if (value != null)
			{
				value.text = "";
			}
			if (value != null)
			{
				value.enabled = true;
			}
			if (cost != null)
			{
				cost.value = 0;
			}
			if (image != null)
			{
				image.enabled = false;
			}
			if (frame != null)
			{
				frame.enabled = false;
			}
			if (delete_btn != null)
			{
				delete_btn.SetVisible(visi: false);
			}
			base.gameObject.SetActive(value: false);
		}

		public CardData GetCard()
		{
			return card;
		}

		public VariantData GetVariant()
		{
			return variant;
		}

		public DeckData GetDeck()
		{
			return deck;
		}

		public UserDeckData GetUserDeck()
		{
			return udeck;
		}

		public bool IsHidden()
		{
			return hidden;
		}

		public void OnPointerClick(PointerEventData eventData)
		{
			if (!hidden)
			{
				if (eventData.button == PointerEventData.InputButton.Left)
				{
					onClick?.Invoke(this);
					AudioTool.Get().PlaySFX("ui", click_audio);
				}
				if (eventData.button == PointerEventData.InputButton.Right)
				{
					onClickRight?.Invoke(this);
					AudioTool.Get().PlaySFX("ui", click_audio);
				}
			}
		}

		public void OnClickDelete()
		{
			onClickDelete?.Invoke(this);
			AudioTool.Get().PlaySFX("ui", click_audio);
		}

		public void OnPointerEnter(PointerEventData eventData)
		{
			hover = true;
		}

		public void OnPointerExit(PointerEventData eventData)
		{
			hover = false;
		}
	}
}
