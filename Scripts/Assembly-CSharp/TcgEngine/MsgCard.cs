using Unity.Netcode;

namespace TcgEngine
{
	public class MsgCard : INetworkSerializable
	{
		public string card_uid;

		public void NetworkSerialize<T>(BufferSerializer<T> serializer) where T : IReaderWriter
		{
			serializer.SerializeValue(ref card_uid);
		}
	}
}
