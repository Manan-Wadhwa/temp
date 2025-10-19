using System.Collections.Generic;
using UnityEngine;

namespace TcgEngine
{
	[CreateAssetMenu(fileName = "filter", menuName = "TcgEngine/Filter/LowestStat", order = 10)]
	public class FilterLowestStat : FilterData
	{
		public ConditionStatType stat;

		public override List<Card> FilterTargets(Game data, AbilityData ability, Card caster, List<Card> source, List<Card> dest)
		{
			int num = 99999;
			foreach (Card item in source)
			{
				int num2 = GetStat(item);
				if (num2 < num)
				{
					num = num2;
				}
			}
			foreach (Card item2 in source)
			{
				if (GetStat(item2) == num)
				{
					dest.Add(item2);
				}
			}
			return dest;
		}

		private int GetStat(Card card)
		{
			if (stat == ConditionStatType.Attack)
			{
				return card.GetAttack();
			}
			if (stat == ConditionStatType.HP)
			{
				return card.GetHP();
			}
			if (stat == ConditionStatType.Mana)
			{
				return card.GetMana();
			}
			return 0;
		}
	}
}
