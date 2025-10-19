using System.Collections;
using UnityEngine;
using UnityEngine.UI;

namespace TcgEngine.UI
{
	public class LoginMenu : MonoBehaviour
	{
		[Header("Login")]
		public UIPanel login_panel;

		public InputField login_user;

		public InputField login_password;

		public Button login_button;

		public GameObject login_bottom;

		public Text error_msg;

		[Header("Register")]
		public UIPanel register_panel;

		public InputField register_username;

		public InputField register_email;

		public InputField register_password;

		public InputField register_password_confirm;

		public Button register_button;

		[Header("Other")]
		public GameObject test_area;

		[Header("Music")]
		public AudioClip music;

		private bool clicked;

		private static LoginMenu instance;

		private void Awake()
		{
			instance = this;
		}

		private void Start()
		{
			AudioTool.Get().PlayMusic("music", music);
			BlackPanel.Get().Show(instant: true);
			error_msg.text = "";
			test_area.SetActive(Authenticator.Get().IsTest());
			string text = PlayerPrefs.GetString("tcg_last_user", "");
			login_user.text = text;
			if (Authenticator.Get().IsTest())
			{
				login_password.gameObject.SetActive(value: false);
				login_bottom.SetActive(value: false);
			}
			else if (!string.IsNullOrEmpty(text))
			{
				SelectField(login_password);
			}
			RefreshLogin();
		}

		private void Update()
		{
			login_button.interactable = !clicked && !string.IsNullOrWhiteSpace(login_user.text);
			register_button.interactable = !clicked && !string.IsNullOrWhiteSpace(register_username.text) && !string.IsNullOrWhiteSpace(register_email.text) && !string.IsNullOrWhiteSpace(register_password.text) && register_password.text == register_password_confirm.text;
			if (login_panel.IsVisible())
			{
				if (Input.GetKeyDown(KeyCode.Tab))
				{
					if (login_user.isFocused)
					{
						SelectField(login_password);
					}
					else
					{
						SelectField(login_user);
					}
				}
				if (Input.GetKeyDown(KeyCode.Return) && login_button.interactable)
				{
					OnClickLogin();
				}
			}
			if (!register_panel.IsVisible())
			{
				return;
			}
			if (Input.GetKeyDown(KeyCode.Tab))
			{
				if (register_username.isFocused)
				{
					SelectField(register_email);
				}
				else if (register_email.isFocused)
				{
					SelectField(register_password);
				}
				else if (register_password.isFocused)
				{
					SelectField(register_password_confirm);
				}
				else
				{
					SelectField(register_username);
				}
			}
			if (Input.GetKeyDown(KeyCode.Return) && register_button.interactable)
			{
				OnClickRegister();
			}
		}

		private async void RefreshLogin()
		{
			if (await Authenticator.Get().RefreshLogin())
			{
				SceneNav.GoTo("Menu");
				return;
			}
			login_panel.Show();
			BlackPanel.Get().Hide();
		}

		private async void Login(string user, string password)
		{
			clicked = true;
			error_msg.text = "";
			if (await Authenticator.Get().Login(user, password))
			{
				PlayerPrefs.SetString("tcg_last_user", login_user.text);
				FadeToScene("Menu");
			}
			else
			{
				clicked = false;
				error_msg.text = Authenticator.Get().GetError();
			}
		}

		private async void Register(string email, string user, string password)
		{
			clicked = true;
			error_msg.text = "";
			if (await Authenticator.Get().Register(register_email.text, register_username.text, register_password.text))
			{
				login_user.text = register_username.text;
				login_password.text = register_password.text;
				login_panel.Show();
				register_panel.Hide();
			}
			else
			{
				error_msg.text = Authenticator.Get().GetError();
			}
			clicked = false;
		}

		public void OnClickLogin()
		{
			if (!string.IsNullOrWhiteSpace(login_user.text) && !clicked)
			{
				Login(login_user.text, login_password.text);
			}
		}

		public void OnClickRegister()
		{
			if (!string.IsNullOrWhiteSpace(register_username.text) && !string.IsNullOrWhiteSpace(register_email.text) && !(register_password.text != register_password_confirm.text) && !clicked)
			{
				Register(register_email.text, register_username.text, register_password.text);
			}
		}

		public void OnClickSwitchLogin()
		{
			login_panel.Show();
			register_panel.Hide();
			login_user.text = "";
			login_password.text = "";
			error_msg.text = "";
			SelectField(login_user);
		}

		public void OnClickSwitchRegister()
		{
			login_panel.Hide();
			register_panel.Show();
			error_msg.text = "";
			SelectField(register_username);
		}

		public void OnClickSwitchReset()
		{
			RecoveryPanel.Get().Show();
		}

		public void OnClickGo()
		{
			FadeToScene("Menu");
		}

		public void OnClickQuit()
		{
			Application.Quit();
		}

		private void SelectField(InputField field)
		{
			if (!GameTool.IsMobile())
			{
				field.Select();
			}
		}

		public void FadeToScene(string scene)
		{
			StartCoroutine(FadeToRun(scene));
		}

		private IEnumerator FadeToRun(string scene)
		{
			BlackPanel.Get().Show();
			AudioTool.Get().FadeOutMusic("music");
			yield return new WaitForSeconds(1f);
			SceneNav.GoTo(scene);
		}

		public static LoginMenu Get()
		{
			return instance;
		}
	}
}
