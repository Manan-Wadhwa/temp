using System;
using System.Collections;
using TcgEngine.Client;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

namespace TcgEngine.UI
{
	public class MainMenu : MonoBehaviour
	{
		public AudioClip music;

		public AudioClip ambience;

		[Header("Player UI")]
		public Text username_txt;

		public Text credits_txt;

		public AvatarUI avatar;

		public GameObject loader;

		[Header("UI")]
		public Text version_text;

		public DeckSelector deck_selector;

		public DeckDisplay deck_preview;

		private bool starting;

		private static MainMenu instance;

		private void Awake()
		{
			instance = this;
			Application.targetFrameRate = 120;
			GameClient.game_settings = GameSettings.Default;
		}

		private void Start()
		{
			BlackPanel.Get().Show(instant: true);
			AudioTool.Get().PlayMusic("music", music);
			AudioTool.Get().PlaySFX("ambience", ambience, 0.5f, priority: true, loop: true);
			username_txt.text = "";
			credits_txt.text = "";
			version_text.text = "Version " + Application.version;
			DeckSelector deckSelector = deck_selector;
			deckSelector.onChange = (UnityAction<string>)Delegate.Combine(deckSelector.onChange, new UnityAction<string>(OnChangeDeck));
			if (Authenticator.Get().IsConnected())
			{
				AfterLogin();
			}
			else
			{
				RefreshLogin();
			}
		}

		private void Update()
		{
			UserData userData = Authenticator.Get().UserData;
			if (userData != null)
			{
				credits_txt.text = GameUI.FormatNumber(userData.coins);
			}
			bool flag = GameClientMatchmaker.Get().IsMatchmaking();
			if (loader.activeSelf != flag)
			{
				loader.SetActive(flag);
			}
			if (MatchmakingPanel.Get().IsVisible() != flag)
			{
				MatchmakingPanel.Get().SetVisible(flag);
			}
		}

		private async void RefreshLogin()
		{
			if (await Authenticator.Get().RefreshLogin())
			{
				AfterLogin();
			}
			else
			{
				SceneNav.GoTo("LoginMenu");
			}
		}

		private void AfterLogin()
		{
			BlackPanel.Get().Hide();
			GameClientMatchmaker gameClientMatchmaker = GameClientMatchmaker.Get();
			gameClientMatchmaker.onMatchmaking = (UnityAction<MatchmakingResult>)Delegate.Combine(gameClientMatchmaker.onMatchmaking, new UnityAction<MatchmakingResult>(OnMatchmakingDone));
			gameClientMatchmaker.onMatchList = (UnityAction<MatchList>)Delegate.Combine(gameClientMatchmaker.onMatchList, new UnityAction<MatchList>(OnReceiveObserver));
			GameClient.player_settings.deck.tid = PlayerPrefs.GetString("tcg_deck_" + Authenticator.Get().Username, "");
			RefreshUserData();
		}

		public async void RefreshUserData()
		{
			UserData userData = await Authenticator.Get().LoadUserData();
			if (userData != null)
			{
				username_txt.text = userData.username;
				credits_txt.text = GameUI.FormatNumber(userData.coins);
				AvatarData avatarData = AvatarData.Get(userData.avatar);
				avatar.SetAvatar(avatarData);
				RefreshDeckList();
			}
		}

		public void RefreshDeckList()
		{
			deck_selector.RefreshDeckList();
			deck_selector.SelectDeck(GameClient.player_settings.deck.tid);
			RefreshDeck(deck_selector.GetDeckID());
		}

		private void RefreshDeck(string tid)
		{
			if (deck_preview != null)
			{
				deck_preview.SetDeck(tid);
			}
		}

		private void OnChangeDeck(string tid)
		{
			GameClient.player_settings.deck = deck_selector.GetDeck();
			PlayerPrefs.SetString("tcg_deck_" + Authenticator.Get().Username, tid);
			RefreshDeck(tid);
		}

		private void OnMatchmakingDone(MatchmakingResult result)
		{
			if (result != null)
			{
				if (result.success)
				{
					Debug.Log("Matchmaking found: " + result.success + " " + result.server_url + "/" + result.game_uid);
					StartGame(GameType.Multiplayer, result.game_uid, result.server_url);
				}
				else
				{
					MatchmakingPanel.Get().SetCount(result.players);
				}
			}
		}

		private void OnReceiveObserver(MatchList list)
		{
			MatchListItem matchListItem = null;
			MatchListItem[] items = list.items;
			foreach (MatchListItem matchListItem2 in items)
			{
				if (matchListItem2.username == GameClient.observe_user)
				{
					matchListItem = matchListItem2;
				}
			}
			if (matchListItem != null)
			{
				StartGame(GameType.Observer, matchListItem.game_uid, matchListItem.game_url);
			}
		}

		public void StartGame(GameType type, GameMode mode)
		{
			string game_uid = GameTool.GenerateRandomID();
			GameClient.game_settings.game_type = type;
			GameClient.game_settings.game_mode = mode;
			StartGame(game_uid);
		}

		public void StartGame(GameType type, string game_uid, string server_url = "")
		{
			GameClient.game_settings.game_type = type;
			StartGame(game_uid, server_url);
		}

		public void StartGame(string game_uid, string server_url = "")
		{
			if (!starting)
			{
				starting = true;
				GameClient.game_settings.server_url = server_url;
				GameClient.game_settings.game_uid = game_uid;
				GameClientMatchmaker.Get().Disconnect();
				FadeToScene(GameClient.game_settings.GetScene());
			}
		}

		public void StartObserve(string user)
		{
			GameClient.observe_user = user;
			GameClientMatchmaker.Get().StopMatchmaking();
			GameClientMatchmaker.Get().RefreshMatchList(user);
		}

		public void StartChallenge(string user)
		{
			string username = Authenticator.Get().Username;
			if (!(username == user))
			{
				string text = ((username.CompareTo(user) <= 0) ? (user + "-" + username) : (username + "-" + user));
				StartMathmaking(GameMode.Casual, text);
			}
		}

		public void StartMathmaking(GameMode mode, string group)
		{
			UserDeckData deck = deck_selector.GetDeck();
			if (deck != null)
			{
				GameClient.game_settings.game_type = GameType.Multiplayer;
				GameClient.game_settings.game_mode = mode;
				GameClient.player_settings.deck = deck;
				GameClient.game_settings.scene = GameplayData.Get().GetRandomArena();
				GameClientMatchmaker.Get().StartMatchmaking(group, GameClient.game_settings.nb_players);
			}
		}

		public void OnClickSolo()
		{
			if (!Authenticator.Get().IsConnected())
			{
				FadeToScene("LoginMenu");
				return;
			}
			GameClient.player_settings.deck.tid = deck_selector.GetDeckID();
			GameClient.ai_settings.deck.tid = GameplayData.Get().GetRandomAIDeck();
			GameClient.ai_settings.ai_level = GameplayData.Get().ai_level;
			GameClient.game_settings.scene = GameplayData.Get().GetRandomArena();
			StartGame(GameType.Solo, GameMode.Casual);
		}

		public void OnClickPvP()
		{
			if (!Authenticator.Get().IsConnected())
			{
				FadeToScene("LoginMenu");
			}
			else
			{
				StartMathmaking(GameMode.Ranked, "");
			}
		}

		public void OnClickAdventure()
		{
			AdventurePanel.Get().Show();
		}

		public void OnClickPlayCode()
		{
			JoinCodePanel.Get().Show();
		}

		public void OnClickCancelMatch()
		{
			GameClientMatchmaker.Get().StopMatchmaking();
		}

		public void OnClickSettings()
		{
			SettingsPanel.Get().Show();
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

		public void OnClickLogout()
		{
			TcgNetwork.Get().Disconnect();
			Authenticator.Get().Logout();
			FadeToScene("LoginMenu");
		}

		public void OnClickQuit()
		{
			Application.Quit();
		}

		public static MainMenu Get()
		{
			return instance;
		}
	}
}
