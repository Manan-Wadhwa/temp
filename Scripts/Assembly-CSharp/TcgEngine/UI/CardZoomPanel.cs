using System;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

namespace TcgEngine.UI
{
	public class CardZoomPanel : UIPanel
	{
		public CardUI card_ui;

		public Text desc;

		public Image quantity_bar;

		public Text quantity_txt;

		public GameObject trade_area;

		public InputField trade_quantity;

		public Text buy_cost;

		public Text sell_cost;

		public Text trade_error;

		private CardData card;

		private VariantData variant;

		private static CardZoomPanel instance;

		protected override void Awake()
		{
			base.Awake();
			instance = this;
			TabButton.onClickAny = (UnityAction<TabButton>)Delegate.Combine(TabButton.onClickAny, new UnityAction<TabButton>(OnClickTab));
		}

		private void OnDestroy()
		{
			TabButton.onClickAny = (UnityAction<TabButton>)Delegate.Remove(TabButton.onClickAny, new UnityAction<TabButton>(OnClickTab));
		}

		protected override void Update()
		{
			base.Update();
			if (card != null)
			{
				int num = GetBuyQuantity() * card.cost * variant.cost_factor;
				buy_cost.text = num.ToString();
				sell_cost.text = Mathf.RoundToInt((float)num * GameplayData.Get().sell_ratio).ToString();
			}
		}

		public void ShowCard(CardData card, VariantData variant)
		{
			this.card = card;
			this.variant = variant;
			int cardQuantity = Authenticator.Get().UserData.GetCardQuantity(card, variant);
			quantity_txt.text = cardQuantity.ToString();
			quantity_txt.enabled = cardQuantity > 0;
			quantity_bar.enabled = cardQuantity > 0;
			trade_quantity.text = "1";
			trade_error.text = "";
			trade_area?.SetActive(card.deckbuilding && card.cost > 0);
			card_ui.SetCard(card, variant);
			string text = card.GetDesc();
			string abilitiesDesc = card.GetAbilitiesDesc();
			if (!string.IsNullOrWhiteSpace(text))
			{
				desc.text = text + "\n\n" + abilitiesDesc;
			}
			else
			{
				desc.text = abilitiesDesc;
			}
			Show();
		}

		public void RefreshCard()
		{
			ShowCard(card, variant);
		}

		private async void BuyCardTest()
		{
			int buyQuantity = GetBuyQuantity();
			int num = buyQuantity * card.cost * variant.cost_factor;
			if (buyQuantity > 0)
			{
				UserData userData = Authenticator.Get().UserData;
				if (userData.coins >= num)
				{
					userData.AddCard(card.id, variant.id, buyQuantity);
					userData.coins -= num;
					await Authenticator.Get().SaveUserData();
					CollectionPanel.Get().ReloadUser();
					Hide();
				}
			}
		}

		private async void BuyCardApi()
		{
			BuyCardRequest buyCardRequest = new BuyCardRequest
			{
				card = card.id,
				variant = variant.id,
				quantity = GetBuyQuantity()
			};
			if (buyCardRequest.quantity > 0)
			{
				string url = ApiClient.ServerURL + "/users/cards/buy/";
				string json_data = ApiTool.ToJson(buyCardRequest);
				trade_error.text = "";
				WebResponse webResponse = await ApiClient.Get().SendPostRequest(url, json_data);
				if (webResponse.success)
				{
					CollectionPanel.Get().ReloadUser();
					Hide();
				}
				else
				{
					trade_error.text = webResponse.error;
				}
			}
		}

		private async void SellCardTest()
		{
			int buyQuantity = GetBuyQuantity();
			int num = Mathf.RoundToInt((float)(buyQuantity * card.cost * variant.cost_factor) * GameplayData.Get().sell_ratio);
			if (buyQuantity > 0)
			{
				UserData userData = Authenticator.Get().UserData;
				if (userData.HasCard(card.id, variant.id, buyQuantity))
				{
					userData.AddCard(card.id, variant.id, -buyQuantity);
					userData.coins += num;
					await Authenticator.Get().SaveUserData();
					CollectionPanel.Get().ReloadUser();
					MainMenu.Get().RefreshDeckList();
					Hide();
				}
			}
		}

		private async void SellCardApi()
		{
			BuyCardRequest buyCardRequest = new BuyCardRequest
			{
				card = card.id,
				variant = variant.id,
				quantity = GetBuyQuantity()
			};
			if (buyCardRequest.quantity > 0)
			{
				string url = ApiClient.ServerURL + "/users/cards/sell/";
				string json_data = ApiTool.ToJson(buyCardRequest);
				trade_error.text = "";
				WebResponse webResponse = await ApiClient.Get().SendPostRequest(url, json_data);
				if (webResponse.success)
				{
					CollectionPanel.Get().ReloadUser();
					Hide();
				}
				else
				{
					trade_error.text = webResponse.error;
				}
			}
		}

		public void OnClickBuy()
		{
			if (Authenticator.Get().IsTest())
			{
				BuyCardTest();
			}
			if (Authenticator.Get().IsApi())
			{
				BuyCardApi();
			}
		}

		public void OnClickSell()
		{
			if (Authenticator.Get().IsTest())
			{
				SellCardTest();
			}
			if (Authenticator.Get().IsApi())
			{
				SellCardApi();
			}
		}

		private void OnClickTab(TabButton btn)
		{
			if (btn.group == "menu")
			{
				Hide();
			}
		}

		public int GetBuyQuantity()
		{
			if (int.TryParse(trade_quantity.text, out var result))
			{
				return result;
			}
			return 0;
		}

		public CardData GetCard()
		{
			return card;
		}

		public string GetCardId()
		{
			return card.id;
		}

		public string GetCardVariant()
		{
			return variant.id;
		}

		public static CardZoomPanel Get()
		{
			return instance;
		}
	}
}
