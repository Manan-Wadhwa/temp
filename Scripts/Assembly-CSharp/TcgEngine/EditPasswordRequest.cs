using System;

namespace TcgEngine
{
	[Serializable]
	public struct EditPasswordRequest
	{
		public string password_previous;

		public string password_new;
	}
}
