using UnityEngine;

namespace TcgEngine
{
	[CreateAssetMenu(fileName = "condition", menuName = "TcgEngine/Condition/CardSelf", order = 10)]
	public class ConditionSelf : ConditionData
	{
		[Header("Target is caster")]
		public ConditionOperatorBool oper;

		public override bool IsTargetConditionMet(Game data, AbilityData ability, Card caster, Card target)
		{
			return CompareBool(caster == target, oper);
		}

		public override bool IsTargetConditionMet(Game data, AbilityData ability, Card caster, Player target)
		{
			bool condition = caster.player_id == target.player_id;
			return CompareBool(condition, oper);
		}

		public override bool IsTargetConditionMet(Game data, AbilityData ability, Card caster, Slot target)
		{
			return CompareBool(caster.slot == target, oper);
		}
	}
}
