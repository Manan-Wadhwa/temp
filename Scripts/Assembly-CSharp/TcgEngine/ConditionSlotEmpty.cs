using UnityEngine;

namespace TcgEngine
{
	[CreateAssetMenu(fileName = "condition", menuName = "TcgEngine/Condition/SlotEmpty", order = 11)]
	public class ConditionSlotEmpty : ConditionData
	{
		[Header("Slot Is Empty")]
		public ConditionOperatorBool oper;

		public override bool IsTargetConditionMet(Game data, AbilityData ability, Card caster, Card target)
		{
			return CompareBool(condition: false, oper);
		}

		public override bool IsTargetConditionMet(Game data, AbilityData ability, Card caster, Player target)
		{
			return CompareBool(condition: false, oper);
		}

		public override bool IsTargetConditionMet(Game data, AbilityData ability, Card caster, Slot target)
		{
			Card slotCard = data.GetSlotCard(target);
			return CompareBool(slotCard == null, oper);
		}
	}
}
