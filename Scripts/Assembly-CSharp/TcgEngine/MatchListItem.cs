using System;
using Unity.Netcode;

namespace TcgEngine
{
	[Serializable]
	public class MatchListItem : INetworkSerializable
	{
		public string group;

		public string username;

		public string game_uid;

		public string game_url;

		public void NetworkSerialize<T>(BufferSerializer<T> serializer) where T : IReaderWriter
		{
			serializer.SerializeValue(ref group);
			serializer.SerializeValue(ref username);
			serializer.SerializeValue(ref game_uid);
			serializer.SerializeValue(ref game_url);
		}
	}
}
