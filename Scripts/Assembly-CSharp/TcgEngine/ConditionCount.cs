using System.Collections.Generic;
using UnityEngine;

namespace TcgEngine
{
	[CreateAssetMenu(fileName = "condition", menuName = "TcgEngine/Condition/Count", order = 10)]
	public class ConditionCount : ConditionData
	{
		[Header("Count cards of type")]
		public ConditionPlayerType target;

		public PileType pile;

		public ConditionOperatorInt oper;

		public int value;

		[Header("Traits")]
		public CardType has_type;

		public TeamData has_team;

		public TraitData has_trait;

		public override bool IsTriggerConditionMet(Game data, AbilityData ability, Card caster)
		{
			int num = 0;
			if (target == ConditionPlayerType.Self || target == ConditionPlayerType.Both)
			{
				Player player = data.GetPlayer(caster.player_id);
				num += CountPile(player, pile);
			}
			if (target == ConditionPlayerType.Opponent || target == ConditionPlayerType.Both)
			{
				Player opponentPlayer = data.GetOpponentPlayer(caster.player_id);
				num += CountPile(opponentPlayer, pile);
			}
			return CompareInt(num, oper, value);
		}

		private int CountPile(Player player, PileType pile)
		{
			List<Card> list = null;
			if (pile == PileType.Hand)
			{
				list = player.cards_hand;
			}
			if (pile == PileType.Board)
			{
				list = player.cards_board;
			}
			if (pile == PileType.Equipped)
			{
				list = player.cards_equip;
			}
			if (pile == PileType.Deck)
			{
				list = player.cards_deck;
			}
			if (pile == PileType.Discard)
			{
				list = player.cards_discard;
			}
			if (pile == PileType.Secret)
			{
				list = player.cards_secret;
			}
			if (pile == PileType.Temp)
			{
				list = player.cards_temp;
			}
			if (list != null)
			{
				int num = 0;
				{
					foreach (Card item in list)
					{
						if (IsTrait(item))
						{
							num++;
						}
					}
					return num;
				}
			}
			return 0;
		}

		private bool IsTrait(Card card)
		{
			bool num = card.CardData.type == has_type || has_type == CardType.None;
			bool flag = card.CardData.team == has_team || has_team == null;
			bool flag2 = card.HasTrait(has_trait) || has_trait == null;
			return num && flag && flag2;
		}
	}
}
