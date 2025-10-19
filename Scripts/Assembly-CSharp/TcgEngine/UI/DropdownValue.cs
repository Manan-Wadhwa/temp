using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

namespace TcgEngine.UI
{
	[RequireComponent(typeof(Dropdown))]
	public class DropdownValue : MonoBehaviour
	{
		public UnityAction<int, string> onValueChanged;

		private List<DropdownValueItem> values = new List<DropdownValueItem>();

		private Dropdown dropdown;

		public bool interactable
		{
			get
			{
				return dropdown.interactable;
			}
			set
			{
				dropdown.interactable = value;
			}
		}

		public int value
		{
			get
			{
				return dropdown.value;
			}
			set
			{
				dropdown.value = value;
				dropdown.RefreshShownValue();
			}
		}

		private void Awake()
		{
			dropdown = GetComponent<Dropdown>();
			dropdown.onValueChanged.AddListener(OnChangeValue);
		}

		private void Start()
		{
		}

		public void AddOption(string id, string text)
		{
			Dropdown.OptionData item = new Dropdown.OptionData(text);
			dropdown.options.Add(item);
			DropdownValueItem dropdownValueItem = new DropdownValueItem();
			dropdownValueItem.id = id;
			dropdownValueItem.text = text;
			values.Add(dropdownValueItem);
			dropdown.RefreshShownValue();
		}

		public void ClearOptions()
		{
			values.Clear();
			dropdown.ClearOptions();
		}

		public void SetValue(string value)
		{
			int num = 0;
			foreach (DropdownValueItem value2 in values)
			{
				if (value2.id == value)
				{
					dropdown.value = num;
				}
				num++;
			}
		}

		private void OnChangeValue(int selected_index)
		{
			if (selected_index >= 0 && selected_index < values.Count)
			{
				DropdownValueItem dropdownValueItem = values[selected_index];
				if (onValueChanged != null)
				{
					onValueChanged(selected_index, dropdownValueItem.id);
				}
			}
		}

		public DropdownValueItem GetSelected()
		{
			if (dropdown.value >= 0 && dropdown.value < values.Count)
			{
				return values[dropdown.value];
			}
			return null;
		}

		public string GetSelectedValue()
		{
			DropdownValueItem selected = GetSelected();
			if (selected != null)
			{
				return selected.id;
			}
			return "";
		}

		public string GetSelectedText()
		{
			DropdownValueItem selected = GetSelected();
			if (selected != null)
			{
				return selected.text;
			}
			return "";
		}
	}
}
