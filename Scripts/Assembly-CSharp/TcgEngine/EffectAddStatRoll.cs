using TcgEngine.Gameplay;
using UnityEngine;

namespace TcgEngine
{
	[CreateAssetMenu(fileName = "effect", menuName = "TcgEngine/Effect/AddStatRoll", order = 10)]
	public class EffectAddStatRoll : EffectData
	{
		public EffectStatType type;

		public override void DoEffect(GameLogic logic, AbilityData ability, Card caster, Player target)
		{
			Game gameData = logic.GetGameData();
			if (type == EffectStatType.HP)
			{
				target.hp += gameData.rolled_value;
				target.hp_max += gameData.rolled_value;
			}
			if (type == EffectStatType.Mana)
			{
				target.mana += gameData.rolled_value;
				target.mana_max += gameData.rolled_value;
				target.mana = Mathf.Max(target.mana, 0);
				target.mana_max = Mathf.Clamp(target.mana_max, 0, GameplayData.Get().mana_max);
			}
		}

		public override void DoEffect(GameLogic logic, AbilityData ability, Card caster, Card target)
		{
			Game gameData = logic.GetGameData();
			if (type == EffectStatType.Attack)
			{
				target.attack += gameData.rolled_value;
			}
			if (type == EffectStatType.HP)
			{
				target.hp += gameData.rolled_value;
			}
			if (type == EffectStatType.Mana)
			{
				target.mana += gameData.rolled_value;
			}
		}
	}
}
