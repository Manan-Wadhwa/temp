using TcgEngine.Gameplay;
using UnityEngine;

namespace TcgEngine
{
	[CreateAssetMenu(fileName = "effect", menuName = "TcgEngine/Effect/SendPile", order = 10)]
	public class EffectSendPile : EffectData
	{
		public PileType pile;

		public override void DoEffect(GameLogic logic, AbilityData ability, Card caster, Card target)
		{
			Player player = logic.GetGameData().GetPlayer(target.player_id);
			if (pile == PileType.Deck)
			{
				player.RemoveCardFromAllGroups(target);
				player.cards_deck.Add(target);
				target.Clear();
			}
			if (pile == PileType.Hand)
			{
				player.RemoveCardFromAllGroups(target);
				player.cards_hand.Add(target);
				target.Clear();
			}
			if (pile == PileType.Discard)
			{
				player.RemoveCardFromAllGroups(target);
				player.cards_discard.Add(target);
				target.Clear();
			}
			if (pile == PileType.Temp)
			{
				player.RemoveCardFromAllGroups(target);
				player.cards_temp.Add(target);
				target.Clear();
			}
		}
	}
}
