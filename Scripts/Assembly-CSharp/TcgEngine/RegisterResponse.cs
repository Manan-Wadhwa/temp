using System;

namespace TcgEngine
{
	[Serializable]
	public struct RegisterResponse
	{
		public string id;

		public string username;

		public string version;

		public bool success;

		public string error;
	}
}
