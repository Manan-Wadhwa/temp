using TcgEngine.Gameplay;
using UnityEngine;

namespace TcgEngine
{
	[CreateAssetMenu(fileName = "effect", menuName = "TcgEngine/Effect/ResetStat", order = 10)]
	public class EffectResetStat : EffectData
	{
		public override void DoEffect(GameLogic logic, AbilityData ability, Card caster, Card target)
		{
			target.SetCard(target.CardData, target.VariantData);
		}
	}
}
