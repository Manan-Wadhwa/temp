using TcgEngine.Client;

namespace TcgEngine.UI
{
	public class ConnectionPanel : UIPanel
	{
		private static ConnectionPanel instance;

		protected override void Awake()
		{
			base.Awake();
			instance = this;
		}

		public void OnClickQuit()
		{
			GameClient.Get()?.Disconnect();
			SceneNav.GoTo("LoginMenu");
		}

		public static ConnectionPanel Get()
		{
			return instance;
		}
	}
}
