using TcgEngine.Client;
using UnityEngine;
using UnityEngine.UI;

namespace TcgEngine.UI
{
	public class CardPreviewUI : MonoBehaviour
	{
		public UIPanel ui_panel;

		public CardUI card_ui;

		public Text desc;

		public float hover_delay_board = 0.7f;

		public float hover_delay_hand = 0.4f;

		public float hover_delay_mobile = 0.1f;

		public RectTransform[] side_rows;

		public StatusLine[] status_lines;

		private float preview_timer;

		private Vector2[] start_pos;

		private void Start()
		{
			start_pos = new Vector2[side_rows.Length];
			for (int i = 0; i < side_rows.Length; i++)
			{
				start_pos[i] = side_rows[i].anchoredPosition;
			}
		}

		private void Update()
		{
			if (!GameClient.Get().IsReady())
			{
				return;
			}
			StatusLine[] array = status_lines;
			for (int i = 0; i < array.Length; i++)
			{
				array[i].Hide();
			}
			PlayerControls.Get();
			HandCard focus = HandCard.GetFocus();
			BoardCard focus2 = BoardCard.GetFocus();
			HeroUI focus3 = HeroUI.GetFocus();
			Card hoverCard = TurnHistoryLine.GetHoverCard();
			float num = ((focus != null) ? hover_delay_hand : hover_delay_board);
			if (GameTool.IsMobile())
			{
				num = hover_delay_mobile;
			}
			Card card = ((!(focus != null)) ? focus2?.GetFocusCard() : focus?.GetCard());
			if (card == null)
			{
				card = hoverCard;
			}
			if (card == null)
			{
				card = focus3?.GetCard();
			}
			int num2;
			if (!Input.GetMouseButton(0) && !HandCardArea.Get().IsDragging() && !GameUI.IsUIOpened())
			{
				num2 = ((card != null) ? 1 : 0);
				if (num2 != 0)
				{
					preview_timer += Time.deltaTime;
					goto IL_0101;
				}
			}
			else
			{
				num2 = 0;
			}
			preview_timer = 0f;
			goto IL_0101;
			IL_0101:
			bool flag = num2 != 0 && preview_timer >= num;
			ui_panel.SetVisible(flag);
			if (!flag)
			{
				return;
			}
			CardData cardData = card.CardData;
			card_ui.SetCard(cardData, card.VariantData);
			string text = cardData.GetDesc();
			string abilitiesDesc = cardData.GetAbilitiesDesc();
			if (!string.IsNullOrWhiteSpace(text))
			{
				desc.text = text + "\n\n" + abilitiesDesc;
			}
			else
			{
				desc.text = abilitiesDesc;
			}
			int num3 = 0;
			foreach (AbilityData ability in card.GetAbilities())
			{
				if (num3 < status_lines.Length && !card.CardData.HasAbility(ability) && !string.IsNullOrWhiteSpace(ability.desc))
				{
					status_lines[num3].SetLine(card.CardData, ability);
					num3++;
				}
			}
			foreach (CardStatus item in card.GetAllStatus())
			{
				if (num3 < status_lines.Length)
				{
					StatusData statusData = StatusData.Get(item.type);
					if (statusData != null && !string.IsNullOrWhiteSpace(statusData.desc))
					{
						int value = Mathf.Max(item.value, Mathf.CeilToInt((float)item.duration / 2f));
						status_lines[num3].SetLine(statusData, value);
						num3++;
					}
				}
			}
		}
	}
}
