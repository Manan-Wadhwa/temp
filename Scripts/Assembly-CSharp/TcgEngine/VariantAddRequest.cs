using System;

namespace TcgEngine
{
	[Serializable]
	public class VariantAddRequest
	{
		public string tid;

		public int cost_factor;

		public bool is_default;
	}
}
