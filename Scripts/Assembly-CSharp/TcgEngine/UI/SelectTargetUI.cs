using TcgEngine.Client;
using UnityEngine.UI;

namespace TcgEngine.UI
{
	public class SelectTargetUI : SelectorPanel
	{
		public Text title;

		public Text desc;

		private static SelectTargetUI _instance;

		protected override void Awake()
		{
			_instance = this;
			base.Awake();
		}

		protected override void Update()
		{
			base.Update();
			Game gameData = GameClient.Get().GetGameData();
			if (gameData != null && gameData.selector == SelectorType.None)
			{
				Hide();
			}
		}

		public override void Show(AbilityData ability, Card caster)
		{
			title.text = ability.title;
			Show();
		}

		public void OnClickClose()
		{
			GameClient.Get().CancelSelection();
		}

		public override bool ShouldShow()
		{
			Game gameData = GameClient.Get().GetGameData();
			int playerID = GameClient.Get().GetPlayerID();
			if (gameData.selector == SelectorType.SelectTarget)
			{
				return gameData.selector_player_id == playerID;
			}
			return false;
		}

		public static SelectTargetUI Get()
		{
			return _instance;
		}
	}
}
