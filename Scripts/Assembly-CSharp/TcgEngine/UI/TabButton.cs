using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

namespace TcgEngine.UI
{
	public class TabButton : MonoBehaviour
	{
		public string group;

		public bool active;

		public GameObject highlight;

		public UIPanel ui_panel;

		public UnityAction onClick;

		public static UnityAction<TabButton> onClickAny;

		private static List<TabButton> tab_list = new List<TabButton>();

		private void Awake()
		{
			tab_list.Add(this);
		}

		private void OnDestroy()
		{
			tab_list.Remove(this);
		}

		private void Start()
		{
			Button component = GetComponent<Button>();
			if (component != null)
			{
				component.onClick.AddListener(OnClick);
			}
			if (active && ui_panel != null)
			{
				ui_panel.Show();
			}
		}

		private void Update()
		{
			if (highlight != null)
			{
				highlight.SetActive(active);
			}
		}

		private void OnClick()
		{
			Activate();
			onClick?.Invoke();
			onClickAny?.Invoke(this);
		}

		public void Activate()
		{
			SetAll(group, act: false);
			active = true;
			if (ui_panel != null)
			{
				ui_panel.Show();
			}
		}

		public void Deactivate()
		{
			active = false;
			if (ui_panel != null)
			{
				ui_panel.Hide();
			}
		}

		public bool IsActive()
		{
			return active;
		}

		public static void SetAll(string group, bool act)
		{
			foreach (TabButton item in tab_list)
			{
				if (item.group == group)
				{
					item.active = act;
					if (item.ui_panel != null)
					{
						item.ui_panel.SetVisible(act);
					}
				}
			}
		}

		public static List<TabButton> GetAll(string group)
		{
			List<TabButton> list = new List<TabButton>();
			foreach (TabButton item in tab_list)
			{
				if (item.group == group)
				{
					list.Add(item);
				}
			}
			return list;
		}

		public static List<TabButton> GetAll()
		{
			return tab_list;
		}
	}
}
