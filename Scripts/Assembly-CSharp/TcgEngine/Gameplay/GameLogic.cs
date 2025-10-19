using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

namespace TcgEngine.Gameplay
{
	public class GameLogic
	{
		public UnityAction onGameStart;

		public UnityAction<Player> onGameEnd;

		public UnityAction onTurnStart;

		public UnityAction onTurnPlay;

		public UnityAction onTurnEnd;

		public UnityAction<Card, Slot> onCardPlayed;

		public UnityAction<Card, Slot> onCardSummoned;

		public UnityAction<Card, Slot> onCardMoved;

		public UnityAction<Card> onCardTransformed;

		public UnityAction<Card> onCardDiscarded;

		public UnityAction<int> onCardDrawn;

		public UnityAction<int> onRollValue;

		public UnityAction<AbilityData, Card> onAbilityStart;

		public UnityAction<AbilityData, Card, Card> onAbilityTargetCard;

		public UnityAction<AbilityData, Card, Player> onAbilityTargetPlayer;

		public UnityAction<AbilityData, Card, Slot> onAbilityTargetSlot;

		public UnityAction<AbilityData, Card> onAbilityEnd;

		public UnityAction<Card, Card> onAttackStart;

		public UnityAction<Card, Card> onAttackEnd;

		public UnityAction<Card, Player> onAttackPlayerStart;

		public UnityAction<Card, Player> onAttackPlayerEnd;

		public UnityAction<Card, Card> onSecretTrigger;

		public UnityAction<Card, Card> onSecretResolve;

		public UnityAction onRefresh;

		private Game game_data;

		private ResolveQueue resolve_queue;

		private bool is_ai_predict;

		private System.Random random = new System.Random();

		private ListSwap<Card> card_array = new ListSwap<Card>();

		private ListSwap<Player> player_array = new ListSwap<Player>();

		private ListSwap<Slot> slot_array = new ListSwap<Slot>();

		private ListSwap<CardData> card_data_array = new ListSwap<CardData>();

		private List<Card> cards_to_clear = new List<Card>();

		public Game GameData => game_data;

		public ResolveQueue ResolveQueue => resolve_queue;

		public GameLogic(bool is_ai)
		{
			resolve_queue = new ResolveQueue(null, is_ai);
			is_ai_predict = is_ai;
		}

		public GameLogic(Game game)
		{
			game_data = game;
			resolve_queue = new ResolveQueue(game, skip: false);
		}

		public virtual void SetData(Game game)
		{
			game_data = game;
			resolve_queue.SetData(game);
		}

		public virtual void Update(float delta)
		{
			resolve_queue.Update(delta);
		}

		public virtual void StartGame()
		{
			if (game_data.state == GameState.GameEnded)
			{
				return;
			}
			game_data.state = GameState.Play;
			game_data.first_player = ((!(random.NextDouble() < 0.5)) ? 1 : 0);
			game_data.current_player = game_data.first_player;
			game_data.turn_count = 1;
			LevelData level = game_data.settings.GetLevel();
			if (level != null)
			{
				if (level != null && level.first_player == LevelFirst.Player)
				{
					game_data.first_player = 0;
				}
				if (level != null && level.first_player == LevelFirst.AI)
				{
					game_data.first_player = 1;
				}
				game_data.current_player = game_data.first_player;
			}
			Player[] players = game_data.players;
			foreach (Player player in players)
			{
				DeckPuzzleData deckPuzzleData = DeckPuzzleData.Get(player.deck);
				player.hp_max = ((deckPuzzleData != null) ? deckPuzzleData.start_hp : GameplayData.Get().hp_start);
				player.hp = player.hp_max;
				player.mana_max = ((deckPuzzleData != null) ? deckPuzzleData.start_mana : GameplayData.Get().mana_start);
				player.mana = player.mana_max;
				int nb = ((deckPuzzleData != null) ? deckPuzzleData.start_cards : GameplayData.Get().cards_start);
				DrawCard(player, nb);
				if ((level == null || level.first_player == LevelFirst.Random) && player.player_id != game_data.first_player && GameplayData.Get().second_bonus != null)
				{
					Card item = Card.Create(GameplayData.Get().second_bonus, VariantData.GetDefault(), player);
					player.cards_hand.Add(item);
				}
			}
			RefreshData();
			onGameStart?.Invoke();
			StartTurn();
		}

		public virtual void StartTurn()
		{
			if (game_data.state == GameState.GameEnded)
			{
				return;
			}
			ClearTurnData();
			game_data.phase = GamePhase.StartTurn;
			onTurnStart?.Invoke();
			RefreshData();
			Player activePlayer = game_data.GetActivePlayer();
			if (game_data.turn_count > 1 || activePlayer.player_id != game_data.first_player)
			{
				DrawCard(activePlayer, GameplayData.Get().cards_per_turn);
			}
			activePlayer.mana_max += GameplayData.Get().mana_per_turn;
			activePlayer.mana_max = Mathf.Min(activePlayer.mana_max, GameplayData.Get().mana_max);
			activePlayer.mana = activePlayer.mana_max;
			game_data.turn_timer = GameplayData.Get().turn_duration;
			activePlayer.history_list.Clear();
			if (activePlayer.HasStatus(StatusType.Poisoned))
			{
				activePlayer.hp -= activePlayer.GetStatusValue(StatusType.Poisoned);
			}
			if (activePlayer.hero != null)
			{
				activePlayer.hero.Refresh();
			}
			for (int num = activePlayer.cards_board.Count - 1; num >= 0; num--)
			{
				Card card = activePlayer.cards_board[num];
				if (!card.HasStatus(StatusType.Sleep))
				{
					card.Refresh();
				}
				if (card.HasStatus(StatusType.Poisoned))
				{
					DamageCard(card, card.GetStatusValue(StatusType.Poisoned));
				}
			}
			UpdateOngoing();
			TriggerPlayerCardsAbilityType(activePlayer, AbilityTrigger.StartOfTurn);
			TriggerPlayerSecrets(activePlayer, AbilityTrigger.StartOfTurn);
			resolve_queue.AddCallback(StartMainPhase);
			resolve_queue.ResolveAll(0.2f);
		}

		public virtual void StartNextTurn()
		{
			if (game_data.state != GameState.GameEnded)
			{
				game_data.current_player = (game_data.current_player + 1) % game_data.settings.nb_players;
				if (game_data.current_player == game_data.first_player)
				{
					game_data.turn_count++;
				}
				CheckForWinner();
				StartTurn();
			}
		}

		public virtual void StartMainPhase()
		{
			if (game_data.state != GameState.GameEnded)
			{
				game_data.phase = GamePhase.Main;
				onTurnPlay?.Invoke();
				RefreshData();
			}
		}

		public virtual void EndTurn()
		{
			if (game_data.state == GameState.GameEnded || game_data.phase != GamePhase.Main)
			{
				return;
			}
			game_data.selector = SelectorType.None;
			game_data.phase = GamePhase.EndTurn;
			Player[] players = game_data.players;
			foreach (Player player in players)
			{
				foreach (Card item in player.cards_board)
				{
					item.ReduceStatusDurations();
				}
				foreach (Card item2 in player.cards_equip)
				{
					item2.ReduceStatusDurations();
				}
			}
			Player activePlayer = game_data.GetActivePlayer();
			TriggerPlayerCardsAbilityType(activePlayer, AbilityTrigger.EndOfTurn);
			onTurnEnd?.Invoke();
			RefreshData();
			resolve_queue.AddCallback(StartNextTurn);
			resolve_queue.ResolveAll(0.2f);
		}

		public virtual void EndGame(int winner)
		{
			if (game_data.state != GameState.GameEnded)
			{
				game_data.state = GameState.GameEnded;
				game_data.phase = GamePhase.None;
				game_data.selector = SelectorType.None;
				game_data.current_player = winner;
				resolve_queue.Clear();
				Player player = game_data.GetPlayer(winner);
				onGameEnd?.Invoke(player);
				RefreshData();
			}
		}

		public virtual void NextStep()
		{
			if (game_data.state != GameState.GameEnded)
			{
				CancelSelection();
				resolve_queue.AddCallback(EndTurn);
				resolve_queue.ResolveAll();
			}
		}

		protected virtual void CheckForWinner()
		{
			int num = 0;
			Player player = null;
			Player[] players = game_data.players;
			foreach (Player player2 in players)
			{
				if (!player2.IsDead())
				{
					player = player2;
					num++;
				}
			}
			switch (num)
			{
			case 0:
				EndGame(-1);
				break;
			case 1:
				EndGame(player.player_id);
				break;
			}
		}

		protected virtual void ClearTurnData()
		{
			game_data.selector = SelectorType.None;
			resolve_queue.Clear();
			card_array.Clear();
			player_array.Clear();
			slot_array.Clear();
			card_data_array.Clear();
			game_data.last_played = null;
			game_data.last_destroyed = null;
			game_data.last_target = null;
			game_data.last_summoned = null;
			game_data.ability_triggerer = null;
			game_data.ability_played.Clear();
			game_data.cards_attacked.Clear();
		}

		public virtual void SetPlayerDeck(Player player, DeckData deck)
		{
			player.cards_all.Clear();
			player.cards_deck.Clear();
			player.deck = deck.id;
			player.hero = null;
			VariantData ivariant = VariantData.GetDefault();
			if (deck.hero != null)
			{
				player.hero = Card.Create(deck.hero, ivariant, player);
			}
			CardData[] cards = deck.cards;
			foreach (CardData cardData in cards)
			{
				if (cardData != null)
				{
					Card item = Card.Create(cardData, ivariant, player);
					player.cards_deck.Add(item);
				}
			}
			DeckPuzzleData deckPuzzleData = deck as DeckPuzzleData;
			if (deckPuzzleData != null)
			{
				DeckCardSlot[] board_cards = deckPuzzleData.board_cards;
				foreach (DeckCardSlot deckCardSlot in board_cards)
				{
					Card card = Card.Create(deckCardSlot.card, ivariant, player);
					card.slot = new Slot(deckCardSlot.slot, Slot.GetP(player.player_id));
					player.cards_board.Add(card);
				}
			}
			if (deckPuzzleData == null || !deckPuzzleData.dont_shuffle_deck)
			{
				ShuffleDeck(player.cards_deck);
			}
		}

		public virtual void SetPlayerDeck(Player player, UserDeckData deck)
		{
			player.cards_all.Clear();
			player.cards_deck.Clear();
			player.deck = deck.tid;
			player.hero = null;
			if (deck.hero != null)
			{
				CardData cardData = CardData.Get(deck.hero.tid);
				VariantData variantData = VariantData.Get(deck.hero.variant);
				if (cardData != null && variantData != null)
				{
					player.hero = Card.Create(cardData, variantData, player);
				}
			}
			UserCardData[] cards = deck.cards;
			foreach (UserCardData userCardData in cards)
			{
				CardData cardData2 = CardData.Get(userCardData.tid);
				VariantData variantData2 = VariantData.Get(userCardData.variant);
				if (cardData2 != null && variantData2 != null)
				{
					for (int j = 0; j < userCardData.quantity; j++)
					{
						Card item = Card.Create(cardData2, variantData2, player);
						player.cards_deck.Add(item);
					}
				}
			}
			ShuffleDeck(player.cards_deck);
		}

		public virtual void PlayCard(Card card, Slot slot, bool skip_cost = false)
		{
			if (game_data.CanPlayCard(card, slot, skip_cost))
			{
				Player player = game_data.GetPlayer(card.player_id);
				if (!skip_cost)
				{
					player.PayMana(card);
				}
				player.RemoveCardFromAllGroups(card);
				CardData cardData = card.CardData;
				if (cardData.IsBoardCard())
				{
					player.cards_board.Add(card);
					card.slot = slot;
					card.exhausted = true;
				}
				else if (cardData.IsEquipment())
				{
					Card slotCard = game_data.GetSlotCard(slot);
					EquipCard(slotCard, card);
					card.exhausted = true;
				}
				else if (cardData.IsSecret())
				{
					player.cards_secret.Add(card);
				}
				else
				{
					player.cards_discard.Add(card);
					card.slot = slot;
				}
				if (!is_ai_predict && !cardData.IsSecret())
				{
					player.AddHistory(1000, card);
				}
				game_data.last_played = card.uid;
				UpdateOngoing();
				TriggerSecrets(AbilityTrigger.OnPlayOther, card);
				TriggerCardAbilityType(AbilityTrigger.OnPlay, card);
				TriggerOtherCardsAbilityType(AbilityTrigger.OnPlayOther, card);
				RefreshData();
				onCardPlayed?.Invoke(card, slot);
				resolve_queue.ResolveAll(0.3f);
			}
		}

		public virtual void MoveCard(Card card, Slot slot, bool skip_cost = false)
		{
			if (game_data.CanMoveCard(card, slot, skip_cost))
			{
				card.slot = slot;
				Card equipCard = game_data.GetEquipCard(card.equipped_uid);
				if (equipCard != null)
				{
					equipCard.slot = slot;
				}
				UpdateOngoing();
				RefreshData();
				onCardMoved?.Invoke(card, slot);
				resolve_queue.ResolveAll(0.2f);
			}
		}

		public virtual void CastAbility(Card card, AbilityData iability)
		{
			if (game_data.CanCastAbility(card, iability))
			{
				Player player = game_data.GetPlayer(card.player_id);
				if (!is_ai_predict && iability.target != AbilityTarget.SelectTarget)
				{
					player.AddHistory(1020, card, iability);
				}
				card.RemoveStatus(StatusType.Stealth);
				TriggerCardAbility(iability, card);
				resolve_queue.ResolveAll();
			}
		}

		public virtual void AttackTarget(Card attacker, Card target, bool skip_cost = false)
		{
			if (game_data.CanAttackTarget(attacker, target, skip_cost))
			{
				Player player = game_data.GetPlayer(attacker.player_id);
				if (!is_ai_predict)
				{
					player.AddHistory(1010, attacker, target);
				}
				TriggerCardAbilityType(AbilityTrigger.OnBeforeAttack, attacker, target);
				TriggerCardAbilityType(AbilityTrigger.OnBeforeDefend, target, attacker);
				TriggerSecrets(AbilityTrigger.OnBeforeAttack, attacker);
				TriggerSecrets(AbilityTrigger.OnBeforeDefend, target);
				resolve_queue.AddAttack(attacker, target, ResolveAttack, skip_cost);
				resolve_queue.ResolveAll();
			}
		}

		protected virtual void ResolveAttack(Card attacker, Card target, bool skip_cost)
		{
			if (game_data.IsOnBoard(attacker) && game_data.IsOnBoard(target))
			{
				onAttackStart?.Invoke(attacker, target);
				attacker.RemoveStatus(StatusType.Stealth);
				UpdateOngoing();
				resolve_queue.AddAttack(attacker, target, ResolveAttackHit, skip_cost);
				resolve_queue.ResolveAll(0.3f);
			}
		}

		protected virtual void ResolveAttackHit(Card attacker, Card target, bool skip_cost)
		{
			int attack = attacker.GetAttack();
			int attack2 = target.GetAttack();
			DamageCard(attacker, target, attack);
			if (!attacker.HasStatus(StatusType.Intimidate))
			{
				DamageCard(target, attacker, attack2);
			}
			if (!skip_cost)
			{
				ExhaustBattle(attacker);
			}
			UpdateOngoing();
			bool num = game_data.IsOnBoard(attacker);
			bool flag = game_data.IsOnBoard(target);
			if (num)
			{
				TriggerCardAbilityType(AbilityTrigger.OnAfterAttack, attacker, target);
			}
			if (flag)
			{
				TriggerCardAbilityType(AbilityTrigger.OnAfterDefend, target, attacker);
			}
			if (num)
			{
				TriggerSecrets(AbilityTrigger.OnAfterAttack, attacker);
			}
			if (flag)
			{
				TriggerSecrets(AbilityTrigger.OnAfterDefend, target);
			}
			onAttackEnd?.Invoke(attacker, target);
			RefreshData();
			CheckForWinner();
			resolve_queue.ResolveAll(0.2f);
		}

		public virtual void AttackPlayer(Card attacker, Player target, bool skip_cost = false)
		{
			if (attacker != null && target != null && game_data.CanAttackTarget(attacker, target, skip_cost))
			{
				Player player = game_data.GetPlayer(attacker.player_id);
				if (!is_ai_predict)
				{
					player.AddHistory(1012, attacker, target);
				}
				TriggerSecrets(AbilityTrigger.OnBeforeAttack, attacker);
				TriggerCardAbilityType(AbilityTrigger.OnBeforeAttack, attacker, target);
				resolve_queue.AddAttack(attacker, target, ResolveAttackPlayer, skip_cost);
				resolve_queue.ResolveAll();
			}
		}

		protected virtual void ResolveAttackPlayer(Card attacker, Player target, bool skip_cost)
		{
			if (game_data.IsOnBoard(attacker))
			{
				onAttackPlayerStart?.Invoke(attacker, target);
				attacker.RemoveStatus(StatusType.Stealth);
				UpdateOngoing();
				resolve_queue.AddAttack(attacker, target, ResolveAttackPlayerHit, skip_cost);
				resolve_queue.ResolveAll(0.3f);
			}
		}

		protected virtual void ResolveAttackPlayerHit(Card attacker, Player target, bool skip_cost)
		{
			DamagePlayer(attacker, target, attacker.GetAttack());
			if (!skip_cost)
			{
				ExhaustBattle(attacker);
			}
			UpdateOngoing();
			if (game_data.IsOnBoard(attacker))
			{
				TriggerCardAbilityType(AbilityTrigger.OnAfterAttack, attacker, target);
			}
			TriggerSecrets(AbilityTrigger.OnAfterAttack, attacker);
			onAttackPlayerEnd?.Invoke(attacker, target);
			RefreshData();
			CheckForWinner();
			resolve_queue.ResolveAll(0.2f);
		}

		public virtual void ExhaustBattle(Card attacker)
		{
			bool flag = game_data.cards_attacked.Contains(attacker.uid);
			game_data.cards_attacked.Add(attacker.uid);
			bool flag2 = attacker.HasStatus(StatusType.Fury) && !flag;
			attacker.exhausted = !flag2;
		}

		public virtual void RedirectAttack(Card attacker, Card new_target)
		{
			foreach (AttackQueueElement item in resolve_queue.GetAttackQueue())
			{
				if (item.attacker.uid == attacker.uid)
				{
					item.target = new_target;
					item.ptarget = null;
					item.callback = ResolveAttack;
					item.pcallback = null;
				}
			}
		}

		public virtual void RedirectAttack(Card attacker, Player new_target)
		{
			foreach (AttackQueueElement item in resolve_queue.GetAttackQueue())
			{
				if (item.attacker.uid == attacker.uid)
				{
					item.ptarget = new_target;
					item.target = null;
					item.pcallback = ResolveAttackPlayer;
					item.callback = null;
				}
			}
		}

		public virtual void ShuffleDeck(List<Card> cards)
		{
			for (int i = 0; i < cards.Count; i++)
			{
				Card value = cards[i];
				int index = random.Next(i, cards.Count);
				cards[i] = cards[index];
				cards[index] = value;
			}
		}

		public virtual void DrawCard(Player player, int nb = 1)
		{
			for (int i = 0; i < nb; i++)
			{
				if (player.cards_deck.Count > 0 && player.cards_hand.Count < GameplayData.Get().cards_max)
				{
					Card item = player.cards_deck[0];
					player.cards_deck.RemoveAt(0);
					player.cards_hand.Add(item);
				}
			}
			onCardDrawn?.Invoke(nb);
		}

		public virtual void DrawDiscardCard(Player player, int nb = 1)
		{
			for (int i = 0; i < nb; i++)
			{
				if (player.cards_deck.Count > 0)
				{
					Card item = player.cards_deck[0];
					player.cards_deck.RemoveAt(0);
					player.cards_discard.Add(item);
				}
			}
		}

		public virtual Card SummonCopy(Player player, Card copy, Slot slot)
		{
			CardData cardData = copy.CardData;
			return SummonCard(player, cardData, copy.VariantData, slot);
		}

		public virtual Card SummonCopyHand(Player player, Card copy)
		{
			CardData cardData = copy.CardData;
			return SummonCardHand(player, cardData, copy.VariantData);
		}

		public virtual Card SummonCard(Player player, CardData card, VariantData variant, Slot slot)
		{
			if (!slot.IsValid())
			{
				return null;
			}
			if (game_data.GetSlotCard(slot) != null)
			{
				return null;
			}
			Card card2 = SummonCardHand(player, card, variant);
			PlayCard(card2, slot, skip_cost: true);
			onCardSummoned?.Invoke(card2, slot);
			return card2;
		}

		public virtual Card SummonCardHand(Player player, CardData card, VariantData variant)
		{
			Card card2 = Card.Create(card, variant, player);
			player.cards_hand.Add(card2);
			game_data.last_summoned = card2.uid;
			return card2;
		}

		public virtual Card TransformCard(Card card, CardData transform_to)
		{
			card.SetCard(transform_to, card.VariantData);
			onCardTransformed?.Invoke(card);
			return card;
		}

		public virtual void EquipCard(Card card, Card equipment)
		{
			if (card != null && equipment != null && card.player_id == equipment.player_id && !card.CardData.IsEquipment() && equipment.CardData.IsEquipment())
			{
				UnequipAll(card);
				Player player = game_data.GetPlayer(card.player_id);
				player.RemoveCardFromAllGroups(equipment);
				player.cards_equip.Add(equipment);
				card.equipped_uid = equipment.uid;
				equipment.slot = card.slot;
			}
		}

		public virtual void UnequipAll(Card card)
		{
			if (card != null && card.equipped_uid != null)
			{
				Card equipCard = game_data.GetPlayer(card.player_id).GetEquipCard(card.equipped_uid);
				if (equipCard != null)
				{
					card.equipped_uid = null;
					DiscardCard(equipCard);
				}
			}
		}

		public virtual void ChangeOwner(Card card, Player owner)
		{
			if (card.player_id != owner.player_id)
			{
				Player player = game_data.GetPlayer(card.player_id);
				player.RemoveCardFromAllGroups(card);
				player.cards_all.Remove(card.uid);
				owner.cards_all[card.uid] = card;
				card.player_id = owner.player_id;
			}
		}

		public virtual void DamagePlayer(Card attacker, Player target, int value)
		{
			target.hp -= value;
			target.hp = Mathf.Clamp(target.hp, 0, target.hp_max);
			Player player = game_data.GetPlayer(attacker.player_id);
			if (attacker.HasStatus(StatusType.LifeSteal))
			{
				player.hp += value;
			}
		}

		public virtual void HealCard(Card target, int value)
		{
			if (target != null && !target.HasStatus(StatusType.Invincibility))
			{
				target.damage -= value;
				target.damage = Mathf.Max(target.damage, 0);
			}
		}

		public virtual void HealPlayer(Player target, int value)
		{
			if (target != null)
			{
				target.hp += value;
				target.hp = Mathf.Clamp(target.hp, 0, target.hp_max);
			}
		}

		public virtual void DamageCard(Card target, int value)
		{
			if (target != null && !target.HasStatus(StatusType.Invincibility) && !target.HasStatus(StatusType.SpellImmunity))
			{
				target.damage += value;
				if (target.GetHP() <= 0)
				{
					DiscardCard(target);
				}
			}
		}

		public virtual void DamageCard(Card attacker, Card target, int value, bool spell_damage = false)
		{
			if (attacker == null || target == null || target.HasStatus(StatusType.Invincibility) || (target.HasStatus(StatusType.SpellImmunity) && attacker.CardData.type != CardType.Character))
			{
				return;
			}
			if (target.HasStatus(StatusType.Shell) && value > 0)
			{
				target.RemoveStatus(StatusType.Shell);
				return;
			}
			if (!spell_damage && target.HasStatus(StatusType.Armor))
			{
				value = Mathf.Max(value - target.GetStatusValue(StatusType.Armor), 0);
			}
			int num = Mathf.Min(value, target.GetHP());
			int num2 = value - target.GetHP();
			target.damage += value;
			Player player = game_data.GetPlayer(target.player_id);
			if (!spell_damage && num2 > 0 && attacker.player_id == game_data.current_player && attacker.HasStatus(StatusType.Trample))
			{
				player.hp -= num2;
			}
			Player player2 = game_data.GetPlayer(attacker.player_id);
			if (!spell_damage && attacker.HasStatus(StatusType.LifeSteal))
			{
				player2.hp += num;
			}
			target.RemoveStatus(StatusType.Sleep);
			if (value > 0 && attacker.HasStatus(StatusType.Deathtouch) && target.CardData.type == CardType.Character)
			{
				KillCard(attacker, target);
			}
			if (target.GetHP() <= 0)
			{
				KillCard(attacker, target);
			}
		}

		public virtual void KillCard(Card attacker, Card target)
		{
			if (attacker != null && target != null && (game_data.IsOnBoard(target) || game_data.IsEquipped(target)) && !target.HasStatus(StatusType.Invincibility))
			{
				Player player = game_data.GetPlayer(attacker.player_id);
				if (attacker.player_id != target.player_id)
				{
					player.kill_count++;
				}
				DiscardCard(target);
				TriggerCardAbilityType(AbilityTrigger.OnKill, attacker, target);
			}
		}

		public virtual void DiscardCard(Card card)
		{
			if (card != null && !game_data.IsInDiscard(card))
			{
				_ = card.CardData;
				Player player = game_data.GetPlayer(card.player_id);
				bool flag = game_data.IsOnBoard(card) || game_data.IsEquipped(card);
				UnequipAll(card);
				player.RemoveCardFromAllGroups(card);
				player.cards_discard.Add(card);
				game_data.last_destroyed = card.uid;
				Card bearerCard = player.GetBearerCard(card);
				if (bearerCard != null)
				{
					bearerCard.equipped_uid = null;
				}
				if (flag)
				{
					TriggerCardAbilityType(AbilityTrigger.OnDeath, card);
					TriggerOtherCardsAbilityType(AbilityTrigger.OnDeathOther, card);
					TriggerSecrets(AbilityTrigger.OnDeathOther, card);
				}
				cards_to_clear.Add(card);
				onCardDiscarded?.Invoke(card);
			}
		}

		public int RollRandomValue(int dice)
		{
			return RollRandomValue(1, dice + 1);
		}

		public virtual int RollRandomValue(int min, int max)
		{
			game_data.rolled_value = random.Next(min, max);
			onRollValue?.Invoke(game_data.rolled_value);
			resolve_queue.SetDelay(1f);
			return game_data.rolled_value;
		}

		public virtual void TriggerCardAbilityType(AbilityTrigger type, Card caster, Card triggerer = null)
		{
			foreach (AbilityData ability in caster.GetAbilities())
			{
				if ((bool)ability && ability.trigger == type)
				{
					TriggerCardAbility(ability, caster, triggerer);
				}
			}
			Card equipCard = game_data.GetEquipCard(caster.equipped_uid);
			if (equipCard != null)
			{
				TriggerCardAbilityType(type, equipCard, triggerer);
			}
		}

		public virtual void TriggerCardAbilityType(AbilityTrigger type, Card caster, Player triggerer)
		{
			foreach (AbilityData ability in caster.GetAbilities())
			{
				if ((bool)ability && ability.trigger == type)
				{
					TriggerCardAbility(ability, caster, triggerer);
				}
			}
			Card equipCard = game_data.GetEquipCard(caster.equipped_uid);
			if (equipCard != null)
			{
				TriggerCardAbilityType(type, equipCard, triggerer);
			}
		}

		public virtual void TriggerOtherCardsAbilityType(AbilityTrigger type, Card triggerer)
		{
			Player[] players = game_data.players;
			foreach (Player player in players)
			{
				if (player.hero != null)
				{
					TriggerCardAbilityType(type, player.hero, triggerer);
				}
				foreach (Card item in player.cards_board)
				{
					TriggerCardAbilityType(type, item, triggerer);
				}
			}
		}

		public virtual void TriggerPlayerCardsAbilityType(Player player, AbilityTrigger type)
		{
			if (player.hero != null)
			{
				TriggerCardAbilityType(type, player.hero, player.hero);
			}
			foreach (Card item in player.cards_board)
			{
				TriggerCardAbilityType(type, item, item);
			}
		}

		public virtual void TriggerCardAbility(AbilityData iability, Card caster, Card triggerer = null)
		{
			Card card = ((triggerer != null) ? triggerer : caster);
			if (!caster.HasStatus(StatusType.Silenced) && iability.AreTriggerConditionsMet(game_data, caster, card))
			{
				resolve_queue.AddAbility(iability, caster, card, ResolveCardAbility);
			}
		}

		public virtual void TriggerCardAbility(AbilityData iability, Card caster, Player triggerer)
		{
			if (!caster.HasStatus(StatusType.Silenced) && iability.AreTriggerConditionsMet(game_data, caster, triggerer))
			{
				resolve_queue.AddAbility(iability, caster, caster, ResolveCardAbility);
			}
		}

		protected virtual void ResolveCardAbility(AbilityData iability, Card caster, Card triggerer)
		{
			if (caster.CanDoAbilities())
			{
				onAbilityStart?.Invoke(iability, caster);
				game_data.ability_triggerer = triggerer.uid;
				if (!ResolveCardAbilitySelector(iability, caster))
				{
					ResolveCardAbilityPlayTarget(iability, caster);
					ResolveCardAbilityPlayers(iability, caster);
					ResolveCardAbilityCards(iability, caster);
					ResolveCardAbilitySlots(iability, caster);
					ResolveCardAbilityCardData(iability, caster);
					ResolveCardAbilityNoTarget(iability, caster);
					AfterAbilityResolved(iability, caster);
				}
			}
		}

		protected virtual bool ResolveCardAbilitySelector(AbilityData iability, Card caster)
		{
			if (iability.target == AbilityTarget.SelectTarget)
			{
				GoToSelectTarget(iability, caster);
				return true;
			}
			if (iability.target == AbilityTarget.CardSelector)
			{
				GoToSelectorCard(iability, caster);
				return true;
			}
			if (iability.target == AbilityTarget.ChoiceSelector)
			{
				GoToSelectorChoice(iability, caster);
				return true;
			}
			return false;
		}

		protected virtual void ResolveCardAbilityPlayTarget(AbilityData iability, Card caster)
		{
			if (iability.target != AbilityTarget.PlayTarget)
			{
				return;
			}
			Slot slot = caster.slot;
			Card slotCard = game_data.GetSlotCard(slot);
			if (slot.IsPlayerSlot())
			{
				Player player = game_data.GetPlayer(slot.p);
				if (iability.CanTarget(game_data, caster, player))
				{
					ResolveEffectTarget(iability, caster, player);
				}
			}
			else if (slotCard != null)
			{
				if (iability.CanTarget(game_data, caster, slotCard))
				{
					ResolveEffectTarget(iability, caster, slotCard);
				}
			}
			else if (iability.CanTarget(game_data, caster, slot))
			{
				ResolveEffectTarget(iability, caster, slot);
			}
		}

		protected virtual void ResolveCardAbilityPlayers(AbilityData iability, Card caster)
		{
			foreach (Player playerTarget in iability.GetPlayerTargets(game_data, caster, player_array))
			{
				ResolveEffectTarget(iability, caster, playerTarget);
			}
		}

		protected virtual void ResolveCardAbilityCards(AbilityData iability, Card caster)
		{
			foreach (Card cardTarget in iability.GetCardTargets(game_data, caster, card_array))
			{
				ResolveEffectTarget(iability, caster, cardTarget);
			}
		}

		protected virtual void ResolveCardAbilitySlots(AbilityData iability, Card caster)
		{
			foreach (Slot slotTarget in iability.GetSlotTargets(game_data, caster, slot_array))
			{
				ResolveEffectTarget(iability, caster, slotTarget);
			}
		}

		protected virtual void ResolveCardAbilityCardData(AbilityData iability, Card caster)
		{
			foreach (CardData cardDataTarget in iability.GetCardDataTargets(game_data, caster, card_data_array))
			{
				ResolveEffectTarget(iability, caster, cardDataTarget);
			}
		}

		protected virtual void ResolveCardAbilityNoTarget(AbilityData iability, Card caster)
		{
			if (iability.target == AbilityTarget.None)
			{
				iability.DoEffects(this, caster);
			}
		}

		protected virtual void ResolveEffectTarget(AbilityData iability, Card caster, Player target)
		{
			iability.DoEffects(this, caster, target);
			onAbilityTargetPlayer?.Invoke(iability, caster, target);
		}

		protected virtual void ResolveEffectTarget(AbilityData iability, Card caster, Card target)
		{
			iability.DoEffects(this, caster, target);
			onAbilityTargetCard?.Invoke(iability, caster, target);
			game_data.last_target = target.uid;
		}

		protected virtual void ResolveEffectTarget(AbilityData iability, Card caster, Slot target)
		{
			iability.DoEffects(this, caster, target);
			onAbilityTargetSlot?.Invoke(iability, caster, target);
		}

		protected virtual void ResolveEffectTarget(AbilityData iability, Card caster, CardData target)
		{
			iability.DoEffects(this, caster, target);
		}

		protected virtual void AfterAbilityResolved(AbilityData iability, Card caster)
		{
			Player player = game_data.GetPlayer(caster.player_id);
			game_data.ability_played.Add(iability.id);
			if (iability.trigger == AbilityTrigger.Activate || iability.trigger == AbilityTrigger.None)
			{
				player.mana -= iability.mana_cost;
				caster.exhausted = caster.exhausted || iability.exhaust;
			}
			UpdateOngoing();
			CheckForWinner();
			if (iability.target != AbilityTarget.ChoiceSelector && game_data.state != GameState.GameEnded)
			{
				AbilityData[] chain_abilities = iability.chain_abilities;
				foreach (AbilityData abilityData in chain_abilities)
				{
					if (abilityData != null)
					{
						TriggerCardAbility(abilityData, caster);
					}
				}
			}
			onAbilityEnd?.Invoke(iability, caster);
			resolve_queue.ResolveAll(0.5f);
			RefreshData();
		}

		public virtual void UpdateOngoing()
		{
			for (int i = 0; i < game_data.players.Length; i++)
			{
				Player player = game_data.players[i];
				player.ClearOngoing();
				for (int j = 0; j < player.cards_board.Count; j++)
				{
					player.cards_board[j].ClearOngoing();
				}
				for (int k = 0; k < player.cards_equip.Count; k++)
				{
					player.cards_equip[k].ClearOngoing();
				}
				for (int l = 0; l < player.cards_hand.Count; l++)
				{
					player.cards_hand[l].ClearOngoing();
				}
			}
			for (int m = 0; m < game_data.players.Length; m++)
			{
				Player player2 = game_data.players[m];
				UpdateOngoingAbilities(player2, player2.hero);
				for (int n = 0; n < player2.cards_board.Count; n++)
				{
					Card card = player2.cards_board[n];
					UpdateOngoingAbilities(player2, card);
				}
				for (int num = 0; num < player2.cards_equip.Count; num++)
				{
					Card card2 = player2.cards_equip[num];
					UpdateOngoingAbilities(player2, card2);
				}
			}
			for (int num2 = 0; num2 < game_data.players.Length; num2++)
			{
				Player player3 = game_data.players[num2];
				for (int num3 = 0; num3 < player3.cards_board.Count; num3++)
				{
					Card card3 = player3.cards_board[num3];
					if (card3.HasStatus(StatusType.Protection) && !card3.HasStatus(StatusType.Stealth))
					{
						player3.AddOngoingStatus(StatusType.Protected, 0);
						for (int num4 = 0; num4 < player3.cards_board.Count; num4++)
						{
							Card card4 = player3.cards_board[num4];
							if (!card4.HasStatus(StatusType.Protection) && !card4.HasStatus(StatusType.Protected))
							{
								card4.AddOngoingStatus(StatusType.Protected, 0);
							}
						}
					}
					foreach (CardStatus item in card3.status)
					{
						AddOngoingStatusBonus(card3, item);
					}
					foreach (CardStatus item2 in card3.ongoing_status)
					{
						AddOngoingStatusBonus(card3, item2);
					}
				}
				for (int num5 = 0; num5 < player3.cards_hand.Count; num5++)
				{
					Card card5 = player3.cards_hand[num5];
					foreach (CardStatus item3 in card5.status)
					{
						AddOngoingStatusBonus(card5, item3);
					}
					foreach (CardStatus item4 in card5.ongoing_status)
					{
						AddOngoingStatusBonus(card5, item4);
					}
				}
			}
			for (int num6 = 0; num6 < game_data.players.Length; num6++)
			{
				Player player4 = game_data.players[num6];
				for (int num7 = player4.cards_board.Count - 1; num7 >= 0; num7--)
				{
					Card card6 = player4.cards_board[num7];
					if (card6.GetHP() <= 0)
					{
						DiscardCard(card6);
					}
				}
				for (int num8 = player4.cards_equip.Count - 1; num8 >= 0; num8--)
				{
					Card card7 = player4.cards_equip[num8];
					if (card7.GetHP() <= 0)
					{
						DiscardCard(card7);
					}
					if (player4.GetBearerCard(card7) == null)
					{
						DiscardCard(card7);
					}
				}
			}
			for (int num9 = 0; num9 < cards_to_clear.Count; num9++)
			{
				cards_to_clear[num9].Clear();
			}
			cards_to_clear.Clear();
		}

		protected virtual void UpdateOngoingAbilities(Player player, Card card)
		{
			if (card == null || !card.CanDoAbilities())
			{
				return;
			}
			List<AbilityData> abilities = card.GetAbilities();
			for (int i = 0; i < abilities.Count; i++)
			{
				AbilityData abilityData = abilities[i];
				if (!(abilityData != null) || abilityData.trigger != AbilityTrigger.Ongoing || !abilityData.AreTriggerConditionsMet(game_data, card))
				{
					continue;
				}
				if (abilityData.target == AbilityTarget.Self && abilityData.AreTargetConditionsMet(game_data, card, card))
				{
					abilityData.DoOngoingEffects(this, card, card);
				}
				if (abilityData.target == AbilityTarget.PlayerSelf && abilityData.AreTargetConditionsMet(game_data, card, player))
				{
					abilityData.DoOngoingEffects(this, card, player);
				}
				if (abilityData.target == AbilityTarget.AllPlayers || abilityData.target == AbilityTarget.PlayerOpponent)
				{
					for (int j = 0; j < game_data.players.Length; j++)
					{
						if (abilityData.target == AbilityTarget.AllPlayers || j != player.player_id)
						{
							Player player2 = game_data.players[j];
							if (abilityData.AreTargetConditionsMet(game_data, card, player2))
							{
								abilityData.DoOngoingEffects(this, card, player2);
							}
						}
					}
				}
				if (abilityData.target == AbilityTarget.EquippedCard)
				{
					if (card.CardData.IsEquipment())
					{
						Card bearerCard = player.GetBearerCard(card);
						if (bearerCard != null && abilityData.AreTargetConditionsMet(game_data, card, bearerCard))
						{
							abilityData.DoOngoingEffects(this, card, bearerCard);
						}
					}
					else if (card.equipped_uid != null)
					{
						Card card2 = game_data.GetCard(card.equipped_uid);
						if (card2 != null && abilityData.AreTargetConditionsMet(game_data, card, card2))
						{
							abilityData.DoOngoingEffects(this, card, card2);
						}
					}
				}
				if (abilityData.target != AbilityTarget.AllCardsAllPiles && abilityData.target != AbilityTarget.AllCardsHand && abilityData.target != AbilityTarget.AllCardsBoard)
				{
					continue;
				}
				for (int k = 0; k < game_data.players.Length; k++)
				{
					Player player3 = game_data.players[k];
					if (abilityData.target == AbilityTarget.AllCardsAllPiles || abilityData.target == AbilityTarget.AllCardsHand)
					{
						for (int l = 0; l < player3.cards_hand.Count; l++)
						{
							Card card3 = player3.cards_hand[l];
							if (abilityData.AreTargetConditionsMet(game_data, card, card3))
							{
								abilityData.DoOngoingEffects(this, card, card3);
							}
						}
					}
					if (abilityData.target == AbilityTarget.AllCardsAllPiles || abilityData.target == AbilityTarget.AllCardsBoard)
					{
						for (int m = 0; m < player3.cards_board.Count; m++)
						{
							Card card4 = player3.cards_board[m];
							if (abilityData.AreTargetConditionsMet(game_data, card, card4))
							{
								abilityData.DoOngoingEffects(this, card, card4);
							}
						}
					}
					if (abilityData.target != AbilityTarget.AllCardsAllPiles)
					{
						continue;
					}
					for (int n = 0; n < player3.cards_equip.Count; n++)
					{
						Card card5 = player3.cards_equip[n];
						if (abilityData.AreTargetConditionsMet(game_data, card, card5))
						{
							abilityData.DoOngoingEffects(this, card, card5);
						}
					}
				}
			}
		}

		protected virtual void AddOngoingStatusBonus(Card card, CardStatus status)
		{
			if (status.type == StatusType.AddAttack)
			{
				card.attack_ongoing += status.value;
			}
			if (status.type == StatusType.AddHP)
			{
				card.hp_ongoing += status.value;
			}
			if (status.type == StatusType.AddManaCost)
			{
				card.mana_ongoing += status.value;
			}
		}

		public virtual bool TriggerPlayerSecrets(Player player, AbilityTrigger secret_trigger)
		{
			for (int num = player.cards_secret.Count - 1; num >= 0; num--)
			{
				Card card = player.cards_secret[num];
				if (card.CardData.type == CardType.Secret && !card.exhausted && card.AreAbilityConditionsMet(secret_trigger, game_data, card, card))
				{
					resolve_queue.AddSecret(secret_trigger, card, card, ResolveSecret);
					resolve_queue.SetDelay(0.5f);
					card.exhausted = true;
					if (onSecretTrigger != null)
					{
						onSecretTrigger(card, card);
					}
					return true;
				}
			}
			return false;
		}

		public virtual bool TriggerSecrets(AbilityTrigger secret_trigger, Card trigger_card)
		{
			if (trigger_card != null && trigger_card.HasStatus(StatusType.SpellImmunity))
			{
				return false;
			}
			for (int i = 0; i < game_data.players.Length; i++)
			{
				if (i == game_data.current_player)
				{
					continue;
				}
				Player player = game_data.players[i];
				for (int num = player.cards_secret.Count - 1; num >= 0; num--)
				{
					Card card = player.cards_secret[num];
					if (card.CardData.type == CardType.Secret && !card.exhausted)
					{
						Card card2 = ((trigger_card != null) ? trigger_card : card);
						if (card.AreAbilityConditionsMet(secret_trigger, game_data, card, card2))
						{
							resolve_queue.AddSecret(secret_trigger, card, card2, ResolveSecret);
							resolve_queue.SetDelay(0.5f);
							card.exhausted = true;
							if (onSecretTrigger != null)
							{
								onSecretTrigger(card, card2);
							}
							return true;
						}
					}
				}
			}
			return false;
		}

		protected virtual void ResolveSecret(AbilityTrigger secret_trigger, Card secret_card, Card trigger)
		{
			CardData cardData = secret_card.CardData;
			game_data.GetPlayer(secret_card.player_id);
			if (cardData.type == CardType.Secret)
			{
				Player player = game_data.GetPlayer(trigger.player_id);
				if (!is_ai_predict)
				{
					player.AddHistory(2060, secret_card, trigger);
				}
				TriggerCardAbilityType(secret_trigger, secret_card, trigger);
				DiscardCard(secret_card);
				if (onSecretResolve != null)
				{
					onSecretResolve(secret_card, trigger);
				}
			}
		}

		public virtual void SelectCard(Card target)
		{
			if (game_data.selector == SelectorType.None)
			{
				return;
			}
			Card card = game_data.GetCard(game_data.selector_caster_uid);
			AbilityData abilityData = AbilityData.Get(game_data.selector_ability_id);
			if (card == null || target == null || abilityData == null)
			{
				return;
			}
			if (game_data.selector == SelectorType.SelectTarget)
			{
				if (!abilityData.CanTarget(game_data, card, target))
				{
					return;
				}
				Player player = game_data.GetPlayer(card.player_id);
				if (!is_ai_predict)
				{
					player.AddHistory(1020, card, abilityData, target);
				}
				game_data.selector = SelectorType.None;
				ResolveEffectTarget(abilityData, card, target);
				AfterAbilityResolved(abilityData, card);
				resolve_queue.ResolveAll();
			}
			if (game_data.selector == SelectorType.SelectorCard && abilityData.IsCardSelectionValid(game_data, card, target, card_array))
			{
				game_data.selector = SelectorType.None;
				ResolveEffectTarget(abilityData, card, target);
				AfterAbilityResolved(abilityData, card);
				resolve_queue.ResolveAll();
			}
		}

		public virtual void SelectPlayer(Player target)
		{
			if (game_data.selector == SelectorType.None)
			{
				return;
			}
			Card card = game_data.GetCard(game_data.selector_caster_uid);
			AbilityData abilityData = AbilityData.Get(game_data.selector_ability_id);
			if (card != null && target != null && !(abilityData == null) && game_data.selector == SelectorType.SelectTarget && abilityData.CanTarget(game_data, card, target))
			{
				Player player = game_data.GetPlayer(card.player_id);
				if (!is_ai_predict)
				{
					player.AddHistory(1020, card, abilityData, target);
				}
				game_data.selector = SelectorType.None;
				ResolveEffectTarget(abilityData, card, target);
				AfterAbilityResolved(abilityData, card);
				resolve_queue.ResolveAll();
			}
		}

		public virtual void SelectSlot(Slot target)
		{
			if (game_data.selector == SelectorType.None)
			{
				return;
			}
			Card card = game_data.GetCard(game_data.selector_caster_uid);
			AbilityData abilityData = AbilityData.Get(game_data.selector_ability_id);
			if (card != null && !(abilityData == null) && target.IsValid() && game_data.selector == SelectorType.SelectTarget && abilityData.CanTarget(game_data, card, target))
			{
				Player player = game_data.GetPlayer(card.player_id);
				if (!is_ai_predict)
				{
					player.AddHistory(1020, card, abilityData, target);
				}
				game_data.selector = SelectorType.None;
				ResolveEffectTarget(abilityData, card, target);
				AfterAbilityResolved(abilityData, card);
				resolve_queue.ResolveAll();
			}
		}

		public virtual void SelectChoice(int choice)
		{
			if (game_data.selector == SelectorType.None)
			{
				return;
			}
			Card card = game_data.GetCard(game_data.selector_caster_uid);
			AbilityData abilityData = AbilityData.Get(game_data.selector_ability_id);
			if (card != null && !(abilityData == null) && choice >= 0 && game_data.selector == SelectorType.SelectorChoice && abilityData.target == AbilityTarget.ChoiceSelector && choice >= 0 && choice < abilityData.chain_abilities.Length)
			{
				AbilityData abilityData2 = abilityData.chain_abilities[choice];
				if (abilityData2 != null && game_data.CanSelectAbility(card, abilityData2))
				{
					game_data.selector = SelectorType.None;
					AfterAbilityResolved(abilityData, card);
					ResolveCardAbility(abilityData2, card, card);
					resolve_queue.ResolveAll();
				}
			}
		}

		public virtual void CancelSelection()
		{
			if (game_data.selector != SelectorType.None)
			{
				game_data.selector = SelectorType.None;
				RefreshData();
			}
		}

		protected virtual void GoToSelectTarget(AbilityData iability, Card caster)
		{
			game_data.selector = SelectorType.SelectTarget;
			game_data.selector_player_id = caster.player_id;
			game_data.selector_ability_id = iability.id;
			game_data.selector_caster_uid = caster.uid;
			RefreshData();
		}

		protected virtual void GoToSelectorCard(AbilityData iability, Card caster)
		{
			game_data.selector = SelectorType.SelectorCard;
			game_data.selector_player_id = caster.player_id;
			game_data.selector_ability_id = iability.id;
			game_data.selector_caster_uid = caster.uid;
			RefreshData();
		}

		protected virtual void GoToSelectorChoice(AbilityData iability, Card caster)
		{
			game_data.selector = SelectorType.SelectorChoice;
			game_data.selector_player_id = caster.player_id;
			game_data.selector_ability_id = iability.id;
			game_data.selector_caster_uid = caster.uid;
			RefreshData();
		}

		public virtual void RefreshData()
		{
			onRefresh?.Invoke();
		}

		public virtual void ClearResolve()
		{
			resolve_queue.Clear();
		}

		public virtual bool IsResolving()
		{
			return resolve_queue.IsResolving();
		}

		public virtual bool IsGameStarted()
		{
			return game_data.HasStarted();
		}

		public virtual bool IsGameEnded()
		{
			return game_data.HasEnded();
		}

		public virtual Game GetGameData()
		{
			return game_data;
		}

		public System.Random GetRandom()
		{
			return random;
		}
	}
}
