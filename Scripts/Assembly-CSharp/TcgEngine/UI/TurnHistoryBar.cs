using TcgEngine.Client;
using UnityEngine;

namespace TcgEngine.UI
{
	public class TurnHistoryBar : MonoBehaviour
	{
		public bool is_opponent;

		public TurnHistoryLine[] history_lines;

		private void Start()
		{
		}

		private void Update()
		{
			if (!GameClient.Get().IsReady())
			{
				return;
			}
			int id = (is_opponent ? GameClient.Get().GetOpponentPlayerID() : GameClient.Get().GetPlayerID());
			Player player = GameClient.Get().GetGameData().GetPlayer(id);
			if (player == null || player.history_list == null)
			{
				return;
			}
			int i = 0;
			foreach (ActionHistory item in player.history_list)
			{
				if (i < history_lines.Length)
				{
					history_lines[i].SetLine(item);
					i++;
				}
			}
			for (; i < history_lines.Length; i++)
			{
				history_lines[i].Hide();
			}
		}
	}
}
