using UnityEngine.Events;

namespace TcgEngine.Server
{
	public class CommandEvent
	{
		public ushort tag;

		public UnityAction<ClientData, SerializedData> callback;
	}
}
