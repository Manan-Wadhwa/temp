using System;
using System.Collections.Generic;
using TcgEngine.UI;
using UnityEngine.Events;

namespace TcgEngine.Client
{
	public class BoardSlotPlayer : BSlot
	{
		public bool opponent;

		public float range_x = 3f;

		public float range_y = 1f;

		private static BoardSlotPlayer instance_self;

		private static BoardSlotPlayer instance_other;

		private static List<BoardSlotPlayer> zone_list = new List<BoardSlotPlayer>();

		protected override void Awake()
		{
			base.Awake();
			zone_list.Add(this);
			if (opponent)
			{
				instance_other = this;
			}
			else
			{
				instance_self = this;
			}
		}

		protected override void OnDestroy()
		{
			base.OnDestroy();
			zone_list.Remove(this);
		}

		private void Start()
		{
			GameClient gameClient = GameClient.Get();
			gameClient.onAbilityTargetPlayer = (UnityAction<AbilityData, Card, Player>)Delegate.Combine(gameClient.onAbilityTargetPlayer, new UnityAction<AbilityData, Card, Player>(OnAbilityEffect));
		}

		protected override void Update()
		{
			base.Update();
			if (!GameClient.Get().IsReady() || !opponent)
			{
				return;
			}
			BoardCard selected = PlayerControls.Get().GetSelected();
			HandCard drag = HandCard.GetDrag();
			bool flag = GameClient.Get().IsYourTurn();
			Game gameData = GameClient.Get().GetGameData();
			Player player = GameClient.Get().GetPlayer();
			Player opponentPlayer = GameClient.Get().GetOpponentPlayer();
			target_alpha = 0f;
			Card card = selected?.GetCard();
			if (card != null)
			{
				bool num = gameData.IsPlayerActionTurn(player) && card.CanAttack();
				bool flag2 = gameData.CanAttackTarget(card, opponentPlayer);
				if (num && flag2)
				{
					target_alpha = 1f;
				}
			}
			if (flag && drag != null && drag.CardData.IsRequireTargetSpell() && gameData.IsPlayTargetValid(drag.GetCard(), GetPlayer()))
			{
				target_alpha = 1f;
			}
			if (gameData.selector == SelectorType.SelectTarget && player.player_id == gameData.selector_player_id)
			{
				Card card2 = gameData.GetCard(gameData.selector_caster_uid);
				AbilityData abilityData = AbilityData.Get(gameData.selector_ability_id);
				if (abilityData != null && abilityData.AreTargetConditionsMet(gameData, card2, GetPlayer()))
				{
					target_alpha = 1f;
				}
			}
		}

		private void OnAbilityEffect(AbilityData iability, Card caster, Player target)
		{
			if (iability != null && caster != null && target != null)
			{
				int num = (opponent ? GameClient.Get().GetOpponentPlayerID() : GameClient.Get().GetPlayerID());
				if (target.player_id == num)
				{
					FXTool.DoFX(iability.target_fx, base.transform.position);
					AudioTool.Get().PlaySFX("fx", iability.target_audio);
				}
			}
		}

		public void OnMouseDown()
		{
			if (!GameUI.IsUIOpened() && !GameUI.IsOverUILayer("UI"))
			{
				Game gameData = GameClient.Get().GetGameData();
				int playerID = GameClient.Get().GetPlayerID();
				if (gameData.selector == SelectorType.SelectTarget && playerID == gameData.selector_player_id)
				{
					GameClient.Get().SelectPlayer(GetPlayer());
				}
			}
		}

		public int GetPlayerID()
		{
			if (!opponent)
			{
				return GameClient.Get().GetPlayerID();
			}
			return GameClient.Get().GetOpponentPlayerID();
		}

		public override Player GetPlayer()
		{
			if (!opponent)
			{
				return GameClient.Get().GetPlayer();
			}
			return GameClient.Get().GetOpponentPlayer();
		}

		public override Slot GetSlot()
		{
			return new Slot(GetPlayerID());
		}

		public static BoardSlotPlayer Get(bool opponent)
		{
			if (opponent)
			{
				return instance_other;
			}
			return instance_self;
		}
	}
}
