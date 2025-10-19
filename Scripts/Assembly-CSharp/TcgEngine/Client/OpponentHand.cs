using System.Collections.Generic;
using UnityEngine;

namespace TcgEngine.Client
{
	public class OpponentHand : MonoBehaviour
	{
		public GameObject card_prefab;

		public RectTransform card_area;

		public float card_spacing = 100f;

		public float card_angle = 10f;

		public float card_offset_y = 10f;

		private List<HandCardBack> cards = new List<HandCardBack>();

		private void Start()
		{
		}

		private void Update()
		{
			if (GameClient.Get().IsReady())
			{
				Player player = GameClient.Get().GetGameData().GetPlayer(GameClient.Get().GetOpponentPlayerID());
				if (cards.Count < player.cards_hand.Count)
				{
					GameObject obj = Object.Instantiate(card_prefab, card_area);
					HandCardBack component = obj.GetComponent<HandCardBack>();
					CardbackData cardback = CardbackData.Get(player.cardback);
					component.SetCardback(cardback);
					obj.GetComponent<RectTransform>().anchoredPosition = new Vector2(0f, 100f);
					cards.Add(component);
				}
				if (cards.Count > player.cards_hand.Count)
				{
					HandCardBack handCardBack = cards[cards.Count - 1];
					cards.RemoveAt(cards.Count - 1);
					Object.Destroy(handCardBack.gameObject);
				}
				int num = Mathf.Min(cards.Count, player.cards_hand.Count);
				for (int i = 0; i < num; i++)
				{
					HandCardBack handCardBack2 = cards[i];
					RectTransform rect = handCardBack2.GetRect();
					float num2 = (float)num / 2f;
					Vector3 b = new Vector3(((float)i - num2) * card_spacing, ((float)i - num2) * ((float)i - num2) * card_offset_y);
					float z = ((float)i - num2) * card_angle;
					rect.anchoredPosition = Vector3.Lerp(rect.anchoredPosition, b, 4f * Time.deltaTime);
					handCardBack2.transform.localRotation = Quaternion.Slerp(handCardBack2.transform.localRotation, Quaternion.Euler(0f, 0f, z), 4f * Time.deltaTime);
				}
			}
		}
	}
}
