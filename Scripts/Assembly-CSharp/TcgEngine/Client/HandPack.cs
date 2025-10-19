using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace TcgEngine.Client
{
	public class HandPack : MonoBehaviour
	{
		public Image pack_sprite;

		public Image pack_glow;

		public Text pack_quantity;

		public float move_speed = 10f;

		public float move_rotate_speed = 4f;

		public float move_max_rotate = 10f;

		[HideInInspector]
		public Vector2 deck_position;

		[HideInInspector]
		public float deck_angle;

		[Header("FX")]
		public GameObject pack_open_fx;

		public AudioClip pack_open_audio;

		private string pack_tid = "";

		private int quantity;

		private RectTransform hand_transform;

		private RectTransform card_transform;

		private Vector3 start_scale;

		private float current_alpha;

		private Vector3 current_rotate;

		private Vector3 target_rotate;

		private Vector3 prev_pos;

		private bool destroyed;

		private float focus_timer;

		private bool focus;

		private bool drag;

		private static List<HandPack> pack_list = new List<HandPack>();

		public PackData PackData => GetPackData();

		private void Awake()
		{
			pack_list.Add(this);
			card_transform = base.transform.GetComponent<RectTransform>();
			hand_transform = base.transform.parent.GetComponent<RectTransform>();
			start_scale = base.transform.localScale;
		}

		private void Start()
		{
		}

		private void OnDestroy()
		{
			pack_list.Remove(this);
		}

		private void Update()
		{
			focus_timer += Time.deltaTime;
			Vector2 b = deck_position;
			Vector3 b2 = start_scale;
			float target = 1f;
			bool flag = HandPackArea.Get().IsDragging();
			if (focus && focus_timer > 0.5f)
			{
				b = deck_position + Vector2.up * 40f;
			}
			if (drag)
			{
				b = GetTargetPosition();
				b2 = start_scale * 0.8f;
				Vector3 vector = card_transform.position - prev_pos;
				Vector3 vector2 = new Vector3(vector.y * 90f, (0f - vector.x) * 90f, 0f);
				target_rotate += vector2 * move_rotate_speed * Time.deltaTime;
				target_rotate = new Vector3(Mathf.Clamp(target_rotate.x, 0f - move_max_rotate, move_max_rotate), Mathf.Clamp(target_rotate.y, 0f - move_max_rotate, move_max_rotate), 0f);
				current_rotate = Vector3.Lerp(current_rotate, target_rotate, move_rotate_speed * Time.deltaTime);
				move_speed = 9f;
				target = 0.8f;
			}
			else
			{
				target_rotate = new Vector3(0f, 0f, deck_angle);
				current_rotate = new Vector3(0f, 0f, deck_angle);
			}
			card_transform.anchoredPosition = Vector2.Lerp(card_transform.anchoredPosition, b, Time.deltaTime * move_speed);
			card_transform.rotation = Quaternion.Slerp(card_transform.rotation, Quaternion.Euler(current_rotate), Time.deltaTime * move_speed);
			card_transform.localScale = Vector3.Lerp(card_transform.localScale, b2, 4f * Time.deltaTime);
			pack_glow.enabled = (focus && !flag) || drag;
			current_alpha = Mathf.MoveTowards(current_alpha, target, 2f * Time.deltaTime);
			pack_sprite.color = new Color(1f, 1f, 1f, current_alpha);
			pack_glow.color = new Color(pack_glow.color.r, pack_glow.color.g, pack_glow.color.b, current_alpha * 0.8f);
			pack_quantity.text = quantity.ToString();
			prev_pos = Vector3.Lerp(prev_pos, card_transform.position, 1f * Time.deltaTime);
		}

		private Vector2 GetTargetPosition()
		{
			RectTransformUtility.ScreenPointToLocalPointInRectangle(hand_transform, Input.mousePosition, Camera.main, out var localPoint);
			return localPoint;
		}

		public void SetPack(UserCardData pack)
		{
			pack_tid = pack.tid;
			quantity = pack.quantity;
			PackData packData = PackData.Get(pack.tid);
			if ((bool)packData)
			{
				pack_sprite.sprite = packData.pack_img;
			}
		}

		public void OpenPack()
		{
			FXTool.DoFX(pack_open_fx, base.transform.position);
			AudioTool.Get().PlaySFX("pack_open", pack_open_audio);
			Object.Destroy(base.gameObject);
			OpenPackMenu.Get().OpenPack(pack_tid);
		}

		public void Remove()
		{
			quantity--;
			if (quantity <= 0)
			{
				Kill();
			}
		}

		public void Kill()
		{
			if (!destroyed)
			{
				destroyed = true;
				Object.Destroy(base.gameObject);
			}
		}

		public bool IsFocus()
		{
			if (focus)
			{
				return !drag;
			}
			return false;
		}

		public bool IsDrag()
		{
			return drag;
		}

		public PackData GetPackData()
		{
			return PackData.Get(pack_tid);
		}

		public string GetPackTid()
		{
			return pack_tid;
		}

		public int GetPackQuantity()
		{
			return Authenticator.Get().UserData.GetPackQuantity(pack_tid);
		}

		public void OnMouseEnterCard()
		{
			if (!HandPackArea.Get().IsLocked())
			{
				focus = true;
			}
		}

		public void OnMouseExitCard()
		{
			focus = false;
			focus_timer = 0f;
		}

		public void OnMouseDownCard()
		{
			if (!HandPackArea.Get().IsLocked())
			{
				drag = true;
				AudioTool.Get().PlaySFX("hand_card", AssetData.Get().hand_card_click_audio);
			}
		}

		public void OnMouseUpCard()
		{
			Vector3 vector = MouseToWorld(Input.mousePosition);
			if (drag && vector.y > -2.5f)
			{
				OpenPack();
			}
			else
			{
				HandPackArea.Get().SortCards();
			}
			drag = false;
		}

		public Vector3 MouseToWorld(Vector3 mouse_pos)
		{
			Vector3 result = Camera.main.ScreenToWorldPoint(mouse_pos);
			result.z = 0f;
			return result;
		}

		public static HandPack GetDrag()
		{
			foreach (HandPack item in pack_list)
			{
				if (item.IsDrag())
				{
					return item;
				}
			}
			return null;
		}

		public static HandPack GetFocus()
		{
			foreach (HandPack item in pack_list)
			{
				if (item.IsFocus())
				{
					return item;
				}
			}
			return null;
		}

		public static HandPack Get(string uid)
		{
			foreach (HandPack item in pack_list)
			{
				if ((bool)item && item.GetPackTid() == uid)
				{
					return item;
				}
			}
			return null;
		}

		public static List<HandPack> GetAll()
		{
			return pack_list;
		}
	}
}
