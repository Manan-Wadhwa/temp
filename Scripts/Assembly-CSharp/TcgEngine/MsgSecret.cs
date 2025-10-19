using Unity.Netcode;

namespace TcgEngine
{
	public class MsgSecret : INetworkSerializable
	{
		public string secret_uid;

		public string triggerer_uid;

		public void NetworkSerialize<T>(BufferSerializer<T> serializer) where T : IReaderWriter
		{
			serializer.SerializeValue(ref secret_uid);
			serializer.SerializeValue(ref triggerer_uid);
		}
	}
}
