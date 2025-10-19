using System;

namespace TcgEngine
{
	public class AttackQueueElement
	{
		public Card attacker;

		public Card target;

		public Player ptarget;

		public bool skip_cost;

		public Action<Card, Card, bool> callback;

		public Action<Card, Player, bool> pcallback;
	}
}
