using System;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.Events;

namespace TcgEngine
{
	[DefaultExecutionOrder(-10)]
	[RequireComponent(typeof(NetworkManager))]
	[RequireComponent(typeof(TcgTransport))]
	public class TcgNetwork : MonoBehaviour
	{
		public delegate bool ApprovalEvent(ulong client_id, ConnectionData connect_data);

		public NetworkData data;

		public UnityAction onTick;

		public UnityAction onConnect;

		public UnityAction onDisconnect;

		public UnityAction<ulong> onClientJoin;

		public UnityAction<ulong> onClientQuit;

		public UnityAction<ulong> onClientReady;

		public ApprovalEvent checkApproval;

		private NetworkManager network;

		private TcgTransport transport;

		private NetworkMessaging messaging;

		private Authenticator auth;

		private ConnectionData connection;

		[NonSerialized]
		private static bool inited;

		private static TcgNetwork instance;

		private const int msg_size = 1048576;

		private bool offline_mode;

		private bool connected;

		public string Address => transport.GetAddress();

		public ushort Port => transport.GetPort();

		public ulong ClientID
		{
			get
			{
				if (!offline_mode)
				{
					return network.LocalClientId;
				}
				return ServerID;
			}
		}

		public ulong ServerID => 0uL;

		public bool IsServer
		{
			get
			{
				if (!offline_mode)
				{
					return network.IsServer;
				}
				return true;
			}
		}

		public bool IsClient
		{
			get
			{
				if (!offline_mode)
				{
					return network.IsClient;
				}
				return true;
			}
		}

		public bool IsHost
		{
			get
			{
				if (IsClient)
				{
					return IsServer;
				}
				return false;
			}
		}

		public bool IsOnline
		{
			get
			{
				if (!offline_mode)
				{
					return IsActive();
				}
				return false;
			}
		}

		public NetworkTime LocalTime => network.LocalTime;

		public NetworkTime ServerTime => network.ServerTime;

		public float DeltaTick => 1f / (float)network.NetworkTickSystem.TickRate;

		public NetworkManager NetworkManager => network;

		public TcgTransport Transport => transport;

		public NetworkMessaging Messaging => messaging;

		public Authenticator Auth => auth;

		public static int MsgSizeMax => 1048576;

		public static int MsgSize => MsgSizeMax;

		private void Awake()
		{
			if (instance != null && instance != this)
			{
				UnityEngine.Object.Destroy(base.gameObject);
				return;
			}
			Init();
			UnityEngine.Object.DontDestroyOnLoad(base.gameObject);
		}

		public void Init()
		{
			if (!inited || transport == null)
			{
				instance = this;
				inited = true;
				network = GetComponent<NetworkManager>();
				transport = GetComponent<TcgTransport>();
				messaging = new NetworkMessaging(this);
				connection = new ConnectionData();
				transport.Init();
				NetworkManager networkManager = network;
				networkManager.ConnectionApprovalCallback = (Action<NetworkManager.ConnectionApprovalRequest, NetworkManager.ConnectionApprovalResponse>)Delegate.Combine(networkManager.ConnectionApprovalCallback, new Action<NetworkManager.ConnectionApprovalRequest, NetworkManager.ConnectionApprovalResponse>(ApprovalCheck));
				network.OnClientConnectedCallback += OnClientConnect;
				network.OnClientDisconnectCallback += OnClientDisconnect;
				InitAuth();
			}
		}

		private void Update()
		{
		}

		public void StartHost(ushort port)
		{
			Debug.Log("Host Server Port " + port);
			transport.SetServer(port);
			connection.user_id = auth.UserID;
			connection.username = auth.Username;
			network.NetworkConfig.ConnectionData = NetworkTool.NetSerialize(connection);
			offline_mode = false;
			network.StartHost();
			AfterConnected();
		}

		public void StartServer(ushort port)
		{
			Debug.Log("Start Server Port " + port);
			transport.SetServer(port);
			connection.user_id = "";
			connection.username = "";
			network.NetworkConfig.ConnectionData = NetworkTool.NetSerialize(connection);
			offline_mode = false;
			network.StartServer();
			AfterConnected();
		}

		public void StartClient(string server_url, ushort port)
		{
			Debug.Log("Join Server: " + server_url + " " + port);
			transport.SetClient(server_url, port);
			connection.user_id = auth.UserID;
			connection.username = auth.Username;
			network.NetworkConfig.ConnectionData = NetworkTool.NetSerialize(connection);
			offline_mode = false;
			network.StartClient();
		}

		public void StartHostOffline()
		{
			Debug.Log("Host Offline");
			Disconnect();
			offline_mode = true;
			AfterConnected();
		}

		public void Disconnect()
		{
			if (IsClient || IsServer)
			{
				Debug.Log("Disconnect");
				network.Shutdown();
				AfterDisconnected();
			}
		}

		public void SetConnectionExtraData(byte[] bytes)
		{
			connection.extra = bytes;
		}

		public void SetConnectionExtraData(string data)
		{
			connection.extra = NetworkTool.SerializeString(data);
		}

		public void SetConnectionExtraData<T>(T data) where T : INetworkSerializable, new()
		{
			connection.extra = NetworkTool.NetSerialize(data);
		}

		private async void InitAuth()
		{
			auth = Authenticator.Create(data.auth_type);
			await auth.Initialize();
		}

		private void AfterConnected()
		{
			if (!connected)
			{
				if (network.NetworkTickSystem != null)
				{
					network.NetworkTickSystem.Tick += OnTick;
				}
				connected = true;
				onConnect?.Invoke();
			}
		}

		private void AfterDisconnected()
		{
			if (connected)
			{
				if (network.NetworkTickSystem != null)
				{
					network.NetworkTickSystem.Tick -= OnTick;
				}
				offline_mode = false;
				connected = false;
				onDisconnect?.Invoke();
			}
		}

		private void OnClientConnect(ulong client_id)
		{
			if (IsServer && client_id != ServerID)
			{
				Debug.Log("Client Connected: " + client_id);
				onClientJoin?.Invoke(client_id);
			}
			if (!IsServer)
			{
				AfterConnected();
			}
		}

		private void OnClientDisconnect(ulong client_id)
		{
			if (IsServer && client_id != ServerID)
			{
				Debug.Log("Client Disconnected: " + client_id);
				onClientQuit?.Invoke(client_id);
			}
			if (ClientID == client_id || client_id == ServerID)
			{
				AfterDisconnected();
			}
		}

		private void OnTick()
		{
			onTick?.Invoke();
		}

		private void ApprovalCheck(NetworkManager.ConnectionApprovalRequest req, NetworkManager.ConnectionApprovalResponse res)
		{
			ConnectionData connect = NetworkTool.NetDeserialize<ConnectionData>(req.Payload);
			bool approved = ApproveClient(req.ClientNetworkId, connect);
			res.Approved = approved;
		}

		private bool ApproveClient(ulong client_id, ConnectionData connect)
		{
			if (client_id == ServerID)
			{
				return true;
			}
			if (offline_mode)
			{
				return false;
			}
			if (connect == null)
			{
				return false;
			}
			if (string.IsNullOrEmpty(connect.username) || string.IsNullOrEmpty(connect.user_id))
			{
				return false;
			}
			if (checkApproval != null && !checkApproval(client_id, connect))
			{
				return false;
			}
			return true;
		}

		public IReadOnlyList<ulong> GetClientsIds()
		{
			return network.ConnectedClientsIds;
		}

		public int CountClients()
		{
			if (offline_mode)
			{
				return 1;
			}
			if (IsServer && IsConnected())
			{
				return network.ConnectedClientsIds.Count;
			}
			return 0;
		}

		public bool IsConnecting()
		{
			if (IsActive())
			{
				return !IsConnected();
			}
			return false;
		}

		public bool IsConnected()
		{
			if (!offline_mode && !network.IsServer)
			{
				return network.IsConnectedClient;
			}
			return true;
		}

		public bool IsActive()
		{
			if (!offline_mode && !network.IsServer)
			{
				return network.IsClient;
			}
			return true;
		}

		public static TcgNetwork Get()
		{
			if (instance == null)
			{
				UnityEngine.Object.FindObjectOfType<TcgNetwork>()?.Init();
			}
			return instance;
		}
	}
}
