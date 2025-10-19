using Unity.Netcode;

namespace TcgEngine
{
	public class MsgAfterConnected : INetworkSerializable
	{
		public bool success;

		public int player_id;

		public Game game_data;

		public void NetworkSerialize<T>(BufferSerializer<T> serializer) where T : IReaderWriter
		{
			serializer.SerializeValue(ref success, default(FastBufferWriter.ForPrimitives));
			serializer.SerializeValue(ref player_id, default(FastBufferWriter.ForPrimitives));
			if (serializer.IsReader)
			{
				int value = 0;
				serializer.SerializeValue(ref value, default(FastBufferWriter.ForPrimitives));
				if (value > 0)
				{
					byte[] value2 = new byte[value];
					serializer.SerializeValue(ref value2, default(FastBufferWriter.ForPrimitives));
					game_data = NetworkTool.Deserialize<Game>(value2);
				}
			}
			if (serializer.IsWriter)
			{
				byte[] value3 = NetworkTool.Serialize(game_data);
				int value4 = value3.Length;
				serializer.SerializeValue(ref value4, default(FastBufferWriter.ForPrimitives));
				if (value4 > 0)
				{
					serializer.SerializeValue(ref value3, default(FastBufferWriter.ForPrimitives));
				}
			}
		}
	}
}
