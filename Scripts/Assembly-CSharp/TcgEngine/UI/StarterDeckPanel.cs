using System.Collections.Generic;
using UnityEngine.UI;

namespace TcgEngine.UI
{
	public class StarterDeckPanel : UIPanel
	{
		public DeckDisplay[] decks;

		public Text error;

		private static StarterDeckPanel instance;

		protected override void Awake()
		{
			base.Awake();
			instance = this;
		}

		private void RefreshPanel()
		{
			int num = 0;
			DeckData[] starter_decks = GameplayData.Get().starter_decks;
			foreach (DeckData deck in starter_decks)
			{
				if (num < decks.Length)
				{
					decks[num].SetDeck(deck);
					num++;
				}
			}
		}

		private void ChooseDeck(string deck_id)
		{
			if (Authenticator.Get().IsTest())
			{
				ChooseDeckTest(deck_id);
			}
			if (Authenticator.Get().IsApi())
			{
				ChooseDeckApi(deck_id);
			}
		}

		private async void ChooseDeckTest(string deck_id)
		{
			UserData userData = Authenticator.Get().UserData;
			DeckData deckData = DeckData.Get(deck_id);
			if (!(deckData == null))
			{
				UserDeckData userDeckData = new UserDeckData();
				userDeckData.tid = deck_id + "_" + GameTool.GenerateRandomID(4, 7);
				userDeckData.title = deckData.title;
				userDeckData.hero = new UserCardData(deckData.hero, VariantData.GetDefault());
				List<UserCardData> list = new List<UserCardData>();
				CardData[] cards = deckData.cards;
				for (int i = 0; i < cards.Length; i++)
				{
					UserCardData item = new UserCardData(cards[i], VariantData.GetDefault());
					list.Add(item);
				}
				userDeckData.cards = list.ToArray();
				userData.AddDeck(userDeckData);
				userData.AddReward(userDeckData.tid);
				await Authenticator.Get().SaveUserData();
				CollectionPanel.Get().ReloadUserDecks();
				Hide();
			}
		}

		private async void ChooseDeckApi(string deck_id)
		{
			RewardGainRequest rewardGainRequest = new RewardGainRequest
			{
				reward = deck_id
			};
			if (error != null)
			{
				error.text = "";
			}
			string url = ApiClient.ServerURL + "/users/rewards/gain/" + ApiClient.Get().UserID;
			string json_data = ApiTool.ToJson(rewardGainRequest);
			WebResponse webResponse = await ApiClient.Get().SendPostRequest(url, json_data);
			if (webResponse.success)
			{
				CollectionPanel.Get().ReloadUserDecks();
				Hide();
			}
			else if (error != null)
			{
				error.text = webResponse.error;
			}
		}

		public override void Show(bool instant = false)
		{
			base.Show(instant);
			if (error != null)
			{
				error.text = "";
			}
			RefreshPanel();
		}

		public void OnClickDeck(int index)
		{
			if (index < decks.Length)
			{
				string deck = decks[index].GetDeck();
				ChooseDeck(deck);
			}
		}

		public static StarterDeckPanel Get()
		{
			return instance;
		}
	}
}
