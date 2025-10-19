namespace TcgEngine.Server
{
	public class MatchData
	{
		public string group;

		public string game_uid;

		public string server_url;

		public bool ended;

		public string[] players;

		public MatchData(string grp, string uid, string url, int players)
		{
			group = grp;
			game_uid = uid;
			server_url = url;
			this.players = new string[players];
		}
	}
}
