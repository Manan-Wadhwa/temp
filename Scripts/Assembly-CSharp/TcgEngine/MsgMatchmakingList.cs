using Unity.Netcode;

namespace TcgEngine
{
	public class MsgMatchmakingList : INetworkSerializable
	{
		public string username;

		public void NetworkSerialize<T>(BufferSerializer<T> serializer) where T : IReaderWriter
		{
			serializer.SerializeValue(ref username);
		}
	}
}
