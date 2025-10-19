using UnityEngine;

namespace TcgEngine
{
	[CreateAssetMenu(fileName = "NetworkData", menuName = "TcgEngine/NetworkData", order = 0)]
	public class NetworkData : ScriptableObject
	{
		[Header("Game Server")]
		public string url;

		public ushort port;

		[Header("API")]
		public string api_url;

		public bool api_https;

		[Header("Settings")]
		public SoloType solo_type;

		public AuthenticatorType auth_type;

		public static NetworkData Get()
		{
			return TcgNetwork.Get().data;
		}
	}
}
