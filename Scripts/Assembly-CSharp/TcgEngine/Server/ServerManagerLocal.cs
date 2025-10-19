using System;
using System.Collections.Generic;
using TcgEngine.Client;
using Unity.Collections;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.Events;

namespace TcgEngine.Server
{
	public class ServerManagerLocal : MonoBehaviour
	{
		private GameServer server;

		private Dictionary<ulong, ClientData> client_list = new Dictionary<ulong, ClientData>();

		public ulong ServerID => TcgNetwork.Get().ServerID;

		public NetworkMessaging Messaging => TcgNetwork.Get().Messaging;

		protected virtual void Start()
		{
			if (GameClient.game_settings.IsHost())
			{
				StartServer();
			}
		}

		protected virtual void StartServer()
		{
			TcgNetwork tcgNetwork = TcgNetwork.Get();
			tcgNetwork.onClientJoin = (UnityAction<ulong>)Delegate.Combine(tcgNetwork.onClientJoin, new UnityAction<ulong>(OnClientJoin));
			tcgNetwork.onClientQuit = (UnityAction<ulong>)Delegate.Combine(tcgNetwork.onClientQuit, new UnityAction<ulong>(OnClientQuit));
			tcgNetwork.Messaging.ListenMsg("connect", ReceiveConnectPlayer);
			tcgNetwork.Messaging.ListenMsg("action", ReceiveGameAction);
			client_list[tcgNetwork.ServerID] = new ClientData(tcgNetwork.ServerID);
			server = new GameServer(GameClient.game_settings.game_uid, GameClient.game_settings.nb_players, online: false);
		}

		protected virtual void OnDestroy()
		{
			TcgNetwork tcgNetwork = TcgNetwork.Get();
			if (tcgNetwork != null)
			{
				tcgNetwork.onClientJoin = (UnityAction<ulong>)Delegate.Remove(tcgNetwork.onClientJoin, new UnityAction<ulong>(OnClientJoin));
				tcgNetwork.onClientQuit = (UnityAction<ulong>)Delegate.Remove(tcgNetwork.onClientQuit, new UnityAction<ulong>(OnClientQuit));
				tcgNetwork.Messaging.UnListenMsg("connect");
				tcgNetwork.Messaging.UnListenMsg("action");
			}
		}

		protected virtual void OnClientJoin(ulong client_id)
		{
			client_list[client_id] = new ClientData(client_id);
		}

		protected virtual void OnClientQuit(ulong client_id)
		{
			ClientData client = GetClient(client_id);
			server?.RemoveClient(client);
			client_list.Remove(client_id);
		}

		protected virtual void Update()
		{
			if (server != null)
			{
				server.Update();
			}
		}

		protected virtual void ReceiveConnectPlayer(ulong client_id, FastBufferReader reader)
		{
			reader.ReadNetworkSerializable(out MsgPlayerConnect value);
			if (value != null && !string.IsNullOrWhiteSpace(value.username) && !string.IsNullOrWhiteSpace(value.game_uid))
			{
				ClientData client = GetClient(client_id);
				if (client != null && (server.IsPlayer(value.user_id) || server.CountPlayers() < server.nb_players))
				{
					client.game_uid = value.game_uid;
					client.user_id = value.user_id;
					client.username = value.username;
					server.AddClient(client);
					int player_id = server.AddPlayer(client);
					MsgAfterConnected msgAfterConnected = new MsgAfterConnected();
					msgAfterConnected.success = true;
					msgAfterConnected.player_id = player_id;
					msgAfterConnected.game_data = server.GetGameData();
					SendToClient(client_id, 2000, msgAfterConnected, NetworkDelivery.ReliableFragmentedSequenced);
				}
			}
		}

		protected virtual void ReceiveGameAction(ulong client_id, FastBufferReader reader)
		{
			ClientData client = GetClient(client_id);
			if (client != null && server.IsConnectedPlayer(client.user_id))
			{
				server.ReceiveAction(client_id, reader);
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

		public ClientData GetClient(ulong client_id)
		{
			if (client_list.ContainsKey(client_id))
			{
				return client_list[client_id];
			}
			return null;
		}
	}
}
