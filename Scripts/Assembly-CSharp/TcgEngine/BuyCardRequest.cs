using System;

namespace TcgEngine
{
	[Serializable]
	public struct BuyCardRequest
	{
		public string card;

		public string variant;

		public int quantity;
	}
}
