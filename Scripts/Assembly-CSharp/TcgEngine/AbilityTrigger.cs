namespace TcgEngine
{
	public enum AbilityTrigger
	{
		None = 0,
		Ongoing = 2,
		Activate = 5,
		OnPlay = 10,
		OnPlayOther = 12,
		StartOfTurn = 20,
		EndOfTurn = 22,
		OnBeforeAttack = 30,
		OnAfterAttack = 31,
		OnBeforeDefend = 32,
		OnAfterDefend = 33,
		OnKill = 35,
		OnDeath = 40,
		OnDeathOther = 42
	}
}
