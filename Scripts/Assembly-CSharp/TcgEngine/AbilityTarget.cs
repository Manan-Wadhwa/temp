namespace TcgEngine
{
	public enum AbilityTarget
	{
		None = 0,
		Self = 1,
		PlayerSelf = 4,
		PlayerOpponent = 5,
		AllPlayers = 7,
		AllCardsBoard = 10,
		AllCardsHand = 11,
		AllCardsAllPiles = 12,
		AllSlots = 15,
		AllCardData = 17,
		PlayTarget = 20,
		AbilityTriggerer = 25,
		EquippedCard = 27,
		SelectTarget = 30,
		CardSelector = 40,
		ChoiceSelector = 50,
		LastPlayed = 70,
		LastTargeted = 72,
		LastDestroyed = 74,
		LastSummoned = 77
	}
}
