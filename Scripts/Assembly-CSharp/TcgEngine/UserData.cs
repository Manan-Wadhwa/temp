using System;
using System.Collections.Generic;
using UnityEngine;

namespace TcgEngine
{
	[Serializable]
	public class UserData
	{
		public string id;

		public string username;

		public string email;

		public string avatar;

		public string cardback;

		public int permission_level = 1;

		public int validation_level = 1;

		public int coins;

		public int xp;

		public int elo;

		public int matches;

		public int victories;

		public int defeats;

		public UserCardData[] cards;

		public UserCardData[] packs;

		public UserDeckData[] decks;

		public string[] rewards;

		public string[] avatars;

		public string[] cardbacks;

		public string[] friends;

		public UserData()
		{
			cards = new UserCardData[0];
			packs = new UserCardData[0];
			decks = new UserDeckData[0];
			rewards = new string[0];
			avatars = new string[0];
			cardbacks = new string[0];
			friends = new string[0];
			permission_level = 1;
			coins = 10000;
			elo = 1000;
		}

		public int GetLevel()
		{
			return Mathf.FloorToInt(xp / 1000) + 1;
		}

		public string GetAvatar()
		{
			if (avatar != null)
			{
				return avatar;
			}
			return "";
		}

		public string GetCardback()
		{
			if (cardback != null)
			{
				return cardback;
			}
			return "";
		}

		public void SetDeck(UserDeckData deck)
		{
			for (int i = 0; i < decks.Length; i++)
			{
				if (decks[i].tid == deck.tid)
				{
					decks[i] = deck;
					return;
				}
			}
			List<UserDeckData> list = new List<UserDeckData>(decks);
			list.Add(deck);
			decks = list.ToArray();
		}

		public UserDeckData GetDeck(string tid)
		{
			UserDeckData[] array = decks;
			foreach (UserDeckData userDeckData in array)
			{
				if (userDeckData.tid == tid)
				{
					return userDeckData;
				}
			}
			return null;
		}

		public UserCardData GetCard(string tid, string variant)
		{
			UserCardData[] array = cards;
			foreach (UserCardData userCardData in array)
			{
				if (userCardData.tid == tid && userCardData.variant == variant)
				{
					return userCardData;
				}
			}
			return null;
		}

		public int GetCardQuantity(CardData card, VariantData variant)
		{
			return GetCardQuantity(card.id, variant.id, variant.is_default);
		}

		public int GetCardQuantity(string tid, string variant, bool default_variant = false)
		{
			if (cards == null)
			{
				return 0;
			}
			UserCardData[] array = cards;
			foreach (UserCardData userCardData in array)
			{
				if (userCardData.tid == tid && userCardData.variant == variant)
				{
					return userCardData.quantity;
				}
				if (userCardData.tid == tid && userCardData.variant == "" && default_variant)
				{
					return userCardData.quantity;
				}
			}
			return 0;
		}

		public UserCardData GetPack(string tid)
		{
			UserCardData[] array = packs;
			foreach (UserCardData userCardData in array)
			{
				if (userCardData.tid == tid)
				{
					return userCardData;
				}
			}
			return null;
		}

		public int GetPackQuantity(string tid)
		{
			if (packs == null)
			{
				return 0;
			}
			UserCardData[] array = packs;
			foreach (UserCardData userCardData in array)
			{
				if (userCardData.tid == tid)
				{
					return userCardData.quantity;
				}
			}
			return 0;
		}

		public int CountUniqueCards()
		{
			if (cards == null)
			{
				return 0;
			}
			HashSet<string> hashSet = new HashSet<string>();
			UserCardData[] array = cards;
			foreach (UserCardData userCardData in array)
			{
				if (!hashSet.Contains(userCardData.tid))
				{
					hashSet.Add(userCardData.tid);
				}
			}
			return hashSet.Count;
		}

		public int CountCardType(VariantData variant)
		{
			int num = 0;
			UserCardData[] array = cards;
			for (int i = 0; i < array.Length; i++)
			{
				if (array[i].variant == variant.id)
				{
					num++;
				}
			}
			return num;
		}

		public bool HasDeckCards(UserDeckData deck)
		{
			UserCardData[] array = deck.cards;
			foreach (UserCardData userCardData in array)
			{
				bool default_variant = true;
				if (GetCardQuantity(userCardData.tid, userCardData.variant, default_variant) < userCardData.quantity)
				{
					return false;
				}
			}
			return true;
		}

		public bool IsDeckValid(UserDeckData deck)
		{
			if (Authenticator.Get().IsApi())
			{
				if (HasDeckCards(deck))
				{
					return deck.IsValid();
				}
				return false;
			}
			return deck.IsValid();
		}

		public void AddDeck(UserDeckData deck)
		{
			List<UserDeckData> list = new List<UserDeckData>(decks);
			list.Add(deck);
			decks = list.ToArray();
			UserCardData[] array = deck.cards;
			foreach (UserCardData userCardData in array)
			{
				AddCard(userCardData.tid, userCardData.variant, 1);
			}
		}

		public void AddPack(string tid, int quantity)
		{
			bool flag = false;
			UserCardData[] array = packs;
			foreach (UserCardData userCardData in array)
			{
				if (userCardData.tid == tid)
				{
					flag = true;
					userCardData.quantity += quantity;
				}
			}
			if (!flag)
			{
				UserCardData userCardData2 = new UserCardData();
				userCardData2.tid = tid;
				userCardData2.quantity = quantity;
				List<UserCardData> list = new List<UserCardData>(packs);
				list.Add(userCardData2);
				packs = list.ToArray();
			}
		}

		public void AddCard(string tid, string variant, int quantity)
		{
			bool flag = false;
			UserCardData[] array = cards;
			foreach (UserCardData userCardData in array)
			{
				if (userCardData.tid == tid && userCardData.variant == variant)
				{
					flag = true;
					userCardData.quantity += quantity;
				}
			}
			if (!flag)
			{
				UserCardData userCardData2 = new UserCardData();
				userCardData2.tid = tid;
				userCardData2.variant = variant;
				userCardData2.quantity = quantity;
				List<UserCardData> list = new List<UserCardData>(cards);
				list.Add(userCardData2);
				cards = list.ToArray();
			}
		}

		public void AddReward(string tid)
		{
			if (!HasReward(tid))
			{
				List<string> list = new List<string>(rewards);
				list.Add(tid);
				rewards = list.ToArray();
			}
		}

		public bool HasCard(string card_tid, string variant, int quantity = 1)
		{
			UserCardData[] array = cards;
			foreach (UserCardData userCardData in array)
			{
				if (userCardData.tid == card_tid && userCardData.variant == variant && userCardData.quantity >= quantity)
				{
					return true;
				}
			}
			return false;
		}

		public bool HasPack(string pack_tid, int quantity = 1)
		{
			UserCardData[] array = packs;
			foreach (UserCardData userCardData in array)
			{
				if (userCardData.tid == pack_tid && userCardData.quantity >= quantity)
				{
					return true;
				}
			}
			return false;
		}

		public bool HasReward(string reward_id)
		{
			string[] array = rewards;
			for (int i = 0; i < array.Length; i++)
			{
				if (array[i] == reward_id)
				{
					return true;
				}
			}
			return false;
		}

		public string GetCoinsString()
		{
			return coins.ToString();
		}

		public bool HasFriend(string username)
		{
			return new List<string>(friends).Contains(username);
		}

		public void AddFriend(string username)
		{
			List<string> list = new List<string>(friends);
			if (!list.Contains(username))
			{
				list.Add(username);
			}
			friends = list.ToArray();
		}

		public void RemoveFriend(string username)
		{
			List<string> list = new List<string>(friends);
			if (list.Contains(username))
			{
				list.Remove(username);
			}
			friends = list.ToArray();
		}
	}
}
