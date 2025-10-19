using System;
using Unity.Netcode;

namespace TcgEngine
{
	[Serializable]
	public class PlayerSettings : INetworkSerializable
	{
		public string username;

		public string avatar;

		public string cardback;

		public int ai_level;

		public UserDeckData deck = UserDeckData.Default;

		public static PlayerSettings Default => new PlayerSettings
		{
			username = "Player",
			avatar = "",
			cardback = "",
			deck = UserDeckData.Default,
			ai_level = 1
		};

		public static PlayerSettings DefaultAI => new PlayerSettings
		{
			username = "AI",
			avatar = "",
			cardback = "",
			deck = UserDeckData.Default,
			ai_level = 10
		};

		public bool HasDeck()
		{
			if (deck != null)
			{
				return !string.IsNullOrEmpty(deck.tid);
			}
			return false;
		}

		public void NetworkSerialize<T>(BufferSerializer<T> serializer) where T : IReaderWriter
		{
			serializer.SerializeValue(ref username);
			serializer.SerializeValue(ref avatar);
			serializer.SerializeValue(ref cardback);
			serializer.SerializeValue(ref ai_level, default(FastBufferWriter.ForPrimitives));
			serializer.SerializeValue(ref deck, default(FastBufferWriter.ForNetworkSerializable));
		}
	}
}
