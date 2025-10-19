using System;

namespace TcgEngine
{
	[Serializable]
	public class CardAddRequest
	{
		public string tid;

		public string type;

		public string team;

		public string rarity;

		public int mana;

		public int attack;

		public int hp;

		public int cost;

		public string[] packs;
	}
}
