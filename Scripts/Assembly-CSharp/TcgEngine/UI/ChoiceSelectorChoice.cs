using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

namespace TcgEngine.UI
{
	public class ChoiceSelectorChoice : MonoBehaviour
	{
		public Text title;

		public Text subtitle;

		public Image highlight;

		public UnityAction<int> onClick;

		private Button button;

		private int choice;

		private bool focus;

		private void Awake()
		{
			button = GetComponent<Button>();
			button.onClick.AddListener(OnClick);
		}

		private void Update()
		{
			if (highlight != null)
			{
				highlight.enabled = focus;
			}
		}

		public void SetChoice(int choice, AbilityData ability)
		{
			this.choice = choice;
			title.text = ability.title;
			subtitle.text = ability.desc;
			button.interactable = true;
			base.gameObject.SetActive(value: true);
			if (ability.mana_cost > 0)
			{
				Text text = title;
				text.text = text.text + " (" + ability.mana_cost + ")";
			}
		}

		public void SetInteractable(bool interact)
		{
			button.interactable = interact;
		}

		public void Hide()
		{
			base.gameObject.SetActive(value: false);
		}

		public void OnClick()
		{
			onClick?.Invoke(choice);
		}

		public void MouseEnter()
		{
			if (button.interactable)
			{
				focus = true;
			}
		}

		public void MouseExit()
		{
			focus = false;
		}
	}
}
