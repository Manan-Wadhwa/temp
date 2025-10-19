using System;

namespace TcgEngine
{
	[Serializable]
	public struct FriendResponse
	{
		public string username;

		public string server_time;

		public FriendData[] friends;

		public FriendData[] friends_requests;
	}
}
