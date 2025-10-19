using TcgEngine.Gameplay;
using UnityEngine;

namespace TcgEngine
{
	[CreateAssetMenu(fileName = "effect", menuName = "TcgEngine/Effect/Draw", order = 10)]
	public class EffectDraw : EffectData
	{
		public override void DoEffect(GameLogic logic, AbilityData ability, Card caster, Player target)
		{
			logic.DrawCard(target, ability.value);
		}

		public override void DoEffect(GameLogic logic, AbilityData ability, Card caster, Card target)
		{
			Player player = logic.GameData.GetPlayer(target.player_id);
			logic.DrawCard(player, ability.value);
		}
	}
}
