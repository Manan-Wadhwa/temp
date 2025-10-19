using Unity.Netcode;

namespace TcgEngine
{
	public class MsgPlayCard : INetworkSerializable
	{
		public string card_uid;

		public Slot slot;

		public void NetworkSerialize<T>(BufferSerializer<T> serializer) where T : IReaderWriter
		{
			serializer.SerializeValue(ref card_uid);
			serializer.SerializeNetworkSerializable(ref slot);
		}
	}
}
