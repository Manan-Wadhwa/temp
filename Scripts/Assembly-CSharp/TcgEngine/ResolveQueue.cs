using System;
using System.Collections.Generic;
using UnityEngine;

namespace TcgEngine
{
	public class ResolveQueue
	{
		private Pool<AbilityQueueElement> ability_elem_pool = new Pool<AbilityQueueElement>();

		private Pool<SecretQueueElement> secret_elem_pool = new Pool<SecretQueueElement>();

		private Pool<AttackQueueElement> attack_elem_pool = new Pool<AttackQueueElement>();

		private Pool<CallbackQueueElement> callback_elem_pool = new Pool<CallbackQueueElement>();

		private Queue<AbilityQueueElement> ability_queue = new Queue<AbilityQueueElement>();

		private Queue<SecretQueueElement> secret_queue = new Queue<SecretQueueElement>();

		private Queue<AttackQueueElement> attack_queue = new Queue<AttackQueueElement>();

		private Queue<CallbackQueueElement> callback_queue = new Queue<CallbackQueueElement>();

		private Game game_data;

		private bool is_resolving;

		private float resolve_delay;

		private bool skip_delay;

		public ResolveQueue(Game data, bool skip)
		{
			game_data = data;
			skip_delay = skip;
		}

		public void SetData(Game data)
		{
			game_data = data;
		}

		public virtual void Update(float delta)
		{
			if (resolve_delay > 0f)
			{
				resolve_delay -= delta;
				if (resolve_delay <= 0f)
				{
					ResolveAll();
				}
			}
		}

		public virtual void AddAbility(AbilityData ability, Card caster, Card triggerer, Action<AbilityData, Card, Card> callback)
		{
			if (ability != null && caster != null)
			{
				AbilityQueueElement abilityQueueElement = ability_elem_pool.Create();
				abilityQueueElement.caster = caster;
				abilityQueueElement.triggerer = triggerer;
				abilityQueueElement.ability = ability;
				abilityQueueElement.callback = callback;
				ability_queue.Enqueue(abilityQueueElement);
			}
		}

		public virtual void AddAttack(Card attacker, Card target, Action<Card, Card, bool> callback, bool skip_cost = false)
		{
			if (attacker != null && target != null)
			{
				AttackQueueElement attackQueueElement = attack_elem_pool.Create();
				attackQueueElement.attacker = attacker;
				attackQueueElement.target = target;
				attackQueueElement.ptarget = null;
				attackQueueElement.skip_cost = skip_cost;
				attackQueueElement.callback = callback;
				attack_queue.Enqueue(attackQueueElement);
			}
		}

		public virtual void AddAttack(Card attacker, Player target, Action<Card, Player, bool> callback, bool skip_cost = false)
		{
			if (attacker != null && target != null)
			{
				AttackQueueElement attackQueueElement = attack_elem_pool.Create();
				attackQueueElement.attacker = attacker;
				attackQueueElement.target = null;
				attackQueueElement.ptarget = target;
				attackQueueElement.skip_cost = skip_cost;
				attackQueueElement.pcallback = callback;
				attack_queue.Enqueue(attackQueueElement);
			}
		}

		public virtual void AddSecret(AbilityTrigger secret_trigger, Card secret, Card trigger, Action<AbilityTrigger, Card, Card> callback)
		{
			if (secret != null && trigger != null)
			{
				SecretQueueElement secretQueueElement = secret_elem_pool.Create();
				secretQueueElement.secret_trigger = secret_trigger;
				secretQueueElement.secret = secret;
				secretQueueElement.triggerer = trigger;
				secretQueueElement.callback = callback;
				secret_queue.Enqueue(secretQueueElement);
			}
		}

		public virtual void AddCallback(Action callback)
		{
			if (callback != null)
			{
				CallbackQueueElement callbackQueueElement = callback_elem_pool.Create();
				callbackQueueElement.callback = callback;
				callback_queue.Enqueue(callbackQueueElement);
			}
		}

		public virtual void Resolve()
		{
			if (ability_queue.Count > 0)
			{
				AbilityQueueElement abilityQueueElement = ability_queue.Dequeue();
				ability_elem_pool.Dispose(abilityQueueElement);
				abilityQueueElement.callback?.Invoke(abilityQueueElement.ability, abilityQueueElement.caster, abilityQueueElement.triggerer);
			}
			else if (secret_queue.Count > 0)
			{
				SecretQueueElement secretQueueElement = secret_queue.Dequeue();
				secret_elem_pool.Dispose(secretQueueElement);
				secretQueueElement.callback?.Invoke(secretQueueElement.secret_trigger, secretQueueElement.secret, secretQueueElement.triggerer);
			}
			else if (attack_queue.Count > 0)
			{
				AttackQueueElement attackQueueElement = attack_queue.Dequeue();
				attack_elem_pool.Dispose(attackQueueElement);
				if (attackQueueElement.ptarget != null)
				{
					attackQueueElement.pcallback?.Invoke(attackQueueElement.attacker, attackQueueElement.ptarget, attackQueueElement.skip_cost);
				}
				else
				{
					attackQueueElement.callback?.Invoke(attackQueueElement.attacker, attackQueueElement.target, attackQueueElement.skip_cost);
				}
			}
			else if (callback_queue.Count > 0)
			{
				CallbackQueueElement callbackQueueElement = callback_queue.Dequeue();
				callback_elem_pool.Dispose(callbackQueueElement);
				callbackQueueElement.callback();
			}
		}

		public virtual void ResolveAll(float delay)
		{
			SetDelay(delay);
			ResolveAll();
		}

		public virtual void ResolveAll()
		{
			if (!is_resolving)
			{
				is_resolving = true;
				while (CanResolve())
				{
					Resolve();
				}
				is_resolving = false;
			}
		}

		public virtual void SetDelay(float delay)
		{
			if (!skip_delay)
			{
				resolve_delay = Mathf.Max(resolve_delay, delay);
			}
		}

		public virtual bool CanResolve()
		{
			if (resolve_delay > 0f)
			{
				return false;
			}
			if (game_data.state == GameState.GameEnded)
			{
				return false;
			}
			if (game_data.selector != SelectorType.None)
			{
				return false;
			}
			if (attack_queue.Count <= 0 && ability_queue.Count <= 0 && secret_queue.Count <= 0)
			{
				return callback_queue.Count > 0;
			}
			return true;
		}

		public virtual bool IsResolving()
		{
			if (!is_resolving)
			{
				return resolve_delay > 0f;
			}
			return true;
		}

		public virtual void Clear()
		{
			attack_elem_pool.DisposeAll();
			ability_elem_pool.DisposeAll();
			secret_elem_pool.DisposeAll();
			callback_elem_pool.DisposeAll();
			attack_queue.Clear();
			ability_queue.Clear();
			secret_queue.Clear();
			callback_queue.Clear();
		}

		public Queue<AttackQueueElement> GetAttackQueue()
		{
			return attack_queue;
		}

		public Queue<AbilityQueueElement> GetAbilityQueue()
		{
			return ability_queue;
		}

		public Queue<SecretQueueElement> GetSecretQueue()
		{
			return secret_queue;
		}

		public Queue<CallbackQueueElement> GetCallbackQueue()
		{
			return callback_queue;
		}
	}
}
