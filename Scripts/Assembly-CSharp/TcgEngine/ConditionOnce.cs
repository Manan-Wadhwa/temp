using UnityEngine;

namespace TcgEngine
{
	[CreateAssetMenu(fileName = "condition", menuName = "TcgEngine/Condition/OncePerTurn", order = 10)]
	public class ConditionOnce : ConditionData
	{
		public override bool IsTriggerConditionMet(Game data, AbilityData ability, Card caster)
		{
			return !data.ability_played.Contains(ability.id);
		}
	}
}
