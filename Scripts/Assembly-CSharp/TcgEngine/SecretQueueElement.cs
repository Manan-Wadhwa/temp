using System;

namespace TcgEngine
{
	public class SecretQueueElement
	{
		public AbilityTrigger secret_trigger;

		public Card secret;

		public Card triggerer;

		public Action<AbilityTrigger, Card, Card> callback;
	}
}
