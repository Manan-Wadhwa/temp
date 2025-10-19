using TcgEngine.Gameplay;
using UnityEngine;

namespace TcgEngine
{
	[CreateAssetMenu(fileName = "effect", menuName = "TcgEngine/Effect/ClearTemp ", order = 10)]
	public class EffectClearTemp : EffectData
	{
		public override void DoEffect(GameLogic logic, AbilityData ability, Card caster)
		{
			logic.GameData.GetPlayer(caster.player_id).cards_temp.Clear();
		}

		public override void DoEffect(GameLogic logic, AbilityData ability, Card caster, Card target)
		{
			logic.GameData.GetPlayer(caster.player_id).cards_temp.Clear();
		}
	}
}
