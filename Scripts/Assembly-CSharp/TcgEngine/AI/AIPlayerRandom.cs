using System;
using System.Collections;
using System.Collections.Generic;
using TcgEngine.Gameplay;
using UnityEngine;

namespace TcgEngine.AI
{
	public class AIPlayerRandom : AIPlayer
	{
		private bool is_playing;

		private bool is_selecting;

		private System.Random rand = new System.Random();

		public AIPlayerRandom(GameLogic gameplay, int id, int level)
		{
			base.gameplay = gameplay;
			player_id = id;
		}

		public override void Update()
		{
			if (!CanPlay())
			{
				return;
			}
			Game gameData = gameplay.GetGameData();
			Player player = gameData.GetPlayer(player_id);
			if (!gameData.IsPlayerTurn(player) || gameplay.IsResolving())
			{
				return;
			}
			if (!is_playing && gameData.selector == SelectorType.None && gameData.current_player == player_id)
			{
				is_playing = true;
				TimeTool.StartCoroutine(AiTurn());
			}
			if (!is_selecting && gameData.selector != SelectorType.None && gameData.selector_player_id == player_id)
			{
				if (gameData.selector == SelectorType.SelectTarget)
				{
					is_selecting = true;
					TimeTool.StartCoroutine(AiSelectTarget());
				}
				if (gameData.selector == SelectorType.SelectorCard)
				{
					is_selecting = true;
					TimeTool.StartCoroutine(AiSelectCard());
				}
				if (gameData.selector == SelectorType.SelectorChoice)
				{
					is_selecting = true;
					TimeTool.StartCoroutine(AiSelectChoice());
				}
			}
		}

		private IEnumerator AiTurn()
		{
			yield return new WaitForSeconds(1f);
			PlayCard();
			yield return new WaitForSeconds(0.5f);
			PlayCard();
			yield return new WaitForSeconds(0.5f);
			PlayCard();
			yield return new WaitForSeconds(0.5f);
			Attack();
			yield return new WaitForSeconds(0.5f);
			Attack();
			yield return new WaitForSeconds(0.5f);
			AttackPlayer();
			yield return new WaitForSeconds(0.5f);
			EndTurn();
			is_playing = false;
		}

		private IEnumerator AiSelectCard()
		{
			yield return new WaitForSeconds(0.5f);
			SelectCard();
			yield return new WaitForSeconds(0.5f);
			CancelSelect();
			is_selecting = false;
		}

		private IEnumerator AiSelectTarget()
		{
			yield return new WaitForSeconds(0.5f);
			SelectTarget();
			yield return new WaitForSeconds(0.5f);
			CancelSelect();
			is_selecting = false;
		}

		private IEnumerator AiSelectChoice()
		{
			yield return new WaitForSeconds(0.5f);
			SelectChoice();
			yield return new WaitForSeconds(0.5f);
			CancelSelect();
			is_selecting = false;
		}

		public void PlayCard()
		{
			if (!CanPlay())
			{
				return;
			}
			Game gameData = gameplay.GetGameData();
			Player player = gameData.GetPlayer(player_id);
			if (player.cards_hand.Count > 0 && gameData.IsPlayerActionTurn(player))
			{
				Card randomCard = player.GetRandomCard(player.cards_hand, rand);
				Slot slot = player.GetRandomEmptySlot(rand);
				if (randomCard != null && randomCard.CardData.IsRequireTargetSpell())
				{
					slot = gameData.GetRandomSlot(rand);
				}
				if (randomCard != null && randomCard.CardData.IsEquipment())
				{
					slot = player.GetRandomOccupiedSlot(rand);
				}
				if (randomCard != null)
				{
					gameplay.PlayCard(randomCard, slot);
				}
			}
		}

		public void Attack()
		{
			if (!CanPlay())
			{
				return;
			}
			Game gameData = gameplay.GetGameData();
			Player player = gameData.GetPlayer(player_id);
			if (player.cards_board.Count > 0 && gameData.IsPlayerActionTurn(player))
			{
				Card randomCard = player.GetRandomCard(player.cards_board, rand);
				Card randomBoardCard = gameData.GetRandomBoardCard(rand);
				if (randomCard != null && randomBoardCard != null)
				{
					gameplay.AttackTarget(randomCard, randomBoardCard);
				}
			}
		}

		public void AttackPlayer()
		{
			if (!CanPlay())
			{
				return;
			}
			Game gameData = gameplay.GetGameData();
			Player player = gameData.GetPlayer(player_id);
			Player randomPlayer = gameData.GetRandomPlayer(rand);
			if (player.cards_board.Count > 0 && gameData.IsPlayerActionTurn(player))
			{
				Card randomCard = player.GetRandomCard(player.cards_board, rand);
				if (randomCard != null && randomPlayer != null && randomPlayer != player)
				{
					gameplay.AttackPlayer(randomCard, randomPlayer);
				}
			}
		}

		public void SelectCard()
		{
			if (!CanPlay())
			{
				return;
			}
			Game gameData = gameplay.GetGameData();
			Player player = gameData.GetPlayer(player_id);
			AbilityData abilityData = AbilityData.Get(gameData.selector_ability_id);
			Card card = gameData.GetCard(gameData.selector_caster_uid);
			if (player != null && abilityData != null && card != null)
			{
				List<Card> cardTargets = abilityData.GetCardTargets(gameData, card);
				if (cardTargets.Count > 0)
				{
					Card target = cardTargets[rand.Next(0, cardTargets.Count)];
					gameplay.SelectCard(target);
				}
			}
		}

		public void SelectTarget()
		{
			if (!CanPlay())
			{
				return;
			}
			Game gameData = gameplay.GetGameData();
			if (gameData.selector == SelectorType.None)
			{
				return;
			}
			int id = player_id;
			AbilityData abilityData = AbilityData.Get(gameData.selector_ability_id);
			if (abilityData != null && abilityData.target == AbilityTarget.SelectTarget)
			{
				id = ((player_id == 0) ? 1 : 0);
			}
			Player player = gameData.GetPlayer(id);
			if (player.cards_board.Count > 0)
			{
				Card randomCard = player.GetRandomCard(player.cards_board, rand);
				if (randomCard != null)
				{
					gameplay.SelectCard(randomCard);
				}
			}
		}

		public void SelectChoice()
		{
			if (!CanPlay())
			{
				return;
			}
			Game gameData = gameplay.GetGameData();
			if (gameData.selector != SelectorType.None)
			{
				AbilityData abilityData = AbilityData.Get(gameData.selector_ability_id);
				if (abilityData != null && abilityData.chain_abilities.Length != 0)
				{
					int choice = rand.Next(0, abilityData.chain_abilities.Length);
					gameplay.SelectChoice(choice);
				}
			}
		}

		public void CancelSelect()
		{
			if (CanPlay())
			{
				gameplay.CancelSelection();
			}
		}

		public void EndTurn()
		{
			if (CanPlay())
			{
				gameplay.EndTurn();
			}
		}
	}
}
