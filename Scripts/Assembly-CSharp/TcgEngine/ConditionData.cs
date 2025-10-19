using UnityEngine;

namespace TcgEngine
{
	public class ConditionData : ScriptableObject
	{
		public virtual bool IsTriggerConditionMet(Game data, AbilityData ability, Card caster)
		{
			return true;
		}

		public virtual bool IsTargetConditionMet(Game data, AbilityData ability, Card caster, Card target)
		{
			return true;
		}

		public virtual bool IsTargetConditionMet(Game data, AbilityData ability, Card caster, Player target)
		{
			return true;
		}

		public virtual bool IsTargetConditionMet(Game data, AbilityData ability, Card caster, Slot target)
		{
			return true;
		}

		public virtual bool IsTargetConditionMet(Game data, AbilityData ability, Card caster, CardData target)
		{
			return true;
		}

		public bool CompareBool(bool condition, ConditionOperatorBool oper)
		{
			if (oper == ConditionOperatorBool.IsFalse)
			{
				return !condition;
			}
			return condition;
		}

		public bool CompareInt(int ival1, ConditionOperatorInt oper, int ival2)
		{
			return oper switch
			{
				ConditionOperatorInt.Equal => ival1 == ival2, 
				ConditionOperatorInt.NotEqual => ival1 != ival2, 
				ConditionOperatorInt.GreaterEqual => ival1 >= ival2, 
				ConditionOperatorInt.LessEqual => ival1 <= ival2, 
				ConditionOperatorInt.Greater => ival1 > ival2, 
				ConditionOperatorInt.Less => ival1 < ival2, 
				_ => false, 
			};
		}
	}
}
