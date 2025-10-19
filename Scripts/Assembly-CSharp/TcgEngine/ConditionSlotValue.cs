using UnityEngine;

namespace TcgEngine
{
	[CreateAssetMenu(fileName = "condition", menuName = "TcgEngine/Condition/SlotValue", order = 11)]
	public class ConditionSlotValue : ConditionData
	{
		[Header("Slot Value")]
		public ConditionOperatorInt oper_x;

		public int value_x;

		public ConditionOperatorInt oper_y;

		public int value_y;

		public override bool IsTargetConditionMet(Game data, AbilityData ability, Card caster, Card target)
		{
			return IsTargetConditionMet(data, ability, caster, target.slot);
		}

		public override bool IsTargetConditionMet(Game data, AbilityData ability, Card caster, Slot target)
		{
			bool num = CompareInt(target.x, oper_x, value_x);
			bool flag = CompareInt(target.y, oper_y, value_y);
			return num && flag;
		}
	}
}
