using Unity.Netcode;

namespace TcgEngine
{
	public class MatchmakingList : INetworkSerializable
	{
		public MatchmakingListItem[] items;

		public void NetworkSerialize<T>(BufferSerializer<T> serializer) where T : IReaderWriter
		{
			NetworkTool.NetSerializeArray(serializer, ref items);
		}
	}
}
