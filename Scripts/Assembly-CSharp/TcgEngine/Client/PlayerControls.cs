using TcgEngine.UI;
using UnityEngine;

namespace TcgEngine.Client
{
	public class PlayerControls : MonoBehaviour
	{
		private BoardCard selected_card;

		private static PlayerControls instance;

		private void Awake()
		{
			instance = this;
		}

		private void Update()
		{
			if (GameClient.Get().IsReady())
			{
				if (Input.GetMouseButtonDown(1))
				{
					UnselectAll();
				}
				if (selected_card != null && Input.GetMouseButtonUp(0))
				{
					ReleaseClick();
				}
			}
		}

		public void SelectCard(BoardCard bcard)
		{
			Game gameData = GameClient.Get().GetGameData();
			Player player = GameClient.Get().GetPlayer();
			Card focusCard = bcard.GetFocusCard();
			if (gameData.IsPlayerSelectorTurn(player) && gameData.selector == SelectorType.SelectTarget)
			{
				GameClient.Get().SelectCard(focusCard);
			}
			else if (gameData.IsPlayerActionTurn(player) && focusCard.player_id == player.player_id)
			{
				selected_card = bcard;
			}
		}

		public void SelectCardRight(BoardCard card)
		{
			Input.GetMouseButton(0);
		}

		private void ReleaseClick()
		{
			if (GameClient.Get().IsYourTurn() && selected_card != null)
			{
				Card card = selected_card.GetCard();
				Vector3 vector = GameBoard.Get().RaycastMouseBoard();
				BSlot nearest = BSlot.GetNearest(vector);
				Card card2 = nearest?.GetSlotCard(vector);
				AbilityButton focus = AbilityButton.GetFocus(vector, 1f);
				if (focus != null && focus.IsVisible())
				{
					GameClient.Get().CastAbility(card, focus.GetAbility());
				}
				else if (nearest is BoardSlotPlayer)
				{
					if (card.exhausted)
					{
						WarningText.ShowExhausted();
					}
					else
					{
						GameClient.Get().AttackPlayer(card, nearest.GetPlayer());
					}
				}
				else if (card2 != null && card2.uid != card.uid && card2.player_id != card.player_id)
				{
					if (card.exhausted)
					{
						WarningText.ShowExhausted();
					}
					else
					{
						GameClient.Get().AttackTarget(card, card2);
					}
				}
				else if (nearest != null && nearest is BoardSlot)
				{
					GameClient.Get().Move(card, nearest.GetSlot());
				}
			}
			UnselectAll();
		}

		public void UnselectAll()
		{
			selected_card = null;
		}

		public BoardCard GetSelected()
		{
			return selected_card;
		}

		public static PlayerControls Get()
		{
			return instance;
		}
	}
}
