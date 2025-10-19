using System;

namespace TcgEngine
{
	[Serializable]
	public enum GameState
	{
		Connecting = 0,
		Play = 20,
		GameEnded = 99
	}
}
