using System;
using System.Collections.Generic;

namespace TcgEngine
{
	[Serializable]
	public class Game
	{
		public string game_uid;

		public GameSettings settings;

		public int first_player;

		public int current_player;

		public int turn_count;

		public float turn_timer;

		public GameState state;

		public GamePhase phase;

		public Player[] players;

		public SelectorType selector;

		public int selector_player_id;

		public string selector_ability_id;

		public string selector_caster_uid;

		public string last_played;

		public string last_target;

		public string last_destroyed;

		public string last_summoned;

		public string ability_triggerer;

		public int rolled_value;

		public HashSet<string> ability_played = new HashSet<string>();

		public HashSet<string> cards_attacked = new HashSet<string>();

		public Game()
		{
		}

		public Game(string uid, int nb_players)
		{
			game_uid = uid;
			players = new Player[nb_players];
			for (int i = 0; i < nb_players; i++)
			{
				players[i] = new Player(i);
			}
			settings = GameSettings.Default;
		}

		public virtual bool AreAllPlayersReady()
		{
			int num = 0;
			Player[] array = players;
			for (int i = 0; i < array.Length; i++)
			{
				if (array[i].IsReady())
				{
					num++;
				}
			}
			return num >= settings.nb_players;
		}

		public virtual bool AreAllPlayersConnected()
		{
			int num = 0;
			Player[] array = players;
			for (int i = 0; i < array.Length; i++)
			{
				if (array[i].IsConnected())
				{
					num++;
				}
			}
			return num >= settings.nb_players;
		}

		public virtual bool IsPlayerTurn(Player player)
		{
			if (!IsPlayerActionTurn(player))
			{
				return IsPlayerSelectorTurn(player);
			}
			return true;
		}

		public virtual bool IsPlayerActionTurn(Player player)
		{
			if (player != null && current_player == player.player_id && state == GameState.Play)
			{
				return selector == SelectorType.None;
			}
			return false;
		}

		public virtual bool IsPlayerSelectorTurn(Player player)
		{
			if (player != null && selector_player_id == player.player_id && state == GameState.Play)
			{
				return selector != SelectorType.None;
			}
			return false;
		}

		public virtual bool CanPlayCard(Card card, Slot slot, bool skip_cost = false)
		{
			if (card == null)
			{
				return false;
			}
			Player player = GetPlayer(card.player_id);
			if (!skip_cost && !player.CanPayMana(card))
			{
				return false;
			}
			if (!player.HasCard(player.cards_hand, card))
			{
				return false;
			}
			if (card.CardData.IsBoardCard())
			{
				if (!slot.IsValid() || IsCardOnSlot(slot))
				{
					return false;
				}
				if (Slot.GetP(card.player_id) != slot.p)
				{
					return false;
				}
				return true;
			}
			if (card.CardData.IsEquipment())
			{
				if (!slot.IsValid())
				{
					return false;
				}
				Card slotCard = GetSlotCard(slot);
				if (slotCard == null || slotCard.CardData.type != CardType.Character || slotCard.player_id != card.player_id)
				{
					return false;
				}
				return true;
			}
			if (card.CardData.IsRequireTargetSpell())
			{
				return IsPlayTargetValid(card, slot);
			}
			return true;
		}

		public virtual bool CanMoveCard(Card card, Slot slot, bool skip_cost = false)
		{
			if (card == null || !slot.IsValid())
			{
				return false;
			}
			if (!IsOnBoard(card))
			{
				return false;
			}
			if (!card.CanMove(skip_cost))
			{
				return false;
			}
			if (Slot.GetP(card.player_id) != slot.p)
			{
				return false;
			}
			if (card.slot == slot)
			{
				return false;
			}
			if (GetSlotCard(slot) != null)
			{
				return false;
			}
			return true;
		}

		public virtual bool CanAttackTarget(Card attacker, Player target, bool skip_cost = false)
		{
			if (attacker == null || target == null)
			{
				return false;
			}
			if (!attacker.CanAttack(skip_cost))
			{
				return false;
			}
			if (attacker.player_id == target.player_id)
			{
				return false;
			}
			if (!IsOnBoard(attacker) || !attacker.CardData.IsCharacter())
			{
				return false;
			}
			if (target.HasStatus(StatusType.Protected) && !attacker.HasStatus(StatusType.Flying))
			{
				return false;
			}
			return true;
		}

		public virtual bool CanAttackTarget(Card attacker, Card target, bool skip_cost = false)
		{
			if (attacker == null || target == null)
			{
				return false;
			}
			if (!attacker.CanAttack(skip_cost))
			{
				return false;
			}
			if (attacker.player_id == target.player_id)
			{
				return false;
			}
			if (!IsOnBoard(attacker) || !IsOnBoard(target))
			{
				return false;
			}
			if (!attacker.CardData.IsCharacter() || !target.CardData.IsBoardCard())
			{
				return false;
			}
			if (target.HasStatus(StatusType.Stealth))
			{
				return false;
			}
			if (target.HasStatus(StatusType.Protected) && !attacker.HasStatus(StatusType.Flying))
			{
				return false;
			}
			return true;
		}

		public virtual bool CanCastAbility(Card card, AbilityData ability)
		{
			if (ability == null || card == null || !card.CanDoActivatedAbilities())
			{
				return false;
			}
			if (ability.trigger != AbilityTrigger.Activate)
			{
				return false;
			}
			if (!GetPlayer(card.player_id).CanPayAbility(card, ability))
			{
				return false;
			}
			if (!ability.AreTriggerConditionsMet(this, card))
			{
				return false;
			}
			return true;
		}

		public virtual bool CanSelectAbility(Card card, AbilityData ability)
		{
			if (ability == null || card == null || !card.CanDoAbilities())
			{
				return false;
			}
			if (!GetPlayer(card.player_id).CanPayAbility(card, ability))
			{
				return false;
			}
			if (!ability.AreTriggerConditionsMet(this, card))
			{
				return false;
			}
			return true;
		}

		public virtual bool IsPlayTargetValid(Card caster, Player target)
		{
			if (caster == null || target == null)
			{
				return false;
			}
			foreach (AbilityData ability in caster.GetAbilities())
			{
				if ((bool)ability && ability.trigger == AbilityTrigger.OnPlay && ability.target == AbilityTarget.PlayTarget && !ability.CanTarget(this, caster, target))
				{
					return false;
				}
			}
			return true;
		}

		public virtual bool IsPlayTargetValid(Card caster, Card target)
		{
			if (caster == null || target == null)
			{
				return false;
			}
			foreach (AbilityData ability in caster.GetAbilities())
			{
				if ((bool)ability && ability.trigger == AbilityTrigger.OnPlay && ability.target == AbilityTarget.PlayTarget && !ability.CanTarget(this, caster, target))
				{
					return false;
				}
			}
			return true;
		}

		public virtual bool IsPlayTargetValid(Card caster, Slot target)
		{
			if (caster == null)
			{
				return false;
			}
			if (target.IsPlayerSlot())
			{
				return IsPlayTargetValid(caster, GetPlayer(target.p));
			}
			Card slotCard = GetSlotCard(target);
			if (slotCard != null)
			{
				return IsPlayTargetValid(caster, slotCard);
			}
			foreach (AbilityData ability in caster.GetAbilities())
			{
				if ((bool)ability && ability.trigger == AbilityTrigger.OnPlay && ability.target == AbilityTarget.PlayTarget && !ability.CanTarget(this, caster, target))
				{
					return false;
				}
			}
			return true;
		}

		public Player GetPlayer(int id)
		{
			if (id >= 0 && id < players.Length)
			{
				return players[id];
			}
			return null;
		}

		public Player GetActivePlayer()
		{
			return GetPlayer(current_player);
		}

		public Player GetOpponentPlayer(int id)
		{
			int id2 = ((id == 0) ? 1 : 0);
			return GetPlayer(id2);
		}

		public Card GetCard(string card_uid)
		{
			Player[] array = players;
			for (int i = 0; i < array.Length; i++)
			{
				Card card = array[i].GetCard(card_uid);
				if (card != null)
				{
					return card;
				}
			}
			return null;
		}

		public Card GetBoardCard(string card_uid)
		{
			Player[] array = players;
			for (int i = 0; i < array.Length; i++)
			{
				foreach (Card item in array[i].cards_board)
				{
					if (item != null && item.uid == card_uid)
					{
						return item;
					}
				}
			}
			return null;
		}

		public Card GetEquipCard(string card_uid)
		{
			Player[] array = players;
			for (int i = 0; i < array.Length; i++)
			{
				foreach (Card item in array[i].cards_equip)
				{
					if (item != null && item.uid == card_uid)
					{
						return item;
					}
				}
			}
			return null;
		}

		public Card GetHandCard(string card_uid)
		{
			Player[] array = players;
			for (int i = 0; i < array.Length; i++)
			{
				foreach (Card item in array[i].cards_hand)
				{
					if (item != null && item.uid == card_uid)
					{
						return item;
					}
				}
			}
			return null;
		}

		public Card GetDeckCard(string card_uid)
		{
			Player[] array = players;
			for (int i = 0; i < array.Length; i++)
			{
				foreach (Card item in array[i].cards_deck)
				{
					if (item != null && item.uid == card_uid)
					{
						return item;
					}
				}
			}
			return null;
		}

		public Card GetDiscardCard(string card_uid)
		{
			Player[] array = players;
			for (int i = 0; i < array.Length; i++)
			{
				foreach (Card item in array[i].cards_discard)
				{
					if (item != null && item.uid == card_uid)
					{
						return item;
					}
				}
			}
			return null;
		}

		public Card GetSecretCard(string card_uid)
		{
			Player[] array = players;
			for (int i = 0; i < array.Length; i++)
			{
				foreach (Card item in array[i].cards_secret)
				{
					if (item != null && item.uid == card_uid)
					{
						return item;
					}
				}
			}
			return null;
		}

		public Card GetTempCard(string card_uid)
		{
			Player[] array = players;
			for (int i = 0; i < array.Length; i++)
			{
				foreach (Card item in array[i].cards_temp)
				{
					if (item != null && item.uid == card_uid)
					{
						return item;
					}
				}
			}
			return null;
		}

		public Card GetSlotCard(Slot slot)
		{
			Player[] array = players;
			for (int i = 0; i < array.Length; i++)
			{
				foreach (Card item in array[i].cards_board)
				{
					if (item != null && item.slot == slot)
					{
						return item;
					}
				}
			}
			return null;
		}

		public virtual Player GetRandomPlayer(Random rand)
		{
			return GetPlayer((rand.NextDouble() < 0.5) ? 1 : 0);
		}

		public virtual Card GetRandomBoardCard(Random rand)
		{
			Player randomPlayer = GetRandomPlayer(rand);
			return randomPlayer.GetRandomCard(randomPlayer.cards_board, rand);
		}

		public virtual Slot GetRandomSlot(Random rand)
		{
			return GetRandomPlayer(rand).GetRandomSlot(rand);
		}

		public bool IsInHand(Card card)
		{
			if (card != null)
			{
				return GetHandCard(card.uid) != null;
			}
			return false;
		}

		public bool IsOnBoard(Card card)
		{
			if (card != null)
			{
				return GetBoardCard(card.uid) != null;
			}
			return false;
		}

		public bool IsEquipped(Card card)
		{
			if (card != null)
			{
				return GetEquipCard(card.uid) != null;
			}
			return false;
		}

		public bool IsInDeck(Card card)
		{
			if (card != null)
			{
				return GetDeckCard(card.uid) != null;
			}
			return false;
		}

		public bool IsInDiscard(Card card)
		{
			if (card != null)
			{
				return GetDiscardCard(card.uid) != null;
			}
			return false;
		}

		public bool IsInSecret(Card card)
		{
			if (card != null)
			{
				return GetSecretCard(card.uid) != null;
			}
			return false;
		}

		public bool IsInTemp(Card card)
		{
			if (card != null)
			{
				return GetTempCard(card.uid) != null;
			}
			return false;
		}

		public bool IsCardOnSlot(Slot slot)
		{
			return GetSlotCard(slot) != null;
		}

		public bool HasStarted()
		{
			return state != GameState.Connecting;
		}

		public bool HasEnded()
		{
			return state == GameState.GameEnded;
		}

		public static Game CloneNew(Game source)
		{
			Game game = new Game();
			Clone(source, game);
			return game;
		}

		public static void Clone(Game source, Game dest)
		{
			dest.game_uid = source.game_uid;
			dest.settings = source.settings;
			dest.first_player = source.first_player;
			dest.current_player = source.current_player;
			dest.turn_count = source.turn_count;
			dest.turn_timer = source.turn_timer;
			dest.state = source.state;
			dest.phase = source.phase;
			if (dest.players == null)
			{
				dest.players = new Player[source.players.Length];
				for (int i = 0; i < source.players.Length; i++)
				{
					dest.players[i] = new Player(i);
				}
			}
			for (int j = 0; j < source.players.Length; j++)
			{
				Player.Clone(source.players[j], dest.players[j]);
			}
			dest.selector = source.selector;
			dest.selector_player_id = source.selector_player_id;
			dest.selector_caster_uid = source.selector_caster_uid;
			dest.selector_ability_id = source.selector_ability_id;
			dest.last_destroyed = source.last_destroyed;
			dest.last_played = source.last_played;
			dest.last_target = source.last_target;
			dest.last_summoned = source.last_summoned;
			dest.ability_triggerer = source.ability_triggerer;
			dest.rolled_value = source.rolled_value;
			CloneHash(source.ability_played, dest.ability_played);
			CloneHash(source.cards_attacked, dest.cards_attacked);
		}

		public static void CloneHash(HashSet<string> source, HashSet<string> dest)
		{
			dest.Clear();
			foreach (string item in source)
			{
				dest.Add(item);
			}
		}
	}
}
