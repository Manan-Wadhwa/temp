using System;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.Events;

namespace TcgEngine.Client
{
	public class RewardManager : MonoBehaviour
	{
		private bool reward_gained;

		private static RewardManager instance;

		private void Awake()
		{
			instance = this;
		}

		private void Start()
		{
			GameClient gameClient = GameClient.Get();
			gameClient.onGameEnd = (UnityAction<int>)Delegate.Combine(gameClient.onGameEnd, new UnityAction<int>(OnGameEnd));
		}

		private void OnGameEnd(int winner)
		{
			int playerID = GameClient.Get().GetPlayerID();
			if (GameClient.game_settings.game_type != GameType.Adventure || winner != playerID)
			{
				return;
			}
			UserData userData = Authenticator.Get().UserData;
			LevelData levelData = LevelData.Get(GameClient.game_settings.level);
			if (levelData != null && !userData.HasReward(levelData.id) && !reward_gained)
			{
				if (Authenticator.Get().IsTest())
				{
					GainRewardTest(levelData);
				}
				if (Authenticator.Get().IsApi())
				{
					GainRewardAPI(levelData);
				}
			}
		}

		private async void GainRewardTest(LevelData level)
		{
			VariantData variantData = VariantData.GetDefault();
			UserData userData = Authenticator.Get().UserData;
			userData.coins += level.reward_coins;
			userData.xp += level.reward_xp;
			userData.AddReward(level.id);
			CardData[] reward_cards = level.reward_cards;
			foreach (CardData cardData in reward_cards)
			{
				userData.AddCard(cardData.id, variantData.id, 1);
			}
			PackData[] reward_packs = level.reward_packs;
			foreach (PackData packData in reward_packs)
			{
				userData.AddPack(packData.id, 1);
			}
			reward_gained = true;
			await Authenticator.Get().SaveUserData();
		}

		private async void GainRewardAPI(LevelData level)
		{
			reward_gained = await GainRewardAPI(level.id);
		}

		public async Task<bool> GainRewardAPI(string reward_id)
		{
			RewardGainRequest rewardGainRequest = new RewardGainRequest
			{
				reward = reward_id
			};
			string url = ApiClient.ServerURL + "/users/rewards/gain/" + ApiClient.Get().UserID;
			string json_data = ApiTool.ToJson(rewardGainRequest);
			WebResponse webResponse = await ApiClient.Get().SendPostRequest(url, json_data);
			Debug.Log("Gain Reward: " + reward_id + " " + webResponse.success);
			return webResponse.success;
		}

		public bool IsRewardGained()
		{
			return reward_gained;
		}

		public static RewardManager Get()
		{
			return instance;
		}
	}
}
