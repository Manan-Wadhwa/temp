using UnityEngine;

namespace TcgEngine
{
	[CreateAssetMenu(fileName = "condition", menuName = "TcgEngine/Condition/SlotRange", order = 11)]
	public class ConditionSlotRange : ConditionData
	{
		[Header("Slot Range")]
		public int range_x = 1;

		public int range_y = 1;

		public int range_p;

		public override bool IsTargetConditionMet(Game data, AbilityData ability, Card caster, Card target)
		{
			return IsTargetConditionMet(data, ability, caster, target.slot);
		}

		public override bool IsTargetConditionMet(Game data, AbilityData ability, Card caster, Slot target)
		{
			Slot slot = caster.slot;
			int num = Mathf.Abs(slot.x - target.x);
			int num2 = Mathf.Abs(slot.y - target.y);
			int num3 = Mathf.Abs(slot.p - target.p);
			if (num <= range_x && num2 <= range_y)
			{
				return num3 <= range_p;
			}
			return false;
		}
	}
}
