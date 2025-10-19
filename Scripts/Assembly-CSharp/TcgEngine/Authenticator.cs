using System.Threading.Tasks;

namespace TcgEngine
{
	public abstract class Authenticator
	{
		protected string user_id;

		protected string username;

		protected bool logged_in;

		protected bool inited;

		public string UserID => GetUserId();

		public string Username => GetUsername();

		public UserData UserData => GetUserData();

		public virtual async Task Initialize()
		{
			inited = true;
			await Task.Yield();
		}

		public virtual async Task<bool> Login(string username)
		{
			await Task.Yield();
			return false;
		}

		public virtual async Task<bool> Login(string username, string token)
		{
			return await Login(username);
		}

		public virtual async Task<bool> RefreshLogin()
		{
			return await Login(username);
		}

		public virtual void LoginTest(string username)
		{
			user_id = username;
			this.username = username;
			logged_in = true;
		}

		public virtual async Task<bool> Register(string username, string email, string token)
		{
			return await Login(username, token);
		}

		public virtual async Task<UserData> LoadUserData()
		{
			await Task.Yield();
			return null;
		}

		public virtual async Task<bool> SaveUserData()
		{
			await Task.Yield();
			return false;
		}

		public virtual void Logout()
		{
			logged_in = false;
			user_id = null;
			username = null;
		}

		public virtual bool IsInited()
		{
			return inited;
		}

		public virtual bool IsConnected()
		{
			if (IsSignedIn())
			{
				return !IsExpired();
			}
			return false;
		}

		public virtual bool IsSignedIn()
		{
			return logged_in;
		}

		public virtual bool IsExpired()
		{
			return false;
		}

		public virtual string GetUserId()
		{
			return user_id;
		}

		public virtual string GetUsername()
		{
			return username;
		}

		public virtual int GetPermission()
		{
			if (!logged_in)
			{
				return 0;
			}
			return 1;
		}

		public virtual UserData GetUserData()
		{
			return null;
		}

		public virtual string GetError()
		{
			return "";
		}

		public bool IsTest()
		{
			return NetworkData.Get().auth_type == AuthenticatorType.LocalSave;
		}

		public bool IsApi()
		{
			return NetworkData.Get().auth_type == AuthenticatorType.Api;
		}

		public static Authenticator Create(AuthenticatorType type)
		{
			if (type == AuthenticatorType.Api)
			{
				return new AuthenticatorApi();
			}
			return new AuthenticatorTest();
		}

		public static Authenticator Get()
		{
			return TcgNetwork.Get().Auth;
		}
	}
}
