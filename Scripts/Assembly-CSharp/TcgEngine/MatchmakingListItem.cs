using System;
using Unity.Netcode;

namespace TcgEngine
{
	[Serializable]
	public struct MatchmakingListItem : INetworkSerializable
	{
		public string group;

		public string user_id;

		public string username;

		public void NetworkSerialize<T>(BufferSerializer<T> serializer) where T : IReaderWriter
		{
			serializer.SerializeValue(ref group);
			serializer.SerializeValue(ref user_id);
			serializer.SerializeValue(ref username);
		}
	}
}
