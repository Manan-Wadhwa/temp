using System;
using System.Collections.Generic;
using Unity.Collections;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.Events;

namespace TcgEngine.Server
{
	public class ServerManager : MonoBehaviour
	{
		[Header("API")]
		public string api_username;

		public string api_password;

		private Dictionary<ulong, ClientData> client_list = new Dictionary<ulong, ClientData>();

		private Dictionary<string, GameServer> game_list = new Dictionary<string, GameServer>();

		private List<string> game_remove_list = new List<string>();

		private float login_timer;

		public ulong ServerID => TcgNetwork.Get().ServerID;

		public NetworkMessaging Messaging => TcgNetwork.Get().Messaging;

		protected virtual void Awake()
		{
			Application.runInBackground = true;
			Application.targetFrameRate = 200;
		}

		protected virtual void Start()
		{
			TcgNetwork tcgNetwork = TcgNetwork.Get();
			tcgNetwork.onClientJoin = (UnityAction<ulong>)Delegate.Combine(tcgNetwork.onClientJoin, new UnityAction<ulong>(OnClientConnected));
			tcgNetwork.onClientQuit = (UnityAction<ulong>)Delegate.Combine(tcgNetwork.onClientQuit, new UnityAction<ulong>(OnClientDisconnected));
			Messaging.ListenMsg("connect", ReceiveConnectPlayer);
			Messaging.ListenMsg("action", ReceiveGameAction);
			if (!tcgNetwork.IsActive())
			{
				tcgNetwork.StartServer(NetworkData.Get().port);
			}
			Login();
		}

		protected virtual void Update()
		{
			foreach (KeyValuePair<string, GameServer> item in game_list)
			{
				GameServer value = item.Value;
				value.Update();
				if (value.IsGameExpired())
				{
					game_remove_list.Add(item.Key);
				}
			}
			foreach (string item2 in game_remove_list)
			{
				game_list.Remove(item2);
				if ((bool)ServerMatchmaker.Get())
				{
					ServerMatchmaker.Get().EndMatch(item2);
				}
			}
			game_remove_list.Clear();
			login_timer += Time.deltaTime;
			if (login_timer > 15f && !Authenticator.Get().IsConnected())
			{
				login_timer = 0f;
				Login();
			}
		}

		protected virtual async void Login()
		{
			await Authenticator.Get().Login(api_username, api_password);
			bool flag = Authenticator.Get().IsConnected();
			int permission = Authenticator.Get().GetPermission();
			string text = (Authenticator.Get().IsApi() ? "API" : "Local");
			Debug.Log(text + " authentication: " + flag + " (" + permission + ")");
			if (flag)
			{
				return;
			}
			TimeTool.WaitFor(5f, delegate
			{
				if (!Authenticator.Get().IsConnected())
				{
					Login();
				}
			});
		}

		protected virtual void OnClientConnected(ulong client_id)
		{
			ClientData value = new ClientData(client_id);
			client_list[client_id] = value;
		}

		protected virtual void OnClientDisconnected(ulong client_id)
		{
			ClientData client = GetClient(client_id);
			client_list.Remove(client_id);
			ReceiveDisconnectPlayer(client);
		}

		protected virtual void ReceiveConnectPlayer(ulong client_id, FastBufferReader reader)
		{
			ClientData client = GetClient(client_id);
			reader.ReadNetworkSerializable(out MsgPlayerConnect value);
			if (client != null && value != null && !string.IsNullOrWhiteSpace(value.username) && !string.IsNullOrWhiteSpace(value.game_uid))
			{
				Debug.Log("Client " + client_id + " connecting to game: " + value.game_uid);
				if (value.observer)
				{
					ConnectObserverToGame(client, value.user_id, value.username, value.game_uid);
				}
				else
				{
					ConnectPlayerToGame(client, value.user_id, value.username, value.game_uid, value.nb_players);
				}
				GetGame(value.game_uid)?.RefreshAll();
			}
		}

		protected virtual void ReceiveDisconnectPlayer(ClientData iclient)
		{
			if (iclient != null)
			{
				GetGame(iclient.game_uid)?.RemoveClient(iclient);
			}
		}

		protected virtual void ReceiveGameAction(ulong client_id, FastBufferReader reader)
		{
			ClientData client = GetClient(client_id);
			if (client != null)
			{
				GameServer game = GetGame(client.game_uid);
				if (game != null && game.IsConnectedPlayer(client.user_id))
				{
					game.ReceiveAction(client_id, reader);
				}
			}
		}

		protected virtual void ConnectPlayerToGame(ClientData client, string user_id, string username, string game_uid, int nb_players)
		{
			GameServer gameServer = GetGame(game_uid);
			if (gameServer == null)
			{
				gameServer = CreateGame(game_uid, nb_players);
			}
			bool flag = gameServer.IsPlayer(user_id) || gameServer.CountPlayers() < gameServer.nb_players;
			if (gameServer != null && flag)
			{
				client.game_uid = game_uid;
				client.user_id = user_id;
				client.username = username;
				gameServer.AddClient(client);
				int player_id = gameServer.AddPlayer(client);
				MsgAfterConnected msgAfterConnected = new MsgAfterConnected();
				msgAfterConnected.success = true;
				msgAfterConnected.player_id = player_id;
				msgAfterConnected.game_data = gameServer.GetGameData();
				SendToClient(client.client_id, 2000, msgAfterConnected, NetworkDelivery.ReliableFragmentedSequenced);
			}
		}

		protected virtual void ConnectObserverToGame(ClientData client, string user_id, string username, string game_uid)
		{
			GameServer game = GetGame(game_uid);
			if (game != null && client != null)
			{
				client.game_uid = game_uid;
				client.user_id = user_id;
				client.username = username;
				game.AddClient(client);
				MsgAfterConnected msgAfterConnected = new MsgAfterConnected();
				msgAfterConnected.success = true;
				msgAfterConnected.player_id = -1;
				msgAfterConnected.game_data = game.GetGameData();
				SendToClient(client.client_id, 2000, msgAfterConnected, NetworkDelivery.ReliableFragmentedSequenced);
			}
		}

		public void SendToClient(ulong client_id, ushort tag, INetworkSerializable data, NetworkDelivery delivery)
		{
			FastBufferWriter writer = new FastBufferWriter(128, Allocator.Temp, TcgNetwork.MsgSizeMax);
			writer.WriteValueSafe(in tag, default(FastBufferWriter.ForPrimitives));
			writer.WriteNetworkSerializable(in data);
			Messaging.Send("refresh", client_id, writer, delivery);
			writer.Dispose();
		}

		public void SendMsgToClient(ushort client_id, string msg)
		{
			FastBufferWriter writer = new FastBufferWriter(128, Allocator.Temp, TcgNetwork.MsgSizeMax);
			writer.WriteValueSafe<ushort>((ushort)2190, default(FastBufferWriter.ForPrimitives));
			writer.WriteValueSafe(msg);
			Messaging.Send("refresh", client_id, writer, NetworkDelivery.Reliable);
			writer.Dispose();
		}

		public GameServer CreateGame(string uid, int nb_players)
		{
			GameServer gameServer = new GameServer(uid, nb_players, online: true);
			game_list[gameServer.game_uid] = gameServer;
			return gameServer;
		}

		public void RemoveGame(string game_id)
		{
			game_list.Remove(game_id);
		}

		public GameServer GetGame(string game_uid)
		{
			if (string.IsNullOrEmpty(game_uid))
			{
				return null;
			}
			if (game_list.ContainsKey(game_uid))
			{
				return game_list[game_uid];
			}
			return null;
		}

		public ClientData GetClient(ulong client_id)
		{
			if (client_list.ContainsKey(client_id))
			{
				return client_list[client_id];
			}
			return null;
		}

		public ClientData GetClientByUser(string username)
		{
			foreach (KeyValuePair<ulong, ClientData> item in client_list)
			{
				if (item.Value.username == username)
				{
					return item.Value;
				}
			}
			return null;
		}
	}
}
