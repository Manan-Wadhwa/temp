using Unity.Netcode;

namespace TcgEngine
{
	public class MsgCastAbilityPlayer : INetworkSerializable
	{
		public string ability_id;

		public string caster_uid;

		public int target_id;

		public void NetworkSerialize<T>(BufferSerializer<T> serializer) where T : IReaderWriter
		{
			serializer.SerializeValue(ref ability_id);
			serializer.SerializeValue(ref caster_uid);
			serializer.SerializeValue(ref target_id, default(FastBufferWriter.ForPrimitives));
		}
	}
}
