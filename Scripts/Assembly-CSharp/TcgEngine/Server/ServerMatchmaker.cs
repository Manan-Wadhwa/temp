using System;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.Events;

namespace TcgEngine.Server
{
	public class ServerMatchmaker : MonoBehaviour
	{
		[Header("Matchmaker")]
		public string[] servers;

		private Dictionary<ulong, ClientData> client_list = new Dictionary<ulong, ClientData>();

		private Dictionary<string, MatchPlayerData> matchmaking_players = new Dictionary<string, MatchPlayerData>();

		private Dictionary<string, MatchData> matched_players = new Dictionary<string, MatchData>();

		private List<MatchPlayerData> valid_users = new List<MatchPlayerData>();

		private float matchmake_timer;

		private static ServerMatchmaker _instance;

		public ulong ServerID => TcgNetwork.Get().ServerID;

		public NetworkMessaging Messaging => TcgNetwork.Get().Messaging;

		protected virtual void Awake()
		{
			_instance = this;
			Application.runInBackground = true;
		}

		protected virtual void Start()
		{
			TcgNetwork tcgNetwork = TcgNetwork.Get();
			tcgNetwork.onClientJoin = (UnityAction<ulong>)Delegate.Combine(tcgNetwork.onClientJoin, new UnityAction<ulong>(OnClientConnected));
			tcgNetwork.onClientQuit = (UnityAction<ulong>)Delegate.Combine(tcgNetwork.onClientQuit, new UnityAction<ulong>(OnClientDisconnected));
			Messaging.ListenMsg("matchmaking", ReceiveMatchmaking);
			Messaging.ListenMsg("matchmaking_list", ReceiveMatchmakingList);
			Messaging.ListenMsg("match_list", ReceiveMatchList);
			if (!tcgNetwork.IsActive())
			{
				tcgNetwork.StartServer(NetworkData.Get().port);
			}
		}

		protected virtual void Update()
		{
			matchmake_timer += Time.deltaTime;
			if (matchmake_timer > 20f)
			{
				matchmake_timer = 0f;
				matchmaking_players.Clear();
			}
		}

		protected virtual void OnClientConnected(ulong client_id)
		{
			ClientData value = new ClientData(client_id);
			client_list[client_id] = value;
		}

		protected virtual void OnClientDisconnected(ulong client_id)
		{
			if (client_list.ContainsKey(client_id))
			{
				ClientData clientData = client_list[client_id];
				if (clientData.username != null)
				{
					matchmaking_players.Remove(clientData.user_id);
				}
				client_list.Remove(client_id);
			}
		}

		protected virtual void ReceiveMatchmaking(ulong client_id, FastBufferReader reader)
		{
			ClientData client = GetClient(client_id);
			reader.ReadNetworkSerializable(out MsgMatchmaking value);
			if (client == null || string.IsNullOrWhiteSpace(value.user_id) || string.IsNullOrWhiteSpace(value.username))
			{
				return;
			}
			string user_id = value.user_id;
			bool refresh = value.refresh;
			client.user_id = value.user_id;
			client.username = value.username;
			if (!refresh)
			{
				matched_players.Remove(user_id);
			}
			if (matched_players.ContainsKey(user_id))
			{
				MatchData matchData = matched_players[user_id];
				if (!matchData.ended)
				{
					SendMatchmakingResponse(client, matchData, value.group, matchData.players.Length);
					return;
				}
			}
			MatchPlayerData matchPlayerData = new MatchPlayerData();
			matchPlayerData.user_id = value.user_id;
			matchPlayerData.username = value.username;
			matchPlayerData.group = value.group;
			matchPlayerData.elo_rank = value.elo;
			matchPlayerData.nb_players = value.players;
			if (!matchmaking_players.ContainsKey(user_id))
			{
				matchmaking_players.Add(user_id, matchPlayerData);
			}
			float num = 20f;
			int num2 = 2000;
			bool flag = value.group.StartsWith("u_");
			int num3 = Mathf.RoundToInt(Mathf.Clamp01(value.time / num) * (float)num2);
			valid_users.Clear();
			valid_users.Add(matchPlayerData);
			foreach (KeyValuePair<string, MatchPlayerData> matchmaking_player in matchmaking_players)
			{
				string key = matchmaking_player.Key;
				MatchPlayerData value2 = matchmaking_player.Value;
				int num4 = Mathf.Abs(value2.elo_rank - value.elo);
				bool flag2 = value2.group == value.group;
				bool flag3 = value2.nb_players == value.players;
				bool flag4 = flag || num4 < num3;
				if (key != user_id && flag4 && flag2 && flag3)
				{
					valid_users.Add(value2);
				}
			}
			if (valid_users.Count < value.players)
			{
				SendMatchmakingResponse(client, null, value.group, valid_users.Count);
				return;
			}
			string uid = ((value.group.Length >= 2) ? value.group.Substring(0, 2) : "") + GameTool.GenerateRandomID(12);
			string url = "";
			if (servers.Length != 0)
			{
				url = servers[UnityEngine.Random.Range(0, servers.Length)];
			}
			int num5 = 0;
			MatchData matchData2 = new MatchData(value.group, uid, url, value.players);
			foreach (MatchPlayerData valid_user in valid_users)
			{
				if (num5 < matchData2.players.Length)
				{
					matchmaking_players.Remove(valid_user.user_id);
					matched_players[valid_user.user_id] = matchData2;
					matchData2.players[num5] = valid_user.username;
					num5++;
				}
			}
			if (matched_players.ContainsKey(user_id))
			{
				SendMatchmakingResponse(client, matchData2, matchData2.group, matchData2.players.Length);
			}
		}

		protected virtual void SendMatchmakingResponse(ClientData iclient, MatchData match, string group, int players)
		{
			MatchmakingResult matchmakingResult = new MatchmakingResult();
			matchmakingResult.success = match != null;
			matchmakingResult.players = players;
			matchmakingResult.group = group;
			matchmakingResult.game_uid = ((match != null) ? match.game_uid : "");
			matchmakingResult.server_url = ((match != null) ? match.server_url : "");
			Messaging.SendObject("matchmaking", iclient.client_id, matchmakingResult, NetworkDelivery.Reliable);
		}

		protected virtual void ReceiveMatchmakingList(ulong client_id, FastBufferReader reader)
		{
			reader.ReadNetworkSerializable(out MsgMatchmakingList value);
			List<MatchmakingListItem> list = new List<MatchmakingListItem>();
			foreach (KeyValuePair<string, MatchPlayerData> matchmaking_player in matchmaking_players)
			{
				if (string.IsNullOrEmpty(value.username) || matchmaking_player.Key == value.username)
				{
					MatchPlayerData value2 = matchmaking_player.Value;
					list.Add(new MatchmakingListItem
					{
						group = value2.group,
						user_id = value2.user_id,
						username = value2.username
					});
				}
			}
			MatchmakingList matchmakingList = new MatchmakingList();
			matchmakingList.items = list.ToArray();
			Messaging.SendObject("matchmaking_list", client_id, matchmakingList, NetworkDelivery.Reliable);
		}

		protected virtual void ReceiveMatchList(ulong client_id, FastBufferReader reader)
		{
			reader.ReadNetworkSerializable(out MsgMatchmakingList value);
			List<MatchListItem> list = new List<MatchListItem>();
			foreach (KeyValuePair<string, MatchData> matched_player in matched_players)
			{
				if (!matched_player.Value.ended && (string.IsNullOrEmpty(value.username) || Contains(matched_player.Value.players, value.username)))
				{
					MatchData value2 = matched_player.Value;
					MatchListItem matchListItem = new MatchListItem();
					matchListItem.group = matched_player.Value.group;
					matchListItem.username = value.username;
					matchListItem.game_uid = value2.game_uid;
					matchListItem.game_url = value2.server_url;
					list.Add(matchListItem);
				}
			}
			MatchList matchList = new MatchList();
			matchList.items = list.ToArray();
			Messaging.SendObject("match_list", client_id, matchList, NetworkDelivery.Reliable);
		}

		private bool Contains(string[] users, string user)
		{
			for (int i = 0; i < users.Length; i++)
			{
				if (users[i] == user)
				{
					return true;
				}
			}
			return false;
		}

		public void EndMatch(string uid)
		{
			foreach (KeyValuePair<string, MatchData> matched_player in matched_players)
			{
				if (matched_player.Value.game_uid == uid)
				{
					matched_player.Value.ended = true;
				}
			}
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

		public static ServerMatchmaker Get()
		{
			return _instance;
		}
	}
}
