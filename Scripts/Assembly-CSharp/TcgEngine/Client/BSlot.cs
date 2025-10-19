using System.Collections.Generic;
using UnityEngine;

namespace TcgEngine.Client
{
	public class BSlot : MonoBehaviour
	{
		protected SpriteRenderer render;

		protected Collider collide;

		protected Bounds bounds;

		protected float start_alpha;

		protected float current_alpha;

		protected float target_alpha;

		private static List<BSlot> slot_list = new List<BSlot>();

		protected virtual void Awake()
		{
			slot_list.Add(this);
			render = GetComponent<SpriteRenderer>();
			collide = GetComponent<Collider>();
			start_alpha = render.color.a;
			render.color = new Color(render.color.r, render.color.g, render.color.b, 0f);
			bounds = collide.bounds;
		}

		protected virtual void OnDestroy()
		{
			slot_list.Remove(this);
		}

		protected virtual void Update()
		{
			current_alpha = Mathf.MoveTowards(current_alpha, target_alpha * start_alpha, 2f * Time.deltaTime);
			render.color = new Color(render.color.r, render.color.g, render.color.b, current_alpha);
		}

		public virtual Slot GetSlot()
		{
			return Slot.None;
		}

		public virtual Slot GetSlot(Vector3 wpos)
		{
			return GetSlot();
		}

		public virtual Slot GetEmptySlot(Vector3 wpos)
		{
			return GetSlot();
		}

		public virtual Card GetSlotCard(Vector3 wpos)
		{
			Game gameData = GameClient.Get().GetGameData();
			Slot slot = GetSlot(wpos);
			return gameData.GetSlotCard(slot);
		}

		public virtual Vector3 GetPosition(Slot slot)
		{
			return base.transform.position;
		}

		public virtual Player GetPlayer()
		{
			return null;
		}

		public virtual bool HasSlot(Slot slot)
		{
			return GetSlot() == slot;
		}

		public virtual bool IsPlayer()
		{
			Slot slot = GetSlot();
			if (slot.x == 0)
			{
				return slot.y == 0;
			}
			return false;
		}

		public virtual bool IsInside(Vector3 wpos)
		{
			return bounds.Contains(wpos);
		}

		public static BSlot GetNearest(Vector3 pos)
		{
			BSlot result = null;
			float num = 999f;
			foreach (BSlot item in GetAll())
			{
				float magnitude = (item.transform.position - pos).magnitude;
				if (item.IsInside(pos) && magnitude < num)
				{
					num = magnitude;
					result = item;
				}
			}
			return result;
		}

		public static BSlot Get(Slot slot)
		{
			foreach (BSlot item in GetAll())
			{
				if (item.HasSlot(slot))
				{
					return item;
				}
			}
			return null;
		}

		public static List<BSlot> GetAll()
		{
			return slot_list;
		}
	}
}
