using System.Collections.Generic;
using UnityEngine;

namespace TcgEngine.Client
{
	public class HandCardArea : MonoBehaviour
	{
		public GameObject card_prefab;

		public RectTransform card_area;

		public float card_spacing = 100f;

		public float card_angle = 10f;

		public float card_offset_y = 10f;

		private List<HandCard> cards = new List<HandCard>();

		private bool is_dragging;

		private string last_destroyed;

		private float last_destroyed_timer;

		private static HandCardArea _instance;

		private void Awake()
		{
			_instance = this;
		}

		private void Update()
		{
			if (!GameClient.Get().IsReady())
			{
				return;
			}
			int playerID = GameClient.Get().GetPlayerID();
			Player player = GameClient.Get().GetGameData().GetPlayer(playerID);
			last_destroyed_timer += Time.deltaTime;
			foreach (Card item in player.cards_hand)
			{
				if (!HasCard(item.uid))
				{
					SpawnNewCard(item);
				}
			}
			for (int num = cards.Count - 1; num >= 0; num--)
			{
				HandCard handCard = cards[num];
				if (handCard == null || player.GetHandCard(handCard.GetCard().uid) == null)
				{
					cards.RemoveAt(num);
					if (handCard != null)
					{
						handCard.Kill();
					}
				}
			}
			int num2 = 0;
			float num3 = (float)cards.Count / 2f;
			foreach (HandCard card in cards)
			{
				card.deck_position = new Vector2(((float)num2 - num3) * card_spacing, ((float)num2 - num3) * ((float)num2 - num3) * (0f - card_offset_y));
				card.deck_angle = ((float)num2 - num3) * (0f - card_angle);
				num2++;
			}
			HandCard drag = HandCard.GetDrag();
			is_dragging = drag != null;
		}

		public void SpawnNewCard(Card card)
		{
			GameObject gameObject = Object.Instantiate(card_prefab, card_area.transform);
			gameObject.GetComponent<HandCard>().SetCard(card);
			gameObject.GetComponent<RectTransform>().anchoredPosition = new Vector2(0f, -100f);
			cards.Add(gameObject.GetComponent<HandCard>());
		}

		public void DelayRefresh(Card card)
		{
			last_destroyed_timer = 0f;
			last_destroyed = card.uid;
		}

		public void SortCards()
		{
			cards.Sort(SortFunc);
			int num = 0;
			foreach (HandCard card in cards)
			{
				card.transform.SetSiblingIndex(num);
				num++;
			}
		}

		private int SortFunc(HandCard a, HandCard b)
		{
			return a.transform.position.x.CompareTo(b.transform.position.x);
		}

		public bool HasCard(string card_uid)
		{
			HandCard handCard = HandCard.Get(card_uid);
			bool flag = card_uid == last_destroyed && last_destroyed_timer < 0.7f;
			return handCard != null || flag;
		}

		public bool IsDragging()
		{
			return is_dragging;
		}

		public static HandCardArea Get()
		{
			return _instance;
		}
	}
}
