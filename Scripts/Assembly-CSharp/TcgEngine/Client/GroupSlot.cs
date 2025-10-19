using UnityEngine;

namespace TcgEngine.Client
{
	public class GroupSlot
	{
		public Slot slot;

		public Vector3 pos;

		public float timer;

		public bool IsOccupied => timer > 0.01f;
	}
}
