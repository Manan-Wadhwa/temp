using Unity.Netcode;

namespace TcgEngine
{
	public class MsgPlayerConnect : INetworkSerializable
	{
		public string user_id;

		public string username;

		public string game_uid;

		public int nb_players;

		public bool observer;

		public void NetworkSerialize<T>(BufferSerializer<T> serializer) where T : IReaderWriter
		{
			serializer.SerializeValue(ref user_id);
			serializer.SerializeValue(ref username);
			serializer.SerializeValue(ref game_uid);
			serializer.SerializeValue(ref nb_players, default(FastBufferWriter.ForPrimitives));
			serializer.SerializeValue(ref observer, default(FastBufferWriter.ForPrimitives));
		}
	}
}
