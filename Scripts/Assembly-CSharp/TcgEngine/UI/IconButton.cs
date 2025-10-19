using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

namespace TcgEngine.UI
{
	public class IconButton : MonoBehaviour
	{
		public string group;

		public string value;

		public Image active_img;

		public Image disabled_img;

		public bool on_if_all_off;

		public UnityAction<IconButton> onClick;

		private bool active;

		private Button button;

		private static List<IconButton> toggle_list = new List<IconButton>();

		private void Awake()
		{
			toggle_list.Add(this);
			button = GetComponent<Button>();
			button.onClick.AddListener(OnClick);
			if (!on_if_all_off && active_img != null)
			{
				active_img.enabled = false;
			}
		}

		private void OnDestroy()
		{
			toggle_list.Remove(this);
		}

		private void Start()
		{
		}

		private void Update()
		{
			if (on_if_all_off && active_img != null && IsAllOff(group))
			{
				active_img.enabled = true;
			}
		}

		private void OnClick()
		{
			bool num = active;
			DeactivateAll(group);
			if (!num)
			{
				Activate();
			}
			if (onClick != null)
			{
				onClick(this);
			}
		}

		public void SetActive(bool act)
		{
			if (act)
			{
				Activate();
			}
			else
			{
				Deactivate();
			}
		}

		public void Activate()
		{
			active = true;
			if (active_img != null)
			{
				active_img.enabled = true;
			}
		}

		public void Deactivate()
		{
			active = false;
			if (active_img != null)
			{
				active_img.enabled = false;
			}
		}

		public bool IsActive()
		{
			return active;
		}

		public static bool IsAllOff(string group)
		{
			bool result = true;
			foreach (IconButton item in toggle_list)
			{
				if (item.group == group && item.IsActive())
				{
					result = false;
				}
			}
			return result;
		}

		public static void DeactivateAll(string group)
		{
			foreach (IconButton item in toggle_list)
			{
				if (item.group == group)
				{
					item.Deactivate();
				}
			}
		}

		public static List<IconButton> GetAll(string group)
		{
			List<IconButton> list = new List<IconButton>();
			foreach (IconButton item in toggle_list)
			{
				if (item.group == group)
				{
					list.Add(item);
				}
			}
			return list;
		}
	}
}
