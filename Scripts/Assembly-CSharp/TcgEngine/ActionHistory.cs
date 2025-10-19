using System;

namespace TcgEngine
{
	[Serializable]
	public class ActionHistory
	{
		public ushort type;

		public string card_id;

		public string card_uid;

		public string target_uid;

		public string ability_id;

		public int target_id;

		public Slot slot;
	}
}
