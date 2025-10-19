using UnityEngine;

namespace TcgEngine
{
	[CreateAssetMenu(fileName = "condition", menuName = "TcgEngine/Condition/SlotDist", order = 11)]
	public class ConditionSlotDist : ConditionData
	{
		[Header("Slot Distance")]
		public int distance = 1;

		public bool diagonals;

		public override bool IsTargetConditionMet(Game data, AbilityData ability, Card caster, Card target)
		{
			return IsTargetConditionMet(data, ability, caster, target.slot);
		}

		public override bool IsTargetConditionMet(Game data, AbilityData ability, Card caster, Slot target)
		{
			Slot slot = caster.slot;
			if (diagonals)
			{
				return slot.IsInDistance(target, distance);
			}
			return slot.IsInDistanceStraight(target, distance);
		}
	}
}
