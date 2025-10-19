using System.Threading.Tasks;
using UnityEngine;

namespace TcgEngine
{
	public class AuthenticatorTest : Authenticator
	{
		private UserData udata;

		public override async Task<bool> Login(string username)
		{
			user_id = username;
			base.username = username;
			logged_in = true;
			await Task.Yield();
			PlayerPrefs.SetString("tcg_user", username);
			return true;
		}

		public override async Task<bool> RefreshLogin()
		{
			string value = PlayerPrefs.GetString("tcg_user", "");
			if (!string.IsNullOrEmpty(value))
			{
				return await Login(value);
			}
			return false;
		}

		public override async Task<UserData> LoadUserData()
		{
			string value = PlayerPrefs.GetString("tcg_user", "");
			string filename = username + ".user";
			if (!string.IsNullOrEmpty(value) && SaveTool.DoesFileExist(filename))
			{
				udata = SaveTool.LoadFile<UserData>(filename);
			}
			if (udata == null)
			{
				udata = new UserData();
				udata.username = username;
				udata.id = username;
			}
			await Task.Yield();
			return udata;
		}

		public override async Task<bool> SaveUserData()
		{
			if (udata != null && SaveTool.IsValidFilename(username))
			{
				SaveTool.SaveFile(username + ".user", udata);
				await Task.Yield();
				return true;
			}
			return false;
		}

		public override void Logout()
		{
			base.Logout();
			udata = null;
			PlayerPrefs.DeleteKey("tcg_user");
		}

		public override UserData GetUserData()
		{
			return udata;
		}
	}
}
