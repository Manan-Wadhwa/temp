using TcgEngine.Gameplay;
using UnityEngine;

namespace TcgEngine
{
	[CreateAssetMenu(fileName = "effect", menuName = "TcgEngine/Effect/Transform", order = 10)]
	public class EffectTransform : EffectData
	{
		public CardData transform_to;

		public override void DoEffect(GameLogic logic, AbilityData ability, Card caster, Card target)
		{
			logic.TransformCard(target, transform_to);
		}
	}
}
