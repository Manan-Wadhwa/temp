using System.Collections.Generic;

namespace TcgEngine.UI
{
	public class SelectorPanel : UIPanel
	{
		private static List<SelectorPanel> panel_list = new List<SelectorPanel>();

		protected override void Awake()
		{
			base.Awake();
			panel_list.Add(this);
		}

		protected virtual void OnDestroy()
		{
			panel_list.Remove(this);
		}

		public virtual void Show(AbilityData ability, Card card)
		{
		}

		public virtual bool ShouldShow()
		{
			return false;
		}

		public static List<SelectorPanel> GetAll()
		{
			return panel_list;
		}

		public static void HideAll()
		{
			foreach (SelectorPanel item in panel_list)
			{
				if (item.IsVisible())
				{
					item.Hide();
				}
			}
		}
	}
}
