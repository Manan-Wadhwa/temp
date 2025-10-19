using TcgEngine.Gameplay;
using UnityEngine;

namespace TcgEngine
{
	[CreateAssetMenu(fileName = "effect", menuName = "TcgEngine/Effect/RemoveTrait", order = 10)]
	public class EffectRemoveTrait : EffectData
	{
		public TraitData trait;

		public override void DoEffect(GameLogic logic, AbilityData ability, Card caster, Player target)
		{
			target.RemoveTrait(trait.id);
		}

		public override void DoEffect(GameLogic logic, AbilityData ability, Card caster, Card target)
		{
			target.RemoveTrait(trait.id);
		}
	}
}
