using TcgEngine.Gameplay;
using UnityEngine;

namespace TcgEngine
{
	[CreateAssetMenu(fileName = "effect", menuName = "TcgEngine/Effect/Damage", order = 10)]
	public class EffectDamage : EffectData
	{
		public TraitData bonus_damage;

		public override void DoEffect(GameLogic logic, AbilityData ability, Card caster, Player target)
		{
			int damage = GetDamage(logic.GameData, caster, ability.value);
			logic.DamagePlayer(caster, target, damage);
		}

		public override void DoEffect(GameLogic logic, AbilityData ability, Card caster, Card target)
		{
			int damage = GetDamage(logic.GameData, caster, ability.value);
			logic.DamageCard(caster, target, damage, spell_damage: true);
		}

		private int GetDamage(Game data, Card caster, int value)
		{
			Player player = data.GetPlayer(caster.player_id);
			return value + caster.GetTraitValue(bonus_damage) + player.GetTraitValue(bonus_damage);
		}
	}
}
