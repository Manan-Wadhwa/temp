using System;

namespace TcgEngine
{
	[Serializable]
	public struct MarketResponse
	{
		public string seller;

		public string card;

		public int price;

		public int quantity;
	}
}
