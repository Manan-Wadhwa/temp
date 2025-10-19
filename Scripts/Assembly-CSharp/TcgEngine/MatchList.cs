using Unity.Netcode;

namespace TcgEngine
{
	public class MatchList : INetworkSerializable
	{
		public MatchListItem[] items;

		public void NetworkSerialize<T>(BufferSerializer<T> serializer) where T : IReaderWriter
		{
			NetworkTool.NetSerializeArray(serializer, ref items);
		}
	}
}
