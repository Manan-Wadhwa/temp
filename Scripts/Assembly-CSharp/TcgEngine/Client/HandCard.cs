using System.Collections.Generic;
using TcgEngine.UI;
using UnityEngine;
using UnityEngine.UI;

namespace TcgEngine.Client
{
	public class HandCard : MonoBehaviour
	{
		public Image card_glow;

		public float move_speed = 10f;

		public float move_rotate_speed = 4f;

		public float move_max_rotate = 10f;

		[HideInInspector]
		public Vector2 deck_position;

		[HideInInspector]
		public float deck_angle;

		private string card_uid = "";

		private CardUI card_ui;

		private RectTransform hand_transform;

		private RectTransform card_transform;

		private Vector3 start_scale;

		private Vector3 current_rotate;

		private Vector3 target_rotate;

		private Vector3 prev_pos;

		private bool destroyed;

		private float focus_timer;

		private bool focus;

		private bool drag;

		private bool selected;

		private static List<HandCard> card_list = new List<HandCard>();

		public CardData CardData => GetCardData();

		private void Awake()
		{
			card_list.Add(this);
			card_ui = GetComponent<CardUI>();
			card_transform = base.transform.GetComponent<RectTransform>();
			hand_transform = base.transform.parent.GetComponent<RectTransform>();
			start_scale = base.transform.localScale;
		}

		private void Start()
		{
		}

		private void OnDestroy()
		{
			card_list.Remove(this);
		}

		private void Update()
		{
			if (GameClient.Get().IsReady())
			{
				Card card = GetCard();
				Vector2 b = deck_position;
				Vector3 b2 = start_scale;
				focus_timer += Time.deltaTime;
				if (IsFocus())
				{
					b = deck_position + Vector2.up * 40f;
				}
				if (IsDrag())
				{
					b = GetTargetPosition();
					b2 = start_scale * 0.75f;
					Vector3 vector = card_transform.position - prev_pos;
					Vector3 vector2 = new Vector3(vector.y * 90f, (0f - vector.x) * 90f, 0f);
					target_rotate += vector2 * move_rotate_speed * Time.deltaTime;
					target_rotate = new Vector3(Mathf.Clamp(target_rotate.x, 0f - move_max_rotate, move_max_rotate), Mathf.Clamp(target_rotate.y, 0f - move_max_rotate, move_max_rotate), 0f);
					current_rotate = Vector3.Lerp(current_rotate, target_rotate, move_rotate_speed * Time.deltaTime);
				}
				else
				{
					target_rotate = new Vector3(0f, 0f, deck_angle);
					current_rotate = new Vector3(0f, 0f, deck_angle);
				}
				card_transform.anchoredPosition = Vector2.Lerp(card_transform.anchoredPosition, b, Time.deltaTime * move_speed);
				card_transform.localRotation = Quaternion.Slerp(card_transform.localRotation, Quaternion.Euler(current_rotate), Time.deltaTime * move_speed);
				card_transform.localScale = Vector3.Lerp(card_transform.localScale, b2, 5f * Time.deltaTime);
				card_ui.SetCard(card);
				card_glow.enabled = IsFocus() || IsDrag();
				prev_pos = Vector3.Lerp(prev_pos, card_transform.position, 1f * Time.deltaTime);
				if (!drag && selected && Input.GetMouseButtonDown(0))
				{
					selected = false;
				}
			}
		}

		private Vector2 GetTargetPosition()
		{
			Card card = GetCard();
			RectTransformUtility.ScreenPointToLocalPointInRectangle(hand_transform, Input.mousePosition, Camera.main, out var localPoint);
			if (card.CardData.IsRequireTarget())
			{
				return deck_position + Vector2.up * 150f + Vector2.right * localPoint.x / 10f;
			}
			return localPoint;
		}

		public void SetCard(Card card)
		{
			card_uid = card.uid;
			card_ui.SetCard(card);
		}

		public void Kill()
		{
			if (!destroyed)
			{
				destroyed = true;
				Object.Destroy(base.gameObject);
			}
		}

		public bool IsFocus()
		{
			if (GameTool.IsMobile())
			{
				if (selected)
				{
					return !drag;
				}
				return false;
			}
			if (focus && !drag)
			{
				return focus_timer > 0f;
			}
			return false;
		}

		public bool IsDrag()
		{
			return drag;
		}

		public Card GetCard()
		{
			return GameClient.Get().GetGameData().GetCard(card_uid);
		}

		public CardData GetCardData()
		{
			Card card = GetCard();
			if (card != null)
			{
				return CardData.Get(card.card_id);
			}
			return null;
		}

		public string GetCardUID()
		{
			return card_uid;
		}

		public void OnMouseEnterCard()
		{
			if (!GameUI.IsUIOpened())
			{
				focus = true;
			}
		}

		public void OnMouseExitCard()
		{
			focus = false;
			focus_timer = -0.2f;
		}

		public void OnMouseDownCard()
		{
			if (!GameUI.IsOverUILayer("UI"))
			{
				UnselectAll();
				drag = true;
				selected = true;
				PlayerControls.Get().UnselectAll();
				AudioTool.Get().PlaySFX("hand_card", AssetData.Get().hand_card_click_audio);
			}
		}

		public void OnMouseUpCard()
		{
			Vector2 vector = GameCamera.Get().MouseToPercent(Input.mousePosition);
			Vector3 board_pos = GameBoard.Get().RaycastMouseBoard();
			if (drag && vector.y > 0.25f)
			{
				TryPlayCard(board_pos);
			}
			else
			{
				HandCardArea.Get().SortCards();
			}
			drag = false;
		}

		public void TryPlayCard(Vector3 board_pos)
		{
			if (!GameClient.Get().IsYourTurn())
			{
				WarningText.ShowNotYourTurn();
				return;
			}
			BSlot nearest = BSlot.GetNearest(board_pos);
			int playerID = GameClient.Get().GetPlayerID();
			Game gameData = GameClient.Get().GetGameData();
			Player player = gameData.GetPlayer(playerID);
			Card card = GetCard();
			Slot slot = Slot.None;
			if (nearest != null)
			{
				slot = nearest.GetEmptySlot(board_pos);
			}
			if (nearest != null && card.CardData.IsRequireTarget())
			{
				slot = nearest.GetSlot(board_pos);
			}
			Card card2 = nearest?.GetSlotCard(board_pos);
			if (nearest != null && card.CardData.IsRequireTargetSpell() && card2 != null && card2.HasStatus(StatusType.SpellImmunity))
			{
				WarningText.ShowSpellImmune();
			}
			else if (!player.CanPayMana(card))
			{
				WarningText.ShowNoMana();
			}
			else if (gameData.CanPlayCard(card, slot, skip_cost: true))
			{
				PlayCard(slot);
			}
		}

		public void PlayCard(Slot slot)
		{
			GameClient.Get().PlayCard(GetCard(), slot);
			HandCardArea.Get().DelayRefresh(GetCard());
			Object.Destroy(base.gameObject);
			if (GameTool.IsMobile())
			{
				BoardCard.UnfocusAll();
			}
		}

		public static HandCard GetDrag()
		{
			foreach (HandCard item in card_list)
			{
				if (item.IsDrag())
				{
					return item;
				}
			}
			return null;
		}

		public static HandCard GetFocus()
		{
			foreach (HandCard item in card_list)
			{
				if (item.IsFocus())
				{
					return item;
				}
			}
			return null;
		}

		public static HandCard Get(string uid)
		{
			foreach (HandCard item in card_list)
			{
				if ((bool)item && item.GetCardUID() == uid)
				{
					return item;
				}
			}
			return null;
		}

		public static void UnselectAll()
		{
			foreach (HandCard item in card_list)
			{
				item.selected = false;
			}
		}

		public static List<HandCard> GetAll()
		{
			return card_list;
		}
	}
}
