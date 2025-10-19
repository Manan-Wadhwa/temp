using System.Collections.Generic;
using TcgEngine.Client;
using UnityEngine;
using UnityEngine.UI;

namespace TcgEngine.UI
{
	public class AbilityButton : MonoBehaviour
	{
		public Text text;

		public Image focus_highlight;

		private Card card;

		private AbilityData iability;

		private CanvasGroup canvas_group;

		private float target_alpha;

		private bool focus;

		private bool nextfocus;

		private static List<AbilityButton> button_list = new List<AbilityButton>();

		private void Awake()
		{
			button_list.Add(this);
			canvas_group = GetComponent<CanvasGroup>();
			canvas_group.alpha = 0f;
			if (focus_highlight != null)
			{
				focus_highlight.enabled = false;
			}
		}

		private void OnDestroy()
		{
			button_list.Remove(this);
		}

		private void Update()
		{
			canvas_group.alpha = Mathf.MoveTowards(canvas_group.alpha, target_alpha, 5f * Time.deltaTime);
			focus = nextfocus;
			if (focus_highlight != null && IsVisible())
			{
				focus_highlight.enabled = focus;
			}
		}

		public void SetAbility(Card card, AbilityData iability)
		{
			this.card = card;
			this.iability = iability;
			text.text = iability.title;
			if (this.iability.mana_cost > 0)
			{
				Text obj = text;
				obj.text = obj.text + " (" + this.iability.mana_cost + ")";
			}
			canvas_group.interactable = true;
			canvas_group.blocksRaycasts = true;
			target_alpha = 1f;
		}

		public void Hide()
		{
			if (canvas_group == null)
			{
				canvas_group = GetComponent<CanvasGroup>();
			}
			card = null;
			iability = null;
			canvas_group.interactable = false;
			canvas_group.blocksRaycasts = false;
			target_alpha = 0f;
		}

		public void OnClick()
		{
			if (card != null && iability != null)
			{
				GameClient.Get().CastAbility(card, iability);
				PlayerControls.Get().UnselectAll();
			}
		}

		public AbilityData GetAbility()
		{
			return iability;
		}

		public bool IsVisible()
		{
			return canvas_group.alpha > 0.5f;
		}

		public void MouseEnter()
		{
			focus = true;
			nextfocus = true;
		}

		public void MouseExit()
		{
			nextfocus = false;
		}

		public static AbilityButton GetFocus(Vector3 pos, float range = 999f)
		{
			AbilityButton result = null;
			float num = range;
			foreach (AbilityButton item in button_list)
			{
				float magnitude = (item.transform.position - pos).magnitude;
				if (item.focus && item.IsVisible() && magnitude < num)
				{
					num = magnitude;
					result = item;
				}
			}
			return result;
		}

		public static AbilityButton GetNearest(Vector3 pos, float range = 999f)
		{
			AbilityButton result = null;
			float num = range;
			foreach (AbilityButton item in button_list)
			{
				float magnitude = (item.transform.position - pos).magnitude;
				if (magnitude < num)
				{
					num = magnitude;
					result = item;
				}
			}
			return result;
		}
	}
}
