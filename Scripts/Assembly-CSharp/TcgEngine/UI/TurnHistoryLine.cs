using System.Collections.Generic;
using TcgEngine.Client;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace TcgEngine.UI
{
	public class TurnHistoryLine : MonoBehaviour, IPointerEnterHandler, IEventSystemHandler, IPointerExitHandler
	{
		public HoverTargetUI hover;

		public Image card_img;

		private Card card;

		private float timer;

		private bool is_hover;

		private static List<TurnHistoryLine> line_list = new List<TurnHistoryLine>();

		private void Awake()
		{
			line_list.Add(this);
		}

		private void OnDestroy()
		{
			line_list.Add(this);
		}

		private void Start()
		{
			base.gameObject.SetActive(value: false);
		}

		private void Update()
		{
			timer += Time.deltaTime;
		}

		public void SetLine(ActionHistory history)
		{
			Game gameData = GameClient.Get().GetGameData();
			Card card = gameData.GetCard(history.card_uid);
			Card card2 = gameData.GetCard(history.target_uid);
			Player player = gameData.GetPlayer(history.target_id);
			CardData cardData = CardData.Get(history.card_id);
			CardData cardData2 = CardData.Get(card2?.card_id);
			VariantData variantData = card.VariantData;
			AbilityData abilityData = AbilityData.Get(history.ability_id);
			this.card = card;
			if (cardData == null)
			{
				return;
			}
			if (history.type == 1000)
			{
				string text = cardData.title + " was played";
				SetLine(cardData, variantData, text);
			}
			if (history.type == 1015)
			{
				string text2 = cardData.title + " moved";
				SetLine(cardData, variantData, text2);
			}
			if (history.type == 1010 && cardData2 != null)
			{
				string text3 = cardData.title + " attacked " + cardData2.title;
				SetLine(cardData, variantData, text3);
			}
			if (history.type == 1012 && player != null)
			{
				string text4 = cardData.title + " attacked " + player.username;
				SetLine(cardData, variantData, text4);
			}
			if (history.type == 1020 && abilityData != null)
			{
				if (abilityData.target == AbilityTarget.SelectTarget && cardData2 != null)
				{
					string text5 = cardData.title + " casted " + abilityData.GetTitle() + " on " + cardData2.title;
					SetLine(cardData, variantData, text5);
				}
				else
				{
					string text6 = cardData.title + " casted " + abilityData.GetTitle();
					SetLine(cardData, variantData, text6);
				}
			}
			if (history.type == 2060)
			{
				string text7 = cardData.title + " was triggered";
				SetLine(cardData, variantData, text7);
			}
		}

		public void SetLine(CardData icard, VariantData variant, string text)
		{
			card_img.sprite = icard.GetFullArt(variant);
			hover.text = text;
			base.gameObject.SetActive(value: true);
			timer = 0f;
		}

		public void Hide()
		{
			card = null;
			if (timer > 0.05f)
			{
				base.gameObject.SetActive(value: false);
			}
		}

		public void OnPointerEnter(PointerEventData eventData)
		{
			timer = 0f;
			is_hover = true;
		}

		public void OnPointerExit(PointerEventData eventData)
		{
			timer = 0f;
			is_hover = false;
		}

		private void OnDisable()
		{
			is_hover = false;
		}

		public static Card GetHoverCard()
		{
			foreach (TurnHistoryLine item in line_list)
			{
				if (item.card != null && item.is_hover)
				{
					return item.card;
				}
			}
			return null;
		}
	}
}
