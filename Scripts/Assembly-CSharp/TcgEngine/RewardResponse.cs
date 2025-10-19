using System;

namespace TcgEngine
{
	[Serializable]
	public struct RewardResponse
	{
		public string tid;

		public int coins;

		public int elo;

		public int xp;

		public string[] cards;

		public string[] decks;
	}
}
