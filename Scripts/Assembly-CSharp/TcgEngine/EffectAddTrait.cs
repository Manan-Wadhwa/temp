using TcgEngine.Gameplay;
using UnityEngine;

namespace TcgEngine
{
	[CreateAssetMenu(fileName = "effect", menuName = "TcgEngine/Effect/AddTrait", order = 10)]
	public class EffectAddTrait : EffectData
	{
		public TraitData trait;

		public override void DoEffect(GameLogic logic, AbilityData ability, Card caster, Player target)
		{
			target.AddTrait(trait.id, ability.value);
		}

		public override void DoEffect(GameLogic logic, AbilityData ability, Card caster, Card target)
		{
			target.AddTrait(trait.id, ability.value);
		}

		public override void DoOngoingEffect(GameLogic logic, AbilityData ability, Card caster, Card target)
		{
			target.AddOngoingTrait(trait.id, ability.value);
		}

		public override void DoOngoingEffect(GameLogic logic, AbilityData ability, Card caster, Player target)
		{
			target.AddOngoingTrait(trait.id, ability.value);
		}
	}
}
