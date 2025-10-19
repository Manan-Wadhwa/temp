using System.Collections.Generic;
using UnityEngine;

namespace TcgEngine
{
	public class FilterData : ScriptableObject
	{
		public virtual List<Card> FilterTargets(Game data, AbilityData ability, Card caster, List<Card> source, List<Card> dest)
		{
			return source;
		}

		public virtual List<Player> FilterTargets(Game data, AbilityData ability, Card caster, List<Player> source, List<Player> dest)
		{
			return source;
		}

		public virtual List<Slot> FilterTargets(Game data, AbilityData ability, Card caster, List<Slot> source, List<Slot> dest)
		{
			return source;
		}

		public virtual List<CardData> FilterTargets(Game data, AbilityData ability, Card caster, List<CardData> source, List<CardData> dest)
		{
			return source;
		}
	}
}
