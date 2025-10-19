using TcgEngine.Client;
using UnityEngine.UI;

namespace TcgEngine.UI
{
	public class MatchmakingPanel : UIPanel
	{
		public Text text;

		public Text players_txt;

		public Text code_txt;

		private static MatchmakingPanel instance;

		protected override void Awake()
		{
			base.Awake();
			instance = this;
		}

		protected override void Start()
		{
			base.Start();
			code_txt.text = "";
		}

		protected override void Update()
		{
			base.Update();
			if (GameClientMatchmaker.Get().IsConnected())
			{
				this.text.text = "Finding Opponent...";
			}
			else
			{
				this.text.text = "Connecting to server...";
			}
			code_txt.text = "";
			string text = GameClientMatchmaker.Get().GetGroup();
			if (text != null && text.StartsWith("code_"))
			{
				code_txt.text = text.Replace("code_", "");
			}
		}

		public void SetCount(int players)
		{
			if (players_txt != null)
			{
				players_txt.text = players + "/" + GameClientMatchmaker.Get().GetNbPlayers();
			}
		}

		public void OnClickCancel()
		{
			GameClientMatchmaker.Get().StopMatchmaking();
			Hide();
		}

		public override void Show(bool instant = false)
		{
			base.Show(instant);
			if (players_txt != null)
			{
				players_txt.text = "";
			}
		}

		public static MatchmakingPanel Get()
		{
			return instance;
		}
	}
}
