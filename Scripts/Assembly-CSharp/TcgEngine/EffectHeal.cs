using TcgEngine.Gameplay;
using UnityEngine;

namespace TcgEngine
{
	[CreateAssetMenu(fileName = "effect", menuName = "TcgEngine/Effect/Heal", order = 10)]
	public class EffectHeal : EffectData
	{
		public override void DoEffect(GameLogic logic, AbilityData ability, Card caster, Player target)
		{
			logic.HealPlayer(target, ability.value);
		}

		public override void DoEffect(GameLogic logic, AbilityData ability, Card caster, Card target)
		{
			logic.HealCard(target, ability.value);
		}
	}
}
