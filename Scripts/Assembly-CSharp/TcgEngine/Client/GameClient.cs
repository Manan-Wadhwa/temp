using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Unity.Collections;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.Events;

namespace TcgEngine.Client
{
	public class GameClient : MonoBehaviour
	{
		public static GameSettings game_settings = GameSettings.Default;

		public static PlayerSettings player_settings = PlayerSettings.Default;

		public static PlayerSettings ai_settings = PlayerSettings.DefaultAI;

		public static string observe_user = null;

		public UnityAction onConnectServer;

		public UnityAction onConnectGame;

		public UnityAction<int> onPlayerReady;

		public UnityAction onGameStart;

		public UnityAction<int> onGameEnd;

		public UnityAction<int> onNewTurn;

		public UnityAction<Card, Slot> onCardPlayed;

		public UnityAction<Card, Slot> onCardMoved;

		public UnityAction<Slot> onCardSummoned;

		public UnityAction<Card> onCardTransformed;

		public UnityAction<Card> onCardDiscarded;

		public UnityAction<int> onCardDraw;

		public UnityAction<int> onValueRolled;

		public UnityAction<AbilityData, Card> onAbilityStart;

		public UnityAction<AbilityData, Card, Card> onAbilityTargetCard;

		public UnityAction<AbilityData, Card, Player> onAbilityTargetPlayer;

		public UnityAction<AbilityData, Card, Slot> onAbilityTargetSlot;

		public UnityAction<AbilityData, Card> onAbilityEnd;

		public UnityAction<Card, Card> onSecretTrigger;

		public UnityAction<Card, Card> onSecretResolve;

		public UnityAction<Card, Card> onAttackStart;

		public UnityAction<Card, Card> onAttackEnd;

		public UnityAction<Card, Player> onAttackPlayerStart;

		public UnityAction<Card, Player> onAttackPlayerEnd;

		public UnityAction<int, string> onChatMsg;

		public UnityAction<string> onServerMsg;

		public UnityAction onRefreshAll;

		private int player_id;

		private Game game_data;

		private bool observe_mode;

		private int observe_player_id;

		private float timer;

		private Dictionary<ushort, RefreshEvent> registered_commands = new Dictionary<ushort, RefreshEvent>();

		private static GameClient instance;

		public bool IsHost => TcgNetwork.Get().IsHost;

		public ulong ServerID => TcgNetwork.Get().ServerID;

		public NetworkMessaging Messaging => TcgNetwork.Get().Messaging;

		protected virtual void Awake()
		{
			instance = this;
			Application.targetFrameRate = 120;
		}

		protected virtual void Start()
		{
			RegisterRefresh(2000, OnConnectedToGame);
			RegisterRefresh(2001, OnPlayerReady);
			RegisterRefresh(2010, OnGameStart);
			RegisterRefresh(2012, OnGameEnd);
			RegisterRefresh(2015, OnNewTurn);
			RegisterRefresh(2020, OnCardPlayed);
			RegisterRefresh(2027, OnCardMoved);
			RegisterRefresh(2022, OnCardSummoned);
			RegisterRefresh(2023, OnCardTransformed);
			RegisterRefresh(2025, OnCardDiscarded);
			RegisterRefresh(2026, OnCardDraw);
			RegisterRefresh(2070, OnValueRolled);
			RegisterRefresh(2030, OnAttackStart);
			RegisterRefresh(2032, OnAttackEnd);
			RegisterRefresh(2034, OnAttackPlayerStart);
			RegisterRefresh(2036, OnAttackPlayerEnd);
			RegisterRefresh(2040, OnAbilityTrigger);
			RegisterRefresh(2042, OnAbilityTargetCard);
			RegisterRefresh(2043, OnAbilityTargetPlayer);
			RegisterRefresh(2044, OnAbilityTargetSlot);
			RegisterRefresh(2048, OnAbilityAfter);
			RegisterRefresh(2060, OnSecretTrigger);
			RegisterRefresh(2061, OnSecretResolve);
			RegisterRefresh(1090, OnChat);
			RegisterRefresh(2190, OnServerMsg);
			RegisterRefresh(2100, OnRefreshAll);
			TcgNetwork tcgNetwork = TcgNetwork.Get();
			tcgNetwork.onConnect = (UnityAction)Delegate.Combine(tcgNetwork.onConnect, new UnityAction(OnConnectedServer));
			TcgNetwork.Get().Messaging.ListenMsg("refresh", OnReceiveRefresh);
			ConnectToAPI();
			ConnectToServer();
		}

		protected virtual void OnDestroy()
		{
			TcgNetwork tcgNetwork = TcgNetwork.Get();
			tcgNetwork.onConnect = (UnityAction)Delegate.Remove(tcgNetwork.onConnect, new UnityAction(OnConnectedServer));
			TcgNetwork.Get().Messaging.UnListenMsg("refresh");
		}

		protected virtual void Update()
		{
			bool num = game_data == null || game_data.state == GameState.Connecting;
			bool flag = !game_settings.IsHost();
			bool flag2 = TcgNetwork.Get().IsConnecting();
			bool flag3 = TcgNetwork.Get().IsConnected();
			if (num && flag)
			{
				timer += Time.deltaTime;
				if (timer > 10f)
				{
					SceneNav.GoTo("Menu");
				}
			}
			if (!num && !flag2 && flag && !flag3)
			{
				timer += Time.deltaTime;
				if (timer > 5f)
				{
					timer = 0f;
					ConnectToServer();
				}
			}
		}

		public virtual void ConnectToAPI()
		{
			if (!Authenticator.Get().IsSignedIn())
			{
				Authenticator.Get().LoginTest("Player");
				if (!player_settings.HasDeck())
				{
					player_settings.deck = new UserDeckData(GameplayData.Get().test_deck);
				}
				if (!ai_settings.HasDeck())
				{
					ai_settings.deck = new UserDeckData(GameplayData.Get().test_deck_ai);
					ai_settings.ai_level = GameplayData.Get().ai_level;
				}
			}
			UserData userData = Authenticator.Get().UserData;
			if (userData != null)
			{
				player_settings.avatar = userData.GetAvatar();
				player_settings.cardback = userData.GetCardback();
			}
		}

		public virtual async void ConnectToServer()
		{
			await Task.Yield();
			if (!TcgNetwork.Get().IsActive())
			{
				if (game_settings.IsHost() && NetworkData.Get().solo_type == SoloType.Offline)
				{
					TcgNetwork.Get().StartHostOffline();
				}
				else if (game_settings.IsHost())
				{
					TcgNetwork.Get().StartHost(NetworkData.Get().port);
				}
				else
				{
					TcgNetwork.Get().StartClient(game_settings.GetUrl(), NetworkData.Get().port);
				}
			}
		}

		public virtual async void ConnectToGame(string uid)
		{
			await Task.Yield();
			if (TcgNetwork.Get().IsActive())
			{
				Debug.Log("Connect to Game: " + uid);
				MsgPlayerConnect msgPlayerConnect = new MsgPlayerConnect();
				msgPlayerConnect.user_id = Authenticator.Get().UserID;
				msgPlayerConnect.username = Authenticator.Get().Username;
				msgPlayerConnect.game_uid = uid;
				msgPlayerConnect.nb_players = game_settings.nb_players;
				msgPlayerConnect.observer = game_settings.game_type == GameType.Observer;
				Messaging.SendObject("connect", ServerID, msgPlayerConnect, NetworkDelivery.Reliable);
			}
		}

		public virtual void SendGameSettings()
		{
			if (game_settings.IsOffline())
			{
				SendGameplaySettings(game_settings);
				SendPlayerSettingsAI(ai_settings);
				SendPlayerSettings(player_settings);
			}
			else
			{
				SendGameplaySettings(game_settings);
				SendPlayerSettings(player_settings);
			}
		}

		public virtual void Disconnect()
		{
			TcgNetwork.Get().Disconnect();
		}

		private void RegisterRefresh(ushort tag, UnityAction<SerializedData> callback)
		{
			RefreshEvent refreshEvent = new RefreshEvent();
			refreshEvent.tag = tag;
			refreshEvent.callback = callback;
			registered_commands.Add(tag, refreshEvent);
		}

		public void OnReceiveRefresh(ulong client_id, FastBufferReader reader)
		{
			reader.ReadValueSafe(out ushort value, default(FastBufferWriter.ForPrimitives));
			if (registered_commands.TryGetValue(value, out var value2))
			{
				value2.callback(new SerializedData(reader));
			}
		}

		public void SendPlayerSettings(PlayerSettings psettings)
		{
			SendAction(1100, psettings, NetworkDelivery.ReliableFragmentedSequenced);
		}

		public void SendPlayerSettingsAI(PlayerSettings psettings)
		{
			SendAction(1102, psettings, NetworkDelivery.ReliableFragmentedSequenced);
		}

		public void SendGameplaySettings(GameSettings settings)
		{
			SendAction(1105, settings, NetworkDelivery.ReliableFragmentedSequenced);
		}

		public void PlayCard(Card card, Slot slot)
		{
			MsgPlayCard msgPlayCard = new MsgPlayCard();
			msgPlayCard.card_uid = card.uid;
			msgPlayCard.slot = slot;
			SendAction(1000, msgPlayCard);
		}

		public void AttackTarget(Card card, Card target)
		{
			MsgAttack msgAttack = new MsgAttack();
			msgAttack.attacker_uid = card.uid;
			msgAttack.target_uid = target.uid;
			SendAction(1010, msgAttack);
		}

		public void AttackPlayer(Card card, Player target)
		{
			MsgAttackPlayer msgAttackPlayer = new MsgAttackPlayer();
			msgAttackPlayer.attacker_uid = card.uid;
			msgAttackPlayer.target_id = target.player_id;
			SendAction(1012, msgAttackPlayer);
		}

		public void Move(Card card, Slot slot)
		{
			MsgPlayCard msgPlayCard = new MsgPlayCard();
			msgPlayCard.card_uid = card.uid;
			msgPlayCard.slot = slot;
			SendAction(1015, msgPlayCard);
		}

		public void CastAbility(Card card, AbilityData ability)
		{
			MsgCastAbility msgCastAbility = new MsgCastAbility();
			msgCastAbility.caster_uid = card.uid;
			msgCastAbility.ability_id = ability.id;
			msgCastAbility.target_uid = "";
			SendAction(1020, msgCastAbility);
		}

		public void SelectCard(Card card)
		{
			MsgCard msgCard = new MsgCard();
			msgCard.card_uid = card.uid;
			SendAction(1030, msgCard);
		}

		public void SelectPlayer(Player player)
		{
			MsgPlayer msgPlayer = new MsgPlayer();
			msgPlayer.player_id = player.player_id;
			SendAction(1032, msgPlayer);
		}

		public void SelectSlot(Slot slot)
		{
			SendAction(1034, slot);
		}

		public void SelectChoice(int c)
		{
			MsgInt msgInt = new MsgInt();
			msgInt.value = c;
			SendAction(1036, msgInt);
		}

		public void CancelSelection()
		{
			SendAction(1039);
		}

		public void SendChatMsg(string msg)
		{
			MsgChat msgChat = new MsgChat();
			msgChat.msg = msg;
			msgChat.player_id = player_id;
			SendAction(1090, msgChat);
		}

		public void EndTurn()
		{
			SendAction(1040);
		}

		public void Resign()
		{
			SendAction(1050);
		}

		public void SetObserverMode(int player_id)
		{
			observe_mode = true;
			observe_player_id = player_id;
		}

		public void SetObserverMode(string username)
		{
			observe_player_id = 0;
			Player[] players = GetGameData().players;
			foreach (Player player in players)
			{
				if (player.username == username)
				{
					observe_player_id = player.player_id;
				}
			}
		}

		public void SendAction<T>(ushort type, T data, NetworkDelivery delivery = NetworkDelivery.Reliable) where T : INetworkSerializable
		{
			FastBufferWriter writer = new FastBufferWriter(128, Allocator.Temp, TcgNetwork.MsgSizeMax);
			writer.WriteValueSafe(in type, default(FastBufferWriter.ForPrimitives));
			writer.WriteNetworkSerializable(in data);
			Messaging.Send("action", ServerID, writer, delivery);
			writer.Dispose();
		}

		public void SendAction(ushort type, int data)
		{
			FastBufferWriter writer = new FastBufferWriter(128, Allocator.Temp, TcgNetwork.MsgSizeMax);
			writer.WriteValueSafe(in type, default(FastBufferWriter.ForPrimitives));
			writer.WriteValueSafe(in data, default(FastBufferWriter.ForPrimitives));
			Messaging.Send("action", ServerID, writer, NetworkDelivery.Reliable);
			writer.Dispose();
		}

		public void SendAction(ushort type)
		{
			FastBufferWriter writer = new FastBufferWriter(128, Allocator.Temp, TcgNetwork.MsgSizeMax);
			writer.WriteValueSafe(in type, default(FastBufferWriter.ForPrimitives));
			Messaging.Send("action", ServerID, writer, NetworkDelivery.Reliable);
			writer.Dispose();
		}

		protected virtual void OnConnectedServer()
		{
			ConnectToGame(game_settings.game_uid);
			onConnectServer?.Invoke();
		}

		protected virtual void OnConnectedToGame(SerializedData sdata)
		{
			MsgAfterConnected msgAfterConnected = sdata.Get<MsgAfterConnected>();
			player_id = msgAfterConnected.player_id;
			game_data = msgAfterConnected.game_data;
			observe_mode = player_id < 0;
			if (observe_mode)
			{
				SetObserverMode(observe_user);
			}
			if (onConnectGame != null)
			{
				onConnectGame();
			}
			SendGameSettings();
		}

		protected virtual void OnPlayerReady(SerializedData sdata)
		{
			int value = sdata.Get<MsgInt>().value;
			if (onPlayerReady != null)
			{
				onPlayerReady(value);
			}
		}

		private void OnGameStart(SerializedData sdata)
		{
			onGameStart?.Invoke();
		}

		private void OnGameEnd(SerializedData sdata)
		{
			MsgPlayer msgPlayer = sdata.Get<MsgPlayer>();
			onGameEnd?.Invoke(msgPlayer.player_id);
		}

		private void OnNewTurn(SerializedData sdata)
		{
			MsgPlayer msgPlayer = sdata.Get<MsgPlayer>();
			onNewTurn?.Invoke(msgPlayer.player_id);
		}

		private void OnCardPlayed(SerializedData sdata)
		{
			MsgPlayCard msgPlayCard = sdata.Get<MsgPlayCard>();
			Card card = game_data.GetCard(msgPlayCard.card_uid);
			onCardPlayed?.Invoke(card, msgPlayCard.slot);
		}

		private void OnCardSummoned(SerializedData sdata)
		{
			MsgPlayCard msgPlayCard = sdata.Get<MsgPlayCard>();
			onCardSummoned?.Invoke(msgPlayCard.slot);
		}

		private void OnCardMoved(SerializedData sdata)
		{
			MsgPlayCard msgPlayCard = sdata.Get<MsgPlayCard>();
			Card card = game_data.GetCard(msgPlayCard.card_uid);
			onCardMoved?.Invoke(card, msgPlayCard.slot);
		}

		private void OnCardTransformed(SerializedData sdata)
		{
			MsgCard msgCard = sdata.Get<MsgCard>();
			Card card = game_data.GetCard(msgCard.card_uid);
			onCardTransformed?.Invoke(card);
		}

		private void OnCardDiscarded(SerializedData sdata)
		{
			MsgCard msgCard = sdata.Get<MsgCard>();
			Card card = game_data.GetCard(msgCard.card_uid);
			onCardDiscarded?.Invoke(card);
		}

		private void OnCardDraw(SerializedData sdata)
		{
			MsgInt msgInt = sdata.Get<MsgInt>();
			onCardDraw?.Invoke(msgInt.value);
		}

		private void OnValueRolled(SerializedData sdata)
		{
			MsgInt msgInt = sdata.Get<MsgInt>();
			onValueRolled?.Invoke(msgInt.value);
		}

		private void OnAttackStart(SerializedData sdata)
		{
			MsgAttack msgAttack = sdata.Get<MsgAttack>();
			Card card = game_data.GetCard(msgAttack.attacker_uid);
			Card card2 = game_data.GetCard(msgAttack.target_uid);
			onAttackStart?.Invoke(card, card2);
		}

		private void OnAttackEnd(SerializedData sdata)
		{
			MsgAttack msgAttack = sdata.Get<MsgAttack>();
			Card card = game_data.GetCard(msgAttack.attacker_uid);
			Card card2 = game_data.GetCard(msgAttack.target_uid);
			onAttackEnd?.Invoke(card, card2);
		}

		private void OnAttackPlayerStart(SerializedData sdata)
		{
			MsgAttackPlayer msgAttackPlayer = sdata.Get<MsgAttackPlayer>();
			Card card = game_data.GetCard(msgAttackPlayer.attacker_uid);
			Player player = game_data.GetPlayer(msgAttackPlayer.target_id);
			onAttackPlayerStart?.Invoke(card, player);
		}

		private void OnAttackPlayerEnd(SerializedData sdata)
		{
			MsgAttackPlayer msgAttackPlayer = sdata.Get<MsgAttackPlayer>();
			Card card = game_data.GetCard(msgAttackPlayer.attacker_uid);
			Player player = game_data.GetPlayer(msgAttackPlayer.target_id);
			onAttackPlayerEnd?.Invoke(card, player);
		}

		private void OnAbilityTrigger(SerializedData sdata)
		{
			MsgCastAbility msgCastAbility = sdata.Get<MsgCastAbility>();
			AbilityData arg = AbilityData.Get(msgCastAbility.ability_id);
			Card card = game_data.GetCard(msgCastAbility.caster_uid);
			onAbilityStart?.Invoke(arg, card);
		}

		private void OnAbilityTargetCard(SerializedData sdata)
		{
			MsgCastAbility msgCastAbility = sdata.Get<MsgCastAbility>();
			AbilityData arg = AbilityData.Get(msgCastAbility.ability_id);
			Card card = game_data.GetCard(msgCastAbility.caster_uid);
			Card card2 = game_data.GetCard(msgCastAbility.target_uid);
			onAbilityTargetCard?.Invoke(arg, card, card2);
		}

		private void OnAbilityTargetPlayer(SerializedData sdata)
		{
			MsgCastAbilityPlayer msgCastAbilityPlayer = sdata.Get<MsgCastAbilityPlayer>();
			AbilityData arg = AbilityData.Get(msgCastAbilityPlayer.ability_id);
			Card card = game_data.GetCard(msgCastAbilityPlayer.caster_uid);
			Player player = game_data.GetPlayer(msgCastAbilityPlayer.target_id);
			onAbilityTargetPlayer?.Invoke(arg, card, player);
		}

		private void OnAbilityTargetSlot(SerializedData sdata)
		{
			MsgCastAbilitySlot msgCastAbilitySlot = sdata.Get<MsgCastAbilitySlot>();
			AbilityData arg = AbilityData.Get(msgCastAbilitySlot.ability_id);
			Card card = game_data.GetCard(msgCastAbilitySlot.caster_uid);
			onAbilityTargetSlot?.Invoke(arg, card, msgCastAbilitySlot.slot);
		}

		private void OnAbilityAfter(SerializedData sdata)
		{
			MsgCastAbility msgCastAbility = sdata.Get<MsgCastAbility>();
			AbilityData arg = AbilityData.Get(msgCastAbility.ability_id);
			Card card = game_data.GetCard(msgCastAbility.caster_uid);
			onAbilityEnd?.Invoke(arg, card);
		}

		private void OnSecretTrigger(SerializedData sdata)
		{
			MsgSecret msgSecret = sdata.Get<MsgSecret>();
			Card card = game_data.GetCard(msgSecret.secret_uid);
			Card card2 = game_data.GetCard(msgSecret.triggerer_uid);
			onSecretTrigger?.Invoke(card, card2);
		}

		private void OnSecretResolve(SerializedData sdata)
		{
			MsgSecret msgSecret = sdata.Get<MsgSecret>();
			Card card = game_data.GetCard(msgSecret.secret_uid);
			Card card2 = game_data.GetCard(msgSecret.triggerer_uid);
			onSecretResolve?.Invoke(card, card2);
		}

		private void OnChat(SerializedData sdata)
		{
			MsgChat msgChat = sdata.Get<MsgChat>();
			onChatMsg?.Invoke(msgChat.player_id, msgChat.msg);
		}

		private void OnServerMsg(SerializedData sdata)
		{
			string arg = sdata.GetString();
			onServerMsg?.Invoke(arg);
		}

		private void OnRefreshAll(SerializedData sdata)
		{
			MsgRefreshAll msgRefreshAll = sdata.Get<MsgRefreshAll>();
			game_data = msgRefreshAll.game_data;
			onRefreshAll?.Invoke();
		}

		public virtual bool IsReady()
		{
			if (game_data != null)
			{
				return TcgNetwork.Get().IsConnected();
			}
			return false;
		}

		public Player GetPlayer()
		{
			return GetGameData().GetPlayer(GetPlayerID());
		}

		public Player GetOpponentPlayer()
		{
			return GetGameData().GetPlayer(GetOpponentPlayerID());
		}

		public int GetPlayerID()
		{
			if (observe_mode)
			{
				return observe_player_id;
			}
			return player_id;
		}

		public int GetOpponentPlayerID()
		{
			if (GetPlayerID() != 0)
			{
				return 0;
			}
			return 1;
		}

		public virtual bool IsYourTurn()
		{
			Game gameData = GetGameData();
			Player player = GetPlayer();
			if (IsReady())
			{
				return gameData.IsPlayerTurn(player);
			}
			return false;
		}

		public bool IsObserveMode()
		{
			return observe_mode;
		}

		public Game GetGameData()
		{
			return game_data;
		}

		public bool HasEnded()
		{
			if (game_data != null)
			{
				return game_data.HasEnded();
			}
			return false;
		}

		private void OnApplicationQuit()
		{
			Resign();
		}

		public static GameClient Get()
		{
			return instance;
		}
	}
}
