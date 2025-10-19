using System;

namespace TcgEngine
{
	[Serializable]
	public struct AddMatchRequest
	{
		public string tid;

		public string[] players;

		public string mode;

		public bool ranked;
	}
}
