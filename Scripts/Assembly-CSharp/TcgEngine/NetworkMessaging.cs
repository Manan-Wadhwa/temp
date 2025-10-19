using System;
using System.Collections.Generic;
using Unity.Collections;
using Unity.Netcode;
using UnityEngine.Events;

namespace TcgEngine
{
	public class NetworkMessaging
	{
		private TcgNetwork network;

		private Dictionary<string, Action<ulong, FastBufferReader>> msg_dict = new Dictionary<string, Action<ulong, FastBufferReader>>();

		public IReadOnlyList<ulong> ClientList => network.GetClientsIds();

		public bool IsOnline => network.IsOnline;

		public bool IsServer => network.IsServer;

		public ulong ServerID => network.ServerID;

		public ulong ClientID => network.ClientID;

		public NetworkMessaging(TcgNetwork network)
		{
			this.network = network;
			network.onConnect = (UnityAction)Delegate.Combine(network.onConnect, new UnityAction(OnConnect));
		}

		private void OnConnect()
		{
			foreach (KeyValuePair<string, Action<ulong, FastBufferReader>> item in msg_dict)
			{
				RegisterNetMsg(item.Key, item.Value);
			}
		}

		public void ListenMsg(string type, Action<ulong, FastBufferReader> callback)
		{
			msg_dict[type] = callback;
			RegisterNetMsg(type, callback);
		}

		public void UnListenMsg(string type)
		{
			msg_dict.Remove(type);
			if (network.NetworkManager.CustomMessagingManager != null)
			{
				network.NetworkManager.CustomMessagingManager.UnregisterNamedMessageHandler(type);
			}
		}

		private void RegisterNetMsg(string type, Action<ulong, FastBufferReader> callback)
		{
			if (IsOnline)
			{
				network.NetworkManager.CustomMessagingManager.RegisterNamedMessageHandler(type, delegate(ulong client_id, FastBufferReader reader)
				{
					ReceiveNetMessage(type, client_id, reader);
				});
			}
		}

		private void ReceiveNetMessage(string type, ulong client_id, FastBufferReader reader)
		{
			if (msg_dict.TryGetValue(type, out var value) && IsOnline)
			{
				value(client_id, reader);
			}
		}

		public void SendEmpty(string type, ulong target, NetworkDelivery delivery)
		{
			FastBufferWriter writer = new FastBufferWriter(0, Allocator.Temp);
			Send(type, target, writer, delivery);
			writer.Dispose();
		}

		public void SendBytes(string type, ulong target, byte[] msg, NetworkDelivery delivery)
		{
			FastBufferWriter writer = new FastBufferWriter(msg.Length, Allocator.Temp);
			writer.WriteBytesSafe(msg, msg.Length);
			Send(type, target, writer, delivery);
			writer.Dispose();
		}

		public void SendString(string type, ulong target, string msg, NetworkDelivery delivery)
		{
			FastBufferWriter writer = new FastBufferWriter(msg.Length, Allocator.Temp, TcgNetwork.MsgSizeMax);
			writer.WriteValueSafe(msg);
			Send(type, target, writer, delivery);
			writer.Dispose();
		}

		public void SendInt(string type, ulong target, int data, NetworkDelivery delivery)
		{
			FastBufferWriter writer = new FastBufferWriter(4, Allocator.Temp);
			writer.WriteValueSafe(in data, default(FastBufferWriter.ForPrimitives));
			Send(type, target, writer, delivery);
			writer.Dispose();
		}

		public void SendUInt64(string type, ulong target, ulong data, NetworkDelivery delivery)
		{
			FastBufferWriter writer = new FastBufferWriter(8, Allocator.Temp);
			writer.WriteValueSafe(in data, default(FastBufferWriter.ForPrimitives));
			Send(type, target, writer, delivery);
			writer.Dispose();
		}

		public void SendFloat(string type, ulong target, float data, NetworkDelivery delivery)
		{
			FastBufferWriter writer = new FastBufferWriter(4, Allocator.Temp);
			writer.WriteValueSafe(in data, default(FastBufferWriter.ForPrimitives));
			Send(type, target, writer, delivery);
			writer.Dispose();
		}

		public void SendObject<T>(string type, ulong target, T data, NetworkDelivery delivery) where T : INetworkSerializable
		{
			FastBufferWriter writer = new FastBufferWriter(256, Allocator.Temp, TcgNetwork.MsgSizeMax);
			writer.WriteNetworkSerializable(in data);
			Send(type, target, writer, delivery);
			writer.Dispose();
		}

		public void SendEmpty(string type, IReadOnlyList<ulong> targets, NetworkDelivery delivery)
		{
			if (IsServer)
			{
				FastBufferWriter writer = new FastBufferWriter(0, Allocator.Temp);
				Send(type, targets, writer, delivery);
				writer.Dispose();
			}
		}

		public void SendBytes(string type, IReadOnlyList<ulong> targets, byte[] msg, NetworkDelivery delivery)
		{
			if (IsServer)
			{
				FastBufferWriter writer = new FastBufferWriter(msg.Length, Allocator.Temp);
				writer.WriteBytesSafe(msg, msg.Length);
				Send(type, targets, writer, delivery);
				writer.Dispose();
			}
		}

		public void SendString(string type, IReadOnlyList<ulong> targets, string msg, NetworkDelivery delivery)
		{
			if (IsServer)
			{
				FastBufferWriter writer = new FastBufferWriter(msg.Length, Allocator.Temp, TcgNetwork.MsgSizeMax);
				writer.WriteValueSafe(msg);
				Send(type, targets, writer, delivery);
				writer.Dispose();
			}
		}

		public void SendInt(string type, IReadOnlyList<ulong> targets, int data, NetworkDelivery delivery)
		{
			if (IsServer)
			{
				FastBufferWriter writer = new FastBufferWriter(4, Allocator.Temp);
				writer.WriteValueSafe(in data, default(FastBufferWriter.ForPrimitives));
				Send(type, targets, writer, delivery);
				writer.Dispose();
			}
		}

		public void SendUInt64(string type, IReadOnlyList<ulong> targets, ulong data, NetworkDelivery delivery)
		{
			if (IsServer)
			{
				FastBufferWriter writer = new FastBufferWriter(8, Allocator.Temp);
				writer.WriteValueSafe(in data, default(FastBufferWriter.ForPrimitives));
				Send(type, targets, writer, delivery);
				writer.Dispose();
			}
		}

		public void SendFloat(string type, IReadOnlyList<ulong> targets, float data, NetworkDelivery delivery)
		{
			if (IsServer)
			{
				FastBufferWriter writer = new FastBufferWriter(4, Allocator.Temp);
				writer.WriteValueSafe(in data, default(FastBufferWriter.ForPrimitives));
				Send(type, targets, writer, delivery);
				writer.Dispose();
			}
		}

		public void SendObject<T>(string type, IReadOnlyList<ulong> targets, T data, NetworkDelivery delivery) where T : INetworkSerializable
		{
			if (IsServer)
			{
				FastBufferWriter writer = new FastBufferWriter(256, Allocator.Temp, TcgNetwork.MsgSizeMax);
				writer.WriteNetworkSerializable(in data);
				Send(type, targets, writer, delivery);
				writer.Dispose();
			}
		}

		public void SendEmptyAll(string type, NetworkDelivery delivery)
		{
			if (IsServer)
			{
				FastBufferWriter writer = new FastBufferWriter(0, Allocator.Temp);
				SendAll(type, writer, delivery);
				writer.Dispose();
			}
		}

		public void SendStringAll(string type, string msg, NetworkDelivery delivery)
		{
			if (IsServer)
			{
				FastBufferWriter writer = new FastBufferWriter(msg.Length, Allocator.Temp, TcgNetwork.MsgSizeMax);
				writer.WriteValueSafe(msg);
				SendAll(type, writer, delivery);
				writer.Dispose();
			}
		}

		public void SendIntAll(string type, int data, NetworkDelivery delivery)
		{
			if (IsServer)
			{
				FastBufferWriter writer = new FastBufferWriter(4, Allocator.Temp);
				writer.WriteValueSafe(in data, default(FastBufferWriter.ForPrimitives));
				SendAll(type, writer, delivery);
				writer.Dispose();
			}
		}

		public void SendUInt64All(string type, ulong data, NetworkDelivery delivery)
		{
			if (IsServer)
			{
				FastBufferWriter writer = new FastBufferWriter(8, Allocator.Temp);
				writer.WriteValueSafe(in data, default(FastBufferWriter.ForPrimitives));
				SendAll(type, writer, delivery);
				writer.Dispose();
			}
		}

		public void SendFloatAll(string type, float data, NetworkDelivery delivery)
		{
			if (IsServer)
			{
				FastBufferWriter writer = new FastBufferWriter(4, Allocator.Temp);
				writer.WriteValueSafe(in data, default(FastBufferWriter.ForPrimitives));
				SendAll(type, writer, delivery);
				writer.Dispose();
			}
		}

		public void SendBytesAll(string type, byte[] msg, NetworkDelivery delivery)
		{
			if (IsServer)
			{
				FastBufferWriter writer = new FastBufferWriter(msg.Length, Allocator.Temp);
				writer.WriteBytesSafe(msg, msg.Length);
				SendAll(type, writer, delivery);
				writer.Dispose();
			}
		}

		public void SendObjectAll<T>(string type, T data, NetworkDelivery delivery) where T : INetworkSerializable
		{
			if (IsServer)
			{
				FastBufferWriter writer = new FastBufferWriter(256, Allocator.Temp, TcgNetwork.MsgSizeMax);
				writer.WriteNetworkSerializable(in data);
				SendAll(type, writer, delivery);
				writer.Dispose();
			}
		}

		public void Send(string type, ulong target, FastBufferWriter writer, NetworkDelivery delivery)
		{
			if (IsOnline)
			{
				SendOnline(type, target, writer, delivery);
			}
			else if (target == ClientID)
			{
				SendOffline(type, writer);
			}
		}

		public void Send(string type, IReadOnlyList<ulong> targets, FastBufferWriter writer, NetworkDelivery delivery)
		{
			if (IsOnline)
			{
				SendOnline(type, targets, writer, delivery);
			}
			else if (Contains(targets, ClientID))
			{
				SendOffline(type, writer);
			}
		}

		public void SendAll(string type, FastBufferWriter writer, NetworkDelivery delivery)
		{
			Send(type, ClientList, writer, delivery);
		}

		private void SendOnline(string type, ulong target, FastBufferWriter writer, NetworkDelivery delivery)
		{
			network.NetworkManager.CustomMessagingManager.SendNamedMessage(type, target, writer, delivery);
		}

		private void SendOnline(string type, IReadOnlyList<ulong> targets, FastBufferWriter writer, NetworkDelivery delivery)
		{
			network.NetworkManager.CustomMessagingManager.SendNamedMessage(type, targets, writer, delivery);
		}

		private void SendOffline(string type, FastBufferWriter writer)
		{
			if (msg_dict.TryGetValue(type, out var value))
			{
				FastBufferReader arg = new FastBufferReader(writer, Allocator.Temp);
				value?.Invoke(ClientID, arg);
				arg.Dispose();
			}
		}

		public void Forward(string type, ulong target, FastBufferReader reader, NetworkDelivery delivery)
		{
			if (IsServer && IsOnline)
			{
				reader.Seek(0);
				reader.ReadValueSafe(out ulong _, default(FastBufferWriter.ForPrimitives));
				byte[] value2 = new byte[reader.Length - reader.Position];
				reader.ReadBytesSafe(ref value2, reader.Length - reader.Position);
				FastBufferWriter messageStream = new FastBufferWriter(value2.Length, Allocator.Temp);
				messageStream.WriteBytesSafe(value2, value2.Length);
				network.NetworkManager.CustomMessagingManager.SendNamedMessage(type, target, messageStream, delivery);
				messageStream.Dispose();
			}
		}

		public void Forward(string type, IReadOnlyList<ulong> targets, FastBufferReader reader, NetworkDelivery delivery)
		{
			if (IsServer && IsOnline)
			{
				reader.Seek(0);
				reader.ReadValueSafe(out ulong _, default(FastBufferWriter.ForPrimitives));
				byte[] value2 = new byte[reader.Length - reader.Position];
				reader.ReadBytesSafe(ref value2, reader.Length - reader.Position);
				FastBufferWriter messageStream = new FastBufferWriter(value2.Length, Allocator.Temp);
				messageStream.WriteBytesSafe(value2, value2.Length);
				network.NetworkManager.CustomMessagingManager.SendNamedMessage(type, targets, messageStream, delivery);
				messageStream.Dispose();
			}
		}

		public void ForwardAll(string type, ulong source_client, FastBufferReader reader, NetworkDelivery delivery)
		{
			if (!IsServer || !IsOnline)
			{
				return;
			}
			reader.Seek(0);
			reader.ReadValueSafe(out ulong _, default(FastBufferWriter.ForPrimitives));
			byte[] value2 = new byte[reader.Length - reader.Position];
			reader.ReadBytesSafe(ref value2, reader.Length - reader.Position);
			FastBufferWriter messageStream = new FastBufferWriter(value2.Length, Allocator.Temp);
			messageStream.WriteBytesSafe(value2, value2.Length);
			foreach (ulong client in ClientList)
			{
				if (client != source_client && client != ClientID)
				{
					network.NetworkManager.CustomMessagingManager.SendNamedMessage(type, client, messageStream, delivery);
				}
			}
			messageStream.Dispose();
		}

		private bool Contains(IReadOnlyList<ulong> list, ulong client_id)
		{
			foreach (ulong item in list)
			{
				if (item == client_id)
				{
					return true;
				}
			}
			return false;
		}

		public static NetworkMessaging Get()
		{
			return TcgNetwork.Get().Messaging;
		}
	}
}
