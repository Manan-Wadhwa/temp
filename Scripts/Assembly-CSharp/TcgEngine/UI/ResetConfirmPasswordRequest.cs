using System;

namespace TcgEngine.UI
{
	[Serializable]
	public class ResetConfirmPasswordRequest
	{
		public string email;

		public string code;

		public string password;
	}
}
