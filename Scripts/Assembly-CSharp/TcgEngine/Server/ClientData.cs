namespace TcgEngine.Server
{
	public class ClientData
	{
		public ulong client_id;

		public string user_id;

		public string username;

		public string game_uid;

		public ClientData(ulong id)
		{
			client_id = id;
		}
	}
}
