using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

namespace TcgEngine.Client
{
	public class BoardSlotGroup : BSlot
	{
		public BoardSlotType type;

		public int min_x = 1;

		public int max_x = 5;

		public int y = 1;

		public float spacing = 2.5f;

		public float reduce_delay = 1f;

		private int nb_occupied;

		private List<GroupSlot> group_slots = new List<GroupSlot>();

		protected override void Awake()
		{
			base.Awake();
		}

		protected override void OnDestroy()
		{
			base.OnDestroy();
		}

		private void Start()
		{
			if (min_x < Slot.x_min || max_x > Slot.x_max || y < Slot.y_min || y > Slot.y_max)
			{
				Debug.LogError("Board Slot X and Y value must be within the min and max set for those values, check Slot.cs script to change those min/max.");
			}
			GameClient gameClient = GameClient.Get();
			gameClient.onConnectGame = (UnityAction)Delegate.Combine(gameClient.onConnectGame, new UnityAction(OnConnect));
			nb_occupied = 0;
			collide.enabled = false;
		}

		private void OnConnect()
		{
			foreach (Slot item in Slot.GetAll())
			{
				if (IsInGroup(item))
				{
					GroupSlot groupSlot = new GroupSlot();
					groupSlot.slot = item;
					groupSlot.pos = base.transform.position;
					group_slots.Add(groupSlot);
				}
			}
		}

		protected override void Update()
		{
			base.Update();
			if (!GameClient.Get().IsReady())
			{
				return;
			}
			Game gameData = GameClient.Get().GetGameData();
			HandCard drag = HandCard.GetDrag();
			bool flag = GameClient.Get().IsYourTurn();
			Card card = drag?.GetCard();
			target_alpha = 0f;
			if (flag && card != null && card.CardData.IsBoardCard())
			{
				foreach (GroupSlot group_slot in group_slots)
				{
					if (gameData.CanPlayCard(card, group_slot.slot))
					{
						target_alpha = 1f;
					}
				}
			}
			UpdateOccupied();
			UpdatePositions();
		}

		public void UpdateOccupied()
		{
			int num = 0;
			Game gameData = GameClient.Get().GetGameData();
			foreach (GroupSlot group_slot in group_slots)
			{
				Card slotCard = gameData.GetSlotCard(group_slot.slot);
				group_slot.timer += ((slotCard != null) ? 1f : (-1f)) * Time.deltaTime / reduce_delay;
				group_slot.timer = Mathf.Clamp01(group_slot.timer);
				if (group_slot.IsOccupied)
				{
					num++;
				}
			}
			nb_occupied = num;
		}

		public void UpdatePositions()
		{
			bool num = nb_occupied % 2 == 0;
			float num2 = (float)(nb_occupied / 2) * (0f - spacing);
			if (num)
			{
				num2 += spacing * 0.5f;
			}
			int num3 = 0;
			foreach (GroupSlot group_slot in group_slots)
			{
				if (group_slot.IsOccupied)
				{
					group_slot.pos = base.transform.position + Vector3.right * ((float)num3 * spacing + num2);
					num3++;
				}
				else
				{
					group_slot.pos = base.transform.position + Vector3.right * ((float)nb_occupied * spacing + num2);
				}
			}
		}

		public bool IsInGroup(Slot slot)
		{
			return IsInGroup(slot.x, slot.y, slot.p);
		}

		public bool IsInGroup(int x, int y)
		{
			Slot slotMin = GetSlotMin();
			Slot slotMax = GetSlotMax();
			if (x >= slotMin.x && x <= slotMax.x && y >= slotMin.y)
			{
				return y <= slotMax.y;
			}
			return false;
		}

		public bool IsInGroup(int x, int y, int p)
		{
			Slot slotMin = GetSlotMin();
			Slot slotMax = GetSlotMax();
			if (x >= slotMin.x && x <= slotMax.x && y >= slotMin.y && y <= slotMax.y && p >= slotMin.p)
			{
				return p <= slotMax.p;
			}
			return false;
		}

		public Slot GetSlotMin()
		{
			return GetSlot(min_x, y);
		}

		public Slot GetSlotMax()
		{
			return GetSlot(max_x, y);
		}

		public Slot GetSlot(int x, int y)
		{
			int pid = 0;
			if (type == BoardSlotType.FlipX)
			{
				int playerID = GameClient.Get().GetPlayerID();
				int x2 = x;
				if (playerID % 2 == 1)
				{
					x2 = Slot.x_max - x + Slot.x_min;
				}
				return new Slot(x2, y, pid);
			}
			if (type == BoardSlotType.FlipY)
			{
				int playerID2 = GameClient.Get().GetPlayerID();
				int num = y;
				if (playerID2 % 2 == 1)
				{
					num = Slot.y_max - y + Slot.y_min;
				}
				return new Slot(x, num, pid);
			}
			if (type == BoardSlotType.PlayerSelf)
			{
				pid = GameClient.Get().GetPlayerID();
			}
			if (type == BoardSlotType.PlayerOpponent)
			{
				pid = GameClient.Get().GetOpponentPlayerID();
			}
			return new Slot(x, y, pid);
		}

		public override Slot GetSlot(Vector3 wpos)
		{
			GroupSlot groupSlot = null;
			float num = 99f;
			foreach (GroupSlot group_slot in group_slots)
			{
				float magnitude = (group_slot.pos - wpos).magnitude;
				if (magnitude < num)
				{
					num = magnitude;
					groupSlot = group_slot;
				}
			}
			return groupSlot?.slot ?? Slot.None;
		}

		public virtual Slot GetSlotOccupied(Vector3 wpos)
		{
			GroupSlot groupSlot = null;
			float num = 99f;
			foreach (GroupSlot group_slot in group_slots)
			{
				float magnitude = (group_slot.pos - wpos).magnitude;
				if (group_slot.IsOccupied && magnitude < num)
				{
					num = magnitude;
					groupSlot = group_slot;
				}
			}
			return groupSlot?.slot ?? Slot.None;
		}

		public override Card GetSlotCard(Vector3 wpos)
		{
			Game gameData = GameClient.Get().GetGameData();
			Slot slotOccupied = GetSlotOccupied(wpos);
			if (slotOccupied != Slot.None)
			{
				return gameData.GetSlotCard(slotOccupied);
			}
			return null;
		}

		public override bool HasSlot(Slot slot)
		{
			foreach (GroupSlot group_slot in group_slots)
			{
				if (group_slot.slot == slot)
				{
					return true;
				}
			}
			return false;
		}

		public override Vector3 GetPosition(Slot slot)
		{
			foreach (GroupSlot group_slot in group_slots)
			{
				if (group_slot.slot == slot)
				{
					return group_slot.pos;
				}
			}
			return base.transform.position;
		}

		public override Slot GetEmptySlot(Vector3 wpos)
		{
			foreach (GroupSlot group_slot in group_slots)
			{
				if (!group_slot.IsOccupied)
				{
					return group_slot.slot;
				}
			}
			return Slot.None;
		}
	}
}
