using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

namespace TcgEngine.UI
{
	public class CollectionPanel : UIPanel
	{
		[Header("Cards")]
		public ScrollRect scroll_rect;

		public RectTransform scroll_content;

		public CardGrid grid_content;

		public GameObject card_prefab;

		[Header("Left Side")]
		public IconButton[] team_filters;

		public Toggle toggle_owned;

		public Toggle toggle_not_owned;

		public Toggle toggle_character;

		public Toggle toggle_spell;

		public Toggle toggle_artifact;

		public Toggle toggle_equipment;

		public Toggle toggle_secret;

		public Toggle toggle_common;

		public Toggle toggle_uncommon;

		public Toggle toggle_rare;

		public Toggle toggle_mythic;

		public Toggle toggle_foil;

		public Dropdown sort_dropdown;

		public InputField search;

		[Header("Right Side")]
		public UIPanel deck_list_panel;

		public UIPanel card_list_panel;

		public DeckLine[] deck_lines;

		[Header("Deckbuilding")]
		public InputField deck_title;

		public Text deck_quantity;

		public GameObject deck_cards_prefab;

		public RectTransform deck_content;

		public GridLayoutGroup deck_grid;

		public IconButton[] hero_powers;

		private TeamData filter_team;

		private int filter_dropdown;

		private string filter_search = "";

		private List<CollectionCard> card_list = new List<CollectionCard>();

		private List<CollectionCard> all_list = new List<CollectionCard>();

		private List<DeckLine> deck_card_lines = new List<DeckLine>();

		private string current_deck_tid;

		private bool editing_deck;

		private bool saving;

		private bool spawned;

		private bool update_grid;

		private float update_grid_timer;

		private List<UserCardData> deck_cards = new List<UserCardData>();

		private static CollectionPanel instance;

		protected override void Awake()
		{
			base.Awake();
			instance = this;
			for (int i = 0; i < grid_content.transform.childCount; i++)
			{
				UnityEngine.Object.Destroy(grid_content.transform.GetChild(i).gameObject);
			}
			for (int j = 0; j < deck_grid.transform.childCount; j++)
			{
				UnityEngine.Object.Destroy(deck_grid.transform.GetChild(j).gameObject);
			}
			DeckLine[] array = deck_lines;
			foreach (DeckLine obj in array)
			{
				obj.onClick = (UnityAction<DeckLine>)Delegate.Combine(obj.onClick, new UnityAction<DeckLine>(OnClickDeckLine));
			}
			array = deck_lines;
			foreach (DeckLine obj2 in array)
			{
				obj2.onClickDelete = (UnityAction<DeckLine>)Delegate.Combine(obj2.onClickDelete, new UnityAction<DeckLine>(OnClickDeckDelete));
			}
			IconButton[] array2 = team_filters;
			foreach (IconButton obj3 in array2)
			{
				obj3.onClick = (UnityAction<IconButton>)Delegate.Combine(obj3.onClick, new UnityAction<IconButton>(OnClickTeam));
			}
		}

		protected override void Start()
		{
			base.Start();
			IconButton[] array = hero_powers;
			foreach (IconButton obj in array)
			{
				CardData cardData = CardData.Get(obj.value);
				HoverTargetUI component = obj.GetComponent<HoverTargetUI>();
				AbilityData abilityData = cardData?.GetAbility(AbilityTrigger.Activate);
				if (cardData != null && component != null && abilityData != null)
				{
					string text = ColorUtility.ToHtmlStringRGBA(cardData.team.color);
					component.text = "<b><color=#" + text + ">Hero Power: </color>";
					component.text = component.text + cardData.title + "</b>\n " + abilityData.GetDesc(cardData);
					if (abilityData.mana_cost > 0)
					{
						component.text = component.text + " <size=16>Mana: " + abilityData.mana_cost + "</size>";
					}
				}
			}
		}

		protected override void Update()
		{
			base.Update();
		}

		private void LateUpdate()
		{
			update_grid_timer += Time.deltaTime;
			if (update_grid && update_grid_timer > 0.2f)
			{
				grid_content.GetColumnAndRow(out var rows, out var columns);
				if (columns > 0)
				{
					float num = grid_content.GetGrid().cellSize.y + grid_content.GetGrid().spacing.y;
					float num2 = (float)rows * num;
					scroll_content.sizeDelta = new Vector2(scroll_content.sizeDelta.x, num2 + 100f);
					update_grid = false;
				}
			}
		}

		private void SpawnCards()
		{
			spawned = true;
			foreach (CollectionCard item in all_list)
			{
				UnityEngine.Object.Destroy(item.gameObject);
			}
			all_list.Clear();
			foreach (VariantData item2 in VariantData.GetAll())
			{
				foreach (CardData item3 in CardData.GetAll())
				{
					GameObject obj = UnityEngine.Object.Instantiate(card_prefab, grid_content.transform);
					CollectionCard component = obj.GetComponent<CollectionCard>();
					component.SetCard(item3, item2, 0);
					component.onClick = (UnityAction<CardUI>)Delegate.Combine(component.onClick, new UnityAction<CardUI>(OnClickCard));
					component.onClickRight = (UnityAction<CardUI>)Delegate.Combine(component.onClickRight, new UnityAction<CardUI>(OnClickCardRight));
					all_list.Add(component);
					obj.SetActive(value: false);
				}
			}
		}

		public async void ReloadUser()
		{
			await Authenticator.Get().LoadUserData();
			MainMenu.Get().RefreshDeckList();
			RefreshCardsQuantities();
			if (!editing_deck)
			{
				RefreshDeckList();
			}
		}

		public async void ReloadUserCards()
		{
			await Authenticator.Get().LoadUserData();
			RefreshCardsQuantities();
		}

		public async void ReloadUserDecks()
		{
			await Authenticator.Get().LoadUserData();
			MainMenu.Get().RefreshDeckList();
			RefreshDeckList();
		}

		private void RefreshAll()
		{
			RefreshFilters();
			RefreshCards();
			RefreshDeckList();
			RefreshStarterDeck();
		}

		private void RefreshFilters()
		{
			search.text = "";
			sort_dropdown.value = 0;
			IconButton[] array = team_filters;
			for (int i = 0; i < array.Length; i++)
			{
				array[i].Deactivate();
			}
			filter_team = null;
			filter_dropdown = 0;
			filter_search = "";
		}

		private void ShowDeckList()
		{
			deck_list_panel.Show();
			card_list_panel.Hide();
			editing_deck = false;
		}

		private void ShowDeckCards()
		{
			deck_list_panel.Hide();
			card_list_panel.Show();
		}

		public void RefreshCards()
		{
			if (!spawned)
			{
				SpawnCards();
			}
			foreach (CollectionCard item in all_list)
			{
				item.gameObject.SetActive(value: false);
			}
			card_list.Clear();
			UserData userData = Authenticator.Get().UserData;
			if (userData == null)
			{
				return;
			}
			VariantData variant = VariantData.GetDefault();
			VariantData special = VariantData.GetSpecial();
			if (toggle_foil.isOn && special != null)
			{
				variant = special;
			}
			List<CardDataQ> list = new List<CardDataQ>();
			List<CardDataQ> list2 = new List<CardDataQ>();
			foreach (CardData item2 in CardData.GetAll())
			{
				list.Add(new CardDataQ
				{
					card = item2,
					variant = variant,
					quantity = userData.GetCardQuantity(item2, variant)
				});
			}
			if (filter_dropdown == 0)
			{
				list.Sort((CardDataQ a, CardDataQ b) => a.card.title.CompareTo(b.card.title));
			}
			if (filter_dropdown == 1)
			{
				list.Sort((CardDataQ a, CardDataQ b) => (b.card.attack != a.card.attack) ? b.card.attack.CompareTo(a.card.attack) : b.card.hp.CompareTo(a.card.hp));
			}
			if (filter_dropdown == 2)
			{
				list.Sort((CardDataQ a, CardDataQ b) => (b.card.hp != a.card.hp) ? b.card.hp.CompareTo(a.card.hp) : b.card.attack.CompareTo(a.card.attack));
			}
			if (filter_dropdown == 3)
			{
				list.Sort((CardDataQ a, CardDataQ b) => (b.card.mana != a.card.mana) ? a.card.mana.CompareTo(b.card.mana) : a.card.title.CompareTo(b.card.title));
			}
			foreach (CardDataQ item3 in list)
			{
				if (!item3.card.deckbuilding)
				{
					continue;
				}
				CardData card = item3.card;
				if (filter_team == null || filter_team == card.team)
				{
					bool flag = item3.quantity > 0;
					RarityData rarity = card.rarity;
					CardType type = card.type;
					bool num = (flag && toggle_owned.isOn) || (!flag && toggle_not_owned.isOn) || toggle_owned.isOn == toggle_not_owned.isOn;
					bool flag2 = (type == CardType.Character && toggle_character.isOn) || (type == CardType.Spell && toggle_spell.isOn) || (type == CardType.Artifact && toggle_artifact.isOn) || (type == CardType.Equipment && toggle_equipment.isOn) || (type == CardType.Secret && toggle_secret.isOn) || (!toggle_character.isOn && !toggle_spell.isOn && !toggle_artifact.isOn && !toggle_equipment.isOn && !toggle_secret.isOn);
					bool flag3 = (rarity.rank == 1 && toggle_common.isOn) || (rarity.rank == 2 && toggle_uncommon.isOn) || (rarity.rank == 3 && toggle_rare.isOn) || (rarity.rank == 4 && toggle_mythic.isOn) || (!toggle_common.isOn && !toggle_uncommon.isOn && !toggle_rare.isOn && !toggle_mythic.isOn);
					string value = filter_search.ToLower();
					bool flag4 = string.IsNullOrWhiteSpace(value) || card.id.Contains(value) || card.title.ToLower().Contains(value) || card.GetText().ToLower().Contains(value);
					if (num && flag2 && flag3 && flag4)
					{
						list2.Add(item3);
					}
				}
			}
			int num2 = 0;
			foreach (CardDataQ item4 in list2)
			{
				if (num2 < all_list.Count)
				{
					CollectionCard collectionCard = all_list[num2];
					collectionCard.SetCard(item4.card, item4.variant, 0);
					card_list.Add(collectionCard);
					collectionCard.gameObject.SetActive(value: true);
					num2++;
				}
			}
			update_grid = true;
			update_grid_timer = 0f;
			scroll_rect.verticalNormalizedPosition = 1f;
			RefreshCardsQuantities();
		}

		private void RefreshCardsQuantities()
		{
			UserData userData = Authenticator.Get().UserData;
			foreach (CollectionCard item in card_list)
			{
				CardData card = item.GetCard();
				VariantData variant = item.GetVariant();
				bool flag = IsCardOwned(userData, card, variant, 1);
				int cardQuantity = userData.GetCardQuantity(card, variant);
				item.SetQuantity(cardQuantity);
				item.SetGrayscale(!flag);
			}
		}

		private void RefreshDeckList()
		{
			DeckLine[] array = deck_lines;
			for (int i = 0; i < array.Length; i++)
			{
				array[i].Hide();
			}
			deck_cards.Clear();
			editing_deck = false;
			saving = false;
			UserData userData = Authenticator.Get().UserData;
			if (userData == null)
			{
				return;
			}
			int num = 0;
			UserDeckData[] decks = userData.decks;
			foreach (UserDeckData deck in decks)
			{
				if (num < deck_lines.Length)
				{
					deck_lines[num].SetLine(userData, deck);
				}
				num++;
			}
			if (num < deck_lines.Length)
			{
				deck_lines[num].SetLine("+");
			}
			RefreshCardsQuantities();
		}

		private void RefreshDeck(UserDeckData deck)
		{
			deck_title.text = "Deck Name";
			current_deck_tid = GameTool.GenerateRandomID(7);
			deck_cards.Clear();
			saving = false;
			editing_deck = true;
			IconButton[] array = hero_powers;
			for (int i = 0; i < array.Length; i++)
			{
				array[i].Deactivate();
			}
			if (deck != null)
			{
				deck_title.text = deck.title;
				current_deck_tid = deck.tid;
				array = hero_powers;
				foreach (IconButton iconButton in array)
				{
					if (deck.hero != null && iconButton.value == deck.hero.tid)
					{
						iconButton.Activate();
					}
				}
				for (int j = 0; j < deck.cards.Length; j++)
				{
					CardData cardData = CardData.Get(deck.cards[j].tid);
					VariantData variantData = VariantData.Get(deck.cards[j].variant);
					if (cardData != null && variantData != null)
					{
						AddDeckCard(cardData, variantData, deck.cards[j].quantity);
					}
				}
			}
			RefreshDeckCards();
		}

		private void RefreshDeckCards()
		{
			foreach (DeckLine deck_card_line in deck_card_lines)
			{
				deck_card_line.Hide();
			}
			List<CardDataQ> list = new List<CardDataQ>();
			foreach (UserCardData deck_card in deck_cards)
			{
				list.Add(new CardDataQ
				{
					card = CardData.Get(deck_card.tid),
					variant = VariantData.Get(deck_card.variant),
					quantity = deck_card.quantity
				});
			}
			list.Sort((CardDataQ a, CardDataQ b) => a.card.title.CompareTo(b.card.title));
			UserData userData = Authenticator.Get().UserData;
			int num = 0;
			int num2 = 0;
			foreach (CardDataQ item in list)
			{
				if (num >= deck_card_lines.Count)
				{
					CreateDeckCard();
				}
				if (num < deck_card_lines.Count)
				{
					DeckLine deckLine = deck_card_lines[num];
					if (deckLine != null)
					{
						deckLine.SetLine(item.card, item.variant, item.quantity, !IsCardOwned(userData, item.card, item.variant, item.quantity));
						num2 += item.quantity;
					}
				}
				num++;
			}
			deck_quantity.text = num2 + "/" + GameplayData.Get().deck_size;
			deck_quantity.color = ((num2 >= GameplayData.Get().deck_size) ? Color.white : Color.red);
			RefreshCardsQuantities();
		}

		private void RefreshStarterDeck()
		{
			UserData userData = Authenticator.Get().UserData;
			if ((userData != null && userData.cards.Length == 0) || userData.rewards.Length == 0)
			{
				StarterDeckPanel.Get().Show();
			}
		}

		private void CreateDeckCard()
		{
			DeckLine component = UnityEngine.Object.Instantiate(deck_cards_prefab, deck_grid.transform).GetComponent<DeckLine>();
			deck_card_lines.Add(component);
			float y = (float)deck_card_lines.Count * 70f + 20f;
			deck_content.sizeDelta = new Vector2(deck_content.sizeDelta.x, y);
			component.onClick = (UnityAction<DeckLine>)Delegate.Combine(component.onClick, new UnityAction<DeckLine>(OnClickCardLine));
			component.onClickRight = (UnityAction<DeckLine>)Delegate.Combine(component.onClickRight, new UnityAction<DeckLine>(OnRightClickCardLine));
		}

		private void AddDeckCard(CardData card, VariantData variant, int quantity = 1)
		{
			AddDeckCard(card.id, variant.id, quantity);
		}

		private void RemoveDeckCard(CardData card, VariantData variant)
		{
			RemoveDeckCard(card.id, variant.id);
		}

		private void AddDeckCard(string tid, string variant, int quantity = 1)
		{
			UserCardData deckCard = GetDeckCard(tid, variant);
			if (deckCard != null)
			{
				deckCard.quantity += quantity;
				return;
			}
			deckCard = new UserCardData(tid, variant);
			deckCard.quantity = quantity;
			deck_cards.Add(deckCard);
		}

		private void RemoveDeckCard(string tid, string variant)
		{
			for (int num = deck_cards.Count - 1; num >= 0; num--)
			{
				UserCardData userCardData = deck_cards[num];
				if (userCardData.tid == tid && userCardData.variant == variant)
				{
					userCardData.quantity--;
					if (userCardData.quantity <= 0)
					{
						deck_cards.RemoveAt(num);
					}
				}
			}
		}

		private UserCardData GetDeckCard(string tid, string variant)
		{
			foreach (UserCardData deck_card in deck_cards)
			{
				if (deck_card.tid == tid && deck_card.variant == variant)
				{
					return deck_card;
				}
			}
			return null;
		}

		private void SaveDeck()
		{
			UserData userData = Authenticator.Get().UserData;
			UserDeckData userDeckData = new UserDeckData();
			userDeckData.tid = current_deck_tid;
			userDeckData.title = deck_title.text;
			userDeckData.hero = new UserCardData();
			userDeckData.hero.tid = GetSelectedHeroId();
			userDeckData.hero.variant = VariantData.GetDefault().id;
			userDeckData.cards = deck_cards.ToArray();
			saving = true;
			if (Authenticator.Get().IsTest())
			{
				SaveDeckTest(userData, userDeckData);
			}
			if (Authenticator.Get().IsApi())
			{
				SaveDeckAPI(userData, userDeckData);
			}
			ShowDeckList();
		}

		private async void SaveDeckTest(UserData udata, UserDeckData udeck)
		{
			udata.SetDeck(udeck);
			await Authenticator.Get().SaveUserData();
			ReloadUserDecks();
		}

		private async void SaveDeckAPI(UserData udata, UserDeckData udeck)
		{
			string url = ApiClient.ServerURL + "/users/deck/" + udeck.tid;
			string json_data = ApiTool.ToJson(udeck);
			WebResponse webResponse = await ApiClient.Get().SendPostRequest(url, json_data);
			UserDeckData[] array = ApiTool.JsonToArray<UserDeckData>(webResponse.data);
			saving = webResponse.success;
			if (webResponse.success && array != null)
			{
				udata.decks = array;
				await Authenticator.Get().SaveUserData();
				ReloadUserDecks();
			}
		}

		private async void DeleteDeck(string deck_tid)
		{
			UserData userData = Authenticator.Get().UserData;
			UserDeckData deck = userData.GetDeck(deck_tid);
			List<UserDeckData> list = new List<UserDeckData>(userData.decks);
			list.Remove(deck);
			userData.decks = list.ToArray();
			if (Authenticator.Get().IsApi())
			{
				string url = ApiClient.ServerURL + "/users/deck/" + deck_tid;
				await ApiClient.Get().SendRequest(url, "DELETE");
			}
			await Authenticator.Get().SaveUserData();
			ReloadUserDecks();
		}

		public void OnClickTeam(IconButton button)
		{
			filter_team = null;
			if (button.IsActive())
			{
				foreach (TeamData item in TeamData.GetAll())
				{
					if (button.value == item.id)
					{
						filter_team = item;
					}
				}
			}
			RefreshCards();
		}

		public void OnChangeToggle()
		{
			RefreshCards();
		}

		public void OnChangeDropdown()
		{
			filter_dropdown = sort_dropdown.value;
			RefreshCards();
		}

		public void OnChangeSearch()
		{
			filter_search = search.text;
			RefreshCards();
		}

		public void OnClickCard(CardUI card)
		{
			if (!editing_deck)
			{
				CardZoomPanel.Get().ShowCard(card.GetCard(), card.GetVariant());
				return;
			}
			CardData card2 = card.GetCard();
			VariantData variant = card.GetVariant();
			if (card2 != null)
			{
				int num = CountDeckCards(card2, variant);
				int num2 = CountDeckCards(card2);
				UserData userData = Authenticator.Get().UserData;
				bool num3 = IsCardOwned(userData, card.GetCard(), card.GetVariant(), num + 1);
				bool flag = num2 < GameplayData.Get().deck_duplicate_max;
				if (num3 && flag)
				{
					AddDeckCard(card2, variant);
					RefreshDeckCards();
				}
			}
		}

		public void OnClickCardRight(CardUI card)
		{
			CardZoomPanel.Get().ShowCard(card.GetCard(), card.GetVariant());
		}

		public void OnClickDeckLine(DeckLine line)
		{
			if (!line.IsHidden() && !saving)
			{
				UserDeckData userDeck = line.GetUserDeck();
				RefreshDeck(userDeck);
				ShowDeckCards();
			}
		}

		private void OnClickCardLine(DeckLine line)
		{
			CardData card = line.GetCard();
			VariantData variant = line.GetVariant();
			if (card != null)
			{
				RemoveDeckCard(card, variant);
			}
			RefreshDeckCards();
		}

		private void OnRightClickCardLine(DeckLine line)
		{
			CardData card = line.GetCard();
			if (card != null)
			{
				CardZoomPanel.Get().ShowCard(card, line.GetVariant());
			}
		}

		public void OnClickSaveDeck()
		{
			if (!saving)
			{
				SaveDeck();
			}
		}

		public void OnClickDeckBack()
		{
			ShowDeckList();
		}

		public void OnClickDeleteDeck()
		{
			if (editing_deck && !string.IsNullOrEmpty(current_deck_tid))
			{
				DeleteDeck(current_deck_tid);
			}
		}

		public void OnClickDeckDelete(DeckLine line)
		{
			if (!line.IsHidden())
			{
				UserDeckData userDeck = line.GetUserDeck();
				if (userDeck != null)
				{
					DeleteDeck(userDeck.tid);
				}
			}
		}

		public int CountDeckCards(CardData card, VariantData cvariant)
		{
			int num = 0;
			foreach (UserCardData deck_card in deck_cards)
			{
				if (deck_card.tid == card.id && deck_card.variant == cvariant.id)
				{
					num += deck_card.quantity;
				}
			}
			return num;
		}

		public int CountDeckCards(CardData card)
		{
			int num = 0;
			foreach (UserCardData deck_card in deck_cards)
			{
				if (deck_card.tid == card.id)
				{
					num += deck_card.quantity;
				}
			}
			return num;
		}

		private bool IsCardOwned(UserData udata, CardData card, VariantData variant, int quantity)
		{
			return udata.GetCardQuantity(card, variant) >= quantity;
		}

		private string GetSelectedHeroId()
		{
			IconButton[] array = hero_powers;
			foreach (IconButton iconButton in array)
			{
				if (iconButton.IsActive())
				{
					return iconButton.value;
				}
			}
			return "";
		}

		public override void Show(bool instant = false)
		{
			base.Show(instant);
			RefreshAll();
			ShowDeckList();
		}

		public static CollectionPanel Get()
		{
			return instance;
		}
	}
}
