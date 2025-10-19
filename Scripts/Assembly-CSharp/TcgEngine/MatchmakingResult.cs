using Unity.Netcode;

namespace TcgEngine
{
	public class MatchmakingResult : INetworkSerializable
	{
		public bool success;

		public int players;

		public string group;

		public string server_url;

		public string game_uid;

		public void NetworkSerialize<T>(BufferSerializer<T> serializer) where T : IReaderWriter
		{
			serializer.SerializeValue(ref success, default(FastBufferWriter.ForPrimitives));
			serializer.SerializeValue(ref players, default(FastBufferWriter.ForPrimitives));
			serializer.SerializeValue(ref group);
			serializer.SerializeValue(ref server_url);
			serializer.SerializeValue(ref game_uid);
		}
	}
}
