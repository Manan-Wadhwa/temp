using System;
using Unity.Netcode;

namespace TcgEngine
{
	[Serializable]
	public class UserDeckData : INetworkSerializable
	{
		public string tid;

		public string title;

		public UserCardData hero;

		public UserCardData[] cards;

		public static UserDeckData Default => new UserDeckData
		{
			tid = "",
			title = "",
			hero = new UserCardData(),
			cards = new UserCardData[0]
		};

		public UserDeckData()
		{
		}

		public UserDeckData(string tid, string title)
		{
			this.tid = tid;
			this.title = title;
			hero = new UserCardData();
			cards = new UserCardData[0];
		}

		public UserDeckData(DeckData deck)
		{
			tid = deck.id;
			title = deck.title;
			hero = new UserCardData(deck.hero, VariantData.GetDefault());
			cards = new UserCardData[deck.cards.Length];
			for (int i = 0; i < deck.cards.Length; i++)
			{
				cards[i] = new UserCardData(deck.cards[i], VariantData.GetDefault());
			}
		}

		public int GetQuantity()
		{
			int num = 0;
			UserCardData[] array = cards;
			foreach (UserCardData userCardData in array)
			{
				num += userCardData.quantity;
			}
			return num;
		}

		public bool IsValid()
		{
			if (!string.IsNullOrEmpty(tid) && !string.IsNullOrWhiteSpace(title))
			{
				return GetQuantity() >= GameplayData.Get().deck_size;
			}
			return false;
		}

		public void NetworkSerialize<T>(BufferSerializer<T> serializer) where T : IReaderWriter
		{
			serializer.SerializeValue(ref tid);
			serializer.SerializeValue(ref title);
			serializer.SerializeValue(ref hero, default(FastBufferWriter.ForNetworkSerializable));
			NetworkTool.NetSerializeArray(serializer, ref cards);
		}
	}
}
