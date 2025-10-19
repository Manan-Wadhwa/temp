using System;

namespace TcgEngine
{
	[Serializable]
	public struct MatchResponse
	{
		public string tid;

		public string[] players;

		public DateTime start;

		public DateTime end;

		public string winner;

		public bool completed;

		public MatchDataResponse[] udata;
	}
}
