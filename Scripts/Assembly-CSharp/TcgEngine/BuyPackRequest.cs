using System;

namespace TcgEngine
{
	[Serializable]
	public struct BuyPackRequest
	{
		public string pack;

		public int quantity;
	}
}
