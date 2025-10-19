using TcgEngine.Gameplay;
using UnityEngine;

namespace TcgEngine
{
	[CreateAssetMenu(fileName = "effect", menuName = "TcgEngine/Effect/ChangeOwner", order = 10)]
	public class EffectChangeOwner : EffectData
	{
		public bool owner_opponent;

		public override void DoEffect(GameLogic logic, AbilityData ability, Card caster, Card target)
		{
			Game gameData = logic.GetGameData();
			Player owner = (owner_opponent ? gameData.GetOpponentPlayer(caster.player_id) : gameData.GetPlayer(caster.player_id));
			logic.ChangeOwner(target, owner);
		}
	}
}
