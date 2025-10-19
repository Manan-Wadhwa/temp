using Unity.Netcode;

namespace TcgEngine
{
	public class MsgAttack : INetworkSerializable
	{
		public string attacker_uid;

		public string target_uid;

		public int damage;

		public void NetworkSerialize<T>(BufferSerializer<T> serializer) where T : IReaderWriter
		{
			serializer.SerializeValue(ref attacker_uid);
			serializer.SerializeValue(ref target_uid);
			serializer.SerializeValue(ref damage, default(FastBufferWriter.ForPrimitives));
		}
	}
}
