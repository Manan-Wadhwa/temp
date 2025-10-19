using System;

namespace TcgEngine
{
	[Serializable]
	public enum GameType
	{
		Solo = 0,
		Adventure = 10,
		Multiplayer = 20,
		HostP2P = 30,
		Observer = 40
	}
}
