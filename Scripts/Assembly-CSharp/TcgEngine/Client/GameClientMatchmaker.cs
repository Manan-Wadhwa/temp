using System;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.Events;

namespace TcgEngine.Client
{
	public class GameClientMatchmaker : MonoBehaviour
	{
		public UnityAction<MatchmakingResult> onMatchmaking;

		public UnityAction<MatchmakingList> onMatchmakingList;

		public UnityAction<MatchList> onMatchList;

		private bool matchmaking;

		private float timer;

		private float match_timer;

		private string matchmaking_group;

		private int matchmaking_players;

		private UnityAction<bool> connect_callback;

		private static GameClientMatchmaker _instance;

		public ulong ServerID => TcgNetwork.Get().ServerID;

		public NetworkMessaging Messaging => TcgNetwork.Get().Messaging;

		private void Awake()
		{
			_instance = this;
		}

		private void Start()
		{
			TcgNetwork tcgNetwork = TcgNetwork.Get();
			tcgNetwork.onConnect = (UnityAction)Delegate.Combine(tcgNetwork.onConnect, new UnityAction(OnConnect));
			TcgNetwork tcgNetwork2 = TcgNetwork.Get();
			tcgNetwork2.onDisconnect = (UnityAction)Delegate.Combine(tcgNetwork2.onDisconnect, new UnityAction(OnDisconnect));
			Messaging.ListenMsg("matchmaking", ReceiveMatchmaking);
			Messaging.ListenMsg("matchmaking_list", ReceiveMatchmakingList);
			Messaging.ListenMsg("match_list", ReceiveMatchList);
		}

		private void OnDestroy()
		{
			Disconnect();
			if (TcgNetwork.Get() != null)
			{
				TcgNetwork tcgNetwork = TcgNetwork.Get();
				tcgNetwork.onConnect = (UnityAction)Delegate.Remove(tcgNetwork.onConnect, new UnityAction(OnConnect));
				TcgNetwork tcgNetwork2 = TcgNetwork.Get();
				tcgNetwork2.onDisconnect = (UnityAction)Delegate.Remove(tcgNetwork2.onDisconnect, new UnityAction(OnDisconnect));
				Messaging.UnListenMsg("matchmaking");
				Messaging.UnListenMsg("matchmaking_list");
				Messaging.UnListenMsg("match_list");
			}
		}

		private void Update()
		{
			if (matchmaking)
			{
				timer += Time.deltaTime;
				match_timer += Time.deltaTime;
				if (IsConnected() && timer > 2f)
				{
					timer = 0f;
					SendMatchRequest(refresh: true, matchmaking_group, matchmaking_players);
				}
				if (!IsConnected() && !IsConnecting() && timer > 5f)
				{
					StopMatchmaking();
				}
			}
		}

		public void StartMatchmaking(string group, int nb_players)
		{
			if (matchmaking)
			{
				StopMatchmaking();
			}
			Debug.Log("Start Matchmaking!");
			matchmaking_group = group;
			matchmaking_players = nb_players;
			matchmaking = true;
			match_timer = 0f;
			timer = 0f;
			Connect(NetworkData.Get().url, NetworkData.Get().port, delegate(bool success)
			{
				if (success)
				{
					SendMatchRequest(refresh: false, group, nb_players);
				}
				else
				{
					StopMatchmaking();
				}
			});
		}

		public void StopMatchmaking()
		{
			if (matchmaking)
			{
				Debug.Log("Stop Matchmaking!");
				onMatchmaking?.Invoke(null);
				matchmaking_group = "";
				matchmaking_players = 0;
				matchmaking = false;
			}
		}

		public void RefreshMatchmakingList()
		{
			Connect(NetworkData.Get().url, NetworkData.Get().port, delegate(bool success)
			{
				if (success)
				{
					SendMatchmakingListRequest();
				}
			});
		}

		public void RefreshMatchList(string username)
		{
			Connect(NetworkData.Get().url, NetworkData.Get().port, delegate(bool success)
			{
				if (success)
				{
					SendMatchListRequest(username);
				}
			});
		}

		public void Connect(string url, ushort port, UnityAction<bool> callback = null)
		{
			if (!Authenticator.Get().IsSignedIn())
			{
				callback?.Invoke(arg0: false);
				return;
			}
			if (IsConnected() || IsConnecting())
			{
				callback?.Invoke(IsConnected());
				return;
			}
			connect_callback = callback;
			TcgNetwork.Get().StartClient(url, port);
		}

		public void Disconnect()
		{
			TcgNetwork.Get()?.Disconnect();
		}

		private void OnConnect()
		{
			Debug.Log("Connected to server!");
			connect_callback?.Invoke(arg0: true);
			connect_callback = null;
		}

		private void OnDisconnect()
		{
			StopMatchmaking();
			connect_callback?.Invoke(arg0: false);
			connect_callback = null;
			matchmaking = false;
		}

		private void SendMatchRequest(bool refresh, string group, int nb_players)
		{
			MsgMatchmaking msgMatchmaking = new MsgMatchmaking();
			UserData userData = Authenticator.Get().GetUserData();
			msgMatchmaking.user_id = Authenticator.Get().GetUserId();
			msgMatchmaking.username = Authenticator.Get().GetUsername();
			msgMatchmaking.group = group;
			msgMatchmaking.players = nb_players;
			msgMatchmaking.elo = userData.elo;
			msgMatchmaking.time = match_timer;
			msgMatchmaking.refresh = refresh;
			Messaging.SendObject("matchmaking", ServerID, msgMatchmaking, NetworkDelivery.Reliable);
		}

		private void SendMatchmakingListRequest()
		{
			MsgMatchmakingList msgMatchmakingList = new MsgMatchmakingList();
			msgMatchmakingList.username = "";
			Messaging.SendObject("matchmaking_list", ServerID, msgMatchmakingList, NetworkDelivery.Reliable);
		}

		private void SendMatchListRequest(string username)
		{
			MsgMatchmakingList msgMatchmakingList = new MsgMatchmakingList();
			msgMatchmakingList.username = username;
			Messaging.SendObject("match_list", ServerID, msgMatchmakingList, NetworkDelivery.Reliable);
		}

		private void ReceiveMatchmaking(ulong client_id, FastBufferReader reader)
		{
			reader.ReadNetworkSerializable(out MatchmakingResult value);
			if (IsConnected() && matchmaking && matchmaking_group == value.group)
			{
				matchmaking = !value.success;
				onMatchmaking?.Invoke(value);
			}
		}

		private void ReceiveMatchmakingList(ulong client_id, FastBufferReader reader)
		{
			reader.ReadNetworkSerializable(out MatchmakingList value);
			onMatchmakingList?.Invoke(value);
		}

		private void ReceiveMatchList(ulong client_id, FastBufferReader reader)
		{
			reader.ReadNetworkSerializable(out MatchList value);
			onMatchList?.Invoke(value);
		}

		public bool IsMatchmaking()
		{
			return matchmaking;
		}

		public string GetGroup()
		{
			return matchmaking_group;
		}

		public int GetNbPlayers()
		{
			return matchmaking_players;
		}

		public float GetTimer()
		{
			return match_timer;
		}

		public bool IsConnected()
		{
			return TcgNetwork.Get().IsConnected();
		}

		public bool IsConnecting()
		{
			return TcgNetwork.Get().IsConnecting();
		}

		public static GameClientMatchmaker Get()
		{
			return _instance;
		}
	}
}
