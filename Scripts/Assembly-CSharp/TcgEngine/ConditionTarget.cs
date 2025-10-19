using UnityEngine;

namespace TcgEngine
{
	[CreateAssetMenu(fileName = "condition", menuName = "TcgEngine/Condition/Player", order = 10)]
	public class ConditionTarget : ConditionData
	{
		[Header("Target is of type")]
		public ConditionTargetType type;

		public ConditionOperatorBool oper;

		public override bool IsTargetConditionMet(Game data, AbilityData ability, Card caster, Card target)
		{
			return CompareBool(type == ConditionTargetType.Card, oper);
		}

		public override bool IsTargetConditionMet(Game data, AbilityData ability, Card caster, Player target)
		{
			return CompareBool(type == ConditionTargetType.Player, oper);
		}

		public override bool IsTargetConditionMet(Game data, AbilityData ability, Card caster, Slot target)
		{
			return CompareBool(type == ConditionTargetType.Slot, oper);
		}
	}
}
