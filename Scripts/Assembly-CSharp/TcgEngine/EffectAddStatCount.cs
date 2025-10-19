using System.Collections.Generic;
using TcgEngine.Gameplay;
using UnityEngine;

namespace TcgEngine
{
	[CreateAssetMenu(fileName = "effect", menuName = "TcgEngine/Effect/AddStatCount", order = 10)]
	public class EffectAddStatCount : EffectData
	{
		public EffectStatType type;

		public PileType pile;

		[Header("Count Traits")]
		public CardType has_type;

		public TeamData has_team;

		public TraitData has_trait;

		public override void DoEffect(GameLogic logic, AbilityData ability, Card caster, Player target)
		{
			int num = GetCount(logic.GetGameData(), caster) * ability.value;
			if (type == EffectStatType.HP)
			{
				target.hp += num;
				target.hp_max += ability.value;
			}
			if (type == EffectStatType.Mana)
			{
				target.mana += num;
				target.mana_max += num;
				target.mana = Mathf.Max(target.mana, 0);
				target.mana_max = Mathf.Clamp(target.mana_max, 0, GameplayData.Get().mana_max);
			}
		}

		public override void DoEffect(GameLogic logic, AbilityData ability, Card caster, Card target)
		{
			int num = GetCount(logic.GetGameData(), caster) * ability.value;
			if (type == EffectStatType.Attack)
			{
				target.attack += num;
			}
			if (type == EffectStatType.HP)
			{
				target.hp += num;
			}
			if (type == EffectStatType.Mana)
			{
				target.mana += num;
			}
		}

		public override void DoOngoingEffect(GameLogic logic, AbilityData ability, Card caster, Card target)
		{
			int num = GetCount(logic.GetGameData(), caster) * ability.value;
			if (type == EffectStatType.Attack)
			{
				target.attack_ongoing += num;
			}
			if (type == EffectStatType.HP)
			{
				target.hp_ongoing += num;
			}
			if (type == EffectStatType.Mana)
			{
				target.mana_ongoing += num;
			}
		}

		private int GetCount(Game data, Card caster)
		{
			Player player = data.GetPlayer(caster.player_id);
			return CountPile(player, pile);
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
