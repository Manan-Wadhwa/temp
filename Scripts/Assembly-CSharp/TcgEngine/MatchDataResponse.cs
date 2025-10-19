using System;

namespace TcgEngine
{
	[Serializable]
	public struct MatchDataResponse
	{
		public string username;

		public int rank;

		public DeckData deck;

		public RewardResponse reward;
	}
}
