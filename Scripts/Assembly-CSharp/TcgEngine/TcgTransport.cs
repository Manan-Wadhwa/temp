using Unity.Netcode.Transports.UTP;
using UnityEngine;

namespace TcgEngine
{
	public class TcgTransport : MonoBehaviour
	{
		private UnityTransport transport;

		private const string listen_all = "0.0.0.0";

		public virtual void Init()
		{
			transport = GetComponent<UnityTransport>();
		}

		public virtual void SetServer(ushort port)
		{
			transport.ConnectionData.ServerListenAddress = "0.0.0.0";
			transport.SetConnectionData("0.0.0.0", port);
		}

		public virtual void SetClient(string address, ushort port)
		{
			string ipv4Address = NetworkTool.HostToIP(address);
			transport.SetConnectionData(ipv4Address, port);
		}

		public virtual string GetAddress()
		{
			return transport.ConnectionData.Address;
		}

		public virtual ushort GetPort()
		{
			return transport.ConnectionData.Port;
		}
	}
}
