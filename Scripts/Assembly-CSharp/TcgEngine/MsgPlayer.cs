using Unity.Netcode;

namespace TcgEngine
{
	public class MsgPlayer : INetworkSerializable
	{
		public int player_id;

		public void NetworkSerialize<T>(BufferSerializer<T> serializer) where T : IReaderWriter
		{
			serializer.SerializeValue(ref player_id, default(FastBufferWriter.ForPrimitives));
		}
	}
}
