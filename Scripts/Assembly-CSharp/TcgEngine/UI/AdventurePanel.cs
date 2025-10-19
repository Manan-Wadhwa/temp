using System.Collections.Generic;

namespace TcgEngine.UI
{
	public class AdventurePanel : UIPanel
	{
		private List<LevelUI> level_uis = new List<LevelUI>();

		private static AdventurePanel instance;

		protected override void Awake()
		{
			base.Awake();
			instance = this;
		}

		protected override void Start()
		{
			base.Start();
			level_uis.AddRange(GetComponentsInChildren<LevelUI>());
		}

		private void RefreshLevels()
		{
			foreach (LevelUI level_ui in level_uis)
			{
				level_ui.RefreshLevel();
			}
		}

		public override void Show(bool instant = false)
		{
			base.Show(instant);
			RefreshLevels();
		}

		public static AdventurePanel Get()
		{
			return instance;
		}
	}
}
