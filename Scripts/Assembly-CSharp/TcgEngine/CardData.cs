using System.Collections.Generic;
using UnityEngine;

namespace TcgEngine
{
	[CreateAssetMenu(fileName = "card", menuName = "TcgEngine/CardData", order = 5)]
	public class CardData : ScriptableObject
	{
		public string id;

		[Header("Display")]
		public string title;

		public Sprite art_full;

		public Sprite art_board;

		[Header("Stats")]
		public CardType type;

		public TeamData team;

		public RarityData rarity;

		public int mana;

		public int attack;

		public int hp;

		[Header("Traits")]
		public TraitData[] traits;

		public TraitStat[] stats;

		[Header("Abilities")]
		public AbilityData[] abilities;

		[Header("Card Text")]
		[TextArea(3, 5)]
		public string text;

		[Header("Description")]
		[TextArea(5, 10)]
		public string desc;

		[Header("FX")]
		public GameObject spawn_fx;

		public GameObject death_fx;

		public GameObject attack_fx;

		public GameObject damage_fx;

		public GameObject idle_fx;

		public AudioClip spawn_audio;

		public AudioClip death_audio;

		public AudioClip attack_audio;

		public AudioClip damage_audio;

		[Header("Availability")]
		public bool deckbuilding;

		public int cost = 100;

		public PackData[] packs;

		public static List<CardData> card_list = new List<CardData>();

		public static Dictionary<string, CardData> card_dict = new Dictionary<string, CardData>();

		public static void Load(string folder = "")
		{
			if (card_list.Count != 0)
			{
				return;
			}
			card_list.AddRange(Resources.LoadAll<CardData>(folder));
			foreach (CardData item in card_list)
			{
				card_dict.Add(item.id, item);
			}
		}

		public Sprite GetBoardArt(VariantData variant)
		{
			return art_board;
		}

		public Sprite GetFullArt(VariantData variant)
		{
			return art_full;
		}

		public string GetTitle()
		{
			return title;
		}

		public string GetText()
		{
			return text;
		}

		public string GetDesc()
		{
			return desc;
		}

		public string GetTypeId()
		{
			if (type == CardType.Hero)
			{
				return "hero";
			}
			if (type == CardType.Character)
			{
				return "character";
			}
			if (type == CardType.Artifact)
			{
				return "artifact";
			}
			if (type == CardType.Spell)
			{
				return "spell";
			}
			if (type == CardType.Secret)
			{
				return "secret";
			}
			if (type == CardType.Equipment)
			{
				return "equipment";
			}
			return "";
		}

		public string GetAbilitiesDesc()
		{
			string text = "";
			AbilityData[] array = abilities;
			foreach (AbilityData abilityData in array)
			{
				if (!string.IsNullOrWhiteSpace(abilityData.desc))
				{
					text = text + "<b>" + abilityData.GetTitle() + ":</b> " + abilityData.GetDesc(this) + "\n";
				}
			}
			return text;
		}

		public bool IsCharacter()
		{
			return type == CardType.Character;
		}

		public bool IsSecret()
		{
			return type == CardType.Secret;
		}

		public bool IsBoardCard()
		{
			if (type != CardType.Character)
			{
				return type == CardType.Artifact;
			}
			return true;
		}

		public bool IsRequireTarget()
		{
			if (type != CardType.Equipment)
			{
				return IsRequireTargetSpell();
			}
			return true;
		}

		public bool IsRequireTargetSpell()
		{
			if (type == CardType.Spell)
			{
				return HasAbility(AbilityTrigger.OnPlay, AbilityTarget.PlayTarget);
			}
			return false;
		}

		public bool IsEquipment()
		{
			return type == CardType.Equipment;
		}

		public bool HasTrait(string trait)
		{
			TraitData[] array = traits;
			for (int i = 0; i < array.Length; i++)
			{
				if (array[i].id == trait)
				{
					return true;
				}
			}
			return false;
		}

		public bool HasTrait(TraitData trait)
		{
			if (trait != null)
			{
				return HasTrait(trait.id);
			}
			return false;
		}

		public bool HasStat(string trait)
		{
			if (stats == null)
			{
				return false;
			}
			TraitStat[] array = stats;
			for (int i = 0; i < array.Length; i++)
			{
				if (array[i].trait.id == trait)
				{
					return true;
				}
			}
			return false;
		}

		public bool HasStat(TraitData trait)
		{
			if (trait != null)
			{
				return HasStat(trait.id);
			}
			return false;
		}

		public int GetStat(string trait_id)
		{
			if (stats == null)
			{
				return 0;
			}
			TraitStat[] array = stats;
			for (int i = 0; i < array.Length; i++)
			{
				TraitStat traitStat = array[i];
				if (traitStat.trait.id == trait_id)
				{
					return traitStat.value;
				}
			}
			return 0;
		}

		public int GetStat(TraitData trait)
		{
			if (trait != null)
			{
				return GetStat(trait.id);
			}
			return 0;
		}

		public bool HasAbility(AbilityData tability)
		{
			AbilityData[] array = abilities;
			foreach (AbilityData abilityData in array)
			{
				if ((bool)abilityData && abilityData.id == tability.id)
				{
					return true;
				}
			}
			return false;
		}

		public bool HasAbility(AbilityTrigger trigger)
		{
			AbilityData[] array = abilities;
			foreach (AbilityData abilityData in array)
			{
				if ((bool)abilityData && abilityData.trigger == trigger)
				{
					return true;
				}
			}
			return false;
		}

		public bool HasAbility(AbilityTrigger trigger, AbilityTarget target)
		{
			AbilityData[] array = abilities;
			foreach (AbilityData abilityData in array)
			{
				if ((bool)abilityData && abilityData.trigger == trigger && abilityData.target == target)
				{
					return true;
				}
			}
			return false;
		}

		public AbilityData GetAbility(AbilityTrigger trigger)
		{
			AbilityData[] array = abilities;
			foreach (AbilityData abilityData in array)
			{
				if ((bool)abilityData && abilityData.trigger == trigger)
				{
					return abilityData;
				}
			}
			return null;
		}

		public bool HasPack(PackData pack)
		{
			PackData[] array = packs;
			for (int i = 0; i < array.Length; i++)
			{
				if (array[i] == pack)
				{
					return true;
				}
			}
			return false;
		}

		public static CardData Get(string id)
		{
			if (id == null)
			{
				return null;
			}
			if (card_dict.TryGetValue(id, out var value))
			{
				return value;
			}
			return null;
		}

		public static List<CardData> GetAllDeckbuilding()
		{
			List<CardData> list = new List<CardData>();
			foreach (CardData item in GetAll())
			{
				if (item.deckbuilding)
				{
					list.Add(item);
				}
			}
			return list;
		}

		public static List<CardData> GetAll(PackData pack)
		{
			List<CardData> list = new List<CardData>();
			foreach (CardData item in GetAll())
			{
				if (item.HasPack(pack))
				{
					list.Add(item);
				}
			}
			return list;
		}

		public static List<CardData> GetAll()
		{
			return card_list;
		}
	}
}
