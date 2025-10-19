using TcgEngine.Gameplay;
using UnityEngine;

namespace TcgEngine
{
	[CreateAssetMenu(fileName = "effect", menuName = "TcgEngine/Effect/RemoveAbility", order = 10)]
	public class EffectRemoveAbility : EffectData
	{
		public AbilityData remove_ability;

		public override void DoEffect(GameLogic logic, AbilityData ability, Card caster, Card target)
		{
			target.RemoveAbility(remove_ability);
		}
	}
}
