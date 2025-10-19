namespace TcgEngine.Server
{
	public struct QueuedGameAction
	{
		public ushort type;

		public ClientData client;

		public SerializedData sdata;
	}
}
