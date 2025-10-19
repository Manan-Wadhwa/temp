using UnityEngine;

namespace TcgEngine
{
	[CreateAssetMenu(fileName = "condition", menuName = "TcgEngine/Condition/PlayerStat", order = 10)]
	public class ConditionPlayerStat : ConditionData
	{
		[Header("Card stat is")]
		public ConditionStatType type;

		public ConditionOperatorInt oper;

		public int value;

		public override bool IsTargetConditionMet(Game data, AbilityData ability, Card caster, Card target)
		{
			Player player = data.GetPlayer(target.player_id);
			return IsTargetConditionMet(data, ability, caster, player);
		}

		public override bool IsTargetConditionMet(Game data, AbilityData ability, Card caster, Player target)
		{
			if (type == ConditionStatType.HP)
			{
				return CompareInt(target.hp, oper, value);
			}
			if (type == ConditionStatType.Mana)
			{
				return CompareInt(target.mana, oper, value);
			}
			return false;
		}
	}
}
