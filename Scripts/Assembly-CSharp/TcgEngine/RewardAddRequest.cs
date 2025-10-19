using System;

namespace TcgEngine
{
	[Serializable]
	public class RewardAddRequest
	{
		public string tid;

		public string group;

		public int coins;

		public int xp;

		public string[] packs;

		public string[] cards;

		public string[] decks;

		public bool repeat;
	}
}
