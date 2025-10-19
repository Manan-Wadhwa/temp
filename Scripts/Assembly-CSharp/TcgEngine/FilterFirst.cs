using System.Collections.Generic;
using UnityEngine;

namespace TcgEngine
{
	[CreateAssetMenu(fileName = "filter", menuName = "TcgEngine/Filter/First", order = 10)]
	public class FilterFirst : FilterData
	{
		public int amount = 1;

		public override List<Card> FilterTargets(Game data, AbilityData ability, Card caster, List<Card> source, List<Card> dest)
		{
			int num = Mathf.Min(source.Count, amount);
			for (int i = 0; i < num; i++)
			{
				dest.Add(source[i]);
			}
			return dest;
		}

		public override List<Player> FilterTargets(Game data, AbilityData ability, Card caster, List<Player> source, List<Player> dest)
		{
			int num = Mathf.Min(source.Count, amount);
			for (int i = 0; i < num; i++)
			{
				dest.Add(source[i]);
			}
			return dest;
		}

		public override List<Slot> FilterTargets(Game data, AbilityData ability, Card caster, List<Slot> source, List<Slot> dest)
		{
			int num = Mathf.Min(source.Count, amount);
			for (int i = 0; i < num; i++)
			{
				dest.Add(source[i]);
			}
			return dest;
		}
	}
}
