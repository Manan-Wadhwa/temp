using UnityEngine;

namespace TcgEngine
{
	[CreateAssetMenu(fileName = "condition", menuName = "TcgEngine/Condition/Turn", order = 10)]
	public class ConditionTurn : ConditionData
	{
		public ConditionOperatorBool oper;

		public override bool IsTriggerConditionMet(Game data, AbilityData ability, Card caster)
		{
			bool condition = caster.player_id == data.current_player;
			return CompareBool(condition, oper);
		}
	}
}
