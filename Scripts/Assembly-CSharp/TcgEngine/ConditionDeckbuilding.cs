using UnityEngine;

namespace TcgEngine
{
	[CreateAssetMenu(fileName = "condition", menuName = "TcgEngine/Condition/CardDeckbuilding", order = 10)]
	public class ConditionDeckbuilding : ConditionData
	{
		[Header("Card is Deckbuilding")]
		public ConditionOperatorBool oper;

		public override bool IsTargetConditionMet(Game data, AbilityData ability, Card caster, Card target)
		{
			return CompareBool(target.CardData.deckbuilding, oper);
		}

		public override bool IsTargetConditionMet(Game data, AbilityData ability, Card caster, CardData target)
		{
			return CompareBool(target.deckbuilding, oper);
		}
	}
}
