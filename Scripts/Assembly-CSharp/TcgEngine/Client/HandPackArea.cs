using System.Collections.Generic;
using UnityEngine;

namespace TcgEngine.Client
{
	public class HandPackArea : MonoBehaviour
	{
		public RectTransform hand_area;

		public GameObject pack_template;

		public float card_spacing = 100f;

		public float card_angle = 10f;

		public float card_offset_y = 10f;

		private List<HandPack> packs = new List<HandPack>();

		private Vector3 start_pos;

		private bool is_dragging;

		private bool is_locked;

		private string last_destroyed;

		private float last_destroyed_timer;

		private static HandPackArea _instance;

		private void Awake()
		{
			_instance = this;
		}

		private void Start()
		{
			pack_template.SetActive(value: false);
			start_pos = hand_area.anchoredPosition;
			if (Authenticator.Get().IsConnected())
			{
				LoadPacks();
			}
			else
			{
				RefreshLogin();
			}
		}

		private async void RefreshLogin()
		{
			if (await Authenticator.Get().RefreshLogin())
			{
				LoadPacks();
			}
			else
			{
				SceneNav.GoTo("LoginMenu");
			}
		}

		public async void LoadPacks()
		{
			if (await Authenticator.Get().LoadUserData() != null)
			{
				RefreshPacks();
			}
		}

		public void RefreshPacks()
		{
			UserData userData = Authenticator.Get().UserData;
			UserCardData[] array = userData.packs;
			foreach (UserCardData userCardData in array)
			{
				if (PackData.Get(userCardData.tid) != null && !HasPack(userCardData.tid))
				{
					SpawnNewPack(userCardData);
				}
			}
			for (int num = packs.Count - 1; num >= 0; num--)
			{
				HandPack handPack = packs[num];
				if (handPack == null || !userData.HasPack(handPack.GetPackTid()))
				{
					packs.RemoveAt(num);
					if ((bool)handPack)
					{
						handPack.Remove();
					}
				}
			}
		}

		private void Update()
		{
			last_destroyed_timer += Time.deltaTime;
			Vector3 target = (is_locked ? (start_pos + Vector3.down * 200f) : start_pos);
			hand_area.anchoredPosition = Vector3.MoveTowards(hand_area.anchoredPosition, target, 200f * Time.deltaTime);
			int num = 0;
			float num2 = (float)packs.Count / 2f;
			foreach (HandPack pack in packs)
			{
				pack.deck_position = new Vector2(((float)num - num2) * card_spacing, ((float)num - num2) * ((float)num - num2) * (0f - card_offset_y));
				pack.deck_angle = ((float)num - num2) * (0f - card_angle);
				num++;
			}
			HandPack drag = HandPack.GetDrag();
			is_dragging = drag != null;
		}

		public void SpawnNewPack(UserCardData pack)
		{
			GameObject gameObject = Object.Instantiate(pack_template, hand_area.transform);
			gameObject.SetActive(value: true);
			gameObject.GetComponent<HandPack>().SetPack(pack);
			gameObject.GetComponent<RectTransform>().anchoredPosition = new Vector2(0f, -100f);
			packs.Add(gameObject.GetComponent<HandPack>());
		}

		public void DelayRefresh(Card card)
		{
			last_destroyed_timer = 0f;
			last_destroyed = card.uid;
		}

		public void Lock(bool locked)
		{
			is_locked = locked;
		}

		public void SortCards()
		{
			packs.Sort(SortFunc);
			int num = 0;
			foreach (HandPack pack in packs)
			{
				pack.transform.SetSiblingIndex(num);
				num++;
			}
		}

		private int SortFunc(HandPack a, HandPack b)
		{
			return a.transform.position.x.CompareTo(b.transform.position.x);
		}

		public bool HasPack(string pack_tid)
		{
			HandPack handPack = HandPack.Get(pack_tid);
			bool flag = pack_tid == last_destroyed && last_destroyed_timer < 0.5f;
			return handPack != null || flag;
		}

		public bool IsDragging()
		{
			return is_dragging;
		}

		public bool IsLocked()
		{
			return is_locked;
		}

		public static HandPackArea Get()
		{
			return _instance;
		}
	}
}
