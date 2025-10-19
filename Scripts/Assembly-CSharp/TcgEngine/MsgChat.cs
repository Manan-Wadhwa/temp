using Unity.Netcode;

namespace TcgEngine
{
	public class MsgChat : INetworkSerializable
	{
		public int player_id;

		public string msg;

		public void NetworkSerialize<T>(BufferSerializer<T> serializer) where T : IReaderWriter
		{
			serializer.SerializeValue(ref player_id, default(FastBufferWriter.ForPrimitives));
			serializer.SerializeValue(ref msg);
		}
	}
}
