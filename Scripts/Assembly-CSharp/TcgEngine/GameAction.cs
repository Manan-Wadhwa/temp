namespace TcgEngine
{
	public static class GameAction
	{
		public const ushort None = 0;

		public const ushort PlayCard = 1000;

		public const ushort Attack = 1010;

		public const ushort AttackPlayer = 1012;

		public const ushort Move = 1015;

		public const ushort CastAbility = 1020;

		public const ushort SelectCard = 1030;

		public const ushort SelectPlayer = 1032;

		public const ushort SelectSlot = 1034;

		public const ushort SelectChoice = 1036;

		public const ushort CancelSelect = 1039;

		public const ushort EndTurn = 1040;

		public const ushort Resign = 1050;

		public const ushort ChatMessage = 1090;

		public const ushort PlayerSettings = 1100;

		public const ushort PlayerSettingsAI = 1102;

		public const ushort GameSettings = 1105;

		public const ushort Connected = 2000;

		public const ushort PlayerReady = 2001;

		public const ushort GameStart = 2010;

		public const ushort GameEnd = 2012;

		public const ushort NewTurn = 2015;

		public const ushort CardPlayed = 2020;

		public const ushort CardSummoned = 2022;

		public const ushort CardTransformed = 2023;

		public const ushort CardDiscarded = 2025;

		public const ushort CardDrawn = 2026;

		public const ushort CardMoved = 2027;

		public const ushort AttackStart = 2030;

		public const ushort AttackEnd = 2032;

		public const ushort AttackPlayerStart = 2034;

		public const ushort AttackPlayerEnd = 2036;

		public const ushort AbilityTrigger = 2040;

		public const ushort AbilityTargetCard = 2042;

		public const ushort AbilityTargetPlayer = 2043;

		public const ushort AbilityTargetSlot = 2044;

		public const ushort AbilityEnd = 2048;

		public const ushort SecretTriggered = 2060;

		public const ushort SecretResolved = 2061;

		public const ushort ValueRolled = 2070;

		public const ushort ServerMessage = 2190;

		public const ushort RefreshAll = 2100;

		public static string GetString(ushort type)
		{
			return type switch
			{
				1000 => "play", 
				1015 => "move", 
				1010 => "attack", 
				1012 => "attack_player", 
				1020 => "cast_ability", 
				1040 => "end_turn", 
				1030 => "select_card", 
				1032 => "select_player", 
				1036 => "select_choice", 
				1034 => "select_slot", 
				1039 => "cancel_select", 
				1050 => "resign", 
				1090 => "chat", 
				_ => type.ToString(), 
			};
		}
	}
}
