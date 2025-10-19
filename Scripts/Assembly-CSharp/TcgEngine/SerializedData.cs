using Unity.Netcode;

namespace TcgEngine
{
	public class SerializedData
	{
		private FastBufferReader reader;

		private INetworkSerializable data;

		private byte[] bytes;

		public SerializedData(FastBufferReader r)
		{
			reader = r;
			data = null;
		}

		public SerializedData(INetworkSerializable d)
		{
			data = d;
		}

		public string GetString()
		{
			reader.ReadValueSafe(out var s, oneByteChars: false);
			return s;
		}

		public T Get<T>() where T : INetworkSerializable, new()
		{
			if (data != null)
			{
				return (T)data;
			}
			if (bytes != null)
			{
				data = NetworkTool.NetDeserialize<T>(bytes);
				return (T)data;
			}
			reader.ReadNetworkSerializable(out T value);
			data = value;
			return value;
		}

		public void PreRead()
		{
			int num = reader.Length - reader.Position;
			bytes = new byte[num];
			reader.ReadBytesSafe(ref bytes, num);
		}
	}
}
