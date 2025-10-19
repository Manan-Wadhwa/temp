using TcgEngine.Gameplay;
using UnityEngine;

namespace TcgEngine
{
	[CreateAssetMenu(fileName = "effect", menuName = "TcgEngine/Effect/Mana", order = 10)]
	public class EffectMana : EffectData
	{
		public override void DoEffect(GameLogic logic, AbilityData ability, Card caster, Player target)
		{
			target.mana += ability.value;
			target.mana = Mathf.Max(target.mana, 0);
		}
	}
}
