using System;
using System.Collections.Generic;
using TcgEngine.AI;
using TcgEngine.Gameplay;
using Unity.Collections;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.Events;

namespace TcgEngine.Server
{
	public class GameServer
	{
		public string game_uid;

		public int nb_players = 2;

		public static float game_expire_time = 30f;

		public static float win_expire_time = 60f;

		private Game game_data;

		private GameLogic gameplay;

		private float expiration;

		private float win_expiration;

		private bool is_dedicated_server;

		private List<ClientData> players = new List<ClientData>();

		private List<ClientData> connected_clients = new List<ClientData>();

		private List<AIPlayer> ai_list = new List<AIPlayer>();

		private Queue<QueuedGameAction> queued_actions = new Queue<QueuedGameAction>();

		private Dictionary<ushort, CommandEvent> registered_commands = new Dictionary<ushort, CommandEvent>();

		public ulong ServerID => TcgNetwork.Get().ServerID;

		public NetworkMessaging Messaging => TcgNetwork.Get().Messaging;

		public GameServer(string uid, int players, bool online)
		{
			Init(uid, players, online);
		}

		~GameServer()
		{
			Clear();
		}

		protected virtual void Init(string uid, int players, bool online)
		{
			game_uid = uid;
			nb_players = Mathf.Max(players, 2);
			is_dedicated_server = online;
			game_data = new Game(uid, nb_players);
			gameplay = new GameLogic(game_data);
			RegisterAction(1100, ReceivePlayerSettings);
			RegisterAction(1102, ReceivePlayerSettingsAI);
			RegisterAction(1105, ReceiveGameplaySettings);
			RegisterAction(1000, ReceivePlayCard);
			RegisterAction(1010, ReceiveAttackTarget);
			RegisterAction(1012, ReceiveAttackPlayer);
			RegisterAction(1015, ReceiveMove);
			RegisterAction(1020, ReceiveCastCardAbility);
			RegisterAction(1030, ReceiveSelectCard);
			RegisterAction(1032, ReceiveSelectPlayer);
			RegisterAction(1034, ReceiveSelectSlot);
			RegisterAction(1036, ReceiveSelectChoice);
			RegisterAction(1039, ReceiveCancelSelection);
			RegisterAction(1040, ReceiveEndTurn);
			RegisterAction(1050, ReceiveResign);
			RegisterAction(1090, ReceiveChat);
			GameLogic gameLogic = gameplay;
			gameLogic.onGameStart = (UnityAction)Delegate.Combine(gameLogic.onGameStart, new UnityAction(OnGameStart));
			GameLogic gameLogic2 = gameplay;
			gameLogic2.onGameEnd = (UnityAction<Player>)Delegate.Combine(gameLogic2.onGameEnd, new UnityAction<Player>(OnGameEnd));
			GameLogic gameLogic3 = gameplay;
			gameLogic3.onTurnStart = (UnityAction)Delegate.Combine(gameLogic3.onTurnStart, new UnityAction(OnTurnStart));
			GameLogic gameLogic4 = gameplay;
			gameLogic4.onRefresh = (UnityAction)Delegate.Combine(gameLogic4.onRefresh, new UnityAction(RefreshAll));
			GameLogic gameLogic5 = gameplay;
			gameLogic5.onCardPlayed = (UnityAction<Card, Slot>)Delegate.Combine(gameLogic5.onCardPlayed, new UnityAction<Card, Slot>(OnCardPlayed));
			GameLogic gameLogic6 = gameplay;
			gameLogic6.onCardSummoned = (UnityAction<Card, Slot>)Delegate.Combine(gameLogic6.onCardSummoned, new UnityAction<Card, Slot>(OnCardSummoned));
			GameLogic gameLogic7 = gameplay;
			gameLogic7.onCardMoved = (UnityAction<Card, Slot>)Delegate.Combine(gameLogic7.onCardMoved, new UnityAction<Card, Slot>(OnCardMoved));
			GameLogic gameLogic8 = gameplay;
			gameLogic8.onCardTransformed = (UnityAction<Card>)Delegate.Combine(gameLogic8.onCardTransformed, new UnityAction<Card>(OnCardTransformed));
			GameLogic gameLogic9 = gameplay;
			gameLogic9.onCardDiscarded = (UnityAction<Card>)Delegate.Combine(gameLogic9.onCardDiscarded, new UnityAction<Card>(OnCardDiscarded));
			GameLogic gameLogic10 = gameplay;
			gameLogic10.onCardDrawn = (UnityAction<int>)Delegate.Combine(gameLogic10.onCardDrawn, new UnityAction<int>(OnCardDraw));
			GameLogic gameLogic11 = gameplay;
			gameLogic11.onRollValue = (UnityAction<int>)Delegate.Combine(gameLogic11.onRollValue, new UnityAction<int>(OnValueRolled));
			GameLogic gameLogic12 = gameplay;
			gameLogic12.onAbilityStart = (UnityAction<AbilityData, Card>)Delegate.Combine(gameLogic12.onAbilityStart, new UnityAction<AbilityData, Card>(OnAbilityStart));
			GameLogic gameLogic13 = gameplay;
			gameLogic13.onAbilityTargetCard = (UnityAction<AbilityData, Card, Card>)Delegate.Combine(gameLogic13.onAbilityTargetCard, new UnityAction<AbilityData, Card, Card>(OnAbilityTargetCard));
			GameLogic gameLogic14 = gameplay;
			gameLogic14.onAbilityTargetPlayer = (UnityAction<AbilityData, Card, Player>)Delegate.Combine(gameLogic14.onAbilityTargetPlayer, new UnityAction<AbilityData, Card, Player>(OnAbilityTargetPlayer));
			GameLogic gameLogic15 = gameplay;
			gameLogic15.onAbilityTargetSlot = (UnityAction<AbilityData, Card, Slot>)Delegate.Combine(gameLogic15.onAbilityTargetSlot, new UnityAction<AbilityData, Card, Slot>(OnAbilityTargetSlot));
			GameLogic gameLogic16 = gameplay;
			gameLogic16.onAbilityEnd = (UnityAction<AbilityData, Card>)Delegate.Combine(gameLogic16.onAbilityEnd, new UnityAction<AbilityData, Card>(OnAbilityEnd));
			GameLogic gameLogic17 = gameplay;
			gameLogic17.onAttackStart = (UnityAction<Card, Card>)Delegate.Combine(gameLogic17.onAttackStart, new UnityAction<Card, Card>(OnAttackStart));
			GameLogic gameLogic18 = gameplay;
			gameLogic18.onAttackEnd = (UnityAction<Card, Card>)Delegate.Combine(gameLogic18.onAttackEnd, new UnityAction<Card, Card>(OnAttackEnd));
			GameLogic gameLogic19 = gameplay;
			gameLogic19.onAttackPlayerStart = (UnityAction<Card, Player>)Delegate.Combine(gameLogic19.onAttackPlayerStart, new UnityAction<Card, Player>(OnAttackPlayerStart));
			GameLogic gameLogic20 = gameplay;
			gameLogic20.onAttackPlayerEnd = (UnityAction<Card, Player>)Delegate.Combine(gameLogic20.onAttackPlayerEnd, new UnityAction<Card, Player>(OnAttackPlayerEnd));
			GameLogic gameLogic21 = gameplay;
			gameLogic21.onSecretTrigger = (UnityAction<Card, Card>)Delegate.Combine(gameLogic21.onSecretTrigger, new UnityAction<Card, Card>(OnSecretTriggered));
			GameLogic gameLogic22 = gameplay;
			gameLogic22.onSecretResolve = (UnityAction<Card, Card>)Delegate.Combine(gameLogic22.onSecretResolve, new UnityAction<Card, Card>(OnSecretResolved));
		}

		protected virtual void Clear()
		{
			GameLogic gameLogic = gameplay;
			gameLogic.onGameStart = (UnityAction)Delegate.Remove(gameLogic.onGameStart, new UnityAction(OnGameStart));
			GameLogic gameLogic2 = gameplay;
			gameLogic2.onGameEnd = (UnityAction<Player>)Delegate.Remove(gameLogic2.onGameEnd, new UnityAction<Player>(OnGameEnd));
			GameLogic gameLogic3 = gameplay;
			gameLogic3.onTurnStart = (UnityAction)Delegate.Remove(gameLogic3.onTurnStart, new UnityAction(OnTurnStart));
			GameLogic gameLogic4 = gameplay;
			gameLogic4.onRefresh = (UnityAction)Delegate.Remove(gameLogic4.onRefresh, new UnityAction(RefreshAll));
			GameLogic gameLogic5 = gameplay;
			gameLogic5.onCardPlayed = (UnityAction<Card, Slot>)Delegate.Remove(gameLogic5.onCardPlayed, new UnityAction<Card, Slot>(OnCardPlayed));
			GameLogic gameLogic6 = gameplay;
			gameLogic6.onCardSummoned = (UnityAction<Card, Slot>)Delegate.Remove(gameLogic6.onCardSummoned, new UnityAction<Card, Slot>(OnCardSummoned));
			GameLogic gameLogic7 = gameplay;
			gameLogic7.onCardMoved = (UnityAction<Card, Slot>)Delegate.Remove(gameLogic7.onCardMoved, new UnityAction<Card, Slot>(OnCardMoved));
			GameLogic gameLogic8 = gameplay;
			gameLogic8.onCardTransformed = (UnityAction<Card>)Delegate.Remove(gameLogic8.onCardTransformed, new UnityAction<Card>(OnCardTransformed));
			GameLogic gameLogic9 = gameplay;
			gameLogic9.onCardDiscarded = (UnityAction<Card>)Delegate.Remove(gameLogic9.onCardDiscarded, new UnityAction<Card>(OnCardDiscarded));
			GameLogic gameLogic10 = gameplay;
			gameLogic10.onCardDrawn = (UnityAction<int>)Delegate.Remove(gameLogic10.onCardDrawn, new UnityAction<int>(OnCardDraw));
			GameLogic gameLogic11 = gameplay;
			gameLogic11.onRollValue = (UnityAction<int>)Delegate.Remove(gameLogic11.onRollValue, new UnityAction<int>(OnValueRolled));
			GameLogic gameLogic12 = gameplay;
			gameLogic12.onAbilityStart = (UnityAction<AbilityData, Card>)Delegate.Remove(gameLogic12.onAbilityStart, new UnityAction<AbilityData, Card>(OnAbilityStart));
			GameLogic gameLogic13 = gameplay;
			gameLogic13.onAbilityTargetCard = (UnityAction<AbilityData, Card, Card>)Delegate.Remove(gameLogic13.onAbilityTargetCard, new UnityAction<AbilityData, Card, Card>(OnAbilityTargetCard));
			GameLogic gameLogic14 = gameplay;
			gameLogic14.onAbilityTargetPlayer = (UnityAction<AbilityData, Card, Player>)Delegate.Remove(gameLogic14.onAbilityTargetPlayer, new UnityAction<AbilityData, Card, Player>(OnAbilityTargetPlayer));
			GameLogic gameLogic15 = gameplay;
			gameLogic15.onAbilityTargetSlot = (UnityAction<AbilityData, Card, Slot>)Delegate.Remove(gameLogic15.onAbilityTargetSlot, new UnityAction<AbilityData, Card, Slot>(OnAbilityTargetSlot));
			GameLogic gameLogic16 = gameplay;
			gameLogic16.onAbilityEnd = (UnityAction<AbilityData, Card>)Delegate.Remove(gameLogic16.onAbilityEnd, new UnityAction<AbilityData, Card>(OnAbilityEnd));
			GameLogic gameLogic17 = gameplay;
			gameLogic17.onAttackStart = (UnityAction<Card, Card>)Delegate.Remove(gameLogic17.onAttackStart, new UnityAction<Card, Card>(OnAttackStart));
			GameLogic gameLogic18 = gameplay;
			gameLogic18.onAttackEnd = (UnityAction<Card, Card>)Delegate.Remove(gameLogic18.onAttackEnd, new UnityAction<Card, Card>(OnAttackEnd));
			GameLogic gameLogic19 = gameplay;
			gameLogic19.onAttackPlayerStart = (UnityAction<Card, Player>)Delegate.Remove(gameLogic19.onAttackPlayerStart, new UnityAction<Card, Player>(OnAttackPlayerStart));
			GameLogic gameLogic20 = gameplay;
			gameLogic20.onAttackPlayerEnd = (UnityAction<Card, Player>)Delegate.Remove(gameLogic20.onAttackPlayerEnd, new UnityAction<Card, Player>(OnAttackPlayerEnd));
			GameLogic gameLogic21 = gameplay;
			gameLogic21.onSecretTrigger = (UnityAction<Card, Card>)Delegate.Remove(gameLogic21.onSecretTrigger, new UnityAction<Card, Card>(OnSecretTriggered));
			GameLogic gameLogic22 = gameplay;
			gameLogic22.onSecretResolve = (UnityAction<Card, Card>)Delegate.Remove(gameLogic22.onSecretResolve, new UnityAction<Card, Card>(OnSecretResolved));
		}

		public virtual void Update()
		{
			int num = CountConnectedClients();
			if (HasGameEnded() || num == 0)
			{
				expiration += Time.deltaTime;
			}
			if (num == 1 && HasGameStarted() && !HasGameEnded())
			{
				win_expiration += Time.deltaTime;
			}
			if (is_dedicated_server && !HasGameEnded() && IsWinExpired())
			{
				EndExpiredGame();
			}
			if (game_data.state == GameState.Play && !gameplay.IsResolving())
			{
				game_data.turn_timer -= Time.deltaTime;
				if (game_data.turn_timer <= 0f)
				{
					gameplay.NextStep();
				}
			}
			if (game_data.state == GameState.Connecting)
			{
				bool num2 = game_data.AreAllPlayersConnected();
				bool flag = game_data.AreAllPlayersReady();
				if (num2 && flag)
				{
					StartGame();
				}
			}
			if (queued_actions.Count > 0 && !gameplay.IsResolving())
			{
				QueuedGameAction queuedGameAction = queued_actions.Dequeue();
				ExecuteAction(queuedGameAction.type, queuedGameAction.client, queuedGameAction.sdata);
			}
			gameplay.Update(Time.deltaTime);
			foreach (AIPlayer item in ai_list)
			{
				item.Update();
			}
		}

		protected virtual void StartGame()
		{
			bool flag = !is_dedicated_server && GameplayData.Get().ai_vs_ai;
			Player[] array = game_data.players;
			foreach (Player player in array)
			{
				if (player.is_ai || flag)
				{
					AIPlayer item = AIPlayer.Create(GameplayData.Get().ai_type, gameplay, player.player_id, player.ai_level);
					ai_list.Add(item);
				}
			}
			gameplay.StartGame();
		}

		protected virtual void EndExpiredGame()
		{
			Player[] array = gameplay.GetGameData().players;
			foreach (Player player in array)
			{
				if (player.IsConnected())
				{
					gameplay.EndGame(player.player_id);
					break;
				}
			}
		}

		private void RegisterAction(ushort tag, UnityAction<ClientData, SerializedData> callback)
		{
			CommandEvent commandEvent = new CommandEvent();
			commandEvent.tag = tag;
			commandEvent.callback = callback;
			registered_commands.Add(tag, commandEvent);
		}

		public void ReceiveAction(ulong client_id, FastBufferReader reader)
		{
			ClientData client = GetClient(client_id);
			if (client != null)
			{
				reader.ReadValueSafe(out ushort value, default(FastBufferWriter.ForPrimitives));
				SerializedData serializedData = new SerializedData(reader);
				if (!gameplay.IsResolving())
				{
					ExecuteAction(value, client, serializedData);
					return;
				}
				QueuedGameAction item = new QueuedGameAction
				{
					type = value,
					client = client,
					sdata = serializedData
				};
				serializedData.PreRead();
				queued_actions.Enqueue(item);
			}
		}

		public void ExecuteAction(ushort type, ClientData client, SerializedData sdata)
		{
			if (registered_commands.TryGetValue(type, out var value))
			{
				value.callback(client, sdata);
			}
		}

		public void ReceivePlayerSettings(ClientData iclient, SerializedData sdata)
		{
			PlayerSettings playerSettings = sdata.Get<PlayerSettings>();
			Player player = GetPlayer(iclient);
			if (player != null && playerSettings != null)
			{
				SetPlayerSettings(player.player_id, playerSettings);
			}
		}

		public void ReceivePlayerSettingsAI(ClientData iclient, SerializedData sdata)
		{
			PlayerSettings playerSettings = sdata.Get<PlayerSettings>();
			Player player = GetPlayer(iclient);
			if (player != null && playerSettings != null)
			{
				SetPlayerSettingsAI(player.player_id, playerSettings);
			}
		}

		public void ReceiveGameplaySettings(ClientData iclient, SerializedData sdata)
		{
			GameSettings gameSettings = sdata.Get<GameSettings>();
			if (gameSettings != null)
			{
				SetGameSettings(gameSettings);
			}
		}

		public void ReceivePlayCard(ClientData iclient, SerializedData sdata)
		{
			MsgPlayCard msgPlayCard = sdata.Get<MsgPlayCard>();
			Player player = GetPlayer(iclient);
			if (player != null && msgPlayCard != null && game_data.IsPlayerActionTurn(player) && !gameplay.IsResolving())
			{
				Card card = player.GetCard(msgPlayCard.card_uid);
				if (card != null && card.player_id == player.player_id)
				{
					gameplay.PlayCard(card, msgPlayCard.slot);
				}
			}
		}

		public void ReceiveAttackTarget(ClientData iclient, SerializedData sdata)
		{
			MsgAttack msgAttack = sdata.Get<MsgAttack>();
			Player player = GetPlayer(iclient);
			if (player != null && msgAttack != null && game_data.IsPlayerActionTurn(player) && !gameplay.IsResolving())
			{
				Card card = player.GetCard(msgAttack.attacker_uid);
				Card card2 = game_data.GetCard(msgAttack.target_uid);
				if (card != null && card2 != null && card.player_id == player.player_id)
				{
					gameplay.AttackTarget(card, card2);
				}
			}
		}

		public void ReceiveAttackPlayer(ClientData iclient, SerializedData sdata)
		{
			MsgAttackPlayer msgAttackPlayer = sdata.Get<MsgAttackPlayer>();
			Player player = GetPlayer(iclient);
			if (player != null && msgAttackPlayer != null && game_data.IsPlayerActionTurn(player) && !gameplay.IsResolving())
			{
				Card card = player.GetCard(msgAttackPlayer.attacker_uid);
				Player player2 = game_data.GetPlayer(msgAttackPlayer.target_id);
				if (card != null && player2 != null && card.player_id == player.player_id)
				{
					gameplay.AttackPlayer(card, player2);
				}
			}
		}

		public void ReceiveMove(ClientData iclient, SerializedData sdata)
		{
			MsgPlayCard msgPlayCard = sdata.Get<MsgPlayCard>();
			Player player = GetPlayer(iclient);
			if (player != null && msgPlayCard != null && game_data.IsPlayerActionTurn(player) && !gameplay.IsResolving())
			{
				Card card = player.GetCard(msgPlayCard.card_uid);
				if (card != null && card.player_id == player.player_id)
				{
					gameplay.MoveCard(card, msgPlayCard.slot);
				}
			}
		}

		public void ReceiveCastCardAbility(ClientData iclient, SerializedData sdata)
		{
			MsgCastAbility msgCastAbility = sdata.Get<MsgCastAbility>();
			Player player = GetPlayer(iclient);
			if (player != null && msgCastAbility != null && game_data.IsPlayerActionTurn(player) && !gameplay.IsResolving())
			{
				Card card = player.GetCard(msgCastAbility.caster_uid);
				AbilityData iability = AbilityData.Get(msgCastAbility.ability_id);
				if (card != null && card.player_id == player.player_id)
				{
					gameplay.CastAbility(card, iability);
				}
			}
		}

		public void ReceiveSelectCard(ClientData iclient, SerializedData sdata)
		{
			MsgCard msgCard = sdata.Get<MsgCard>();
			Player player = GetPlayer(iclient);
			if (player != null && msgCard != null && game_data.IsPlayerSelectorTurn(player) && !gameplay.IsResolving())
			{
				Card card = game_data.GetCard(msgCard.card_uid);
				gameplay.SelectCard(card);
			}
		}

		public void ReceiveSelectPlayer(ClientData iclient, SerializedData sdata)
		{
			MsgPlayer msgPlayer = sdata.Get<MsgPlayer>();
			Player player = GetPlayer(iclient);
			if (player != null && msgPlayer != null && game_data.IsPlayerSelectorTurn(player) && !gameplay.IsResolving())
			{
				Player player2 = game_data.GetPlayer(msgPlayer.player_id);
				gameplay.SelectPlayer(player2);
			}
		}

		public void ReceiveSelectSlot(ClientData iclient, SerializedData sdata)
		{
			Slot target = sdata.Get<Slot>();
			Player player = GetPlayer(iclient);
			if (player != null && game_data.IsPlayerSelectorTurn(player) && !gameplay.IsResolving() && target.IsValid())
			{
				gameplay.SelectSlot(target);
			}
		}

		public void ReceiveSelectChoice(ClientData iclient, SerializedData sdata)
		{
			MsgInt msgInt = sdata.Get<MsgInt>();
			Player player = GetPlayer(iclient);
			if (player != null && msgInt != null && game_data.IsPlayerSelectorTurn(player) && !gameplay.IsResolving())
			{
				gameplay.SelectChoice(msgInt.value);
			}
		}

		public void ReceiveCancelSelection(ClientData iclient, SerializedData sdata)
		{
			Player player = GetPlayer(iclient);
			if (player != null && game_data.IsPlayerSelectorTurn(player) && !gameplay.IsResolving())
			{
				gameplay.CancelSelection();
			}
		}

		public void ReceiveEndTurn(ClientData iclient, SerializedData sdata)
		{
			Player player = GetPlayer(iclient);
			if (player != null && game_data.IsPlayerTurn(player))
			{
				gameplay.NextStep();
			}
		}

		public void ReceiveResign(ClientData iclient, SerializedData sdata)
		{
			Player player = GetPlayer(iclient);
			if (player != null && game_data.state != GameState.Connecting && game_data.state != GameState.GameEnded)
			{
				int winner = ((player.player_id == 0) ? 1 : 0);
				gameplay.EndGame(winner);
			}
		}

		public void ReceiveChat(ClientData iclient, SerializedData sdata)
		{
			MsgChat msgChat = sdata.Get<MsgChat>();
			Player player = GetPlayer(iclient);
			if (player != null && msgChat != null)
			{
				msgChat.player_id = player.player_id;
				SendToAll(1090, msgChat, NetworkDelivery.Reliable);
			}
		}

		public virtual async void SetPlayerDeck(int player_id, string username, UserDeckData deck)
		{
			Player player = game_data.GetPlayer(player_id);
			if (player == null || game_data.state != GameState.Connecting)
			{
				return;
			}
			UserData userData = Authenticator.Get().UserData;
			if (Authenticator.Get().IsApi())
			{
				userData = await ApiClient.Get().LoadUserData(username);
			}
			UserDeckData userDeckData = userData?.GetDeck(deck.tid);
			if (userData != null && userDeckData != null)
			{
				if (userData.IsDeckValid(userDeckData))
				{
					gameplay.SetPlayerDeck(player, userDeckData);
					SendPlayerReady(player);
				}
				else
				{
					Debug.Log(userData.username + " deck is invalid: " + userDeckData.title);
				}
				return;
			}
			DeckData deckData = DeckData.Get(deck.tid);
			if (deckData != null)
			{
				gameplay.SetPlayerDeck(player, deckData);
			}
			else if (Authenticator.Get().IsTest())
			{
				gameplay.SetPlayerDeck(player, deck);
			}
			else
			{
				Debug.Log("Player " + player_id + " deck not found: " + deck.tid);
			}
			SendPlayerReady(player);
		}

		public virtual void SetPlayerSettings(int player_id, PlayerSettings psettings)
		{
			if (game_data.state == GameState.Connecting)
			{
				Player player = game_data.GetPlayer(player_id);
				if (player != null && !player.ready)
				{
					player.avatar = psettings.avatar;
					player.cardback = psettings.cardback;
					player.is_ai = false;
					player.ready = true;
					SetPlayerDeck(player_id, player.username, psettings.deck);
					RefreshAll();
				}
			}
		}

		public virtual void SetPlayerSettingsAI(int player_id, PlayerSettings psettings)
		{
			if (game_data.state == GameState.Connecting && !is_dedicated_server)
			{
				Player opponentPlayer = game_data.GetOpponentPlayer(player_id);
				if (opponentPlayer != null && !opponentPlayer.ready)
				{
					opponentPlayer.username = psettings.username;
					opponentPlayer.avatar = psettings.avatar;
					opponentPlayer.cardback = psettings.cardback;
					opponentPlayer.is_ai = true;
					opponentPlayer.ready = true;
					opponentPlayer.ai_level = psettings.ai_level;
					SetPlayerDeck(opponentPlayer.player_id, opponentPlayer.username, psettings.deck);
					RefreshAll();
				}
			}
		}

		public virtual void SetGameSettings(GameSettings settings)
		{
			if (game_data.state == GameState.Connecting)
			{
				game_data.settings = settings;
				RefreshAll();
			}
		}

		public void AddClient(ClientData client)
		{
			if (!connected_clients.Contains(client))
			{
				connected_clients.Add(client);
			}
		}

		public void RemoveClient(ClientData client)
		{
			connected_clients.Remove(client);
			Player player = GetPlayer(client);
			if (player != null && player.connected)
			{
				player.connected = false;
				RefreshAll();
			}
		}

		public ClientData GetClient(ulong client_id)
		{
			foreach (ClientData connected_client in connected_clients)
			{
				if (connected_client.client_id == client_id)
				{
					return connected_client;
				}
			}
			return null;
		}

		public int AddPlayer(ClientData client)
		{
			if (!players.Contains(client))
			{
				players.Add(client);
			}
			int num = FindPlayerID(client.user_id);
			Player player = game_data.GetPlayer(num);
			if (player != null)
			{
				player.username = client.username;
				player.connected = true;
			}
			return num;
		}

		public int FindPlayerID(string user_id)
		{
			int num = 0;
			foreach (ClientData player in players)
			{
				if (player.user_id == user_id)
				{
					return num;
				}
				num++;
			}
			return -1;
		}

		public Player GetPlayer(ClientData client)
		{
			return GetPlayer(client.user_id);
		}

		public Player GetPlayer(string user_id)
		{
			int id = FindPlayerID(user_id);
			return game_data?.GetPlayer(id);
		}

		public bool IsPlayer(string user_id)
		{
			return GetPlayer(user_id) != null;
		}

		public bool IsConnectedPlayer(string user_id)
		{
			return GetPlayer(user_id)?.connected ?? false;
		}

		public int CountPlayers()
		{
			return players.Count;
		}

		public int CountConnectedClients()
		{
			int num = 0;
			Player[] array = GetGameData().players;
			for (int i = 0; i < array.Length; i++)
			{
				if (array[i].IsConnected())
				{
					num++;
				}
			}
			return num;
		}

		public Game GetGameData()
		{
			return gameplay.GetGameData();
		}

		public virtual bool HasGameStarted()
		{
			return gameplay.IsGameStarted();
		}

		public virtual bool HasGameEnded()
		{
			return gameplay.IsGameEnded();
		}

		public virtual bool IsGameExpired()
		{
			return expiration > game_expire_time;
		}

		public virtual bool IsWinExpired()
		{
			return win_expiration > win_expire_time;
		}

		protected virtual void OnGameStart()
		{
			SendToAll(2010);
			if (is_dedicated_server && Authenticator.Get().IsApi())
			{
				ApiClient.Get().CreateMatch(game_data);
			}
		}

		protected virtual void OnGameEnd(Player winner)
		{
			MsgPlayer msgPlayer = new MsgPlayer();
			msgPlayer.player_id = winner?.player_id ?? (-1);
			SendToAll(2012, msgPlayer, NetworkDelivery.Reliable);
			if (is_dedicated_server && Authenticator.Get().IsApi())
			{
				ApiClient.Get().EndMatch(game_data, winner.player_id);
			}
		}

		protected virtual void OnTurnStart()
		{
			MsgPlayer msgPlayer = new MsgPlayer();
			msgPlayer.player_id = game_data.current_player;
			SendToAll(2015, msgPlayer, NetworkDelivery.Reliable);
		}

		protected virtual void OnCardPlayed(Card card, Slot slot)
		{
			MsgPlayCard msgPlayCard = new MsgPlayCard();
			msgPlayCard.card_uid = card.uid;
			msgPlayCard.slot = slot;
			SendToAll(2020, msgPlayCard, NetworkDelivery.Reliable);
		}

		protected virtual void OnCardMoved(Card card, Slot slot)
		{
			MsgPlayCard msgPlayCard = new MsgPlayCard();
			msgPlayCard.card_uid = card.uid;
			msgPlayCard.slot = slot;
			SendToAll(2027, msgPlayCard, NetworkDelivery.Reliable);
		}

		protected virtual void OnCardSummoned(Card card, Slot slot)
		{
			MsgPlayCard msgPlayCard = new MsgPlayCard();
			msgPlayCard.card_uid = card.uid;
			msgPlayCard.slot = slot;
			SendToAll(2022, msgPlayCard, NetworkDelivery.Reliable);
		}

		protected virtual void OnCardTransformed(Card card)
		{
			MsgCard msgCard = new MsgCard();
			msgCard.card_uid = card.uid;
			SendToAll(2023, msgCard, NetworkDelivery.Reliable);
		}

		protected virtual void OnCardDiscarded(Card card)
		{
			MsgCard msgCard = new MsgCard();
			msgCard.card_uid = card.uid;
			SendToAll(2025, msgCard, NetworkDelivery.Reliable);
		}

		protected virtual void OnCardDraw(int nb)
		{
			MsgInt msgInt = new MsgInt();
			msgInt.value = nb;
			SendToAll(2026, msgInt, NetworkDelivery.Reliable);
		}

		protected virtual void OnValueRolled(int nb)
		{
			MsgInt msgInt = new MsgInt();
			msgInt.value = nb;
			SendToAll(2070, msgInt, NetworkDelivery.Reliable);
		}

		protected virtual void OnAttackStart(Card attacker, Card target)
		{
			MsgAttack msgAttack = new MsgAttack();
			msgAttack.attacker_uid = attacker.uid;
			msgAttack.target_uid = target.uid;
			msgAttack.damage = 0;
			SendToAll(2030, msgAttack, NetworkDelivery.Reliable);
		}

		protected virtual void OnAttackEnd(Card attacker, Card target)
		{
			MsgAttack msgAttack = new MsgAttack();
			msgAttack.attacker_uid = attacker.uid;
			msgAttack.target_uid = target.uid;
			msgAttack.damage = 0;
			SendToAll(2032, msgAttack, NetworkDelivery.Reliable);
		}

		protected virtual void OnAttackPlayerStart(Card attacker, Player target)
		{
			MsgAttackPlayer msgAttackPlayer = new MsgAttackPlayer();
			msgAttackPlayer.attacker_uid = attacker.uid;
			msgAttackPlayer.target_id = target.player_id;
			msgAttackPlayer.damage = 0;
			SendToAll(2034, msgAttackPlayer, NetworkDelivery.Reliable);
		}

		protected virtual void OnAttackPlayerEnd(Card attacker, Player target)
		{
			MsgAttackPlayer msgAttackPlayer = new MsgAttackPlayer();
			msgAttackPlayer.attacker_uid = attacker.uid;
			msgAttackPlayer.target_id = target.player_id;
			msgAttackPlayer.damage = 0;
			SendToAll(2036, msgAttackPlayer, NetworkDelivery.Reliable);
		}

		protected virtual void OnAbilityStart(AbilityData ability, Card caster)
		{
			MsgCastAbility msgCastAbility = new MsgCastAbility();
			msgCastAbility.ability_id = ability.id;
			msgCastAbility.caster_uid = caster.uid;
			msgCastAbility.target_uid = "";
			SendToAll(2040, msgCastAbility, NetworkDelivery.Reliable);
		}

		protected virtual void OnAbilityTargetCard(AbilityData ability, Card caster, Card target)
		{
			MsgCastAbility msgCastAbility = new MsgCastAbility();
			msgCastAbility.ability_id = ability.id;
			msgCastAbility.caster_uid = caster.uid;
			msgCastAbility.target_uid = ((target != null) ? target.uid : "");
			SendToAll(2042, msgCastAbility, NetworkDelivery.Reliable);
		}

		protected virtual void OnAbilityTargetPlayer(AbilityData ability, Card caster, Player target)
		{
			MsgCastAbilityPlayer msgCastAbilityPlayer = new MsgCastAbilityPlayer();
			msgCastAbilityPlayer.ability_id = ability.id;
			msgCastAbilityPlayer.caster_uid = caster.uid;
			msgCastAbilityPlayer.target_id = target?.player_id ?? (-1);
			SendToAll(2043, msgCastAbilityPlayer, NetworkDelivery.Reliable);
		}

		protected virtual void OnAbilityTargetSlot(AbilityData ability, Card caster, Slot target)
		{
			MsgCastAbilitySlot msgCastAbilitySlot = new MsgCastAbilitySlot();
			msgCastAbilitySlot.ability_id = ability.id;
			msgCastAbilitySlot.caster_uid = caster.uid;
			msgCastAbilitySlot.slot = target;
			SendToAll(2044, msgCastAbilitySlot, NetworkDelivery.Reliable);
		}

		protected virtual void OnAbilityEnd(AbilityData ability, Card caster)
		{
			MsgCastAbility msgCastAbility = new MsgCastAbility();
			msgCastAbility.ability_id = ability.id;
			msgCastAbility.caster_uid = caster.uid;
			msgCastAbility.target_uid = "";
			SendToAll(2048, msgCastAbility, NetworkDelivery.Reliable);
		}

		protected virtual void OnSecretTriggered(Card secret, Card trigger)
		{
			MsgSecret msgSecret = new MsgSecret();
			msgSecret.secret_uid = secret.uid;
			msgSecret.triggerer_uid = ((trigger != null) ? trigger.uid : "");
			SendToAll(2060, msgSecret, NetworkDelivery.Reliable);
		}

		protected virtual void OnSecretResolved(Card secret, Card trigger)
		{
			MsgSecret msgSecret = new MsgSecret();
			msgSecret.secret_uid = secret.uid;
			msgSecret.triggerer_uid = ((trigger != null) ? trigger.uid : "");
			SendToAll(2061, msgSecret, NetworkDelivery.Reliable);
		}

		protected virtual void SendPlayerReady(Player player)
		{
			if (player != null && player.IsReady())
			{
				MsgInt msgInt = new MsgInt();
				msgInt.value = player.player_id;
				SendToAll(2001, msgInt, NetworkDelivery.Reliable);
			}
		}

		public virtual void RefreshAll()
		{
			MsgRefreshAll msgRefreshAll = new MsgRefreshAll();
			msgRefreshAll.game_data = GetGameData();
			SendToAll(2100, msgRefreshAll, NetworkDelivery.ReliableFragmentedSequenced);
		}

		public void SendToAll(ushort tag)
		{
			FastBufferWriter writer = new FastBufferWriter(128, Allocator.Temp, TcgNetwork.MsgSizeMax);
			writer.WriteValueSafe(in tag, default(FastBufferWriter.ForPrimitives));
			foreach (ClientData connected_client in connected_clients)
			{
				if (connected_client != null)
				{
					Messaging.Send("refresh", connected_client.client_id, writer, NetworkDelivery.Reliable);
				}
			}
			writer.Dispose();
		}

		public void SendToAll(ushort tag, INetworkSerializable data, NetworkDelivery delivery)
		{
			FastBufferWriter writer = new FastBufferWriter(128, Allocator.Temp, TcgNetwork.MsgSizeMax);
			writer.WriteValueSafe(in tag, default(FastBufferWriter.ForPrimitives));
			writer.WriteNetworkSerializable(in data);
			foreach (ClientData connected_client in connected_clients)
			{
				if (connected_client != null)
				{
					Messaging.Send("refresh", connected_client.client_id, writer, delivery);
				}
			}
			writer.Dispose();
		}
	}
}
