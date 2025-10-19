using System;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

namespace TcgEngine
{
	[Serializable]
	public struct Slot : INetworkSerializable
	{
		public int x;

		public int y;

		public int p;

		public static int x_min = 1;

		public static int x_max = 5;

		public static int y_min = 1;

		public static int y_max = 1;

		public static bool ignore_p = false;

		private static Dictionary<int, List<Slot>> player_slots = new Dictionary<int, List<Slot>>();

		private static List<Slot> all_slots = new List<Slot>();

		public static Slot None => new Slot(0, 0, 0);

		public Slot(int pid)
		{
			x = 0;
			y = 0;
			p = pid;
		}

		public Slot(int x, int y, int pid)
		{
			this.x = x;
			this.y = y;
			p = pid;
		}

		public Slot(SlotXY slot, int pid)
		{
			x = slot.x;
			y = slot.y;
			p = pid;
		}

		public bool IsInRangeX(Slot slot, int range)
		{
			return Mathf.Abs(x - slot.x) <= range;
		}

		public bool IsInRangeY(Slot slot, int range)
		{
			return Mathf.Abs(y - slot.y) <= range;
		}

		public bool IsInRangeP(Slot slot, int range)
		{
			return Mathf.Abs(p - slot.p) <= range;
		}

		public bool IsInDistanceStraight(Slot slot, int dist)
		{
			return Mathf.Abs(x - slot.x) + Mathf.Abs(y - slot.y) + Mathf.Abs(p - slot.p) <= dist;
		}

		public bool IsInDistance(Slot slot, int dist)
		{
			int num = Mathf.Abs(x - slot.x);
			int num2 = Mathf.Abs(y - slot.y);
			int num3 = Mathf.Abs(p - slot.p);
			if (num <= dist && num2 <= dist)
			{
				return num3 <= dist;
			}
			return false;
		}

		public bool IsPlayerSlot()
		{
			if (x == 0)
			{
				return y == 0;
			}
			return false;
		}

		public bool IsValid()
		{
			if (x >= x_min && x <= x_max && y >= y_min && y <= y_max)
			{
				return p >= 0;
			}
			return false;
		}

		public static int GetP(int pid)
		{
			if (!ignore_p)
			{
				return pid;
			}
			return 0;
		}

		public static Slot GetRandom(int pid, System.Random rand)
		{
			int pid2 = GetP(pid);
			if (y_max > y_min)
			{
				return new Slot(rand.Next(x_min, x_max + 1), rand.Next(y_min, y_max + 1), pid2);
			}
			return new Slot(rand.Next(x_min, x_max + 1), y_min, pid2);
		}

		public static Slot GetRandom(System.Random rand)
		{
			if (y_max > y_min)
			{
				return new Slot(rand.Next(x_min, x_max + 1), rand.Next(y_min, y_max + 1), rand.Next(0, 2));
			}
			return new Slot(rand.Next(x_min, x_max + 1), y_min, rand.Next(0, 2));
		}

		public static Slot Get(int x, int y, int p)
		{
			foreach (Slot item in GetAll())
			{
				if (item.x == x && item.y == y && item.p == p)
				{
					return item;
				}
			}
			return new Slot(x, y, p);
		}

		public static List<Slot> GetAll(int pid)
		{
			int num = GetP(pid);
			if (player_slots.ContainsKey(num))
			{
				return player_slots[num];
			}
			List<Slot> list = new List<Slot>();
			for (int i = y_min; i <= y_max; i++)
			{
				for (int j = x_min; j <= x_max; j++)
				{
					list.Add(new Slot(j, i, num));
				}
			}
			player_slots[num] = list;
			return list;
		}

		public static List<Slot> GetAll()
		{
			if (all_slots.Count > 0)
			{
				return all_slots;
			}
			for (int i = 0; i <= 1; i++)
			{
				for (int j = y_min; j <= y_max; j++)
				{
					for (int k = x_min; k <= x_max; k++)
					{
						all_slots.Add(new Slot(k, j, i));
					}
				}
			}
			return all_slots;
		}

		public static bool operator ==(Slot slot1, Slot slot2)
		{
			if (slot1.x == slot2.x && slot1.y == slot2.y)
			{
				return slot1.p == slot2.p;
			}
			return false;
		}

		public static bool operator !=(Slot slot1, Slot slot2)
		{
			if (slot1.x == slot2.x && slot1.y == slot2.y)
			{
				return slot1.p != slot2.p;
			}
			return true;
		}

		public override bool Equals(object o)
		{
			return base.Equals(o);
		}

		public override int GetHashCode()
		{
			return base.GetHashCode();
		}

		public void NetworkSerialize<T>(BufferSerializer<T> serializer) where T : IReaderWriter
		{
			serializer.SerializeValue(ref x, default(FastBufferWriter.ForPrimitives));
			serializer.SerializeValue(ref y, default(FastBufferWriter.ForPrimitives));
			serializer.SerializeValue(ref p, default(FastBufferWriter.ForPrimitives));
		}
	}
}
