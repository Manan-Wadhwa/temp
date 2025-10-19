using System;

namespace TcgEngine
{
	[Serializable]
	public class ListJson<T>
	{
		public T[] list;

		public string error;
	}
}
