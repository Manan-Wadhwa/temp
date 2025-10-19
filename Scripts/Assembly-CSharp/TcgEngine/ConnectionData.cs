using System;
using Unity.Netcode;

namespace TcgEngine
{
	[Serializable]
	public class ConnectionData : INetworkSerializable
	{
		public string user_id = "";

		public string username = "";

		public byte[] extra = new byte[0];

		public string GetExtraString()
		{
			return NetworkTool.DeserializeString(extra);
		}

		public T GetExtraData<T>() where T : INetworkSerializable, new()
		{
			return NetworkTool.NetDeserialize<T>(extra);
		}

		public void NetworkSerialize<T>(BufferSerializer<T> serializer) where T : IReaderWriter
		{
			serializer.SerializeValue(ref user_id);
			serializer.SerializeValue(ref username);
			serializer.SerializeValue(ref extra, default(FastBufferWriter.ForPrimitives));
		}
	}
}
