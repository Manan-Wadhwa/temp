using TcgEngine.UI;
using UnityEngine;
using UnityEngine.UI;

namespace TcgEngine.Client
{
	public class BoardDeck : MonoBehaviour
	{
		public bool opponent;

		public UIPanel hover_panel;

		public SpriteRenderer deck_render;

		public Text deck_value;

		public Text discard_value;

		private bool hover;

		private void Start()
		{
			if (GameTool.IsMobile())
			{
				hover_panel?.SetVisible(visi: true);
			}
		}

		private void Update()
		{
			Refresh();
		}

		private void Refresh()
		{
			if (!GameClient.Get().IsReady())
			{
				return;
			}
			Player player = (opponent ? GameClient.Get().GetOpponentPlayer() : GameClient.Get().GetPlayer());
			if (player != null)
			{
				CardbackData cardbackData = CardbackData.Get(player.cardback);
				if (deck_render != null && cardbackData != null)
				{
					deck_render.sprite = cardbackData.deck;
				}
				if (deck_value != null)
				{
					deck_value.text = player.cards_deck.Count.ToString();
				}
				if (discard_value != null)
				{
					discard_value.text = player.cards_discard.Count.ToString();
				}
			}
		}

		public void ShowDeckCards()
		{
			Player player = GameClient.Get().GetPlayer();
			CardSelector.Get().Show(player.cards_deck, "DECK");
		}

		public void ShowDiscardCards()
		{
			Player player = (opponent ? GameClient.Get().GetOpponentPlayer() : GameClient.Get().GetPlayer());
			CardSelector.Get().Show(player.cards_discard, "DISCARD");
		}

		private void ShowHover(bool hover)
		{
			if (!GameTool.IsMobile())
			{
				hover_panel?.SetVisible(hover);
			}
		}

		private void OnMouseEnter()
		{
			hover = true;
			ShowHover(hover);
			Refresh();
		}

		private void OnMouseExit()
		{
			hover = false;
			ShowHover(hover);
		}

		private void OnDisable()
		{
			hover = false;
			ShowHover(hover);
		}

		private void OnMouseOver()
		{
			if (!opponent && Input.GetMouseButtonDown(0))
			{
				ShowDeckCards();
			}
			else if (Input.GetMouseButtonDown(1))
			{
				ShowDiscardCards();
			}
		}
	}
}
