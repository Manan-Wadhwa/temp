using System;

namespace TcgEngine
{
	[Serializable]
	public struct LoginRequest
	{
		public string email;

		public string username;

		public string password;
	}
}
