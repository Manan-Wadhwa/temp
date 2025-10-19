using System.Collections.Generic;
using TcgEngine.Gameplay;
using UnityEngine;

namespace TcgEngine
{
	[CreateAssetMenu(fileName = "ability", menuName = "TcgEngine/AbilityData", order = 5)]
	public class AbilityData : ScriptableObject
	{
		public string id;

		[Header("Trigger")]
		public AbilityTrigger trigger;

		public ConditionData[] conditions_trigger;

		[Header("Target")]
		public AbilityTarget target;

		public ConditionData[] conditions_target;

		public FilterData[] filters_target;

		[Header("Effect")]
		public EffectData[] effects;

		public StatusData[] status;

		public int value;

		public int duration;

		[Header("Chain/Choices")]
		public AbilityData[] chain_abilities;

		[Header("Activated Ability")]
		public int mana_cost;

		public bool exhaust;

		[Header("FX")]
		public GameObject board_fx;

		public GameObject caster_fx;

		public GameObject target_fx;

		public AudioClip cast_audio;

		public AudioClip target_audio;

		public bool charge_target;

		[Header("Text")]
		public string title;

		[TextArea(5, 7)]
		public string desc;

		public static List<AbilityData> ability_list = new List<AbilityData>();

		public static Dictionary<string, AbilityData> ability_dict = new Dictionary<string, AbilityData>();

		public static void Load(string folder = "")
		{
			if (ability_list.Count != 0)
			{
				return;
			}
			ability_list.AddRange(Resources.LoadAll<AbilityData>(folder));
			foreach (AbilityData item in ability_list)
			{
				ability_dict.Add(item.id, item);
			}
		}

		public string GetTitle()
		{
			return title;
		}

		public string GetDesc()
		{
			return desc;
		}

		public string GetDesc(CardData card)
		{
			return desc.Replace("<name>", card.title).Replace("<value>", value.ToString()).Replace("<duration>", duration.ToString());
		}

		public bool AreTriggerConditionsMet(Game data, Card caster)
		{
			return AreTriggerConditionsMet(data, caster, caster);
		}

		public bool AreTriggerConditionsMet(Game data, Card caster, Card trigger_card)
		{
			ConditionData[] array = conditions_trigger;
			foreach (ConditionData conditionData in array)
			{
				if (conditionData != null)
				{
					if (!conditionData.IsTriggerConditionMet(data, this, caster))
					{
						return false;
					}
					if (!conditionData.IsTargetConditionMet(data, this, caster, trigger_card))
					{
						return false;
					}
				}
			}
			return true;
		}

		public bool AreTriggerConditionsMet(Game data, Card caster, Player trigger_player)
		{
			ConditionData[] array = conditions_trigger;
			foreach (ConditionData conditionData in array)
			{
				if (conditionData != null)
				{
					if (!conditionData.IsTriggerConditionMet(data, this, caster))
					{
						return false;
					}
					if (!conditionData.IsTargetConditionMet(data, this, caster, trigger_player))
					{
						return false;
					}
				}
			}
			return true;
		}

		public bool AreTargetConditionsMet(Game data, Card caster, Card target_card)
		{
			ConditionData[] array = conditions_target;
			foreach (ConditionData conditionData in array)
			{
				if (conditionData != null && !conditionData.IsTargetConditionMet(data, this, caster, target_card))
				{
					return false;
				}
			}
			return true;
		}

		public bool AreTargetConditionsMet(Game data, Card caster, Player target_player)
		{
			ConditionData[] array = conditions_target;
			foreach (ConditionData conditionData in array)
			{
				if (conditionData != null && !conditionData.IsTargetConditionMet(data, this, caster, target_player))
				{
					return false;
				}
			}
			return true;
		}

		public bool AreTargetConditionsMet(Game data, Card caster, Slot target_slot)
		{
			ConditionData[] array = conditions_target;
			foreach (ConditionData conditionData in array)
			{
				if (conditionData != null && !conditionData.IsTargetConditionMet(data, this, caster, target_slot))
				{
					return false;
				}
			}
			return true;
		}

		public bool AreTargetConditionsMet(Game data, Card caster, CardData target_card)
		{
			ConditionData[] array = conditions_target;
			foreach (ConditionData conditionData in array)
			{
				if (conditionData != null && !conditionData.IsTargetConditionMet(data, this, caster, target_card))
				{
					return false;
				}
			}
			return true;
		}

		public bool CanTarget(Game data, Card caster, Card target)
		{
			if (target.HasStatus(StatusType.Stealth))
			{
				return false;
			}
			if (target.HasStatus(StatusType.SpellImmunity))
			{
				return false;
			}
			return AreTargetConditionsMet(data, caster, target);
		}

		public bool CanTarget(Game data, Card caster, Player target)
		{
			return AreTargetConditionsMet(data, caster, target);
		}

		public bool CanTarget(Game data, Card caster, Slot target)
		{
			return AreTargetConditionsMet(data, caster, target);
		}

		public bool IsCardSelectionValid(Game data, Card caster, Card target, ListSwap<Card> card_array = null)
		{
			return GetCardTargets(data, caster, card_array).Contains(target);
		}

		public void DoEffects(GameLogic logic, Card caster)
		{
			EffectData[] array = effects;
			for (int i = 0; i < array.Length; i++)
			{
				array[i]?.DoEffect(logic, this, caster);
			}
		}

		public void DoEffects(GameLogic logic, Card caster, Card target)
		{
			EffectData[] array = effects;
			for (int i = 0; i < array.Length; i++)
			{
				array[i]?.DoEffect(logic, this, caster, target);
			}
			StatusData[] array2 = status;
			foreach (StatusData statusData in array2)
			{
				target.AddStatus(statusData, value, duration);
			}
		}

		public void DoEffects(GameLogic logic, Card caster, Player target)
		{
			EffectData[] array = effects;
			for (int i = 0; i < array.Length; i++)
			{
				array[i]?.DoEffect(logic, this, caster, target);
			}
			StatusData[] array2 = status;
			foreach (StatusData statusData in array2)
			{
				target.AddStatus(statusData, value, duration);
			}
		}

		public void DoEffects(GameLogic logic, Card caster, Slot target)
		{
			EffectData[] array = effects;
			for (int i = 0; i < array.Length; i++)
			{
				array[i]?.DoEffect(logic, this, caster, target);
			}
		}

		public void DoEffects(GameLogic logic, Card caster, CardData target)
		{
			EffectData[] array = effects;
			for (int i = 0; i < array.Length; i++)
			{
				array[i]?.DoEffect(logic, this, caster, target);
			}
		}

		public void DoOngoingEffects(GameLogic logic, Card caster, Card target)
		{
			EffectData[] array = effects;
			for (int i = 0; i < array.Length; i++)
			{
				array[i]?.DoOngoingEffect(logic, this, caster, target);
			}
			StatusData[] array2 = status;
			foreach (StatusData statusData in array2)
			{
				target.AddOngoingStatus(statusData, value);
			}
		}

		public void DoOngoingEffects(GameLogic logic, Card caster, Player target)
		{
			EffectData[] array = effects;
			for (int i = 0; i < array.Length; i++)
			{
				array[i]?.DoOngoingEffect(logic, this, caster, target);
			}
			StatusData[] array2 = status;
			foreach (StatusData statusData in array2)
			{
				target.AddOngoingStatus(statusData, value);
			}
		}

		public bool HasEffect<T>() where T : EffectData
		{
			EffectData[] array = effects;
			foreach (EffectData effectData in array)
			{
				if (effectData != null && effectData is T)
				{
					return true;
				}
			}
			return false;
		}

		public bool HasStatus(StatusType type)
		{
			StatusData[] array = status;
			foreach (StatusData statusData in array)
			{
				if (statusData != null && statusData.effect == type)
				{
					return true;
				}
			}
			return false;
		}

		private void AddValidCards(Game data, Card caster, List<Card> source, List<Card> targets)
		{
			foreach (Card item in source)
			{
				if (AreTargetConditionsMet(data, caster, item))
				{
					targets.Add(item);
				}
			}
		}

		public List<Card> GetCardTargets(Game data, Card caster, ListSwap<Card> memory_array = null)
		{
			if (memory_array == null)
			{
				memory_array = new ListSwap<Card>();
			}
			List<Card> list = memory_array.Get();
			if (target == AbilityTarget.Self && AreTargetConditionsMet(data, caster, caster))
			{
				list.Add(caster);
			}
			if (target == AbilityTarget.AllCardsBoard || target == AbilityTarget.SelectTarget)
			{
				Player[] players = data.players;
				for (int i = 0; i < players.Length; i++)
				{
					foreach (Card item in players[i].cards_board)
					{
						if (AreTargetConditionsMet(data, caster, item))
						{
							list.Add(item);
						}
					}
				}
			}
			if (target == AbilityTarget.AllCardsHand)
			{
				Player[] players = data.players;
				for (int i = 0; i < players.Length; i++)
				{
					foreach (Card item2 in players[i].cards_hand)
					{
						if (AreTargetConditionsMet(data, caster, item2))
						{
							list.Add(item2);
						}
					}
				}
			}
			if (target == AbilityTarget.AllCardsAllPiles || target == AbilityTarget.CardSelector)
			{
				Player[] players = data.players;
				foreach (Player player in players)
				{
					AddValidCards(data, caster, player.cards_deck, list);
					AddValidCards(data, caster, player.cards_discard, list);
					AddValidCards(data, caster, player.cards_hand, list);
					AddValidCards(data, caster, player.cards_secret, list);
					AddValidCards(data, caster, player.cards_board, list);
					AddValidCards(data, caster, player.cards_equip, list);
					AddValidCards(data, caster, player.cards_temp, list);
				}
			}
			if (target == AbilityTarget.LastPlayed)
			{
				Card card = data.GetCard(data.last_played);
				if (card != null && AreTargetConditionsMet(data, caster, card))
				{
					list.Add(card);
				}
			}
			if (target == AbilityTarget.LastDestroyed)
			{
				Card card2 = data.GetCard(data.last_destroyed);
				if (card2 != null && AreTargetConditionsMet(data, caster, card2))
				{
					list.Add(card2);
				}
			}
			if (target == AbilityTarget.LastTargeted)
			{
				Card card3 = data.GetCard(data.last_target);
				if (card3 != null && AreTargetConditionsMet(data, caster, card3))
				{
					list.Add(card3);
				}
			}
			if (target == AbilityTarget.LastSummoned)
			{
				Card card4 = data.GetCard(data.last_summoned);
				if (card4 != null && AreTargetConditionsMet(data, caster, card4))
				{
					list.Add(card4);
				}
			}
			if (target == AbilityTarget.AbilityTriggerer)
			{
				Card card5 = data.GetCard(data.ability_triggerer);
				if (card5 != null && AreTargetConditionsMet(data, caster, card5))
				{
					list.Add(card5);
				}
			}
			if (target == AbilityTarget.EquippedCard)
			{
				if (caster.CardData.IsEquipment())
				{
					Card bearerCard = data.GetPlayer(caster.player_id).GetBearerCard(caster);
					if (bearerCard != null && AreTargetConditionsMet(data, caster, bearerCard))
					{
						list.Add(bearerCard);
					}
				}
				else if (caster.equipped_uid != null)
				{
					Card card6 = data.GetCard(caster.equipped_uid);
					if (card6 != null && AreTargetConditionsMet(data, caster, card6))
					{
						list.Add(card6);
					}
				}
			}
			if (filters_target != null && list.Count > 0)
			{
				FilterData[] array = filters_target;
				foreach (FilterData filterData in array)
				{
					if (filterData != null)
					{
						list = filterData.FilterTargets(data, this, caster, list, memory_array.GetOther(list));
					}
				}
			}
			return list;
		}

		public List<Player> GetPlayerTargets(Game data, Card caster, ListSwap<Player> memory_array = null)
		{
			if (memory_array == null)
			{
				memory_array = new ListSwap<Player>();
			}
			List<Player> list = memory_array.Get();
			if (target == AbilityTarget.PlayerSelf)
			{
				Player player = data.GetPlayer(caster.player_id);
				list.Add(player);
			}
			else if (target == AbilityTarget.PlayerOpponent)
			{
				for (int i = 0; i < data.players.Length; i++)
				{
					if (i != caster.player_id)
					{
						Player item = data.players[i];
						list.Add(item);
					}
				}
			}
			else if (target == AbilityTarget.AllPlayers)
			{
				list.AddRange(data.players);
			}
			if (filters_target != null && list.Count > 0)
			{
				FilterData[] array = filters_target;
				foreach (FilterData filterData in array)
				{
					if (filterData != null)
					{
						list = filterData.FilterTargets(data, this, caster, list, memory_array.GetOther(list));
					}
				}
			}
			return list;
		}

		public List<Slot> GetSlotTargets(Game data, Card caster, ListSwap<Slot> memory_array = null)
		{
			if (memory_array == null)
			{
				memory_array = new ListSwap<Slot>();
			}
			List<Slot> list = memory_array.Get();
			if (target == AbilityTarget.AllSlots)
			{
				foreach (Slot item in Slot.GetAll())
				{
					if (AreTargetConditionsMet(data, caster, item))
					{
						list.Add(item);
					}
				}
			}
			if (filters_target != null && list.Count > 0)
			{
				FilterData[] array = filters_target;
				foreach (FilterData filterData in array)
				{
					if (filterData != null)
					{
						list = filterData.FilterTargets(data, this, caster, list, memory_array.GetOther(list));
					}
				}
			}
			return list;
		}

		public List<CardData> GetCardDataTargets(Game data, Card caster, ListSwap<CardData> memory_array = null)
		{
			if (memory_array == null)
			{
				memory_array = new ListSwap<CardData>();
			}
			List<CardData> list = memory_array.Get();
			if (target == AbilityTarget.AllCardData)
			{
				foreach (CardData item in CardData.GetAll())
				{
					if (AreTargetConditionsMet(data, caster, item))
					{
						list.Add(item);
					}
				}
			}
			if (filters_target != null && list.Count > 0)
			{
				FilterData[] array = filters_target;
				foreach (FilterData filterData in array)
				{
					if (filterData != null)
					{
						list = filterData.FilterTargets(data, this, caster, list, memory_array.GetOther(list));
					}
				}
			}
			return list;
		}

		public bool HasValidSelectTarget(Game game_data, Card caster)
		{
			if (target == AbilityTarget.SelectTarget)
			{
				if (HasValidBoardCardTarget(game_data, caster))
				{
					return true;
				}
				if (HasValidPlayerTarget(game_data, caster))
				{
					return true;
				}
				if (HasValidSlotTarget(game_data, caster))
				{
					return true;
				}
				return false;
			}
			if (target == AbilityTarget.CardSelector)
			{
				if (HasValidCardTarget(game_data, caster))
				{
					return true;
				}
				return false;
			}
			if (target == AbilityTarget.ChoiceSelector)
			{
				AbilityData[] array = chain_abilities;
				for (int i = 0; i < array.Length; i++)
				{
					if (array[i].AreTriggerConditionsMet(game_data, caster))
					{
						return true;
					}
				}
				return false;
			}
			return true;
		}

		public bool HasValidBoardCardTarget(Game game_data, Card caster)
		{
			for (int i = 0; i < game_data.players.Length; i++)
			{
				Player player = game_data.players[i];
				for (int j = 0; j < player.cards_board.Count; j++)
				{
					Card card = player.cards_board[j];
					if (CanTarget(game_data, caster, card))
					{
						return true;
					}
				}
			}
			return false;
		}

		public bool HasValidCardTarget(Game game_data, Card caster)
		{
			for (int i = 0; i < game_data.players.Length; i++)
			{
				Player player = game_data.players[i];
				bool num = HasValidCardTarget(game_data, caster, player.cards_deck);
				bool flag = HasValidCardTarget(game_data, caster, player.cards_discard);
				bool flag2 = HasValidCardTarget(game_data, caster, player.cards_hand);
				bool flag3 = HasValidCardTarget(game_data, caster, player.cards_board);
				bool flag4 = HasValidCardTarget(game_data, caster, player.cards_equip);
				bool flag5 = HasValidCardTarget(game_data, caster, player.cards_secret);
				bool flag6 = HasValidCardTarget(game_data, caster, player.cards_temp);
				if (num || flag || flag2 || flag3 || flag4 || flag5 || flag6)
				{
					return true;
				}
			}
			return false;
		}

		public bool HasValidCardTarget(Game game_data, Card caster, List<Card> list)
		{
			for (int i = 0; i < list.Count; i++)
			{
				Card target_card = list[i];
				if (AreTargetConditionsMet(game_data, caster, target_card))
				{
					return true;
				}
			}
			return false;
		}

		public bool HasValidPlayerTarget(Game game_data, Card caster)
		{
			for (int i = 0; i < game_data.players.Length; i++)
			{
				Player player = game_data.players[i];
				if (CanTarget(game_data, caster, player))
				{
					return true;
				}
			}
			return false;
		}

		public bool HasValidSlotTarget(Game game_data, Card caster)
		{
			foreach (Slot item in Slot.GetAll())
			{
				if (CanTarget(game_data, caster, item))
				{
					return true;
				}
			}
			return false;
		}

		public bool IsSelector()
		{
			if (target != AbilityTarget.SelectTarget && target != AbilityTarget.CardSelector)
			{
				return target == AbilityTarget.ChoiceSelector;
			}
			return true;
		}

		public static AbilityData Get(string id)
		{
			if (id == null)
			{
				return null;
			}
			if (ability_dict.TryGetValue(id, out var result))
			{
				return result;
			}
			return null;
		}

		public static List<AbilityData> GetAll()
		{
			return ability_list;
		}
	}
}
