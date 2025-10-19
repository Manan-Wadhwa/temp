using UnityEngine;
using UnityEngine.EventSystems;

namespace TcgEngine.UI
{
	public class HoverTargetUI : MonoBehaviour, IPointerEnterHandler, IEventSystemHandler, IPointerExitHandler
	{
		[TextArea(5, 7)]
		public string text;

		public float delay = 0.5f;

		public int text_size = 22;

		public int width = 350;

		public int height = 140;

		private Canvas canvas;

		private RectTransform rect;

		private float timer;

		private bool hover;

		private void Awake()
		{
			canvas = GetComponentInParent<Canvas>();
			rect = canvas?.GetComponent<RectTransform>();
		}

		private void Start()
		{
			if (HoverTextBox.Get() == null)
			{
				Object.Instantiate(AssetData.Get().hover_text_box, Vector3.zero, Quaternion.identity);
			}
		}

		private void Update()
		{
			if (hover)
			{
				timer += Time.deltaTime;
				if (timer > delay)
				{
					HoverTextBox.Get().Show(this);
				}
			}
		}

		public string GetText()
		{
			return text;
		}

		public void OnPointerEnter(PointerEventData eventData)
		{
			timer = 0f;
			hover = true;
		}

		public void OnPointerExit(PointerEventData eventData)
		{
			timer = 0f;
			hover = false;
		}

		private void OnDisable()
		{
			hover = false;
		}

		public Canvas GetCanvas()
		{
			return canvas;
		}

		public RectTransform GetRect()
		{
			return rect;
		}

		public bool IsHover()
		{
			return hover;
		}
	}
}
