using TcgEngine.Gameplay;
using UnityEngine;

namespace TcgEngine
{
	[CreateAssetMenu(fileName = "effect", menuName = "TcgEngine/Effect/Exhaust", order = 10)]
	public class EffectExhaust : EffectData
	{
		public bool exhausted;

		public override void DoEffect(GameLogic logic, AbilityData ability, Card caster, Card target)
		{
			target.exhausted = exhausted;
		}
	}
}
