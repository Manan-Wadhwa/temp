using TcgEngine.Client;
using UnityEngine;
using UnityEngine.UI;

namespace TcgEngine.UI
{
	public class EndGamePanel : UIPanel
	{
		public Text winner_text;

		public Image winner_glow;

		public Text player_name;

		public Text other_name;

		public Image player_avatar;

		public Image other_avatar;

		public Text coins_text;

		public Text xp_text;

		private bool reward_loaded;

		private float timer;

		private int target_coins;

		private int target_xp;

		private float coins;

		private float xp;

		private static EndGamePanel _instance;

		protected override void Awake()
		{
			base.Awake();
			_instance = this;
		}

		protected override void Start()
		{
			base.Start();
			coins_text.text = "";
			xp_text.text = "";
		}

		protected override void Update()
		{
			base.Update();
			if (!reward_loaded && IsVisible())
			{
				timer += Time.deltaTime;
				if (timer > 1f)
				{
					timer = 0f;
					RefreshRewards();
				}
			}
			if (reward_loaded)
			{
				coins = Mathf.MoveTowards(coins, target_coins, 2000f * Time.deltaTime);
				xp = Mathf.MoveTowards(xp, target_xp, 500f * Time.deltaTime);
				coins_text.text = "+ " + Mathf.RoundToInt(coins) + " coins";
				xp_text.text = "+ " + Mathf.RoundToInt(xp) + " xp";
				if (Mathf.RoundToInt(coins) == 0)
				{
					coins_text.text = "";
				}
				if (Mathf.RoundToInt(xp) == 0)
				{
					xp_text.text = "";
				}
			}
		}

		private void RefreshPanel(int winner)
		{
			Player player = GameClient.Get().GetGameData().GetPlayer(winner);
			Player player2 = GameClient.Get().GetPlayer();
			Player opponentPlayer = GameClient.Get().GetOpponentPlayer();
			player_name.text = player2.username;
			other_name.text = opponentPlayer.username;
			AvatarData avatarData = AvatarData.Get(player2.avatar);
			AvatarData avatarData2 = AvatarData.Get(opponentPlayer.avatar);
			if (avatarData != null)
			{
				player_avatar.sprite = avatarData.avatar;
			}
			if (avatarData2 != null)
			{
				other_avatar.sprite = avatarData2.avatar;
			}
			if (player != null && player == player2)
			{
				winner_text.text = "Victory";
			}
			else if (player != null)
			{
				winner_text.text = "Defeat";
			}
			else
			{
				winner_text.text = "Tie";
			}
			if (player == player2)
			{
				winner_glow.rectTransform.anchoredPosition = player_avatar.rectTransform.anchoredPosition;
			}
			if (player == opponentPlayer)
			{
				winner_glow.rectTransform.anchoredPosition = other_avatar.rectTransform.anchoredPosition;
			}
			winner_glow.gameObject.SetActive(player != null);
		}

		private async void RefreshRewards()
		{
			if (GameClient.game_settings.IsOnline())
			{
				string url = ApiClient.ServerURL + "/matches/" + GameClient.game_settings.game_uid;
				WebResponse webResponse = await ApiClient.Get().SendGetRequest(url);
				if (webResponse.success)
				{
					reward_loaded = true;
					MatchResponse matchResponse = ApiTool.JsonToObject<MatchResponse>(webResponse.data);
					string text = ApiClient.Get().Username.ToLower();
					MatchDataResponse[] udata = matchResponse.udata;
					for (int i = 0; i < udata.Length; i++)
					{
						MatchDataResponse matchDataResponse = udata[i];
						if (matchDataResponse.username.ToLower() == text)
						{
							target_coins = matchDataResponse.reward.coins;
							target_xp = matchDataResponse.reward.xp;
						}
					}
				}
			}
			if (GameClient.game_settings.game_type == GameType.Adventure)
			{
				LevelData levelData = LevelData.Get(GameClient.game_settings.level);
				if (levelData != null && RewardManager.Get().IsRewardGained())
				{
					target_coins = levelData.reward_coins;
					target_xp = levelData.reward_xp;
					reward_loaded = true;
				}
			}
		}

		public void Show(int winner)
		{
			reward_loaded = false;
			RefreshPanel(winner);
			RefreshRewards();
			Show();
		}

		public void OnClickQuit()
		{
			GameUI.Get().OnClickQuit();
		}

		public static EndGamePanel Get()
		{
			return _instance;
		}
	}
}
