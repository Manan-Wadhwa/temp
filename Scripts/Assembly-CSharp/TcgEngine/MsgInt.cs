using Unity.Netcode;

namespace TcgEngine
{
	public class MsgInt : INetworkSerializable
	{
		public int value;

		public void NetworkSerialize<T>(BufferSerializer<T> serializer) where T : IReaderWriter
		{
			serializer.SerializeValue(ref value, default(FastBufferWriter.ForPrimitives));
		}
	}
}
