using System;

namespace TcgEngine
{
	[Serializable]
	public struct RegisterRequest
	{
		public string email;

		public string username;

		public string password;

		public string avatar;
	}
}
