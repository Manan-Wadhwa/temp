using System.Collections.Generic;
using TcgEngine.FX;
using TcgEngine.UI;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

namespace TcgEngine.Client
{
	public class BoardCard : MonoBehaviour
	{
		public SpriteRenderer card_sprite;

		public SpriteRenderer card_glow;

		public SpriteRenderer card_shadow;

		public Image armor_icon;

		public Text armor;

		public CanvasGroup status_group;

		public Text status_text;

		public BoardCardEquip equipment;

		public AbilityButton[] buttons;

		public Color glow_ally;

		public Color glow_enemy;

		public UnityAction onKill;

		private CardUI card_ui;

		private BoardCardFX card_fx;

		private Canvas canvas;

		private string card_uid = "";

		private bool destroyed;

		private bool focus;

		private float timer;

		private float status_alpha_target;

		private bool back_to_hand;

		private Vector3 back_to_hand_target;

		private static List<BoardCard> card_list = new List<BoardCard>();

		public CardData CardData => GetCardData();

		private void Awake()
		{
			card_list.Add(this);
			card_ui = GetComponent<CardUI>();
			card_fx = GetComponent<BoardCardFX>();
			canvas = GetComponentInChildren<Canvas>();
			card_glow.color = new Color(card_glow.color.r, card_glow.color.g, card_glow.color.b, 0f);
			canvas.gameObject.SetActive(value: false);
			status_alpha_target = 0f;
			if (equipment != null)
			{
				equipment.Hide();
			}
			if (status_group != null)
			{
				status_group.alpha = 0f;
			}
		}

		private void OnDestroy()
		{
			card_list.Remove(this);
		}

		private void Start()
		{
			Vector3 angles = GameBoard.Get().GetAngles();
			base.transform.rotation = Quaternion.Euler(angles.x, angles.y, angles.z + Random.Range(-1f, 1f));
		}

		private void Update()
		{
			if (!GameClient.Get().IsReady())
			{
				return;
			}
			timer += Time.deltaTime;
			if (timer > 0.15f && !destroyed && !canvas.gameObject.activeSelf)
			{
				canvas.gameObject.SetActive(value: true);
			}
			PlayerControls playerControls = PlayerControls.Get();
			Game gameData = GameClient.Get().GetGameData();
			Player player = GameClient.Get().GetPlayer();
			Card card = gameData.GetCard(card_uid);
			if (!destroyed)
			{
				card_ui.SetCard(card);
			}
			bool flag = playerControls.GetSelected() == this;
			Vector3 targetPos = GetTargetPos();
			float num = 12f;
			base.transform.position = Vector3.MoveTowards(base.transform.position, targetPos, num * Time.deltaTime);
			float num2 = ((IsFocus() || flag) ? 1f : 0f);
			if (destroyed || timer < 1f)
			{
				num2 = 0f;
			}
			if (equipment != null && equipment.IsFocus())
			{
				num2 = 0f;
			}
			Color color = ((player.player_id == card.player_id) ? glow_ally : glow_enemy);
			float a = Mathf.MoveTowards(card_glow.color.a, num2 * color.a, 4f * Time.deltaTime);
			card_glow.color = new Color(color.r, color.g, color.b, a);
			card_shadow.enabled = !destroyed && timer > 0.4f;
			card_sprite.color = (card.HasStatus(StatusType.Stealth) ? Color.gray : Color.white);
			card_ui.hp.color = ((destroyed || card.damage > 0) ? Color.yellow : Color.white);
			int statusValue = card.GetStatusValue(StatusType.Armor);
			armor.text = statusValue.ToString();
			armor.enabled = statusValue > 0;
			armor_icon.enabled = statusValue > 0;
			Sprite boardArt = card.CardData.GetBoardArt(card.VariantData);
			if (boardArt != card_sprite.sprite)
			{
				card_sprite.sprite = boardArt;
			}
			Sprite frame_board = card.VariantData.frame_board;
			if (frame_board != null && card_ui.frame_image != null)
			{
				card_ui.frame_image.sprite = frame_board;
			}
			if (equipment != null)
			{
				Card equipCard = gameData.GetEquipCard(card.equipped_uid);
				equipment.SetEquip(equipCard);
			}
			AbilityButton[] array = buttons;
			for (int i = 0; i < array.Length; i++)
			{
				array[i].Hide();
			}
			if (flag && card.player_id == player.player_id)
			{
				int num3 = 0;
				foreach (AbilityData ability in card.GetAbilities())
				{
					if ((bool)ability && gameData.CanCastAbility(card, ability) && (ability.target != AbilityTarget.Self || ability.AreTargetConditionsMet(gameData, card, card)))
					{
						if (num3 < buttons.Length)
						{
							buttons[num3].SetAbility(card, ability);
						}
						num3++;
					}
				}
			}
			if (status_group != null)
			{
				status_group.alpha = Mathf.MoveTowards(status_group.alpha, status_alpha_target, 5f * Time.deltaTime);
			}
		}

		private Vector3 GetTargetPos()
		{
			Card card = GameClient.Get().GetGameData().GetCard(card_uid);
			if (destroyed && back_to_hand && timer > 0.5f)
			{
				return back_to_hand_target;
			}
			BSlot bSlot = BSlot.Get(card.slot);
			if (bSlot != null)
			{
				return bSlot.GetPosition(card.slot);
			}
			return base.transform.position;
		}

		public void SetCard(Card card)
		{
			card_uid = card.uid;
			base.transform.position = GetTargetPos();
			CardData cardData = CardData.Get(card.card_id);
			if ((bool)cardData)
			{
				card_ui.SetCard(card);
				card_sprite.sprite = cardData.GetBoardArt(card.VariantData);
				armor.enabled = false;
				armor_icon.enabled = false;
				status_alpha_target = 0f;
			}
		}

		public void SetOrder(int order)
		{
			card_sprite.sortingOrder = order;
			canvas.sortingOrder = order + 1;
		}

		public void Kill()
		{
			if (!destroyed)
			{
				Game gameData = GameClient.Get().GetGameData();
				Card card = gameData.GetCard(card_uid);
				Player player = gameData.GetPlayer(card.player_id);
				destroyed = true;
				timer = 0f;
				status_alpha_target = 0f;
				card_glow.enabled = false;
				card_shadow.enabled = false;
				SetOrder(card_sprite.sortingOrder - 2);
				Object.Destroy(base.gameObject, 1.3f);
				TimeTool.WaitFor(0.8f, delegate
				{
					canvas.gameObject.SetActive(value: false);
				});
				GameBoard gameBoard = GameBoard.Get();
				if (player.HasCard(player.cards_hand, card) || player.HasCard(player.cards_deck, card))
				{
					back_to_hand = true;
					back_to_hand_target = ((player.player_id == GameClient.Get().GetPlayerID()) ? (-gameBoard.transform.up) : gameBoard.transform.up);
					back_to_hand_target *= 10f;
				}
				if (!back_to_hand)
				{
					card.hp = 0;
					card_ui.SetCard(card);
				}
				if (onKill != null)
				{
					onKill();
				}
			}
		}

		private void ShowStatusBar()
		{
			if (GetCard() != null && status_text != null && !destroyed)
			{
				string statusText = GetStatusText();
				string traitText = GetTraitText();
				if (statusText.Length > 0 && traitText.Length > 0)
				{
					status_text.text = traitText + ", " + statusText;
				}
				else
				{
					status_text.text = traitText + statusText;
				}
			}
			bool flag = status_text != null && status_text.text.Length > 0;
			status_alpha_target = (flag ? 1f : 0f);
		}

		public string GetStatusText()
		{
			Card card = GetCard();
			string text = "";
			foreach (CardStatus item in card.GetAllStatus())
			{
				StatusData statusData = StatusData.Get(item.type);
				if (statusData != null && !string.IsNullOrEmpty(statusData.title))
				{
					int num = Mathf.Max(item.value, Mathf.CeilToInt((float)item.duration / 2f));
					string text2 = ((num > 1) ? (" " + num) : "");
					text = text + statusData.GetTitle() + text2 + ", ";
				}
			}
			if (text.Length > 2)
			{
				text = text.Substring(0, text.Length - 2);
			}
			return text;
		}

		public string GetTraitText()
		{
			Card card = GetCard();
			string text = "";
			foreach (CardTrait allTrait in card.GetAllTraits())
			{
				TraitData traitData = TraitData.Get(allTrait.id);
				if (traitData != null && !string.IsNullOrEmpty(traitData.title))
				{
					int value = allTrait.value;
					string text2 = ((value > 1) ? (" " + value) : "");
					text = text + traitData.GetTitle() + text2 + ", ";
				}
			}
			if (text.Length > 2)
			{
				text = text.Substring(0, text.Length - 2);
			}
			return text;
		}

		public bool IsDead()
		{
			return destroyed;
		}

		public bool IsFocus()
		{
			return focus;
		}

		public bool IsEquipFocus()
		{
			if (equipment != null)
			{
				return equipment.IsFocus();
			}
			return false;
		}

		public void OnMouseEnter()
		{
			if (!GameUI.IsUIOpened() && !GameTool.IsMobile())
			{
				focus = true;
				ShowStatusBar();
			}
		}

		public void OnMouseExit()
		{
			focus = false;
			status_alpha_target = 0f;
		}

		public void OnMouseDown()
		{
			if (!GameUI.IsOverUILayer("UI"))
			{
				PlayerControls.Get().SelectCard(this);
				if (GameTool.IsMobile())
				{
					focus = true;
					ShowStatusBar();
				}
			}
		}

		public void OnMouseUp()
		{
		}

		public void OnMouseOver()
		{
			if (Input.GetMouseButtonDown(1))
			{
				PlayerControls.Get().SelectCardRight(this);
			}
		}

		public string GetCardUID()
		{
			return card_uid;
		}

		public Card GetCard()
		{
			return GameClient.Get().GetGameData().GetCard(card_uid);
		}

		public Card GetEquipCard()
		{
			if (equipment != null)
			{
				return equipment.GetCard();
			}
			return null;
		}

		public Card GetFocusCard()
		{
			if (IsEquipFocus())
			{
				return GetEquipCard();
			}
			return GetCard();
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

		public Slot GetSlot()
		{
			return GetCard().slot;
		}

		public BoardCardFX GetCardFX()
		{
			return card_fx;
		}

		public static int GetNbCardsBoardPlayer(int player_id)
		{
			int num = 0;
			foreach (BoardCard item in card_list)
			{
				if (item != null && item.GetCard().player_id == player_id)
				{
					num++;
				}
			}
			return num;
		}

		public static BoardCard GetNearestPlayer(Vector3 pos, int skip_player_id, BoardCard skip, float range = 2f)
		{
			BoardCard result = null;
			float num = range;
			foreach (BoardCard item in card_list)
			{
				float magnitude = (item.transform.position - pos).magnitude;
				if (magnitude < num && item != skip && skip_player_id != item.GetCard().player_id)
				{
					num = magnitude;
					result = item;
				}
			}
			return result;
		}

		public static BoardCard GetNearest(Vector3 pos, BoardCard skip, float range = 2f)
		{
			BoardCard result = null;
			float num = range;
			foreach (BoardCard item in card_list)
			{
				float magnitude = (item.transform.position - pos).magnitude;
				if (magnitude < num && item != skip)
				{
					num = magnitude;
					result = item;
				}
			}
			return result;
		}

		public static BoardCard GetFocus()
		{
			foreach (BoardCard item in card_list)
			{
				if (item.IsFocus())
				{
					return item;
				}
			}
			return null;
		}

		public static void UnfocusAll()
		{
			foreach (BoardCard item in card_list)
			{
				item.focus = false;
				item.status_alpha_target = 0f;
			}
		}

		public static BoardCard Get(string uid)
		{
			foreach (BoardCard item in card_list)
			{
				if (item.card_uid == uid)
				{
					return item;
				}
			}
			return null;
		}

		public static List<BoardCard> GetAll()
		{
			return card_list;
		}
	}
}
