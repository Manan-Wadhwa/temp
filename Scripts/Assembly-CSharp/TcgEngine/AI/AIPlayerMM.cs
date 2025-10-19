using System.Collections;
using TcgEngine.Gameplay;
using UnityEngine;

namespace TcgEngine.AI
{
	public class AIPlayerMM : AIPlayer
	{
		private AILogic ai_logic;

		private bool is_playing;

		public AIPlayerMM(GameLogic gameplay, int id, int level)
		{
			base.gameplay = gameplay;
			player_id = id;
			ai_level = Mathf.Clamp(level, 1, 10);
			ai_logic = AILogic.Create(id, ai_level);
		}

		public override void Update()
		{
			Game gameData = gameplay.GetGameData();
			Player player = gameData.GetPlayer(player_id);
			if (!is_playing && CanPlay())
			{
				is_playing = true;
				TimeTool.StartCoroutine(AiTurn());
			}
			if (!gameData.IsPlayerTurn(player) && ai_logic.IsRunning())
			{
				Stop();
			}
		}

		private IEnumerator AiTurn()
		{
			yield return new WaitForSeconds(1f);
			Game game_data = gameplay.GetGameData();
			ai_logic.RunAI(game_data);
			while (ai_logic.IsRunning())
			{
				yield return new WaitForSeconds(0.1f);
			}
			AIAction bestAction = ai_logic.GetBestAction();
			if (bestAction != null)
			{
				Debug.Log("Execute AI Action: " + bestAction.GetText(game_data) + "\n" + ai_logic.GetNodePath());
				ExecuteAction(bestAction);
			}
			ai_logic.ClearMemory();
			yield return new WaitForSeconds(0.5f);
			is_playing = false;
		}

		private void Stop()
		{
			ai_logic.Stop();
			is_playing = false;
		}

		private void ExecuteAction(AIAction action)
		{
			if (CanPlay())
			{
				if (action.type == 1000)
				{
					PlayCard(action.card_uid, action.slot);
				}
				if (action.type == 1010)
				{
					AttackCard(action.card_uid, action.target_uid);
				}
				if (action.type == 1012)
				{
					AttackPlayer(action.card_uid, action.target_player_id);
				}
				if (action.type == 1015)
				{
					MoveCard(action.card_uid, action.slot);
				}
				if (action.type == 1020)
				{
					CastAbility(action.card_uid, action.ability_id);
				}
				if (action.type == 1030)
				{
					SelectCard(action.target_uid);
				}
				if (action.type == 1032)
				{
					SelectPlayer(action.target_player_id);
				}
				if (action.type == 1034)
				{
					SelectSlot(action.slot);
				}
				if (action.type == 1036)
				{
					SelectChoice(action.value);
				}
				if (action.type == 1039)
				{
					CancelSelect();
				}
				if (action.type == 1040)
				{
					EndTurn();
				}
				if (action.type == 1050)
				{
					Resign();
				}
			}
		}

		private void PlayCard(string card_uid, Slot slot)
		{
			Card card = gameplay.GetGameData().GetCard(card_uid);
			if (card != null)
			{
				gameplay.PlayCard(card, slot);
			}
		}

		private void MoveCard(string card_uid, Slot slot)
		{
			Card card = gameplay.GetGameData().GetCard(card_uid);
			if (card != null)
			{
				gameplay.MoveCard(card, slot);
			}
		}

		private void AttackCard(string attacker_uid, string target_uid)
		{
			Game gameData = gameplay.GetGameData();
			Card card = gameData.GetCard(attacker_uid);
			Card card2 = gameData.GetCard(target_uid);
			if (card != null && card2 != null)
			{
				gameplay.AttackTarget(card, card2);
			}
		}

		private void AttackPlayer(string attacker_uid, int target_player_id)
		{
			Game gameData = gameplay.GetGameData();
			Card card = gameData.GetCard(attacker_uid);
			if (card != null)
			{
				Player player = gameData.GetPlayer(target_player_id);
				gameplay.AttackPlayer(card, player);
			}
		}

		private void CastAbility(string caster_uid, string ability_id)
		{
			Card card = gameplay.GetGameData().GetCard(caster_uid);
			AbilityData abilityData = AbilityData.Get(ability_id);
			if (card != null && abilityData != null)
			{
				gameplay.CastAbility(card, abilityData);
			}
		}

		private void SelectCard(string target_uid)
		{
			Card card = gameplay.GetGameData().GetCard(target_uid);
			if (card != null)
			{
				gameplay.SelectCard(card);
			}
		}

		private void SelectPlayer(int tplayer_id)
		{
			Player player = gameplay.GetGameData().GetPlayer(tplayer_id);
			if (player != null)
			{
				gameplay.SelectPlayer(player);
			}
		}

		private void SelectSlot(Slot slot)
		{
			if (slot != Slot.None)
			{
				gameplay.SelectSlot(slot);
			}
		}

		private void SelectChoice(int choice)
		{
			gameplay.SelectChoice(choice);
		}

		private void CancelSelect()
		{
			if (CanPlay())
			{
				gameplay.CancelSelection();
			}
		}

		private void EndTurn()
		{
			if (CanPlay())
			{
				gameplay.EndTurn();
			}
		}

		private void Resign()
		{
			int winner = ((player_id == 0) ? 1 : 0);
			gameplay.EndGame(winner);
		}
	}
}
