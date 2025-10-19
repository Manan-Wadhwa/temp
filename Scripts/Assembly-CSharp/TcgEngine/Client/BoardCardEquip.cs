using TcgEngine.UI;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace TcgEngine.Client
{
	public class BoardCardEquip : MonoBehaviour
	{
		public Image equip_sprite;

		public Image equip_glow;

		public Text equip_hp;

		public Color glow_ally;

		public Color glow_enemy;

		private Canvas canvas;

		private RectTransform rect;

		private Card equip;

		private bool focus;

		private float target_alpha;

		private void Awake()
		{
			canvas = GetComponentInParent<Canvas>();
			rect = GetComponent<RectTransform>();
		}

		private void Update()
		{
			if (equip != null)
			{
				target_alpha = (focus ? 1f : 0f);
				focus = GameUI.IsOverRectTransform(canvas, rect);
			}
			else
			{
				target_alpha = 0f;
				focus = false;
			}
			if (equip_glow != null)
			{
				Color color = ((GameClient.Get().GetPlayerID() == equip.player_id) ? glow_ally : glow_enemy);
				float a = Mathf.MoveTowards(equip_glow.color.a, target_alpha * color.a, 4f * Time.deltaTime);
				equip_glow.color = new Color(color.r, color.g, color.b, a);
			}
		}

		public void SetEquip(Card equip)
		{
			if (equip != null)
			{
				this.equip = equip;
				equip_sprite.sprite = equip.CardData.GetBoardArt(equip.VariantData);
				equip_hp.text = equip.GetHP().ToString();
				if (!base.gameObject.activeSelf)
				{
					base.gameObject.SetActive(value: true);
				}
			}
			else
			{
				Hide();
			}
		}

		public void Hide()
		{
			equip = null;
			focus = false;
			if (base.gameObject.activeSelf)
			{
				base.gameObject.SetActive(value: false);
			}
		}

		public bool IsFocus()
		{
			if (equip != null)
			{
				return focus;
			}
			return false;
		}

		public Card GetCard()
		{
			return equip;
		}

		public void OnPointerEnter(PointerEventData eventData)
		{
			focus = true;
		}

		public void OnPointerExit(PointerEventData eventData)
		{
			focus = false;
		}

		private void OnDisable()
		{
			focus = false;
		}
	}
}
