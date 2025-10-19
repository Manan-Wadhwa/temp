using Unity.Netcode;

namespace TcgEngine
{
	public class MsgCastAbility : INetworkSerializable
	{
		public string ability_id;

		public string caster_uid;

		public string target_uid;

		public void NetworkSerialize<T>(BufferSerializer<T> serializer) where T : IReaderWriter
		{
			serializer.SerializeValue(ref ability_id);
			serializer.SerializeValue(ref caster_uid);
			serializer.SerializeValue(ref target_uid);
		}
	}
}
