using System;
using System.Collections.Generic;
using UnityEngine;

namespace TcgEngine
{
	[Serializable]
	public class Player
	{
		public int player_id;

		public string username;

		public string avatar;

		public string cardback;

		public string deck;

		public bool is_ai;

		public int ai_level;

		public bool connected;

		public bool ready;

		public int hp;

		public int hp_max;

		public int mana;

		public int mana_max;

		public int kill_count;

		public Dictionary<string, Card> cards_all = new Dictionary<string, Card>();

		public Card hero;

		public List<Card> cards_deck = new List<Card>();

		public List<Card> cards_hand = new List<Card>();

		public List<Card> cards_board = new List<Card>();

		public List<Card> cards_equip = new List<Card>();

		public List<Card> cards_discard = new List<Card>();

		public List<Card> cards_secret = new List<Card>();

		public List<Card> cards_temp = new List<Card>();

		public List<CardTrait> traits = new List<CardTrait>();

		public List<CardTrait> ongoing_traits = new List<CardTrait>();

		public List<CardStatus> status = new List<CardStatus>();

		public List<CardStatus> ongoing_status = new List<CardStatus>();

		public List<ActionHistory> history_list = new List<ActionHistory>();

		public Player(int id)
		{
			player_id = id;
		}

		public bool IsReady()
		{
			if (ready)
			{
				return cards_all.Count > 0;
			}
			return false;
		}

		public bool IsConnected()
		{
			if (!connected)
			{
				return is_ai;
			}
			return true;
		}

		public virtual void ClearOngoing()
		{
			ongoing_status.Clear();
			ongoing_traits.Clear();
		}

		public void AddCard(List<Card> card_list, Card card)
		{
			card_list.Add(card);
		}

		public void RemoveCard(List<Card> card_list, Card card)
		{
			card_list.Remove(card);
		}

		public virtual void RemoveCardFromAllGroups(Card card)
		{
			cards_deck.Remove(card);
			cards_hand.Remove(card);
			cards_board.Remove(card);
			cards_equip.Remove(card);
			cards_deck.Remove(card);
			cards_discard.Remove(card);
			cards_secret.Remove(card);
			cards_temp.Remove(card);
			UnequipFromAllCards(card);
		}

		public virtual void UnequipFromAllCards(Card equip)
		{
			foreach (Card item in cards_board)
			{
				if (item.equipped_uid == equip.uid)
				{
					item.equipped_uid = null;
				}
			}
		}

		public virtual Card GetRandomCard(List<Card> card_list, System.Random rand)
		{
			if (card_list.Count > 0)
			{
				return card_list[rand.Next(0, card_list.Count)];
			}
			return null;
		}

		public bool HasCard(List<Card> card_list, Card card)
		{
			return card_list.Contains(card);
		}

		public Card GetHandCard(string uid)
		{
			foreach (Card item in cards_hand)
			{
				if (item.uid == uid)
				{
					return item;
				}
			}
			return null;
		}

		public Card GetBoardCard(string uid)
		{
			foreach (Card item in cards_board)
			{
				if (item.uid == uid)
				{
					return item;
				}
			}
			return null;
		}

		public Card GetEquipCard(string uid)
		{
			foreach (Card item in cards_equip)
			{
				if (item.uid == uid)
				{
					return item;
				}
			}
			return null;
		}

		public Card GetDeckCard(string uid)
		{
			foreach (Card item in cards_deck)
			{
				if (item.uid == uid)
				{
					return item;
				}
			}
			return null;
		}

		public Card GetDiscardCard(string uid)
		{
			foreach (Card item in cards_discard)
			{
				if (item.uid == uid)
				{
					return item;
				}
			}
			return null;
		}

		public Card GetBearerCard(Card equipment)
		{
			foreach (Card item in cards_board)
			{
				if (item != null && item.equipped_uid == equipment.uid)
				{
					return item;
				}
			}
			return null;
		}

		public Card GetSlotCard(Slot slot)
		{
			foreach (Card item in cards_board)
			{
				if (item != null && item.slot == slot)
				{
					return item;
				}
			}
			return null;
		}

		public Card GetCard(string uid)
		{
			if (uid != null && cards_all.TryGetValue(uid, out var value))
			{
				return value;
			}
			return null;
		}

		public bool IsOnBoard(Card card)
		{
			if (card != null)
			{
				return GetBoardCard(card.uid) != null;
			}
			return false;
		}

		public Slot GetRandomSlot(System.Random rand)
		{
			return Slot.GetRandom(player_id, rand);
		}

		public virtual Slot GetRandomEmptySlot(System.Random rand, List<Slot> list_mem = null)
		{
			List<Slot> emptySlots = GetEmptySlots(list_mem);
			if (emptySlots.Count > 0)
			{
				return emptySlots[rand.Next(0, emptySlots.Count)];
			}
			return Slot.None;
		}

		public virtual Slot GetRandomOccupiedSlot(System.Random rand, List<Slot> list_mem = null)
		{
			List<Slot> occupiedSlots = GetOccupiedSlots(list_mem);
			if (occupiedSlots.Count > 0)
			{
				return occupiedSlots[rand.Next(0, occupiedSlots.Count)];
			}
			return Slot.None;
		}

		public List<Slot> GetEmptySlots(List<Slot> list_mem = null)
		{
			List<Slot> list = ((list_mem != null) ? list_mem : new List<Slot>());
			foreach (Slot item in Slot.GetAll(player_id))
			{
				if (GetSlotCard(item) == null)
				{
					list.Add(item);
				}
			}
			return list;
		}

		public List<Slot> GetOccupiedSlots(List<Slot> list_mem = null)
		{
			List<Slot> list = ((list_mem != null) ? list_mem : new List<Slot>());
			foreach (Slot item in Slot.GetAll(player_id))
			{
				if (GetSlotCard(item) != null)
				{
					list.Add(item);
				}
			}
			return list;
		}

		public void SetTrait(string id, int value)
		{
			CardTrait trait = GetTrait(id);
			if (trait != null)
			{
				trait.value = value;
				return;
			}
			trait = new CardTrait(id, value);
			traits.Add(trait);
		}

		public void AddTrait(string id, int value)
		{
			CardTrait trait = GetTrait(id);
			if (trait != null)
			{
				trait.value += value;
			}
			else
			{
				SetTrait(id, value);
			}
		}

		public void AddOngoingTrait(string id, int value)
		{
			CardTrait ongoingTrait = GetOngoingTrait(id);
			if (ongoingTrait != null)
			{
				ongoingTrait.value += value;
				return;
			}
			ongoingTrait = new CardTrait(id, value);
			ongoing_traits.Add(ongoingTrait);
		}

		public void RemoveTrait(string id)
		{
			for (int num = traits.Count - 1; num >= 0; num--)
			{
				if (traits[num].id == id)
				{
					traits.RemoveAt(num);
				}
			}
		}

		public CardTrait GetTrait(string id)
		{
			foreach (CardTrait trait in traits)
			{
				if (trait.id == id)
				{
					return trait;
				}
			}
			return null;
		}

		public CardTrait GetOngoingTrait(string id)
		{
			foreach (CardTrait ongoing_trait in ongoing_traits)
			{
				if (ongoing_trait.id == id)
				{
					return ongoing_trait;
				}
			}
			return null;
		}

		public List<CardTrait> GetAllTraits()
		{
			List<CardTrait> list = new List<CardTrait>();
			list.AddRange(traits);
			list.AddRange(ongoing_traits);
			return list;
		}

		public int GetTraitValue(TraitData trait)
		{
			if (trait != null)
			{
				return GetTraitValue(trait.id);
			}
			return 0;
		}

		public virtual int GetTraitValue(string id)
		{
			int num = 0;
			CardTrait trait = GetTrait(id);
			CardTrait ongoingTrait = GetOngoingTrait(id);
			if (trait != null)
			{
				num += trait.value;
			}
			if (ongoingTrait != null)
			{
				num += ongoingTrait.value;
			}
			return num;
		}

		public bool HasTrait(TraitData trait)
		{
			if (trait != null)
			{
				return HasTrait(trait.id);
			}
			return false;
		}

		public bool HasTrait(string id)
		{
			foreach (CardTrait trait in traits)
			{
				if (trait.id == id)
				{
					return true;
				}
			}
			return false;
		}

		public void AddStatus(StatusData status, int value, int duration)
		{
			if (status != null)
			{
				AddStatus(status.effect, value, duration);
			}
		}

		public void AddOngoingStatus(StatusData status, int value)
		{
			if (status != null)
			{
				AddOngoingStatus(status.effect, value);
			}
		}

		public void AddStatus(StatusType effect, int value, int duration)
		{
			if (effect != StatusType.None)
			{
				CardStatus cardStatus = GetStatus(effect);
				if (cardStatus == null)
				{
					cardStatus = new CardStatus(effect, value, duration);
					status.Add(cardStatus);
				}
				else
				{
					cardStatus.value += value;
					cardStatus.duration = Mathf.Max(cardStatus.duration, duration);
					cardStatus.permanent = cardStatus.permanent || duration == 0;
				}
			}
		}

		public void AddOngoingStatus(StatusType effect, int value)
		{
			if (effect != StatusType.None)
			{
				CardStatus ongoingStatus = GetOngoingStatus(effect);
				if (ongoingStatus == null)
				{
					ongoingStatus = new CardStatus(effect, value, 0);
					ongoing_status.Add(ongoingStatus);
				}
				else
				{
					ongoingStatus.value += value;
				}
			}
		}

		public void RemoveStatus(StatusType effect)
		{
			for (int num = status.Count - 1; num >= 0; num--)
			{
				if (status[num].type == effect)
				{
					status.RemoveAt(num);
				}
			}
		}

		public CardStatus GetStatus(StatusType effect)
		{
			foreach (CardStatus item in status)
			{
				if (item.type == effect)
				{
					return item;
				}
			}
			return null;
		}

		public CardStatus GetOngoingStatus(StatusType effect)
		{
			foreach (CardStatus item in ongoing_status)
			{
				if (item.type == effect)
				{
					return item;
				}
			}
			return null;
		}

		public List<CardStatus> GetAllStatus()
		{
			List<CardStatus> list = new List<CardStatus>();
			list.AddRange(status);
			list.AddRange(ongoing_status);
			return list;
		}

		public bool HasStatus(StatusType effect)
		{
			if (GetStatus(effect) == null)
			{
				return GetOngoingStatus(effect) != null;
			}
			return true;
		}

		public virtual int GetStatusValue(StatusType effect)
		{
			CardStatus cardStatus = GetStatus(effect);
			CardStatus ongoingStatus = GetOngoingStatus(effect);
			return cardStatus.value + ongoingStatus.value;
		}

		public void AddHistory(ushort type, Card card)
		{
			ActionHistory actionHistory = new ActionHistory();
			actionHistory.type = type;
			actionHistory.card_id = card.card_id;
			actionHistory.card_uid = card.uid;
			history_list.Add(actionHistory);
		}

		public void AddHistory(ushort type, Card card, Card target)
		{
			ActionHistory actionHistory = new ActionHistory();
			actionHistory.type = type;
			actionHistory.card_id = card.card_id;
			actionHistory.card_uid = card.uid;
			actionHistory.target_uid = target.uid;
			history_list.Add(actionHistory);
		}

		public void AddHistory(ushort type, Card card, Player target)
		{
			ActionHistory actionHistory = new ActionHistory();
			actionHistory.type = type;
			actionHistory.card_id = card.card_id;
			actionHistory.card_uid = card.uid;
			actionHistory.target_id = target.player_id;
			history_list.Add(actionHistory);
		}

		public void AddHistory(ushort type, Card card, AbilityData ability)
		{
			ActionHistory actionHistory = new ActionHistory();
			actionHistory.type = type;
			actionHistory.card_id = card.card_id;
			actionHistory.card_uid = card.uid;
			actionHistory.ability_id = ability.id;
			history_list.Add(actionHistory);
		}

		public void AddHistory(ushort type, Card card, AbilityData ability, Card target)
		{
			ActionHistory actionHistory = new ActionHistory();
			actionHistory.type = type;
			actionHistory.card_id = card.card_id;
			actionHistory.card_uid = card.uid;
			actionHistory.ability_id = ability.id;
			actionHistory.target_uid = target.uid;
			history_list.Add(actionHistory);
		}

		public void AddHistory(ushort type, Card card, AbilityData ability, Player target)
		{
			ActionHistory actionHistory = new ActionHistory();
			actionHistory.type = type;
			actionHistory.card_id = card.card_id;
			actionHistory.card_uid = card.uid;
			actionHistory.ability_id = ability.id;
			actionHistory.target_id = target.player_id;
			history_list.Add(actionHistory);
		}

		public void AddHistory(ushort type, Card card, AbilityData ability, Slot target)
		{
			ActionHistory actionHistory = new ActionHistory();
			actionHistory.type = type;
			actionHistory.card_id = card.card_id;
			actionHistory.card_uid = card.uid;
			actionHistory.ability_id = ability.id;
			actionHistory.slot = target;
			history_list.Add(actionHistory);
		}

		public virtual bool CanPayMana(Card card)
		{
			return mana >= card.GetMana();
		}

		public virtual void PayMana(Card card)
		{
			mana -= card.GetMana();
		}

		public virtual bool CanPayAbility(Card card, AbilityData ability)
		{
			if (!card.exhausted || !ability.exhaust)
			{
				return mana >= ability.mana_cost;
			}
			return false;
		}

		public virtual bool IsDead()
		{
			if (cards_hand.Count == 0 && cards_board.Count == 0 && cards_deck.Count == 0)
			{
				return true;
			}
			if (hp <= 0)
			{
				return true;
			}
			return false;
		}

		public static void Clone(Player source, Player dest)
		{
			dest.player_id = source.player_id;
			dest.is_ai = source.is_ai;
			dest.ai_level = source.ai_level;
			dest.hp = source.hp;
			dest.hp_max = source.hp_max;
			dest.mana = source.mana;
			dest.mana_max = source.mana_max;
			dest.kill_count = source.kill_count;
			Card.CloneNull(source.hero, ref dest.hero);
			Card.CloneDict(source.cards_all, dest.cards_all);
			Card.CloneListRef(dest.cards_all, source.cards_board, dest.cards_board);
			Card.CloneListRef(dest.cards_all, source.cards_equip, dest.cards_equip);
			Card.CloneListRef(dest.cards_all, source.cards_hand, dest.cards_hand);
			Card.CloneListRef(dest.cards_all, source.cards_deck, dest.cards_deck);
			Card.CloneListRef(dest.cards_all, source.cards_discard, dest.cards_discard);
			Card.CloneListRef(dest.cards_all, source.cards_secret, dest.cards_secret);
			Card.CloneListRef(dest.cards_all, source.cards_temp, dest.cards_temp);
			CardStatus.CloneList(source.status, dest.status);
			CardStatus.CloneList(source.ongoing_status, dest.ongoing_status);
		}
	}
}
