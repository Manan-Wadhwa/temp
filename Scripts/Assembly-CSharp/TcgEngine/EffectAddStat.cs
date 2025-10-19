using TcgEngine.Gameplay;
using UnityEngine;

namespace TcgEngine
{
	[CreateAssetMenu(fileName = "effect", menuName = "TcgEngine/Effect/AddStat", order = 10)]
	public class EffectAddStat : EffectData
	{
		public EffectStatType type;

		public override void DoEffect(GameLogic logic, AbilityData ability, Card caster, Player target)
		{
			if (type == EffectStatType.HP)
			{
				target.hp += ability.value;
				target.hp_max += ability.value;
			}
			if (type == EffectStatType.Mana)
			{
				target.mana += ability.value;
				target.mana_max += ability.value;
				target.mana = Mathf.Max(target.mana, 0);
				target.mana_max = Mathf.Clamp(target.mana_max, 0, GameplayData.Get().mana_max);
			}
		}

		public override void DoEffect(GameLogic logic, AbilityData ability, Card caster, Card target)
		{
			if (type == EffectStatType.Attack)
			{
				target.attack += ability.value;
			}
			if (type == EffectStatType.HP)
			{
				target.hp += ability.value;
			}
			if (type == EffectStatType.Mana)
			{
				target.mana += ability.value;
			}
		}

		public override void DoOngoingEffect(GameLogic logic, AbilityData ability, Card caster, Card target)
		{
			if (type == EffectStatType.Attack)
			{
				target.attack_ongoing += ability.value;
			}
			if (type == EffectStatType.HP)
			{
				target.hp_ongoing += ability.value;
			}
			if (type == EffectStatType.Mana)
			{
				target.mana_ongoing += ability.value;
			}
		}
	}
}
