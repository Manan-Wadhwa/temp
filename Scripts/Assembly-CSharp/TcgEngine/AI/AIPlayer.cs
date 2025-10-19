using TcgEngine.Gameplay;

namespace TcgEngine.AI
{
	public abstract class AIPlayer
	{
		public int player_id;

		public int ai_level;

		protected GameLogic gameplay;

		public virtual void Update()
		{
		}

		public bool CanPlay()
		{
			Game gameData = gameplay.GetGameData();
			Player player = gameData.GetPlayer(player_id);
			if (gameData.IsPlayerTurn(player))
			{
				return !gameplay.IsResolving();
			}
			return false;
		}

		public static AIPlayer Create(AIType type, GameLogic gameplay, int id, int level = 0)
		{
			return type switch
			{
				AIType.Random => new AIPlayerRandom(gameplay, id, level), 
				AIType.MiniMax => new AIPlayerMM(gameplay, id, level), 
				_ => null, 
			};
		}
	}
}
