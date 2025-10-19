using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading;
using TcgEngine.Gameplay;
using UnityEngine;

namespace TcgEngine.AI
{
	public class AILogic
	{
		public int ai_depth = 3;

		public int ai_depth_wide = 1;

		public int actions_per_turn = 2;

		public int actions_per_turn_wide = 3;

		public int nodes_per_action = 4;

		public int nodes_per_action_wide = 7;

		public int ai_player_id;

		public int ai_level;

		private GameLogic game_logic;

		private Game original_data;

		private AIHeuristic heuristic;

		private Thread ai_thread;

		private NodeState first_node;

		private NodeState best_move;

		private bool running;

		private int nb_calculated;

		private int reached_depth;

		private System.Random random_gen;

		private Pool<NodeState> node_pool = new Pool<NodeState>();

		private Pool<Game> data_pool = new Pool<Game>();

		private Pool<AIAction> action_pool = new Pool<AIAction>();

		private Pool<List<AIAction>> list_pool = new Pool<List<AIAction>>();

		private ListSwap<Card> card_array = new ListSwap<Card>();

		private ListSwap<Slot> slot_array = new ListSwap<Slot>();

		public static AILogic Create(int player_id, int level)
		{
			return new AILogic
			{
				ai_player_id = player_id,
				ai_level = level,
				heuristic = new AIHeuristic(player_id, level),
				game_logic = new GameLogic(is_ai: true)
			};
		}

		public void RunAI(Game data)
		{
			if (!running)
			{
				original_data = Game.CloneNew(data);
				game_logic.ClearResolve();
				game_logic.SetData(original_data);
				random_gen = new System.Random();
				first_node = null;
				reached_depth = 0;
				nb_calculated = 0;
				running = true;
				ai_thread = new Thread(Execute);
				ai_thread.Start();
			}
		}

		public void Stop()
		{
			running = false;
			if (ai_thread != null && ai_thread.IsAlive)
			{
				ai_thread.Abort();
			}
		}

		private void Execute()
		{
			first_node = CreateNode(null, null, ai_player_id, 0, 0);
			first_node.hvalue = heuristic.CalculateHeuristic(original_data, first_node);
			first_node.alpha = int.MinValue;
			first_node.beta = int.MaxValue;
			Stopwatch stopwatch = Stopwatch.StartNew();
			CalculateNode(original_data, first_node);
			UnityEngine.Debug.Log("AI: Time " + stopwatch.ElapsedMilliseconds + "ms Depth " + reached_depth + " Nodes " + nb_calculated);
			best_move = first_node.best_child;
			running = false;
		}

		private void CalculateNode(Game data, NodeState node)
		{
			Player player = data.GetPlayer(data.current_player);
			List<AIAction> list = list_pool.Create();
			int num = ((node.tdepth < ai_depth_wide) ? actions_per_turn_wide : actions_per_turn);
			if (node.taction < num)
			{
				if (data.selector == SelectorType.None)
				{
					for (int i = 0; i < player.cards_hand.Count; i++)
					{
						Card card = player.cards_hand[i];
						AddActions(list, data, node, 1000, card);
					}
					for (int j = 0; j < player.cards_board.Count; j++)
					{
						Card card2 = player.cards_board[j];
						AddActions(list, data, node, 1010, card2);
						AddActions(list, data, node, 1012, card2);
						AddActions(list, data, node, 1020, card2);
					}
					if (player.hero != null)
					{
						AddActions(list, data, node, 1020, player.hero);
					}
				}
				else
				{
					AddSelectActions(list, data, node);
				}
			}
			bool flag = HasAction(list, 1000) && player.mana >= player.mana_max;
			bool flag2 = !HasAction(list, 1012) && !flag && data.selector == SelectorType.None;
			if (list.Count == 0 || flag2)
			{
				AIAction item = CreateAction(1040);
				list.Add(item);
			}
			FilterActions(data, node, list);
			for (int k = 0; k < list.Count; k++)
			{
				AIAction aIAction = list[k];
				if (aIAction.valid && node.alpha < node.beta)
				{
					CalculateChildNode(data, node, aIAction);
				}
			}
			list.Clear();
			list_pool.Dispose(list);
		}

		private void FilterActions(Game data, NodeState node, List<AIAction> action_list)
		{
			int num = 0;
			for (int i = 0; i < action_list.Count; i++)
			{
				AIAction aIAction = action_list[i];
				aIAction.sort = heuristic.CalculateActionSort(data, aIAction);
				aIAction.valid = aIAction.sort <= 0 || aIAction.sort >= node.sort_min;
				if (aIAction.valid)
				{
					num++;
				}
			}
			int num2 = ((node.tdepth < ai_depth_wide) ? nodes_per_action_wide : nodes_per_action);
			int num3 = num2 + 2;
			if (num <= num3)
			{
				return;
			}
			for (int j = 0; j < action_list.Count; j++)
			{
				AIAction aIAction2 = action_list[j];
				if (aIAction2.valid)
				{
					aIAction2.score = heuristic.CalculateActionScore(data, aIAction2);
				}
			}
			action_list.Sort((AIAction a, AIAction b) => b.score.CompareTo(a.score));
			for (int num4 = 0; num4 < action_list.Count; num4++)
			{
				AIAction aIAction3 = action_list[num4];
				aIAction3.valid = aIAction3.valid && num4 < num2;
			}
		}

		private void CalculateChildNode(Game data, NodeState parent, AIAction action)
		{
			if (action.type == 0)
			{
				return;
			}
			int current_player = data.current_player;
			Game game = data_pool.Create();
			Game.Clone(data, game);
			game_logic.ClearResolve();
			game_logic.SetData(game);
			DoAIAction(game, action, current_player);
			bool flag = action.type == 1040;
			int turn_depth = parent.tdepth;
			int turn_action = parent.taction + 1;
			if (flag)
			{
				turn_depth = parent.tdepth + 1;
				turn_action = 0;
			}
			NodeState nodeState = CreateNode(parent, action, current_player, turn_depth, turn_action);
			parent.childs.Add(nodeState);
			nodeState.sort_min = ((!flag) ? Mathf.Max(action.sort, nodeState.sort_min) : 0);
			if (!game.HasEnded() && nodeState.tdepth < ai_depth)
			{
				CalculateNode(game, nodeState);
			}
			else
			{
				nodeState.hvalue = heuristic.CalculateHeuristic(game, nodeState);
			}
			if (current_player == ai_player_id)
			{
				if (parent.best_child == null || nodeState.hvalue > parent.hvalue)
				{
					parent.best_child = nodeState;
					parent.hvalue = nodeState.hvalue;
					parent.alpha = Mathf.Max(parent.alpha, parent.hvalue);
				}
			}
			else if (parent.best_child == null || nodeState.hvalue < parent.hvalue)
			{
				parent.best_child = nodeState;
				parent.hvalue = nodeState.hvalue;
				parent.beta = Mathf.Min(parent.beta, parent.hvalue);
			}
			nb_calculated++;
			if (nodeState.tdepth > reached_depth)
			{
				reached_depth = nodeState.tdepth;
			}
			data_pool.Dispose(game);
		}

		private NodeState CreateNode(NodeState parent, AIAction action, int player_id, int turn_depth, int turn_action)
		{
			NodeState nodeState = node_pool.Create();
			nodeState.current_player = player_id;
			nodeState.tdepth = turn_depth;
			nodeState.taction = turn_action;
			nodeState.parent = parent;
			nodeState.last_action = action;
			nodeState.alpha = parent?.alpha ?? int.MinValue;
			nodeState.beta = parent?.beta ?? int.MaxValue;
			nodeState.hvalue = 0;
			nodeState.sort_min = 0;
			return nodeState;
		}

		private void AddActions(List<AIAction> actions, Game data, NodeState node, ushort type, Card card)
		{
			Player player = data.GetPlayer(data.current_player);
			if (data.selector != SelectorType.None || card.HasStatus(StatusType.Paralysed))
			{
				return;
			}
			if (type == 1000)
			{
				if (card.CardData.IsBoardCard())
				{
					Slot randomEmptySlot = player.GetRandomEmptySlot(random_gen, slot_array.Get());
					if (data.CanPlayCard(card, randomEmptySlot))
					{
						AIAction aIAction = CreateAction(type, card);
						aIAction.slot = randomEmptySlot;
						actions.Add(aIAction);
					}
				}
				else if (card.CardData.IsEquipment())
				{
					Player player2 = data.GetPlayer(card.player_id);
					for (int i = 0; i < player2.cards_board.Count; i++)
					{
						Card card2 = player2.cards_board[i];
						if (data.CanPlayCard(card, card2.slot))
						{
							AIAction aIAction2 = CreateAction(type, card);
							aIAction2.slot = card2.slot;
							aIAction2.target_player_id = player2.player_id;
							actions.Add(aIAction2);
						}
					}
				}
				else if (card.CardData.IsRequireTargetSpell())
				{
					for (int j = 0; j < data.players.Length; j++)
					{
						Player player3 = data.players[j];
						Slot slot = new Slot(player3.player_id);
						if (data.CanPlayCard(card, slot))
						{
							AIAction aIAction3 = CreateAction(type, card);
							aIAction3.slot = slot;
							aIAction3.target_player_id = player3.player_id;
							actions.Add(aIAction3);
						}
					}
					foreach (Slot item2 in Slot.GetAll())
					{
						if (data.CanPlayCard(card, item2))
						{
							Card slotCard = data.GetSlotCard(item2);
							AIAction aIAction4 = CreateAction(type, card);
							aIAction4.slot = item2;
							aIAction4.target_uid = slotCard?.uid;
							actions.Add(aIAction4);
						}
					}
				}
				else if (data.CanPlayCard(card, Slot.None))
				{
					AIAction item = CreateAction(type, card);
					actions.Add(item);
				}
			}
			if (type == 1010 && card.CanAttack())
			{
				for (int k = 0; k < data.players.Length; k++)
				{
					if (k == player.player_id)
					{
						continue;
					}
					Player player4 = data.players[k];
					for (int l = 0; l < player4.cards_board.Count; l++)
					{
						Card card3 = player4.cards_board[l];
						if (data.CanAttackTarget(card, card3))
						{
							AIAction aIAction5 = CreateAction(type, card);
							aIAction5.target_uid = card3.uid;
							actions.Add(aIAction5);
						}
					}
				}
			}
			if (type == 1012 && card.CanAttack())
			{
				for (int m = 0; m < data.players.Length; m++)
				{
					if (m != player.player_id)
					{
						Player player5 = data.players[m];
						if (data.CanAttackTarget(card, player5))
						{
							AIAction aIAction6 = CreateAction(type, card);
							aIAction6.target_player_id = player5.player_id;
							actions.Add(aIAction6);
						}
					}
				}
			}
			if (type == 1020)
			{
				List<AbilityData> abilities = card.GetAbilities();
				for (int n = 0; n < abilities.Count; n++)
				{
					AbilityData abilityData = abilities[n];
					if (abilityData.trigger == AbilityTrigger.Activate && data.CanCastAbility(card, abilityData) && abilityData.HasValidSelectTarget(data, card))
					{
						AIAction aIAction7 = CreateAction(type, card);
						aIAction7.ability_id = abilityData.id;
						actions.Add(aIAction7);
					}
				}
			}
			if (type != 1015)
			{
				return;
			}
			foreach (Slot item3 in Slot.GetAll(player.player_id))
			{
				if (data.CanMoveCard(card, item3))
				{
					AIAction aIAction8 = CreateAction(type, card);
					aIAction8.slot = item3;
					actions.Add(aIAction8);
				}
			}
		}

		private void AddSelectActions(List<AIAction> actions, Game data, NodeState node)
		{
			if (data.selector == SelectorType.None)
			{
				return;
			}
			Player player = data.GetPlayer(data.selector_player_id);
			Card card = data.GetCard(data.selector_caster_uid);
			AbilityData abilityData = AbilityData.Get(data.selector_ability_id);
			if (player == null || card == null || abilityData == null)
			{
				return;
			}
			if (abilityData.target == AbilityTarget.SelectTarget)
			{
				for (int i = 0; i < data.players.Length; i++)
				{
					Player player2 = data.players[i];
					if (abilityData.CanTarget(data, card, player2))
					{
						AIAction aIAction = CreateAction(1032, card);
						aIAction.target_player_id = player2.player_id;
						actions.Add(aIAction);
					}
					foreach (Slot item2 in Slot.GetAll())
					{
						Card slotCard = data.GetSlotCard(item2);
						if (slotCard != null && abilityData.CanTarget(data, card, slotCard))
						{
							AIAction aIAction2 = CreateAction(1030, card);
							aIAction2.target_uid = slotCard.uid;
							actions.Add(aIAction2);
						}
						else if (slotCard == null && abilityData.CanTarget(data, card, item2))
						{
							AIAction aIAction3 = CreateAction(1034, card);
							aIAction3.slot = item2;
							actions.Add(aIAction3);
						}
					}
				}
			}
			if (abilityData.target == AbilityTarget.CardSelector)
			{
				for (int j = 0; j < data.players.Length; j++)
				{
					foreach (Card cardTarget in abilityData.GetCardTargets(data, card, card_array))
					{
						AIAction aIAction4 = CreateAction(1030, card);
						aIAction4.target_uid = cardTarget.uid;
						actions.Add(aIAction4);
					}
				}
			}
			if (abilityData.target == AbilityTarget.ChoiceSelector)
			{
				for (int k = 0; k < abilityData.chain_abilities.Length; k++)
				{
					AbilityData abilityData2 = abilityData.chain_abilities[k];
					if (abilityData2 != null && data.CanSelectAbility(card, abilityData2))
					{
						AIAction aIAction5 = CreateAction(1036, card);
						aIAction5.value = k;
						actions.Add(aIAction5);
					}
				}
			}
			if (actions.Count == 0)
			{
				AIAction item = CreateAction(1039, card);
				actions.Add(item);
			}
		}

		private AIAction CreateAction(ushort type)
		{
			AIAction aIAction = action_pool.Create();
			aIAction.Clear();
			aIAction.type = type;
			aIAction.valid = true;
			return aIAction;
		}

		private AIAction CreateAction(ushort type, Card card)
		{
			AIAction aIAction = action_pool.Create();
			aIAction.Clear();
			aIAction.type = type;
			aIAction.card_uid = card.uid;
			aIAction.valid = true;
			return aIAction;
		}

		private void DoAIAction(Game data, AIAction action, int player_id)
		{
			Player player = data.GetPlayer(player_id);
			if (action.type == 1000)
			{
				Card handCard = player.GetHandCard(action.card_uid);
				game_logic.PlayCard(handCard, action.slot);
			}
			if (action.type == 1015)
			{
				Card boardCard = player.GetBoardCard(action.card_uid);
				game_logic.MoveCard(boardCard, action.slot);
			}
			if (action.type == 1010)
			{
				Card boardCard2 = player.GetBoardCard(action.card_uid);
				Card boardCard3 = data.GetBoardCard(action.target_uid);
				game_logic.AttackTarget(boardCard2, boardCard3);
			}
			if (action.type == 1012)
			{
				Card boardCard4 = player.GetBoardCard(action.card_uid);
				Player player2 = data.GetPlayer(action.target_player_id);
				game_logic.AttackPlayer(boardCard4, player2);
			}
			if (action.type == 1020)
			{
				Card card = player.GetCard(action.card_uid);
				AbilityData iability = AbilityData.Get(action.ability_id);
				game_logic.CastAbility(card, iability);
			}
			if (action.type == 1030)
			{
				Card card2 = data.GetCard(action.target_uid);
				game_logic.SelectCard(card2);
			}
			if (action.type == 1032)
			{
				Player player3 = data.GetPlayer(action.target_player_id);
				game_logic.SelectPlayer(player3);
			}
			if (action.type == 1034)
			{
				game_logic.SelectSlot(action.slot);
			}
			if (action.type == 1036)
			{
				game_logic.SelectChoice(action.value);
			}
			if (action.type == 1039)
			{
				game_logic.CancelSelection();
			}
			if (action.type == 1040)
			{
				game_logic.EndTurn();
			}
		}

		private bool HasAction(List<AIAction> list, ushort type)
		{
			for (int i = 0; i < list.Count; i++)
			{
				if (list[i].type == type)
				{
					return true;
				}
			}
			return false;
		}

		public bool IsRunning()
		{
			return running;
		}

		public string GetNodePath()
		{
			return GetNodePath(first_node);
		}

		public string GetNodePath(NodeState node)
		{
			string text = "Prediction: HValue: " + node.hvalue + "\n";
			for (NodeState nodeState = node; nodeState != null; nodeState = nodeState.best_child)
			{
				AIAction last_action = nodeState.last_action;
				if (last_action != null)
				{
					text = text + "Player " + nodeState.current_player + ": " + last_action.GetText(original_data) + "\n";
				}
			}
			return text;
		}

		public void ClearMemory()
		{
			original_data = null;
			first_node = null;
			best_move = null;
			foreach (NodeState item in node_pool.GetAllActive())
			{
				item.Clear();
			}
			foreach (AIAction item2 in action_pool.GetAllActive())
			{
				item2.Clear();
			}
			data_pool.DisposeAll();
			node_pool.DisposeAll();
			action_pool.DisposeAll();
			list_pool.DisposeAll();
			GC.Collect();
		}

		public int GetNbNodesCalculated()
		{
			return nb_calculated;
		}

		public int GetDepthReached()
		{
			return reached_depth;
		}

		public NodeState GetBest()
		{
			return best_move;
		}

		public NodeState GetFirst()
		{
			return first_node;
		}

		public AIAction GetBestAction()
		{
			if (best_move == null)
			{
				return null;
			}
			return best_move.last_action;
		}

		public bool IsBestFound()
		{
			return best_move != null;
		}
	}
}
