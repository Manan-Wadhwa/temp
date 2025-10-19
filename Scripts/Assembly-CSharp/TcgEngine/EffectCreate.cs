using TcgEngine.Gameplay;
using UnityEngine;

namespace TcgEngine
{
	[CreateAssetMenu(fileName = "effect", menuName = "TcgEngine/Effect/Create", order = 10)]
	public class EffectCreate : EffectData
	{
		public PileType create_pile;

		public bool create_opponent;

		public override void DoEffect(GameLogic logic, AbilityData ability, Card caster, CardData target)
		{
			Player player = logic.GameData.GetPlayer(caster.player_id);
			if (create_opponent)
			{
				player = logic.GameData.GetOpponentPlayer(caster.player_id);
			}
			Card card = Card.Create(target, caster.VariantData, player);
			logic.GameData.last_summoned = card.uid;
			if (create_pile == PileType.Deck)
			{
				player.cards_deck.Add(card);
			}
			if (create_pile == PileType.Discard)
			{
				player.cards_discard.Add(card);
			}
			if (create_pile == PileType.Hand)
			{
				player.cards_hand.Add(card);
			}
			if (create_pile == PileType.Temp)
			{
				player.cards_temp.Add(card);
			}
		}

		public override void DoEffect(GameLogic logic, AbilityData ability, Card caster, Card target)
		{
			DoEffect(logic, ability, caster, target.CardData);
		}
	}
}
