using System;

namespace TcgEngine
{
	public class AbilityQueueElement
	{
		public AbilityData ability;

		public Card caster;

		public Card triggerer;

		public Action<AbilityData, Card, Card> callback;
	}
}
