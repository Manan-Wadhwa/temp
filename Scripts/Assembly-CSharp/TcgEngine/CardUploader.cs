using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.UI;

namespace TcgEngine
{
	public class CardUploader : MonoBehaviour
	{
		public string username = "admin";

		[Header("References")]
		public InputField username_txt;

		public InputField password_txt;

		public Text msg_text;

		[Header("Upload")]
		public bool upload_cards = true;

		public bool upload_packs = true;

		public bool upload_decks = true;

		public bool upload_variants = true;

		public bool upload_rewards = true;

		private void Start()
		{
			username_txt.text = username;
			msg_text.text = "";
		}

		private async void Login()
		{
			LoginResponse loginResponse = await ApiClient.Get().Login(username_txt.text, password_txt.text);
			if (loginResponse.success && loginResponse.permission_level >= 10)
			{
				UploadAll();
			}
			else
			{
				ShowText("Admin Login Failed");
			}
		}

		private async void UploadAll()
		{
			ShowText("Deleting previous data...");
			if (upload_packs)
			{
				await DeleteAllPacks();
			}
			if (upload_cards)
			{
				await DeleteAllCards();
			}
			if (upload_variants)
			{
				await DeleteAllVariants();
			}
			if (upload_decks)
			{
				await DeleteAllDecks();
			}
			if (upload_rewards)
			{
				await DeleteAllRewards();
			}
			if (upload_packs)
			{
				List<PackData> packs = PackData.GetAll();
				for (int i = 0; i < packs.Count; i++)
				{
					PackData packData = packs[i];
					if (packData.available)
					{
						ShowText("Uploading: " + packData.id);
						UploadPack(packData);
						await TimeTool.Delay(100);
					}
				}
			}
			if (upload_cards)
			{
				List<CardData> cards = CardData.GetAll();
				for (int i = 0; i < cards.Count; i++)
				{
					CardData cardData = cards[i];
					if (cardData.deckbuilding)
					{
						ShowText("Uploading: " + cardData.id);
						UploadCard(cardData);
						await TimeTool.Delay(100);
					}
				}
			}
			if (upload_variants)
			{
				List<VariantData> variants = VariantData.GetAll();
				for (int i = 0; i < variants.Count; i++)
				{
					VariantData variantData = variants[i];
					ShowText("Uploading: " + variantData.id);
					UploadVariant(variantData);
					await TimeTool.Delay(100);
				}
			}
			if (upload_decks)
			{
				DeckData[] decks = GameplayData.Get().starter_decks;
				foreach (DeckData deckData in decks)
				{
					ShowText("Uploading: " + deckData.id);
					UploadDeck(deckData);
					UploadDeckReward(deckData);
					await TimeTool.Delay(100);
				}
			}
			if (upload_rewards)
			{
				List<LevelData> levels = LevelData.GetAll();
				for (int i = 0; i < levels.Count; i++)
				{
					LevelData levelData = levels[i];
					ShowText("Uploading: " + levelData.id);
					UploadLevelReward(levelData);
					await TimeTool.Delay(100);
				}
			}
			if (upload_rewards)
			{
				List<RewardData> rewards = RewardData.GetAll();
				for (int i = 0; i < rewards.Count; i++)
				{
					RewardData rewardData = rewards[i];
					ShowText("Uploading: " + rewardData.id);
					UploadReward(rewardData);
					await TimeTool.Delay(100);
				}
			}
			ShowText("Completed!");
			ApiClient.Get().Logout();
		}

		private async Task DeleteAllPacks()
		{
			string url = ApiClient.ServerURL + "/packs";
			await ApiClient.Get().SendRequest(url, "DELETE");
		}

		private async Task DeleteAllCards()
		{
			string url = ApiClient.ServerURL + "/cards";
			await ApiClient.Get().SendRequest(url, "DELETE");
		}

		private async Task DeleteAllVariants()
		{
			string url = ApiClient.ServerURL + "/variants";
			await ApiClient.Get().SendRequest(url, "DELETE");
		}

		private async Task DeleteAllDecks()
		{
			string url = ApiClient.ServerURL + "/decks";
			await ApiClient.Get().SendRequest(url, "DELETE");
		}

		private async Task DeleteAllRewards()
		{
			string url = ApiClient.ServerURL + "/rewards";
			await ApiClient.Get().SendRequest(url, "DELETE");
		}

		private async void UploadPack(PackData pack)
		{
			PackAddRequest packAddRequest = new PackAddRequest();
			packAddRequest.tid = pack.id;
			packAddRequest.cards = pack.cards;
			packAddRequest.cost = pack.cost;
			packAddRequest.random = pack.type == PackType.Random;
			packAddRequest.rarities_1st = new PackAddProbability[pack.rarities_1st.Length];
			packAddRequest.rarities = new PackAddProbability[pack.rarities.Length];
			packAddRequest.variants = new PackAddProbability[pack.variants.Length];
			for (int i = 0; i < packAddRequest.rarities_1st.Length; i++)
			{
				packAddRequest.rarities_1st[i] = AddPackRarity(pack.rarities_1st[i]);
			}
			for (int j = 0; j < packAddRequest.rarities.Length; j++)
			{
				packAddRequest.rarities[j] = AddPackRarity(pack.rarities[j]);
			}
			for (int k = 0; k < packAddRequest.variants.Length; k++)
			{
				packAddRequest.variants[k] = AddPackVariant(pack.variants[k]);
			}
			string url = ApiClient.ServerURL + "/packs/add";
			string json_data = ApiTool.ToJson(packAddRequest);
			await ApiClient.Get().SendPostRequest(url, json_data);
		}

		private PackAddProbability AddPackRarity(PackRarity rarity)
		{
			return new PackAddProbability
			{
				tid = rarity.rarity.id,
				value = rarity.probability
			};
		}

		private PackAddProbability AddPackVariant(PackVariant rarity)
		{
			return new PackAddProbability
			{
				tid = rarity.variant.id,
				value = rarity.probability
			};
		}

		private async void UploadCard(CardData card)
		{
			CardAddRequest cardAddRequest = new CardAddRequest();
			cardAddRequest.tid = card.id;
			cardAddRequest.type = card.GetTypeId();
			cardAddRequest.team = card.team.id;
			cardAddRequest.rarity = card.rarity.id;
			cardAddRequest.mana = card.mana;
			cardAddRequest.attack = card.attack;
			cardAddRequest.hp = card.hp;
			cardAddRequest.cost = card.cost;
			cardAddRequest.packs = new string[card.packs.Length];
			for (int i = 0; i < cardAddRequest.packs.Length; i++)
			{
				cardAddRequest.packs[i] = card.packs[i].id;
			}
			string url = ApiClient.ServerURL + "/cards/add";
			string json_data = ApiTool.ToJson(cardAddRequest);
			await ApiClient.Get().SendPostRequest(url, json_data);
		}

		private async void UploadVariant(VariantData variant)
		{
			VariantAddRequest data = new VariantAddRequest
			{
				tid = variant.id,
				cost_factor = variant.cost_factor,
				is_default = variant.is_default
			};
			string url = ApiClient.ServerURL + "/variants/add";
			string json_data = ApiTool.ToJson(data);
			await ApiClient.Get().SendPostRequest(url, json_data);
		}

		private async void UploadDeckReward(DeckData deck)
		{
			RewardAddRequest rewardAddRequest = new RewardAddRequest();
			rewardAddRequest.tid = deck.id;
			rewardAddRequest.group = "starter_deck";
			rewardAddRequest.decks = new string[1] { deck.id };
			string url = ApiClient.ServerURL + "/rewards/add";
			string json_data = ApiTool.ToJson(rewardAddRequest);
			await ApiClient.Get().SendPostRequest(url, json_data);
		}

		private async void UploadDeck(DeckData deck)
		{
			UserDeckData data = new UserDeckData(deck);
			string url = ApiClient.ServerURL + "/decks/add";
			string json_data = ApiTool.ToJson(data);
			await ApiClient.Get().SendPostRequest(url, json_data);
		}

		private async void UploadReward(RewardData reward)
		{
			RewardAddRequest rewardAddRequest = new RewardAddRequest();
			rewardAddRequest.tid = reward.id;
			rewardAddRequest.group = "";
			rewardAddRequest.coins = reward.coins;
			rewardAddRequest.xp = reward.xp;
			rewardAddRequest.repeat = reward.repeat;
			rewardAddRequest.cards = new string[reward.cards.Length];
			for (int i = 0; i < reward.cards.Length; i++)
			{
				rewardAddRequest.cards[i] = reward.cards[i].id;
			}
			rewardAddRequest.cards = new string[reward.decks.Length];
			for (int j = 0; j < reward.decks.Length; j++)
			{
				rewardAddRequest.cards[j] = reward.decks[j].id;
			}
			rewardAddRequest.packs = new string[reward.packs.Length];
			for (int k = 0; k < reward.packs.Length; k++)
			{
				rewardAddRequest.packs[k] = reward.packs[k].id;
			}
			string url = ApiClient.ServerURL + "/rewards/add";
			string json_data = ApiTool.ToJson(rewardAddRequest);
			await ApiClient.Get().SendPostRequest(url, json_data);
		}

		private async void UploadLevelReward(LevelData level)
		{
			RewardAddRequest rewardAddRequest = new RewardAddRequest();
			rewardAddRequest.tid = level.id;
			rewardAddRequest.group = "";
			rewardAddRequest.coins = level.reward_coins;
			rewardAddRequest.xp = level.reward_xp;
			rewardAddRequest.cards = new string[level.reward_cards.Length];
			for (int i = 0; i < level.reward_cards.Length; i++)
			{
				rewardAddRequest.cards[i] = level.reward_cards[i].id;
			}
			rewardAddRequest.packs = new string[level.reward_packs.Length];
			for (int j = 0; j < level.reward_packs.Length; j++)
			{
				rewardAddRequest.packs[j] = level.reward_packs[j].id;
			}
			string url = ApiClient.ServerURL + "/rewards/add";
			string json_data = ApiTool.ToJson(rewardAddRequest);
			await ApiClient.Get().SendPostRequest(url, json_data);
		}

		private void ShowText(string txt)
		{
			msg_text.text = txt;
			Debug.Log(txt);
		}

		public void OnClickStart()
		{
			msg_text.text = "";
			Login();
		}
	}
}
