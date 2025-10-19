using System;

namespace TcgEngine
{
	[Serializable]
	public enum GamePhase
	{
		None = 0,
		StartTurn = 10,
		Main = 20,
		EndTurn = 30
	}
}
