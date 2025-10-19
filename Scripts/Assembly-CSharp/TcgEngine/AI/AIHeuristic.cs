using System;

namespace TcgEngine.AI
{
	public class AIHeuristic
	{
		public int board_card_value = 20;

		public int secret_card_value = 10;

		public int hand_card_value = 5;

		public int kill_value = 5;

		public int player_hp_value = 4;

		public int card_attack_value = 3;

		public int card_hp_value = 2;

		public int card_status_value = 15;

		private int ai_player_id;

		private int ai_level;

		private int heuristic_modifier;

		private Random random_gen;

		public AIHeuristic(int player_id, int level)
		{
			ai_player_id = player_id;
			ai_level = level;
			heuristic_modifier = GetHeuristicModifier();
			random_gen = new Random();
		}

		public int CalculateHeuristic(Game data, NodeState node)
		{
			Player player = data.GetPlayer(ai_player_id);
			Player opponentPlayer = data.GetOpponentPlayer(ai_player_id);
			return CalculateHeuristic(data, node, player, opponentPlayer);
		}

		public int CalculateHeuristic(Game data, NodeState node, Player aiplayer, Player oplayer)
		{
			int num = 0;
			if (aiplayer.IsDead())
			{
				num += -100000 + node.tdepth * 1000;
			}
			if (oplayer.IsDead())
			{
				num += 100000 - node.tdepth * 1000;
			}
			num += aiplayer.cards_board.Count * board_card_value;
			num += aiplayer.cards_equip.Count * board_card_value;
			num += aiplayer.cards_secret.Count * secret_card_value;
			num += aiplayer.cards_hand.Count * hand_card_value;
			num += aiplayer.kill_count * kill_value;
			num += aiplayer.hp * player_hp_value;
			num -= oplayer.cards_board.Count * board_card_value;
			num -= oplayer.cards_equip.Count * board_card_value;
			num -= oplayer.cards_secret.Count * secret_card_value;
			num -= oplayer.cards_hand.Count * hand_card_value;
			num -= oplayer.kill_count * kill_value;
			num -= oplayer.hp * player_hp_value;
			foreach (Card item in aiplayer.cards_board)
			{
				num += item.GetAttack() * card_attack_value;
				num += item.GetHP() * card_hp_value;
				foreach (CardStatus item2 in item.status)
				{
					num += item2.StatusData.hvalue * card_status_value;
				}
				foreach (CardStatus item3 in item.ongoing_status)
				{
					num += item3.StatusData.hvalue * card_status_value;
				}
			}
			foreach (Card item4 in oplayer.cards_board)
			{
				num -= item4.GetAttack() * card_attack_value;
				num -= item4.GetHP() * card_hp_value;
				foreach (CardStatus item5 in item4.status)
				{
					num -= item5.StatusData.hvalue * card_status_value;
				}
				foreach (CardStatus item6 in item4.ongoing_status)
				{
					num -= item6.StatusData.hvalue * card_status_value;
				}
			}
			if (heuristic_modifier > 0)
			{
				num += random_gen.Next(-heuristic_modifier, heuristic_modifier);
			}
			return num;
		}

		public int CalculateActionScore(Game data, AIAction order)
		{
			if (order.type == 1040)
			{
				return 0;
			}
			if (order.type == 1039)
			{
				return 0;
			}
			if (order.type == 1020)
			{
				return 200;
			}
			if (order.type == 1010)
			{
				Card card = data.GetCard(order.card_uid);
				Card card2 = data.GetCard(order.target_uid);
				int num = ((card.GetAttack() >= card2.GetHP()) ? 300 : 100);
				int num2 = ((card2.GetAttack() >= card.GetHP()) ? (-200) : 0);
				return num + num2 + card2.GetAttack() * 5;
			}
			if (order.type == 1012)
			{
				Card card3 = data.GetCard(order.card_uid);
				Player player = data.GetPlayer(order.target_player_id);
				return ((card3.GetAttack() >= player.hp) ? 500 : 200) + card3.GetAttack() * 10 - player.hp;
			}
			if (order.type == 1000)
			{
				Player player2 = data.GetPlayer(ai_player_id);
				Card card4 = data.GetCard(order.card_uid);
				if (card4.CardData.IsBoardCard())
				{
					return 200 + card4.GetMana() * 5 - 30 * player2.cards_board.Count;
				}
				if (card4.CardData.IsEquipment())
				{
					return 200 + card4.GetMana() * 5 - 30 * player2.cards_equip.Count;
				}
				return 200 + card4.GetMana() * 5;
			}
			_ = order.type;
			_ = 1015;
			return 100;
		}

		public int CalculateActionSort(Game data, AIAction order)
		{
			if (order.type == 1040)
			{
				return 0;
			}
			if (data.selector != SelectorType.None)
			{
				return 0;
			}
			Card card = data.GetCard(order.card_uid);
			Card card2 = ((order.target_uid != null) ? data.GetCard(order.target_uid) : null);
			bool flag = card != null && !card.CardData.IsBoardCard();
			int num = 0;
			if (order.type == 1000 && flag)
			{
				num = 1;
			}
			if (order.type == 1020)
			{
				num = 2;
			}
			if (order.type == 1015)
			{
				num = 3;
			}
			if (order.type == 1010)
			{
				num = 4;
			}
			if (order.type == 1012)
			{
				num = 5;
			}
			if (order.type == 1000 && !flag)
			{
				num = 7;
			}
			int num2 = ((card != null) ? (card.Hash % 100) : 0);
			int num3 = ((card2 != null) ? (card2.Hash % 100) : 0);
			return num * 10000 + num2 * 100 + num3 + 1;
		}

		private int GetHeuristicModifier()
		{
			if (ai_level >= 10)
			{
				return 0;
			}
			if (ai_level == 9)
			{
				return 5;
			}
			if (ai_level == 8)
			{
				return 10;
			}
			if (ai_level == 7)
			{
				return 20;
			}
			if (ai_level == 6)
			{
				return 30;
			}
			if (ai_level == 5)
			{
				return 40;
			}
			if (ai_level == 4)
			{
				return 50;
			}
			if (ai_level == 3)
			{
				return 75;
			}
			if (ai_level == 2)
			{
				return 100;
			}
			if (ai_level <= 1)
			{
				return 200;
			}
			return 0;
		}

		public bool IsWin(NodeState node)
		{
			if (node.hvalue <= 50000)
			{
				return node.hvalue < -50000;
			}
			return true;
		}
	}
}
