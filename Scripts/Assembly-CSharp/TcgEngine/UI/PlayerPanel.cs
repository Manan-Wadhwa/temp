using System;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

namespace TcgEngine.UI
{
	public class PlayerPanel : UIPanel
	{
		[Header("Player")]
		public Text player_name;

		public Text player_level;

		public AvatarUI avatar;

		public CardbackUI cardback;

		public Text elo;

		public Text winrate;

		public Text cards_all;

		public Text victories;

		public Text defeats;

		[Header("Bottom bar")]
		public GameObject buttons_area;

		public GameObject account_button;

		[Header("Avatars")]
		public UIPanel avatar_panel;

		public AvatarUI[] avatars;

		[Header("Cardbacks")]
		public UIPanel cardback_panel;

		public CardbackUI[] cardbacks;

		[Header("Edit Panel")]
		public UIPanel edit_panel;

		public InputField user_email;

		public InputField user_password_prev;

		public InputField user_password_new;

		public InputField user_password_confirm;

		public Button edit_change_email;

		public Button edit_change_password;

		public Button resend_button;

		public Button confirm_button;

		public Text edit_error;

		private string username;

		private UserData user_data;

		private static PlayerPanel instance;

		protected override void Awake()
		{
			base.Awake();
			instance = this;
			AvatarUI[] array = avatars;
			foreach (AvatarUI obj in array)
			{
				obj.onClick = (UnityAction<AvatarData>)Delegate.Combine(obj.onClick, new UnityAction<AvatarData>(OnClickAvatar));
			}
			CardbackUI[] array2 = cardbacks;
			foreach (CardbackUI obj2 in array2)
			{
				obj2.onClick = (UnityAction<CardbackData>)Delegate.Combine(obj2.onClick, new UnityAction<CardbackData>(OnClickCardback));
			}
		}

		protected override void Update()
		{
			base.Update();
		}

		protected override void Start()
		{
			base.Start();
		}

		private async void LoadData()
		{
			if (IsYou())
			{
				user_data = Authenticator.Get().UserData;
			}
			else
			{
				user_data = await ApiClient.Get().LoadUserData(username);
			}
			RefreshPanel();
		}

		private void ClearPanel()
		{
			player_name.text = "";
			elo.text = "";
			winrate.text = "";
			player_level.text = "";
			avatar.Hide();
			cardback.Hide();
		}

		private void RefreshPanel()
		{
			avatar_panel.Hide();
			if (user_data != null)
			{
				UserData userData = user_data;
				player_name.text = userData.username;
				player_level.text = GameplayData.Get().GetPlayerLevel(userData.xp).ToString();
				AvatarData avatarData = AvatarData.Get(userData.avatar);
				avatar.SetAvatar(avatarData);
				CardbackData cardbackData = CardbackData.Get(userData.cardback);
				cardback.SetCardback(cardbackData);
				int num = ((userData.matches > 0) ? Mathf.RoundToInt((float)userData.victories * 100f / (float)userData.matches) : 0);
				winrate.text = num + "%";
				elo.text = userData.elo.ToString();
				victories.text = userData.victories.ToString();
				defeats.text = userData.defeats.ToString();
				cards_all.text = userData.CountUniqueCards() + " / " + CardData.GetAllDeckbuilding().Count;
				buttons_area?.SetActive(IsYou());
				account_button?.SetActive(Authenticator.Get().IsApi());
			}
		}

		private void RefreshAvatarList()
		{
			AvatarUI[] array = avatars;
			for (int i = 0; i < array.Length; i++)
			{
				array[i].SetDefaultAvatar();
			}
			int num = 0;
			foreach (AvatarData item in AvatarData.GetAll())
			{
				if (num < avatars.Length)
				{
					AvatarUI avatarUI = avatars[num];
					if (item != null)
					{
						avatarUI.SetAvatar(item);
						num++;
					}
				}
			}
		}

		private void RefreshCardBackList()
		{
			CardbackUI[] array = cardbacks;
			for (int i = 0; i < array.Length; i++)
			{
				array[i].Hide();
			}
			int num = 0;
			foreach (CardbackData item in CardbackData.GetAll())
			{
				if (num < cardbacks.Length)
				{
					CardbackUI cardbackUI = cardbacks[num];
					if (item != null)
					{
						cardbackUI.SetCardback(item);
						num++;
					}
				}
			}
		}

		private void OnClickAvatar(AvatarData avatar)
		{
			user_data = Authenticator.Get().UserData;
			if (avatar != null && user_data != null && IsYou())
			{
				user_data.avatar = avatar.id;
				RefreshPanel();
				SaveUserAvatar(avatar);
				avatar_panel.Hide();
			}
		}

		private void OnClickCardback(CardbackData cb)
		{
			user_data = Authenticator.Get().UserData;
			if (cb != null && user_data != null && IsYou())
			{
				user_data.cardback = cb.id;
				RefreshPanel();
				SaveUserCardback(cb);
				cardback_panel.Hide();
			}
		}

		private async void SaveUserAvatar(AvatarData avatar)
		{
			if (ApiClient.Get().IsConnected())
			{
				string url = ApiClient.ServerURL + "/users/edit/" + ApiClient.Get().UserID;
				string json_data = ApiTool.ToJson(new EditUserRequest
				{
					avatar = avatar.id
				});
				await ApiClient.Get().SendRequest(url, "POST", json_data);
			}
			await Authenticator.Get().SaveUserData();
			MainMenu.Get().RefreshUserData();
			RefreshPanel();
		}

		private async void SaveUserCardback(CardbackData cardback)
		{
			if (ApiClient.Get().IsConnected())
			{
				string url = ApiClient.ServerURL + "/users/edit/" + ApiClient.Get().UserID;
				string json_data = ApiTool.ToJson(new EditUserRequest
				{
					cardback = cardback.id
				});
				await ApiClient.Get().SendRequest(url, "POST", json_data);
			}
			await Authenticator.Get().SaveUserData();
			MainMenu.Get().RefreshUserData();
			RefreshPanel();
		}

		public void OnClickAvatar()
		{
			if (IsYou())
			{
				RefreshAvatarList();
				avatar_panel.Show();
			}
		}

		public void OnClickCardBack()
		{
			if (IsYou())
			{
				RefreshCardBackList();
				cardback_panel.Show();
			}
		}

		public void OnClickFriends()
		{
			FriendPanel.Get().Show();
		}

		public void OnClickEdit()
		{
			user_email.readOnly = true;
			user_password_prev.readOnly = true;
			user_password_new.readOnly = true;
			user_password_confirm.readOnly = true;
			user_password_new.gameObject.SetActive(value: false);
			user_password_confirm.gameObject.SetActive(value: false);
			UserData userData = Authenticator.Get().UserData;
			user_email.text = userData.email;
			user_password_prev.text = "password";
			user_password_new.text = "password";
			user_password_confirm.text = "password";
			edit_change_email.gameObject.SetActive(value: true);
			edit_change_password.gameObject.SetActive(value: true);
			resend_button.gameObject.SetActive(userData.validation_level == 0);
			confirm_button.gameObject.SetActive(value: false);
			edit_error.text = "";
			edit_panel.Show();
		}

		public void OnClickChangePass()
		{
			OnClickEdit();
			user_password_prev.readOnly = false;
			user_password_new.readOnly = false;
			user_password_confirm.readOnly = false;
			user_password_prev.text = "";
			user_password_new.text = "";
			user_password_confirm.text = "";
			user_password_new.gameObject.SetActive(value: true);
			user_password_confirm.gameObject.SetActive(value: true);
			edit_change_email.gameObject.SetActive(value: false);
			edit_change_password.gameObject.SetActive(value: false);
			resend_button.gameObject.SetActive(value: false);
			confirm_button.gameObject.SetActive(value: true);
			user_password_prev.Select();
		}

		public void OnClickChangeEmail()
		{
			OnClickEdit();
			user_email.readOnly = false;
			edit_change_email.gameObject.SetActive(value: false);
			edit_change_password.gameObject.SetActive(value: false);
			resend_button.gameObject.SetActive(value: false);
			confirm_button.gameObject.SetActive(value: true);
			user_email.Select();
		}

		public async void OnClickResendConfirm()
		{
			edit_error.text = "";
			string url = ApiClient.ServerURL + "/users/email/resend";
			WebResponse webResponse = await ApiClient.Get().SendPostRequest(url, "");
			if (webResponse.success)
			{
				edit_panel.Hide();
			}
			else
			{
				edit_error.text = webResponse.error;
			}
		}

		public async void OnClickEditConfirm()
		{
			edit_error.text = "";
			if (!user_email.readOnly && user_email.text.Length > 0)
			{
				EditEmailRequest editEmailRequest = new EditEmailRequest
				{
					email = user_email.text
				};
				string url = ApiClient.ServerURL + "/users/email/edit/";
				string json_data = ApiTool.ToJson(editEmailRequest);
				WebResponse webResponse = await ApiClient.Get().SendPostRequest(url, json_data);
				if (webResponse.success)
				{
					edit_panel.Hide();
					MainMenu.Get().RefreshUserData();
				}
				else
				{
					edit_error.text = webResponse.error;
				}
			}
			else if (!user_password_new.readOnly && user_password_new.text.Length > 0 && user_password_new.text == user_password_confirm.text)
			{
				EditPasswordRequest editPasswordRequest = new EditPasswordRequest
				{
					password_previous = user_password_prev.text,
					password_new = user_password_new.text
				};
				string url2 = ApiClient.ServerURL + "/users/password/edit/";
				string json_data2 = ApiTool.ToJson(editPasswordRequest);
				WebResponse webResponse2 = await ApiClient.Get().SendPostRequest(url2, json_data2);
				if (webResponse2.success)
				{
					edit_panel.Hide();
				}
				else
				{
					edit_error.text = webResponse2.error;
				}
			}
		}

		public bool IsYou()
		{
			return username == ApiClient.Get().Username;
		}

		public void ShowPlayer()
		{
			string user = ApiClient.Get().Username;
			ShowPlayer(user);
		}

		public void ShowPlayer(string user)
		{
			if (username != user)
			{
				ClearPanel();
			}
			username = user;
			LoadData();
		}

		public override void Show(bool instant = false)
		{
			base.Show(instant);
			ShowPlayer();
		}

		public override void Hide(bool instant = false)
		{
			base.Hide(instant);
			edit_panel.Hide();
		}

		public static PlayerPanel Get()
		{
			return instance;
		}
	}
}
