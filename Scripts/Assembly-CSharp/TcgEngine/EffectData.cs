using TcgEngine.Gameplay;
using UnityEngine;

namespace TcgEngine
{
	public class EffectData : ScriptableObject
	{
		public virtual void DoEffect(GameLogic logic, AbilityData ability, Card caster)
		{
		}

		public virtual void DoEffect(GameLogic logic, AbilityData ability, Card caster, Card target)
		{
		}

		public virtual void DoEffect(GameLogic logic, AbilityData ability, Card caster, Player target)
		{
		}

		public virtual void DoEffect(GameLogic logic, AbilityData ability, Card caster, Slot target)
		{
		}

		public virtual void DoEffect(GameLogic logic, AbilityData ability, Card caster, CardData target)
		{
		}

		public virtual void DoOngoingEffect(GameLogic logic, AbilityData ability, Card caster, Card target)
		{
		}

		public virtual void DoOngoingEffect(GameLogic logic, AbilityData ability, Card caster, Player target)
		{
		}
	}
}
