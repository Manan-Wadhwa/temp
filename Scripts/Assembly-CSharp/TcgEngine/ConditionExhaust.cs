using UnityEngine;

namespace TcgEngine
{
	[CreateAssetMenu(fileName = "condition", menuName = "TcgEngine/Condition/CardExhausted", order = 10)]
	public class ConditionExhaust : ConditionData
	{
		[Header("Target is exhausted")]
		public ConditionOperatorBool oper;

		public override bool IsTargetConditionMet(Game data, AbilityData ability, Card caster, Card target)
		{
			return CompareBool(target.exhausted, oper);
		}

		public override bool IsTargetConditionMet(Game data, AbilityData ability, Card caster, Player target)
		{
			return CompareBool(condition: false, oper);
		}

		public override bool IsTargetConditionMet(Game data, AbilityData ability, Card caster, Slot target)
		{
			return CompareBool(condition: false, oper);
		}
	}
}
