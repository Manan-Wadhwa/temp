using UnityEngine;

namespace TcgEngine
{
	[CreateAssetMenu(fileName = "condition", menuName = "TcgEngine/Condition/CardType", order = 10)]
	public class ConditionCardType : ConditionData
	{
		[Header("Card is of type")]
		public CardType has_type;

		public TeamData has_team;

		public TraitData has_trait;

		public ConditionOperatorBool oper;

		public override bool IsTargetConditionMet(Game data, AbilityData ability, Card caster, Card target)
		{
			return CompareBool(IsTrait(target), oper);
		}

		public override bool IsTargetConditionMet(Game data, AbilityData ability, Card caster, Player target)
		{
			return false;
		}

		public override bool IsTargetConditionMet(Game data, AbilityData ability, Card caster, Slot target)
		{
			return false;
		}

		public override bool IsTargetConditionMet(Game data, AbilityData ability, Card caster, CardData target)
		{
			bool num = target.type == has_type || has_type == CardType.None;
			bool flag = target.team == has_team || has_team == null;
			bool flag2 = target.HasTrait(has_trait) || has_trait == null;
			return num && flag && flag2;
		}

		private bool IsTrait(Card card)
		{
			bool num = card.CardData.type == has_type || has_type == CardType.None;
			bool flag = card.CardData.team == has_team || has_team == null;
			bool flag2 = card.HasTrait(has_trait) || has_trait == null;
			return num && flag && flag2;
		}
	}
}
