using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

namespace TcgEngine.UI
{
	public class OptionSelectorInt : MonoBehaviour
	{
		[Header("Options")]
		public OptionInt[] options;

		[Header("Display")]
		public Text select_text;

		public UnityAction onChange;

		private int position;

		private bool is_locked;

		private void Start()
		{
			SetIndex(0);
		}

		private void Update()
		{
		}

		private void AfterChangeOption()
		{
			if (select_text != null)
			{
				select_text.text = GetSelectedTitle();
			}
			onChange?.Invoke();
		}

		public void OnClickLeft()
		{
			if (!is_locked)
			{
				position = (position + options.Length - 1) % options.Length;
				AfterChangeOption();
			}
		}

		public void OnClickRight()
		{
			if (!is_locked)
			{
				position = (position + options.Length + 1) % options.Length;
				AfterChangeOption();
			}
		}

		public void SetIndex(int index)
		{
			position = index;
			if (select_text != null)
			{
				select_text.text = GetSelectedTitle();
			}
		}

		public void SetValue(int value)
		{
			for (int i = 0; i < options.Length; i++)
			{
				if (options[i].value == value)
				{
					position = i;
				}
			}
			if (select_text != null)
			{
				select_text.text = GetSelectedTitle();
			}
		}

		public void SetLocked(bool locked)
		{
			is_locked = locked;
		}

		public OptionInt GetSelected()
		{
			return options[position];
		}

		public int GetSelectedValue()
		{
			return options[position].value;
		}

		public string GetSelectedTitle()
		{
			if (!string.IsNullOrWhiteSpace(options[position].title))
			{
				return options[position].title;
			}
			return options[position].value.ToString();
		}
	}
}
