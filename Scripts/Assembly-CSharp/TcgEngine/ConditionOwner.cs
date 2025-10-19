using UnityEngine;

namespace TcgEngine
{
	[CreateAssetMenu(fileName = "condition", menuName = "TcgEngine/Condition/CardOwner", order = 10)]
	public class ConditionOwner : ConditionData
	{
		[Header("Target owner is caster owner")]
		public ConditionOperatorBool oper;

		public override bool IsTargetConditionMet(Game data, AbilityData ability, Card caster, Card target)
		{
			bool condition = caster.player_id == target.player_id;
			return CompareBool(condition, oper);
		}

		public override bool IsTargetConditionMet(Game data, AbilityData ability, Card caster, Player target)
		{
			bool condition = caster.player_id == target.player_id;
			return CompareBool(condition, oper);
		}

		public override bool IsTargetConditionMet(Game data, AbilityData ability, Card caster, Slot target)
		{
			bool condition = Slot.GetP(caster.player_id) == target.p;
			return CompareBool(condition, oper);
		}
	}
}
