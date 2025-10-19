using System.Collections.Generic;
using UnityEngine;

namespace TcgEngine.Client
{
	public class OpenPackMenu : MonoBehaviour
	{
		public GameObject card_prefab;

		private bool revealing;

		private static OpenPackMenu instance;

		private void Awake()
		{
			instance = this;
		}

		private void Update()
		{
			if (!revealing || !Input.GetMouseButtonDown(0))
			{
				return;
			}
			bool flag = true;
			foreach (PackCard item in PackCard.GetAll())
			{
				if (!item.IsRevealed())
				{
					flag = false;
				}
			}
			if (flag && PackCard.GetAll().Count > 0)
			{
				StopReveal();
			}
		}

		public void OpenPack(string pack_tid)
		{
			PackData packData = PackData.Get(pack_tid);
			if (packData != null)
			{
				OpenPack(packData);
			}
		}

		public void OpenPack(PackData pack)
		{
			if (Authenticator.Get().IsApi())
			{
				OpenPackApi(pack);
			}
			if (Authenticator.Get().IsTest())
			{
				OpenPackTest(pack);
			}
		}

		public async void OpenPackTest(PackData pack)
		{
			UserData userData = Authenticator.Get().UserData;
			if (!userData.HasPack(pack.id))
			{
				return;
			}
			List<UserCardData> list = new List<UserCardData>();
			List<CardData> all = CardData.GetAll(pack);
			if (pack.type == PackType.Random)
			{
				for (int i = 0; i < pack.cards; i++)
				{
					RarityData randomRarity = GetRandomRarity(pack, i == 0);
					VariantData randomVariant = GetRandomVariant(pack);
					List<CardData> cardArray = GetCardArray(all, randomRarity);
					if (cardArray.Count > 0)
					{
						UserCardData item = new UserCardData(cardArray[Random.Range(0, cardArray.Count)], randomVariant);
						list.Add(item);
					}
				}
			}
			if (pack.type == PackType.Fixed)
			{
				for (int j = 0; j < Mathf.Min(pack.cards, all.Count); j++)
				{
					CardData card = all[j];
					VariantData variant = VariantData.GetDefault();
					UserCardData item2 = new UserCardData(card, variant);
					list.Add(item2);
				}
			}
			RevealCards(pack, list.ToArray());
			userData.AddPack(pack.id, -1);
			foreach (UserCardData item3 in list)
			{
				userData.AddCard(item3.tid, item3.variant, item3.quantity);
			}
			await Authenticator.Get().SaveUserData();
			HandPackArea.Get().LoadPacks();
		}

		public async void OpenPackApi(PackData pack)
		{
			UserData userData = Authenticator.Get().UserData;
			if (userData.HasPack(pack.id))
			{
				userData.AddPack(pack.id, -1);
				OpenPackRequest openPackRequest = new OpenPackRequest
				{
					pack = pack.id
				};
				string url = ApiClient.ServerURL + "/users/packs/open";
				string json_data = ApiTool.ToJson(openPackRequest);
				WebResponse webResponse = await ApiClient.Get().SendPostRequest(url, json_data);
				if (webResponse.success)
				{
					UserCardData[] cards = ApiTool.JsonToArray<UserCardData>(webResponse.data);
					RevealCards(pack, cards);
				}
				HandPackArea.Get().LoadPacks();
			}
		}

		public void RevealCards(PackData pack, UserCardData[] cards)
		{
			CardbackData.Get(Authenticator.Get().UserData.cardback);
			HandPackArea.Get().Lock(locked: true);
			revealing = true;
			int num = 0;
			foreach (UserCardData userCardData in cards)
			{
				PackCard component = Object.Instantiate(card_prefab, new Vector3(0f, -3f, 0f), Quaternion.identity).GetComponent<PackCard>();
				CardData card = CardData.Get(userCardData.tid);
				VariantData variant = VariantData.Get(userCardData.variant);
				component.SetCard(pack, card, variant);
				BoardRef boardRef = BoardRef.Get(BoardRefType.PackCard, num);
				Vector3 target = ((boardRef != null) ? boardRef.transform.position : Vector3.zero);
				component.SetTarget(target);
				num++;
			}
		}

		private List<CardData> GetCardArray(List<CardData> all_cards, RarityData rarity)
		{
			List<CardData> list = new List<CardData>();
			foreach (CardData all_card in all_cards)
			{
				if (all_card.rarity == rarity)
				{
					list.Add(all_card);
				}
			}
			return list;
		}

		private RarityData GetRandomRarity(PackData pack, bool is_first)
		{
			PackRarity[] array = (is_first ? pack.rarities_1st : pack.rarities);
			if (array == null || array.Length == 0)
			{
				return RarityData.GetFirst();
			}
			int num = 0;
			PackRarity[] array2 = array;
			for (int i = 0; i < array2.Length; i++)
			{
				PackRarity packRarity = array2[i];
				num += packRarity.probability;
			}
			int num2 = Mathf.FloorToInt(Random.value * (float)num);
			for (int j = 0; j < array.Length; j++)
			{
				PackRarity packRarity2 = array[j];
				if (num2 < packRarity2.probability)
				{
					return packRarity2.rarity;
				}
				num2 -= packRarity2.probability;
			}
			return RarityData.GetFirst();
		}

		private VariantData GetRandomVariant(PackData pack)
		{
			PackVariant[] variants = pack.variants;
			if (variants == null || variants.Length == 0)
			{
				return VariantData.GetDefault();
			}
			int num = 0;
			PackVariant[] array = variants;
			for (int i = 0; i < array.Length; i++)
			{
				PackVariant packVariant = array[i];
				num += packVariant.probability;
			}
			int num2 = Mathf.FloorToInt(Random.value * (float)num);
			for (int j = 0; j < variants.Length; j++)
			{
				PackVariant packVariant2 = variants[j];
				if (num2 < packVariant2.probability)
				{
					return packVariant2.variant;
				}
				num2 -= packVariant2.probability;
			}
			return VariantData.GetDefault();
		}

		public void StopReveal()
		{
			revealing = false;
			HandPackArea.Get().Lock(locked: false);
			foreach (PackCard item in PackCard.GetAll())
			{
				item.Remove();
			}
		}

		public void OnClickBack()
		{
			SceneNav.GoTo("Menu");
		}

		public static OpenPackMenu Get()
		{
			return instance;
		}
	}
}
