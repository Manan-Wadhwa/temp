using System;
using Unity.Netcode;

namespace TcgEngine
{
	[Serializable]
	public class UserCardData : INetworkSerializable
	{
		public string tid;

		public string variant;

		public int quantity;

		public UserCardData()
		{
			tid = "";
			variant = "";
			quantity = 1;
		}

		public UserCardData(string id, string v)
		{
			tid = id;
			variant = v;
			quantity = 1;
		}

		public UserCardData(CardData card, VariantData variant)
		{
			tid = ((card != null) ? card.id : "");
			this.variant = ((variant != null) ? variant.id : "");
			quantity = 1;
		}

		public void NetworkSerialize<T>(BufferSerializer<T> serializer) where T : IReaderWriter
		{
			serializer.SerializeValue(ref tid);
			serializer.SerializeValue(ref variant);
			serializer.SerializeValue(ref quantity, default(FastBufferWriter.ForPrimitives));
		}
	}
}
