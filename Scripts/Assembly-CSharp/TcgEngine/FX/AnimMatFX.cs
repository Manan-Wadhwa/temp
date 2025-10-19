using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

namespace TcgEngine.FX
{
	public class AnimMatFX : MonoBehaviour
	{
		private Material target;

		private float timer;

		private float start_val;

		private float current_val;

		private AnimMatAction current;

		private Queue<AnimMatAction> sequence = new Queue<AnimMatAction>();

		private void Start()
		{
		}

		private void Update()
		{
			if (target == null)
			{
				return;
			}
			if (current == null && sequence.Count > 0)
			{
				current = sequence.Dequeue();
				start_val = target.GetFloat(current.target_name);
				current_val = start_val;
				timer = 0f;
			}
			if (current == null)
			{
				return;
			}
			if (timer < current.duration)
			{
				timer += Time.deltaTime;
				if (current.type == AnimMatActionType.Float)
				{
					float num = Mathf.Abs(current.target_value - start_val) / Mathf.Max(current.duration, 0.01f);
					current_val = Mathf.MoveTowards(current_val, current.target_value, num * Time.deltaTime);
					target.SetFloat(current.target_name, current_val);
				}
			}
			else
			{
				current.callback?.Invoke();
				current = null;
			}
		}

		public void SetFloat(string name, float value, float duration)
		{
			AnimMatAction animMatAction = new AnimMatAction();
			animMatAction.type = AnimMatActionType.Float;
			animMatAction.duration = duration;
			animMatAction.target_name = name;
			animMatAction.target_value = value;
			sequence.Enqueue(animMatAction);
		}

		public void Callback(float duration, UnityAction callback)
		{
			AnimMatAction animMatAction = new AnimMatAction();
			animMatAction.type = AnimMatActionType.None;
			animMatAction.duration = duration;
			animMatAction.callback = callback;
			sequence.Enqueue(animMatAction);
		}

		public void Clear()
		{
			target = null;
			timer = 0f;
			sequence.Clear();
		}

		public static AnimMatFX Create(GameObject obj, Material target)
		{
			AnimMatFX animMatFX = obj.GetComponent<AnimMatFX>();
			if (animMatFX == null)
			{
				animMatFX = obj.AddComponent<AnimMatFX>();
			}
			animMatFX.Clear();
			animMatFX.target = target;
			return animMatFX;
		}
	}
}
