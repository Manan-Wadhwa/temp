using UnityEngine;

namespace TcgEngine
{
	[CreateAssetMenu(fileName = "condition", menuName = "TcgEngine/Condition/CardOwnerAI", order = 10)]
	public class ConditionOwnerAI : ConditionData
	{
		[Header("AI Only: Target owner is caster owner")]
		public ConditionOperatorBool oper;

		public override bool IsTargetConditionMet(Game data, AbilityData ability, Card caster, Card target)
		{
			if (!IsAIPlayer(data, caster))
			{
				return true;
			}
			bool condition = caster.player_id == target.player_id;
			return CompareBool(condition, oper);
		}

		public override bool IsTargetConditionMet(Game data, AbilityData ability, Card caster, Player target)
		{
			if (!IsAIPlayer(data, caster))
			{
				return true;
			}
			bool condition = caster.player_id == target.player_id;
			return CompareBool(condition, oper);
		}

		public override bool IsTargetConditionMet(Game data, AbilityData ability, Card caster, Slot target)
		{
			if (!IsAIPlayer(data, caster))
			{
				return true;
			}
			bool condition = Slot.GetP(caster.player_id) == target.p;
			return CompareBool(condition, oper);
		}

		private bool IsAIPlayer(Game data, Card caster)
		{
			return data.GetPlayer(caster.player_id).is_ai;
		}
	}
}
