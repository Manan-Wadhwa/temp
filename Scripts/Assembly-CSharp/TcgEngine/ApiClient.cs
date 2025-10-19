using System;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.Networking;

namespace TcgEngine
{
	public class ApiClient : MonoBehaviour
	{
		public bool is_server;

		public UnityAction<RegisterResponse> onRegister;

		public UnityAction<LoginResponse> onLogin;

		public UnityAction<LoginResponse> onRefresh;

		public UnityAction onLogout;

		private string user_id = "";

		private string username = "";

		private string access_token = "";

		private string refresh_token = "";

		private string api_version = "";

		private bool logged_in;

		private bool expired;

		private UserData udata;

		private int sending;

		private string last_error = "";

		private float refresh_timer;

		private float online_timer;

		private long expiration_timestamp;

		private const float online_duration = 300f;

		private static ApiClient instance;

		public UserData UserData => udata;

		public string UserID
		{
			get
			{
				return user_id;
			}
			set
			{
				user_id = value;
			}
		}

		public string Username
		{
			get
			{
				return username;
			}
			set
			{
				username = value;
			}
		}

		public string AccessToken
		{
			get
			{
				return access_token;
			}
			set
			{
				access_token = value;
			}
		}

		public string RefreshToken
		{
			get
			{
				return refresh_token;
			}
			set
			{
				refresh_token = value;
			}
		}

		public string ServerVersion => api_version;

		public string ClientVersion => Application.version;

		public static string ServerURL
		{
			get
			{
				NetworkData networkData = NetworkData.Get();
				return (networkData.api_https ? "https://" : "http://") + networkData.api_url;
			}
		}

		private void Awake()
		{
			if (instance == null)
			{
				instance = this;
			}
			LoadTokens();
		}

		private void Update()
		{
			Refresh();
		}

		private void LoadTokens()
		{
			if (!is_server && string.IsNullOrEmpty(user_id))
			{
				access_token = PlayerPrefs.GetString("tcg_access_token");
				refresh_token = PlayerPrefs.GetString("tcg_refresh_token");
			}
		}

		private void SaveTokens()
		{
			if (!is_server)
			{
				PlayerPrefs.SetString("tcg_access_token", access_token);
				PlayerPrefs.SetString("tcg_refresh_token", refresh_token);
			}
		}

		private async void Refresh()
		{
			if (logged_in)
			{
				if (!expired)
				{
					long timestamp = GetTimestamp();
					expired = timestamp > expiration_timestamp - 10;
				}
				refresh_timer += Time.deltaTime;
				if (expired && refresh_timer > 5f)
				{
					refresh_timer = 0f;
					await RefreshLogin();
				}
				online_timer += Time.deltaTime;
				if (!expired && online_timer > 300f)
				{
					online_timer = 0f;
					await KeepOnline();
				}
			}
		}

		public async Task<RegisterResponse> Register(string email, string user, string password)
		{
			return await Register(new RegisterRequest
			{
				email = email,
				username = user,
				password = password,
				avatar = ""
			});
		}

		public async Task<RegisterResponse> Register(RegisterRequest data)
		{
			Logout();
			string url = ServerURL + "/users/register";
			string json_data = ApiTool.ToJson(data);
			WebResponse webResponse = await SendPostRequest(url, json_data);
			RegisterResponse registerResponse = ApiTool.JsonToObject<RegisterResponse>(webResponse.data);
			registerResponse.success = webResponse.success;
			registerResponse.error = webResponse.error;
			onRegister?.Invoke(registerResponse);
			return registerResponse;
		}

		public async Task<LoginResponse> Login(string user, string password)
		{
			Logout();
			LoginRequest loginRequest = new LoginRequest
			{
				password = password
			};
			if (user.Contains("@"))
			{
				loginRequest.email = user;
			}
			else
			{
				loginRequest.username = user;
			}
			string url = ServerURL + "/auth";
			string json_data = ApiTool.ToJson(loginRequest);
			LoginResponse loginRes = GetLoginRes(await SendPostRequest(url, json_data));
			AfterLogin(loginRes);
			onLogin?.Invoke(loginRes);
			return loginRes;
		}

		public async Task<LoginResponse> RefreshLogin()
		{
			string url = ServerURL + "/auth/refresh";
			string json_data = ApiTool.ToJson(new AutoLoginRequest
			{
				refresh_token = refresh_token
			});
			LoginResponse loginRes = GetLoginRes(await SendPostRequest(url, json_data));
			AfterLogin(loginRes);
			onRefresh?.Invoke(loginRes);
			return loginRes;
		}

		private LoginResponse GetLoginRes(WebResponse res)
		{
			LoginResponse result = ApiTool.JsonToObject<LoginResponse>(res.data);
			result.success = res.success;
			result.error = res.error;
			return result;
		}

		private void AfterLogin(LoginResponse login_res)
		{
			last_error = login_res.error;
			if (login_res.success)
			{
				user_id = login_res.id;
				username = login_res.username;
				access_token = login_res.access_token;
				refresh_token = login_res.refresh_token;
				api_version = login_res.version;
				expiration_timestamp = GetTimestamp() + login_res.duration;
				refresh_timer = 0f;
				online_timer = 0f;
				logged_in = true;
				expired = false;
				SaveTokens();
			}
		}

		public async Task<UserData> LoadUserData()
		{
			udata = await LoadUserData(username);
			return udata;
		}

		public async Task<UserData> LoadUserData(string username)
		{
			if (!IsConnected())
			{
				return null;
			}
			string url = ServerURL + "/users/" + username;
			WebResponse webResponse = await SendGetRequest(url);
			UserData result = null;
			if (webResponse.success)
			{
				result = ApiTool.JsonToObject<UserData>(webResponse.data);
			}
			return result;
		}

		public async Task<bool> KeepOnline()
		{
			if (!IsConnected())
			{
				return false;
			}
			string url = ServerURL + "/auth/keep";
			WebResponse webResponse = await SendGetRequest(url);
			expired = !webResponse.success;
			return webResponse.success;
		}

		public async Task<bool> Validate()
		{
			if (!IsConnected())
			{
				return false;
			}
			string url = ServerURL + "/auth/validate";
			WebResponse webResponse = await SendGetRequest(url);
			expired = !webResponse.success;
			return webResponse.success;
		}

		public void Logout()
		{
			user_id = "";
			username = "";
			access_token = "";
			refresh_token = "";
			api_version = "";
			last_error = "";
			logged_in = false;
			onLogout?.Invoke();
			SaveTokens();
		}

		public async void CreateMatch(Game game_data)
		{
			if (game_data.settings.game_type == GameType.Multiplayer)
			{
				AddMatchRequest addMatchRequest = new AddMatchRequest
				{
					players = new string[2]
				};
				addMatchRequest.players[0] = game_data.players[0].username;
				addMatchRequest.players[1] = game_data.players[1].username;
				addMatchRequest.tid = game_data.game_uid;
				addMatchRequest.ranked = game_data.settings.IsRanked();
				addMatchRequest.mode = game_data.settings.GetGameModeId();
				string url = ServerURL + "/matches/add";
				string json_data = ApiTool.ToJson(addMatchRequest);
				Debug.Log("Match Started! " + (await SendPostRequest(url, json_data)).success);
			}
		}

		public async void EndMatch(Game game_data, int winner_id)
		{
			if (game_data.settings.game_type == GameType.Multiplayer)
			{
				Player player = game_data.GetPlayer(winner_id);
				CompleteMatchRequest completeMatchRequest = new CompleteMatchRequest
				{
					tid = game_data.game_uid,
					winner = ((player != null) ? player.username : "")
				};
				string url = ServerURL + "/matches/complete";
				string json_data = ApiTool.ToJson(completeMatchRequest);
				Debug.Log("Match Completed! " + (await SendPostRequest(url, json_data)).success);
			}
		}

		public async Task<string> SendGetVersion()
		{
			string url = ServerURL + "/version";
			WebResponse webResponse = await SendGetRequest(url);
			if (webResponse.success)
			{
				api_version = ApiTool.JsonToObject<VersionResponse>(webResponse.data).version;
				return api_version;
			}
			return null;
		}

		public async Task<WebResponse> SendGetRequest(string url)
		{
			return await SendRequest(url, "GET");
		}

		public async Task<WebResponse> SendPostRequest(string url, string json_data)
		{
			return await SendRequest(url, "POST", json_data);
		}

		public async Task<WebResponse> SendRequest(string url, string method, string json_data = "")
		{
			UnityWebRequest request = WebRequest.Create(url, method, json_data, access_token);
			return await SendRequest(request);
		}

		private async Task<WebResponse> SendRequest(UnityWebRequest request)
		{
			int wait = 0;
			int wait_max = request.timeout * 1000;
			request.timeout++;
			sending++;
			UnityWebRequestAsyncOperation async_oper = request.SendWebRequest();
			while (!async_oper.isDone)
			{
				await TimeTool.Delay(200);
				wait += 200;
				if (wait >= wait_max)
				{
					request.Abort();
				}
			}
			WebResponse response = WebRequest.GetResponse(request);
			response.error = GetError(response);
			last_error = response.error;
			request.Dispose();
			sending--;
			return response;
		}

		private string GetError(WebResponse res)
		{
			if (res.success)
			{
				return "";
			}
			ErrorResponse errorResponse = ApiTool.JsonToObject<ErrorResponse>(res.data);
			if (errorResponse != null)
			{
				return errorResponse.error;
			}
			return res.error;
		}

		public bool IsConnected()
		{
			if (logged_in)
			{
				return !expired;
			}
			return false;
		}

		public bool IsLoggedIn()
		{
			return logged_in;
		}

		public bool IsExpired()
		{
			return expired;
		}

		public bool IsBusy()
		{
			return sending > 0;
		}

		public long GetTimestamp()
		{
			return DateTimeOffset.UtcNow.ToUnixTimeSeconds();
		}

		public string GetLastRequest()
		{
			return last_error;
		}

		public string GetLastError()
		{
			return last_error;
		}

		public bool IsVersionValid()
		{
			return ClientVersion == ServerVersion;
		}

		public static ApiClient Get()
		{
			if (instance == null)
			{
				instance = UnityEngine.Object.FindObjectOfType<ApiClient>();
			}
			return instance;
		}
	}
}
