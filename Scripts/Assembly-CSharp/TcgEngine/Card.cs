using System;
using System.Collections.Generic;
using UnityEngine;

namespace TcgEngine
{
	[Serializable]
	public class Card
	{
		public string card_id;

		public string uid;

		public int player_id;

		public string variant_id;

		public Slot slot;

		public bool exhausted;

		public int damage;

		public int mana;

		public int attack;

		public int hp;

		public int mana_ongoing;

		public int attack_ongoing;

		public int hp_ongoing;

		public string equipped_uid;

		public List<CardTrait> traits = new List<CardTrait>();

		public List<CardTrait> ongoing_traits = new List<CardTrait>();

		public List<CardStatus> status = new List<CardStatus>();

		public List<CardStatus> ongoing_status = new List<CardStatus>();

		public List<string> abilities = new List<string>();

		public List<string> abilities_ongoing = new List<string>();

		[NonSerialized]
		private int hash;

		[NonSerialized]
		private CardData data;

		[NonSerialized]
		private VariantData vdata;

		[NonSerialized]
		private List<AbilityData> abilities_data;

		public CardData CardData
		{
			get
			{
				if (data == null || data.id != card_id)
				{
					data = CardData.Get(card_id);
				}
				return data;
			}
		}

		public VariantData VariantData
		{
			get
			{
				if (vdata == null || vdata.id != variant_id)
				{
					vdata = VariantData.Get(variant_id);
				}
				return vdata;
			}
		}

		public CardData Data => CardData;

		public int Hash
		{
			get
			{
				if (hash == 0)
				{
					hash = Mathf.Abs(uid.GetHashCode());
				}
				return hash;
			}
		}

		public Card(string card_id, string uid, int player_id)
		{
			this.card_id = card_id;
			this.uid = uid;
			this.player_id = player_id;
		}

		public virtual void Refresh()
		{
			exhausted = false;
		}

		public virtual void ClearOngoing()
		{
			ongoing_status.Clear();
			ongoing_traits.Clear();
			ClearOngoingAbility();
			attack_ongoing = 0;
			hp_ongoing = 0;
			mana_ongoing = 0;
		}

		public virtual void Clear()
		{
			ClearOngoing();
			Refresh();
			damage = 0;
			status.Clear();
			SetCard(CardData, VariantData);
			equipped_uid = null;
		}

		public virtual int GetAttack()
		{
			return Mathf.Max(attack + attack_ongoing, 0);
		}

		public virtual int GetHP()
		{
			return Mathf.Max(hp + hp_ongoing - damage, 0);
		}

		public virtual int GetHPMax()
		{
			return Mathf.Max(hp + hp_ongoing, 0);
		}

		public virtual int GetMana()
		{
			return Mathf.Max(mana + mana_ongoing, 0);
		}

		public virtual void SetCard(CardData icard, VariantData cvariant)
		{
			data = icard;
			card_id = icard.id;
			variant_id = cvariant.id;
			attack = icard.attack;
			hp = icard.hp;
			mana = icard.mana;
			SetTraits(icard);
			SetAbilities(icard);
		}

		public void SetTraits(CardData icard)
		{
			traits.Clear();
			TraitData[] array = icard.traits;
			foreach (TraitData traitData in array)
			{
				SetTrait(traitData.id, 0);
			}
			if (icard.stats != null)
			{
				TraitStat[] stats = icard.stats;
				for (int i = 0; i < stats.Length; i++)
				{
					TraitStat traitStat = stats[i];
					SetTrait(traitStat.trait.id, traitStat.value);
				}
			}
		}

		public void SetAbilities(CardData icard)
		{
			abilities.Clear();
			abilities_ongoing.Clear();
			if (abilities_data != null)
			{
				abilities_data.Clear();
			}
			AbilityData[] array = icard.abilities;
			foreach (AbilityData ability in array)
			{
				AddAbility(ability);
			}
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
			if (GetTrait(id) == null)
			{
				return GetOngoingTrait(id) != null;
			}
			return true;
		}

		public List<CardTrait> GetAllTraits()
		{
			List<CardTrait> list = new List<CardTrait>();
			list.AddRange(traits);
			list.AddRange(ongoing_traits);
			return list;
		}

		public void SetStat(string id, int value)
		{
			SetTrait(id, value);
		}

		public void AddStat(string id, int value)
		{
			AddTrait(id, value);
		}

		public void AddOngoingStat(string id, int value)
		{
			AddOngoingTrait(id, value);
		}

		public void RemoveStat(string id)
		{
			RemoveTrait(id);
		}

		public int GetStatValue(TraitData trait)
		{
			return GetTraitValue(trait);
		}

		public int GetStatValue(string id)
		{
			return GetTraitValue(id);
		}

		public bool HasStat(TraitData trait)
		{
			return HasTrait(trait);
		}

		public bool HasStat(string id)
		{
			return HasTrait(id);
		}

		public List<CardTrait> GetAllStats()
		{
			return GetAllTraits();
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

		public void AddStatus(StatusType type, int value, int duration)
		{
			if (type != StatusType.None)
			{
				CardStatus cardStatus = GetStatus(type);
				if (cardStatus == null)
				{
					cardStatus = new CardStatus(type, value, duration);
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

		public void AddOngoingStatus(StatusType type, int value)
		{
			if (type != StatusType.None)
			{
				CardStatus ongoingStatus = GetOngoingStatus(type);
				if (ongoingStatus == null)
				{
					ongoingStatus = new CardStatus(type, value, 0);
					ongoing_status.Add(ongoingStatus);
				}
				else
				{
					ongoingStatus.value += value;
				}
			}
		}

		public void RemoveStatus(StatusType type)
		{
			for (int num = status.Count - 1; num >= 0; num--)
			{
				if (status[num].type == type)
				{
					status.RemoveAt(num);
				}
			}
		}

		public List<CardStatus> GetAllStatus()
		{
			List<CardStatus> list = new List<CardStatus>();
			list.AddRange(status);
			list.AddRange(ongoing_status);
			return list;
		}

		public bool HasStatus(StatusType type)
		{
			if (GetStatus(type) == null)
			{
				return GetOngoingStatus(type) != null;
			}
			return true;
		}

		public CardStatus GetStatus(StatusType type)
		{
			foreach (CardStatus item in status)
			{
				if (item.type == type)
				{
					return item;
				}
			}
			return null;
		}

		public CardStatus GetOngoingStatus(StatusType type)
		{
			foreach (CardStatus item in ongoing_status)
			{
				if (item.type == type)
				{
					return item;
				}
			}
			return null;
		}

		public virtual int GetStatusValue(StatusType type)
		{
			CardStatus cardStatus = GetStatus(type);
			CardStatus ongoingStatus = GetOngoingStatus(type);
			int num = cardStatus?.value ?? 0;
			int num2 = ongoingStatus?.value ?? 0;
			return num + num2;
		}

		public virtual void ReduceStatusDurations()
		{
			for (int num = status.Count - 1; num >= 0; num--)
			{
				if (!status[num].permanent)
				{
					status[num].duration--;
					if (status[num].duration <= 0)
					{
						status.RemoveAt(num);
					}
				}
			}
		}

		public void AddAbility(AbilityData ability)
		{
			abilities.Add(ability.id);
			if (abilities_data != null)
			{
				abilities_data.Add(ability);
			}
		}

		public void RemoveAbility(AbilityData ability)
		{
			abilities.Remove(ability.id);
			if (abilities_data != null)
			{
				abilities_data.Remove(ability);
			}
		}

		public void AddOngoingAbility(AbilityData ability)
		{
			if (!abilities_ongoing.Contains(ability.id) && !abilities.Contains(ability.id))
			{
				abilities_ongoing.Add(ability.id);
				if (abilities_data != null)
				{
					abilities_data.Add(ability);
				}
			}
		}

		public void ClearOngoingAbility()
		{
			if (abilities_data != null)
			{
				for (int num = abilities_data.Count - 1; num >= 0; num--)
				{
					AbilityData abilityData = abilities_data[num];
					if (abilities_ongoing.Contains(abilityData.id))
					{
						abilities_data.RemoveAt(num);
					}
				}
			}
			abilities_ongoing.Clear();
		}

		public AbilityData GetAbility(AbilityTrigger trigger)
		{
			foreach (AbilityData ability in GetAbilities())
			{
				if (ability.trigger == trigger)
				{
					return ability;
				}
			}
			return null;
		}

		public bool HasAbility(AbilityData ability)
		{
			foreach (AbilityData ability2 in GetAbilities())
			{
				if (ability2.id == ability.id)
				{
					return true;
				}
			}
			return false;
		}

		public bool HasAbility(AbilityTrigger trigger)
		{
			if (GetAbility(trigger) != null)
			{
				return true;
			}
			return false;
		}

		public bool HasAbility(AbilityTrigger trigger, AbilityTarget target)
		{
			foreach (AbilityData ability in GetAbilities())
			{
				if (ability.trigger == trigger && ability.target == target)
				{
					return true;
				}
			}
			return false;
		}

		public bool HasActiveAbility(Game data, AbilityTrigger trigger)
		{
			AbilityData ability = GetAbility(trigger);
			if (ability != null && CanDoAbilities() && ability.AreTriggerConditionsMet(data, this))
			{
				return true;
			}
			return false;
		}

		public bool AreAbilityConditionsMet(AbilityTrigger ability_trigger, Game data, Card caster, Card triggerer)
		{
			foreach (AbilityData ability in GetAbilities())
			{
				if ((bool)ability && ability.trigger == ability_trigger && ability.AreTriggerConditionsMet(data, caster, triggerer))
				{
					return true;
				}
			}
			return false;
		}

		public List<AbilityData> GetAbilities()
		{
			if (abilities_data == null)
			{
				abilities_data = new List<AbilityData>(abilities.Count + abilities_ongoing.Count);
				for (int i = 0; i < abilities.Count; i++)
				{
					abilities_data.Add(AbilityData.Get(abilities[i]));
				}
				for (int j = 0; j < abilities_ongoing.Count; j++)
				{
					abilities_data.Add(AbilityData.Get(abilities_ongoing[j]));
				}
			}
			return abilities_data;
		}

		public virtual bool CanAttack(bool skip_cost = false)
		{
			if (HasStatus(StatusType.Paralysed))
			{
				return false;
			}
			if (!skip_cost && exhausted)
			{
				return false;
			}
			return true;
		}

		public virtual bool CanMove(bool skip_cost = false)
		{
			return true;
		}

		public virtual bool CanDoActivatedAbilities()
		{
			if (HasStatus(StatusType.Paralysed))
			{
				return false;
			}
			if (HasStatus(StatusType.Silenced))
			{
				return false;
			}
			return true;
		}

		public virtual bool CanDoAbilities()
		{
			if (HasStatus(StatusType.Silenced))
			{
				return false;
			}
			return true;
		}

		public virtual bool CanDoAnyAction()
		{
			if (!CanAttack() && !CanMove())
			{
				return CanDoActivatedAbilities();
			}
			return true;
		}

		public static Card Create(CardData icard, VariantData ivariant, Player player)
		{
			return Create(icard, ivariant, player, GameTool.GenerateRandomID(11));
		}

		public static Card Create(CardData icard, VariantData ivariant, Player player, string uid)
		{
			Card card = new Card(icard.id, uid, player.player_id);
			card.SetCard(icard, ivariant);
			player.cards_all[card.uid] = card;
			return card;
		}

		public static Card CloneNew(Card source)
		{
			Card card = new Card(source.card_id, source.uid, source.player_id);
			Clone(source, card);
			return card;
		}

		public static void Clone(Card source, Card dest)
		{
			dest.card_id = source.card_id;
			dest.uid = source.uid;
			dest.player_id = source.player_id;
			dest.variant_id = source.variant_id;
			dest.slot = source.slot;
			dest.exhausted = source.exhausted;
			dest.damage = source.damage;
			dest.attack = source.attack;
			dest.hp = source.hp;
			dest.mana = source.mana;
			dest.mana_ongoing = source.mana_ongoing;
			dest.attack_ongoing = source.attack_ongoing;
			dest.hp_ongoing = source.hp_ongoing;
			dest.equipped_uid = source.equipped_uid;
			CardTrait.CloneList(source.traits, dest.traits);
			CardTrait.CloneList(source.ongoing_traits, dest.ongoing_traits);
			CardStatus.CloneList(source.status, dest.status);
			CardStatus.CloneList(source.ongoing_status, dest.ongoing_status);
			GameTool.CloneList(source.abilities, dest.abilities);
			GameTool.CloneList(source.abilities_ongoing, dest.abilities_ongoing);
			GameTool.CloneListRefNull(source.abilities_data, ref dest.abilities_data);
		}

		public static void CloneNull(Card source, ref Card dest)
		{
			if (source == null)
			{
				dest = null;
			}
			else if (dest == null)
			{
				dest = CloneNew(source);
			}
			else
			{
				Clone(source, dest);
			}
		}

		public static void CloneDict(Dictionary<string, Card> source, Dictionary<string, Card> dest)
		{
			foreach (KeyValuePair<string, Card> item in source)
			{
				if (dest.TryGetValue(item.Key, out var value))
				{
					Clone(item.Value, value);
				}
				else
				{
					dest[item.Key] = CloneNew(item.Value);
				}
			}
		}

		public static void CloneListRef(Dictionary<string, Card> ref_dict, List<Card> source, List<Card> dest)
		{
			for (int i = 0; i < source.Count; i++)
			{
				Card card = source[i];
				if (ref_dict.TryGetValue(card.uid, out var value))
				{
					if (i < dest.Count)
					{
						dest[i] = value;
					}
					else
					{
						dest.Add(value);
					}
				}
			}
			if (dest.Count > source.Count)
			{
				dest.RemoveRange(source.Count, dest.Count - source.Count);
			}
		}
	}
}
