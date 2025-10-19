using UnityEngine;

namespace TcgEngine
{
	[CreateAssetMenu(fileName = "condition", menuName = "TcgEngine/Condition/CardStatus", order = 10)]
	public class ConditionStatus : ConditionData
	{
		[Header("Card has status")]
		public StatusType has_status;

		public int value;

		public ConditionOperatorBool oper;

		public override bool IsTargetConditionMet(Game data, AbilityData ability, Card caster, Card target)
		{
			bool condition = target.HasStatus(has_status) && target.GetStatusValue(has_status) >= value;
			return CompareBool(condition, oper);
		}

		public override bool IsTargetConditionMet(Game data, AbilityData ability, Card caster, Player target)
		{
			bool condition = target.HasStatus(has_status) && target.GetStatusValue(has_status) >= value;
			return CompareBool(condition, oper);
		}

		public override bool IsTargetConditionMet(Game data, AbilityData ability, Card caster, Slot target)
		{
			Card slotCard = data.GetSlotCard(target);
			if (slotCard != null)
			{
				return IsTargetConditionMet(data, ability, caster, slotCard);
			}
			return false;
		}
	}
}
