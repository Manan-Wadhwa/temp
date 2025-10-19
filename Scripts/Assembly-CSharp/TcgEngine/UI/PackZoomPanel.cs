using System;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

namespace TcgEngine.UI
{
	public class PackZoomPanel : UIPanel
	{
		public PackUI pack_ui;

		public Text desc;

		public GameObject buy_area;

		public InputField buy_quantity;

		public Text buy_cost;

		public Text buy_error;

		private PackData pack;

		private static PackZoomPanel instance;

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
			if (pack != null)
			{
				int buyQuantity = GetBuyQuantity();
				buy_cost.text = (pack.cost * buyQuantity).ToString();
			}
		}

		public void ShowPack(PackData pack)
		{
			this.pack = pack;
			int packQuantity = Authenticator.Get().UserData.GetPackQuantity(pack.id);
			pack_ui.SetPack(pack, packQuantity);
			desc.text = pack.GetDesc();
			buy_quantity.text = "1";
			buy_error.text = "";
			buy_area?.SetActive(pack.available);
			Show();
		}

		private async void BuyPackTest()
		{
			int buyQuantity = GetBuyQuantity();
			int num = buyQuantity * pack.cost;
			if (buyQuantity > 0)
			{
				UserData userData = Authenticator.Get().UserData;
				if (userData.coins >= num)
				{
					userData.AddPack(pack.id, buyQuantity);
					userData.coins -= num;
					await Authenticator.Get().SaveUserData();
					PackPanel.Get().ReloadUserPack();
					Hide();
				}
			}
		}

		private async void BuyPackApi()
		{
			BuyPackRequest buyPackRequest = new BuyPackRequest
			{
				pack = pack.id,
				quantity = GetBuyQuantity()
			};
			if (buyPackRequest.quantity > 0)
			{
				string url = ApiClient.ServerURL + "/users/packs/buy/";
				string json_data = ApiTool.ToJson(buyPackRequest);
				buy_error.text = "";
				WebResponse webResponse = await ApiClient.Get().SendPostRequest(url, json_data);
				if (webResponse.success)
				{
					PackPanel.Get().ReloadUserPack();
					Hide();
				}
				else
				{
					buy_error.text = webResponse.error;
				}
			}
		}

		public void OnClickBuy()
		{
			if (Authenticator.Get().IsTest())
			{
				BuyPackTest();
			}
			if (Authenticator.Get().IsApi())
			{
				BuyPackApi();
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
			if (int.TryParse(buy_quantity.text, out var result))
			{
				return result;
			}
			return 0;
		}

		public PackData GetPack()
		{
			return pack;
		}

		public static PackZoomPanel Get()
		{
			return instance;
		}
	}
}
