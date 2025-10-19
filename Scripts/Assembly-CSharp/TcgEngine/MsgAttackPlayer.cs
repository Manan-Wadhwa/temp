using Unity.Netcode;

namespace TcgEngine
{
	public class MsgAttackPlayer : INetworkSerializable
	{
		public string attacker_uid;

		public int target_id;

		public int damage;

		public void NetworkSerialize<T>(BufferSerializer<T> serializer) where T : IReaderWriter
		{
			serializer.SerializeValue(ref attacker_uid);
			serializer.SerializeValue(ref target_id, default(FastBufferWriter.ForPrimitives));
			serializer.SerializeValue(ref damage, default(FastBufferWriter.ForPrimitives));
		}
	}
}
