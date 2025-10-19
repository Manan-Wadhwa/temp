using System;

namespace TcgEngine
{
	[Serializable]
	public struct LoginResponse
	{
		public string id;

		public string username;

		public string refresh_token;

		public string access_token;

		public int permission_level;

		public int validation_level;

		public int duration;

		public string version;

		public string error;

		public bool success;
	}
}
