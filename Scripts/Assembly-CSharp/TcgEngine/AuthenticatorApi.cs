using System.Threading.Tasks;

namespace TcgEngine
{
	public class AuthenticatorApi : Authenticator
	{
		private int permission;

		public ApiClient Client => ApiClient.Get();

		public override async Task Initialize()
		{
			await base.Initialize();
		}

		public override async Task<bool> Login(string username, string password)
		{
			LoginResponse loginResponse = await Client.Login(username, password);
			if (loginResponse.success)
			{
				logged_in = true;
				user_id = loginResponse.id;
				base.username = loginResponse.username;
				permission = loginResponse.permission_level;
			}
			return loginResponse.success;
		}

		public override async Task<bool> RefreshLogin()
		{
			LoginResponse loginResponse = await Client.RefreshLogin();
			if (loginResponse.success)
			{
				logged_in = true;
				user_id = loginResponse.id;
				username = loginResponse.username;
			}
			return loginResponse.success;
		}

		public override async Task<bool> Register(string username, string email, string password)
		{
			RegisterResponse res = await Client.Register(username, email, password);
			if (res.success)
			{
				await Login(username, password);
			}
			return res.success;
		}

		public override async Task<UserData> LoadUserData()
		{
			return await Client.LoadUserData();
		}

		public override async Task<bool> SaveUserData()
		{
			await Task.Yield();
			return false;
		}

		public override void Logout()
		{
			base.Logout();
			Client.Logout();
			permission = 0;
		}

		public override UserData GetUserData()
		{
			return Client.UserData;
		}

		public override bool IsSignedIn()
		{
			return Client.IsLoggedIn();
		}

		public override bool IsExpired()
		{
			return Client.IsExpired();
		}

		public override int GetPermission()
		{
			return permission;
		}

		public override string GetError()
		{
			return Client.GetLastError();
		}
	}
}
