using Unity.Netcode;

namespace TcgEngine
{
	public class MsgMatchmaking : INetworkSerializable
	{
		public string user_id;

		public string username;

		public string group;

		public int players;

		public int elo;

		public bool refresh;

		public float time;

		public void NetworkSerialize<T>(BufferSerializer<T> serializer) where T : IReaderWriter
		{
			serializer.SerializeValue(ref user_id);
			serializer.SerializeValue(ref username);
			serializer.SerializeValue(ref group);
			serializer.SerializeValue(ref players, default(FastBufferWriter.ForPrimitives));
			serializer.SerializeValue(ref elo, default(FastBufferWriter.ForPrimitives));
			serializer.SerializeValue(ref refresh, default(FastBufferWriter.ForPrimitives));
			serializer.SerializeValue(ref time, default(FastBufferWriter.ForPrimitives));
		}
	}
}
