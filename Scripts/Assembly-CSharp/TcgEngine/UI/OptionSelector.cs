using UnityEngine;
using UnityEngine.UI;

namespace TcgEngine.UI
{
	public class OptionSelector : MonoBehaviour
	{
		[Header("Options")]
		public OptionString[] options;

		[Header("Display")]
		public Text select_text;

		private int position;

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
		}

		public void OnClickLeft()
		{
			position = (position + options.Length - 1) % options.Length;
			AfterChangeOption();
		}

		public void OnClickRight()
		{
			position = (position + options.Length + 1) % options.Length;
			AfterChangeOption();
		}

		public void SetIndex(int index)
		{
			position = index;
			AfterChangeOption();
		}

		public OptionString GetSelected()
		{
			return options[position];
		}

		public string GetSelectedValue()
		{
			return options[position].value;
		}

		public string GetSelectedTitle()
		{
			return options[position].title;
		}
	}
}
