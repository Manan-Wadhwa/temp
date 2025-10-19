using UnityEngine.Events;

namespace TcgEngine.Client
{
	public class RefreshEvent
	{
		public ushort tag;

		public UnityAction<SerializedData> callback;
	}
}
