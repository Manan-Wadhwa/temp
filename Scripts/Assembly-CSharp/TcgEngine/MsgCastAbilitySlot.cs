using Unity.Netcode;

namespace TcgEngine
{
	public class MsgCastAbilitySlot : INetworkSerializable
	{
		public string ability_id;

		public string caster_uid;

		public Slot slot;

		public void NetworkSerialize<T>(BufferSerializer<T> serializer) where T : IReaderWriter
		{
			serializer.SerializeValue(ref ability_id);
			serializer.SerializeValue(ref caster_uid);
			serializer.SerializeNetworkSerializable(ref slot);
		}
	}
}
