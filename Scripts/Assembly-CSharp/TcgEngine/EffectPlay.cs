using TcgEngine.Gameplay;
using UnityEngine;

namespace TcgEngine
{
	[CreateAssetMenu(fileName = "effect", menuName = "TcgEngine/Effect/Play", order = 10)]
	public class EffectPlay : EffectData
	{
		public override void DoEffect(GameLogic logic, AbilityData ability, Card caster, Card target)
		{
			Player player = logic.GetGameData().GetPlayer(caster.player_id);
			Slot randomEmptySlot = player.GetRandomEmptySlot(logic.GetRandom());
			player.RemoveCardFromAllGroups(target);
			player.cards_hand.Add(target);
			if (randomEmptySlot != Slot.None)
			{
				logic.PlayCard(target, randomEmptySlot, skip_cost: true);
			}
		}
	}
}
