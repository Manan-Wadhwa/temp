using System.Collections.Generic;
using TcgEngine.UI;
using UnityEngine;

namespace TcgEngine.Client
{
	public class BoardSlot : BSlot
	{
		public BoardSlotType type;

		public int x;

		public int y;

		private static List<BoardSlot> slot_list = new List<BoardSlot>();

		protected override void Awake()
		{
			base.Awake();
			slot_list.Add(this);
		}

		protected override void OnDestroy()
		{
			base.OnDestroy();
			slot_list.Remove(this);
		}

		private void Start()
		{
			if (x < Slot.x_min || x > Slot.x_max || y < Slot.y_min || y > Slot.y_max)
			{
				Debug.LogError("Board Slot X and Y value must be within the min and max set for those values, check Slot.cs script to change those min/max.");
			}
		}

		protected override void Update()
		{
			base.Update();
			if (!GameClient.Get().IsReady())
			{
				return;
			}
			BoardCard selected = PlayerControls.Get().GetSelected();
			HandCard drag = HandCard.GetDrag();
			Game gameData = GameClient.Get().GetGameData();
			Player player = GameClient.Get().GetPlayer();
			Slot slot = GetSlot();
			Card card = drag?.GetCard();
			Card slotCard = gameData.GetSlotCard(GetSlot());
			bool num = GameClient.Get().IsYourTurn();
			collide.enabled = slotCard == null;
			target_alpha = 0f;
			if (num && card != null && card.CardData.IsBoardCard() && gameData.CanPlayCard(card, slot))
			{
				target_alpha = 1f;
			}
			if (num && card != null && card.CardData.IsRequireTarget() && gameData.CanPlayCard(card, slot))
			{
				target_alpha = 1f;
			}
			if (gameData.selector == SelectorType.SelectTarget && player.player_id == gameData.selector_player_id)
			{
				Card card2 = gameData.GetCard(gameData.selector_caster_uid);
				AbilityData abilityData = AbilityData.Get(gameData.selector_ability_id);
				if (abilityData != null && slotCard == null && abilityData.CanTarget(gameData, card2, slot))
				{
					target_alpha = 1f;
				}
				if (abilityData != null && slotCard != null && abilityData.CanTarget(gameData, card2, slotCard))
				{
					target_alpha = 1f;
				}
			}
			Card card3 = selected?.GetCard();
			bool flag = num && card3 != null && slotCard == null && gameData.CanMoveCard(card3, slot);
			if ((num && card3 != null && slotCard != null && gameData.CanAttackTarget(card3, slotCard)) || flag)
			{
				target_alpha = 1f;
			}
		}

		public override Slot GetSlot()
		{
			int pid = 0;
			if (type == BoardSlotType.FlipX)
			{
				int playerID = GameClient.Get().GetPlayerID();
				int num = x;
				if (playerID % 2 == 1)
				{
					num = Slot.x_max - x + Slot.x_min;
				}
				return new Slot(num, y, pid);
			}
			if (type == BoardSlotType.FlipY)
			{
				int playerID2 = GameClient.Get().GetPlayerID();
				int num2 = y;
				if (playerID2 % 2 == 1)
				{
					num2 = Slot.y_max - y + Slot.y_min;
				}
				return new Slot(x, num2, pid);
			}
			if (type == BoardSlotType.PlayerSelf)
			{
				pid = GameClient.Get().GetPlayerID();
			}
			if (type == BoardSlotType.PlayerOpponent)
			{
				pid = GameClient.Get().GetOpponentPlayerID();
			}
			return new Slot(x, y, pid);
		}

		public void OnMouseDown()
		{
			if (GameUI.IsOverUI())
			{
				return;
			}
			Game gameData = GameClient.Get().GetGameData();
			int playerID = GameClient.Get().GetPlayerID();
			if (gameData.selector == SelectorType.SelectTarget && playerID == gameData.selector_player_id)
			{
				Slot slot = GetSlot();
				if (gameData.GetSlotCard(slot) == null)
				{
					GameClient.Get().SelectSlot(slot);
				}
			}
		}
	}
}
