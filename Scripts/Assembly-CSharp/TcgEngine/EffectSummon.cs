using TcgEngine.Gameplay;
using UnityEngine;

namespace TcgEngine
{
	[CreateAssetMenu(fileName = "effect", menuName = "TcgEngine/Effect/Summon", order = 10)]
	public class EffectSummon : EffectData
	{
		public CardData summon;

		public override void DoEffect(GameLogic logic, AbilityData ability, Card caster, Player target)
		{
			logic.SummonCardHand(target, summon, caster.VariantData);
		}

		public override void DoEffect(GameLogic logic, AbilityData ability, Card caster, Card target)
		{
			Player player = logic.GameData.GetPlayer(caster.player_id);
			logic.SummonCard(player, summon, caster.VariantData, target.slot);
		}

		public override void DoEffect(GameLogic logic, AbilityData ability, Card caster, Slot target)
		{
			Player player = logic.GameData.GetPlayer(caster.player_id);
			logic.SummonCard(player, summon, caster.VariantData, target);
		}

		public override void DoEffect(GameLogic logic, AbilityData ability, Card caster, CardData target)
		{
			Player player = logic.GameData.GetPlayer(caster.player_id);
			logic.SummonCardHand(player, target, caster.VariantData);
		}
	}
}
