using UnityEngine;
using UnityEngine.Events;

namespace TcgEngine.UI
{
	[RequireComponent(typeof(CanvasGroup))]
	public class UIPanel : MonoBehaviour
	{
		public float display_speed = 4f;

		public UnityAction onShow;

		public UnityAction onHide;

		protected CanvasGroup canvas_group;

		protected bool visible;

		protected virtual void Awake()
		{
			canvas_group = GetComponent<CanvasGroup>();
			canvas_group.alpha = 0f;
			visible = false;
		}

		protected virtual void Start()
		{
		}

		protected virtual void Update()
		{
			float num = (visible ? display_speed : (0f - display_speed));
			float num2 = Mathf.Clamp01(canvas_group.alpha + num * Time.deltaTime);
			canvas_group.alpha = num2;
			if (!visible && num2 < 0.01f)
			{
				AfterHide();
			}
		}

		public virtual void Toggle(bool instant = false)
		{
			if (IsVisible())
			{
				Hide(instant);
			}
			else
			{
				Show(instant);
			}
		}

		public virtual void Show(bool instant = false)
		{
			visible = true;
			base.gameObject.SetActive(value: true);
			if (instant || display_speed < 0.01f)
			{
				canvas_group.alpha = 1f;
			}
			if (onShow != null)
			{
				onShow();
			}
		}

		public virtual void Hide(bool instant = false)
		{
			visible = false;
			if (instant || display_speed < 0.01f)
			{
				canvas_group.alpha = 0f;
			}
			if (onHide != null)
			{
				onHide();
			}
		}

		public void SetVisible(bool visi)
		{
			if (!visible && visi)
			{
				Show();
			}
			else if (visible && !visi)
			{
				Hide();
			}
		}

		public virtual void AfterHide()
		{
			base.gameObject.SetActive(value: false);
		}

		public bool IsVisible()
		{
			return visible;
		}

		public bool IsFullyVisible()
		{
			if (visible)
			{
				return canvas_group.alpha > 0.99f;
			}
			return false;
		}

		public float GetAlpha()
		{
			return canvas_group.alpha;
		}
	}
}
