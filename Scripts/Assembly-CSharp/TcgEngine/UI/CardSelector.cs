using System.Collections.Generic;
using TcgEngine.Client;
using UnityEngine;
using UnityEngine.UI;

namespace TcgEngine.UI
{
	public class CardSelector : SelectorPanel
	{
		public GameObject card_prefab;

		public RectTransform content;

		public Text title;

		public Text subtitle;

		public Button select_button;

		public Text select_button_text;

		public float card_spacing = 100f;

		private AbilityData iability;

		private List<Card> card_list = new List<Card>();

		private List<CardSelectorCard> selector_list = new List<CardSelectorCard>();

		private Vector2 mouse_start;

		private int mouse_start_index;

		private int selection_index;

		private bool drag;

		private float mouse_scroll;

		private float timer;

		private static CardSelector instance;

		protected override void Awake()
		{
			base.Awake();
			instance = this;
			Hide();
		}

		protected override void Update()
		{
			base.Update();
			timer += Time.deltaTime;
			Vector2 vector = GetMouseRectPosition() - mouse_start;
			if (drag && vector.magnitude > 0.1f)
			{
				selection_index = mouse_start_index - Mathf.RoundToInt(vector.x / card_spacing);
				selection_index = Mathf.Clamp(selection_index, 0, selector_list.Count - 1);
			}
			mouse_scroll += 0f - Input.mouseScrollDelta.y;
			if (mouse_scroll > 0.5f)
			{
				OnClickNext();
				mouse_scroll -= 1f;
			}
			else if (mouse_scroll < -0.5f)
			{
				OnClickPrev();
				mouse_scroll += 1f;
			}
			foreach (CardSelectorCard item in selector_list)
			{
				bool num = item.GetIndex() == selection_index;
				Vector3 targetPos = GetCardPosition(item);
				Vector3 targetScale = (num ? Vector3.one : (Vector3.one / 2f));
				item.SetTargetPos(targetPos);
				item.SetTargetScale(targetScale);
			}
			if (iability == null && Input.GetMouseButtonDown(1) && timer > 1f)
			{
				Hide();
			}
			Game gameData = GameClient.Get().GetGameData();
			if (gameData != null && iability != null && gameData.selector == SelectorType.None)
			{
				Hide();
			}
		}

		public void RefreshPanel()
		{
			foreach (CardSelectorCard item in selector_list)
			{
				Object.Destroy(item.gameObject);
			}
			selector_list.Clear();
			drag = false;
			mouse_scroll = 0f;
			select_button_text.text = ((iability != null) ? "Select" : "OK");
			select_button.gameObject.SetActive(iability != null);
			int num = 0;
			foreach (Card item2 in card_list)
			{
				if (CardData.Get(item2.card_id) != null)
				{
					GameObject obj = Object.Instantiate(card_prefab, content.transform);
					RectTransform component = obj.GetComponent<RectTransform>();
					CardSelectorCard component2 = obj.GetComponent<CardSelectorCard>();
					component2.SetCard(item2);
					component2.SetIndex(num);
					Vector3 vector = GetCardPosition(component2);
					Vector3 targetScale = ((num == selection_index) ? 1f : 0.5f) * Vector3.one;
					component2.SetTargetPos(vector);
					component2.SetTargetScale(targetScale);
					component.anchoredPosition = vector;
					selector_list.Add(component2);
					num++;
				}
			}
		}

		public override void Show(AbilityData iability, Card caster)
		{
			Game gameData = GameClient.Get().GetGameData();
			card_list = iability.GetCardTargets(gameData, caster);
			this.iability = iability;
			title.text = iability.title;
			subtitle.text = iability.desc;
			selection_index = 0;
			timer = 0f;
			Show();
		}

		public void Show(List<Card> card_list, string title)
		{
			this.card_list.Clear();
			this.card_list.AddRange(card_list);
			this.card_list.Sort((Card a, Card b) => a.CardData.title.CompareTo(b.CardData.title));
			iability = null;
			this.title.text = title;
			subtitle.text = "";
			selection_index = 0;
			timer = 0f;
			Show();
		}

		public void OnClickOK()
		{
			Game gameData = GameClient.Get().GetGameData();
			if (iability != null && gameData.selector == SelectorType.SelectorCard)
			{
				CardSelectorCard cardSelectorCard = null;
				if (selection_index >= 0 && selection_index < selector_list.Count)
				{
					cardSelectorCard = selector_list[selection_index];
				}
				if (cardSelectorCard != null)
				{
					Card card = cardSelectorCard.GetCard();
					Card card2 = gameData.GetCard(gameData.selector_caster_uid);
					if (card != null && iability.AreTargetConditionsMet(gameData, card2, card))
					{
						GameClient.Get().SelectCard(card);
						Hide();
					}
				}
			}
			else
			{
				Hide();
			}
		}

		public void OnClickMouseDown()
		{
			mouse_start = GetMouseRectPosition();
			mouse_start_index = selection_index;
			drag = true;
		}

		public void OnClickMouseUp()
		{
			drag = false;
		}

		public void OnClickCancel()
		{
			GameClient.Get().CancelSelection();
			Hide();
		}

		public void OnClickNext()
		{
			selection_index++;
			selection_index = Mathf.Clamp(selection_index, 0, selector_list.Count - 1);
		}

		public void OnClickPrev()
		{
			selection_index--;
			selection_index = Mathf.Clamp(selection_index, 0, selector_list.Count - 1);
		}

		private Vector2 GetCardPosition(CardSelectorCard card)
		{
			int num = card.GetIndex() - selection_index;
			Vector2 vector = new Vector2((float)num * card_spacing, (num != 0) ? 50f : 0f);
			float num2 = ((num != 0) ? (Mathf.Sign(num) * 140f) : 0f);
			return vector + Vector2.right * num2;
		}

		private Vector2 GetMouseRectPosition()
		{
			RectTransformUtility.ScreenPointToLocalPointInRectangle(content, Input.mousePosition, GetComponentInParent<Canvas>().worldCamera, out var localPoint);
			return localPoint;
		}

		public bool IsAbility()
		{
			if (IsVisible())
			{
				return iability != null;
			}
			return false;
		}

		public override void Show(bool instant = false)
		{
			base.Show(instant);
			RefreshPanel();
		}

		public override bool ShouldShow()
		{
			Game gameData = GameClient.Get().GetGameData();
			int playerID = GameClient.Get().GetPlayerID();
			if (gameData.selector == SelectorType.SelectorCard)
			{
				return gameData.selector_player_id == playerID;
			}
			return false;
		}

		public static CardSelector Get()
		{
			return instance;
		}
	}
}
