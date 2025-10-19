using TcgEngine.Gameplay;
using UnityEngine;

namespace TcgEngine
{
	[CreateAssetMenu(fileName = "effect", menuName = "TcgEngine/Effect/Discard", order = 10)]
	public class EffectDiscard : EffectData
	{
		public override void DoEffect(GameLogic logic, AbilityData ability, Card caster, Player target)
		{
			logic.DrawDiscardCard(target, ability.value);
		}

		public override void DoEffect(GameLogic logic, AbilityData ability, Card caster, Card target)
		{
			logic.DiscardCard(target);
		}
	}
}
