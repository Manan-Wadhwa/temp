using System.Threading.Tasks;
using TcgEngine.UI;
using UnityEngine;
using UnityEngine.UI;

namespace TcgEngine
{
	public class ChangePermission : MonoBehaviour
	{
		public string username = "admin";

		[Header("Login")]
		public InputField username_txt;

		public InputField password_txt;

		[Header("Change Permission")]
		public UIPanel permission_panel;

		public InputField target_user_txt;

		public InputField target_perm_txt;

		public Text error;

		private string logged_user;

		private void Start()
		{
			username_txt.text = username;
			error.text = "";
		}

		private async void Login(string user, string pass)
		{
			LoginResponse loginResponse = await ApiClient.Get().Login(user, pass);
			if (loginResponse.success && loginResponse.permission_level >= 10)
			{
				logged_user = user;
				permission_panel.Show();
			}
			else if (loginResponse.success)
			{
				error.text = "Not an admin user";
			}
			else
			{
				error.text = loginResponse.error;
			}
		}

		private async Task<string> GetUserID(string tuser)
		{
			string url = ApiClient.ServerURL + "/users/" + tuser;
			WebResponse webResponse = await ApiClient.Get().SendGetRequest(url);
			UserData userData = ApiTool.JsonToObject<UserData>(webResponse.data);
			if (!webResponse.success)
			{
				error.text = webResponse.error;
			}
			return webResponse.success ? userData.id : null;
		}

		private async void SetPermission(string tuser, int permission)
		{
			string text = await GetUserID(tuser);
			if (text != null)
			{
				ChangePermissionRequest data = new ChangePermissionRequest
				{
					permission_level = permission
				};
				string url = ApiClient.ServerURL + "/users/permission/edit/" + text;
				string json_data = ApiTool.ToJson(data);
				WebResponse webResponse = await ApiClient.Get().SendPostRequest(url, json_data);
				if (!webResponse.success)
				{
					error.text = webResponse.error;
				}
				if (webResponse.success)
				{
					error.text = "Success!";
					error.color = Color.green;
				}
			}
		}

		public void OnClickLogin()
		{
			if (!string.IsNullOrEmpty(username_txt.text) && !string.IsNullOrEmpty(password_txt.text))
			{
				error.text = "";
				error.color = Color.red;
				Login(username_txt.text, password_txt.text);
			}
		}

		public void OnClickUpdate()
		{
			if (!string.IsNullOrEmpty(target_user_txt.text) && int.TryParse(target_perm_txt.text, out var result) && !(logged_user == target_user_txt.text))
			{
				error.text = "";
				error.color = Color.red;
				SetPermission(target_user_txt.text, result);
			}
		}
	}
}
