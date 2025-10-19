using UnityEngine;

namespace TcgEngine.UI
{
	public class HoverTarget : MonoBehaviour
	{
		[TextArea(5, 7)]
		public string text;

		public float delay = 0.5f;

		public int text_size = 22;

		public int width = 350;

		public int height = 140;

		private float timer;

		private bool hover;

		private void Awake()
		{
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

		private void OnMouseEnter()
		{
			if (!GameUI.IsOverUI())
			{
				timer = 0f;
				hover = true;
			}
		}

		private void OnMouseExit()
		{
			timer = 0f;
			hover = false;
		}

		private void OnDisable()
		{
			hover = false;
		}

		public bool IsHover()
		{
			return hover;
		}
	}
}
