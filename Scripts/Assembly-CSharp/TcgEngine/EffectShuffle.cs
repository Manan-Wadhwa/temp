using TcgEngine.Gameplay;
using UnityEngine;

namespace TcgEngine
{
	[CreateAssetMenu(fileName = "effect", menuName = "TcgEngine/Effect/Shuffle", order = 10)]
	public class EffectShuffle : EffectData
	{
		public override void DoEffect(GameLogic logic, AbilityData ability, Card caster, Player target)
		{
			logic.ShuffleDeck(target.cards_deck);
		}
	}
}
