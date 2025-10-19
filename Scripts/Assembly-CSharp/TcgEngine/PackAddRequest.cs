using System;

namespace TcgEngine
{
	[Serializable]
	public class PackAddRequest
	{
		public string tid;

		public int cards;

		public int cost;

		public bool random;

		public PackAddProbability[] rarities_1st;

		public PackAddProbability[] rarities;

		public PackAddProbability[] variants;
	}
}
