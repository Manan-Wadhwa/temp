using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

namespace TcgEngine.FX
{
	public class AnimFX : MonoBehaviour
	{
		private GameObject target;

		private float timer;

		private Vector3 start_pos;

		private Vector3 current_pos;

		private AnimAction current;

		private Queue<AnimAction> sequence = new Queue<AnimAction>();

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
				start_pos = target.transform.position;
				current_pos = target.transform.position;
				timer = 0f;
			}
			if (current == null)
			{
				return;
			}
			if (timer < current.duration)
			{
				timer += Time.deltaTime;
				if (current.type == AnimActionType.Move)
				{
					float num = (current.target_pos - start_pos).magnitude / Mathf.Max(current.duration, 0.01f);
					current_pos = Vector3.MoveTowards(current_pos, current.target_pos, num * Time.deltaTime);
					base.transform.position = current_pos;
				}
				if (current.type == AnimActionType.Size)
				{
					float num2 = Mathf.Abs(base.transform.localScale.y - current.value) / Mathf.Max(current.duration, 0.01f);
					base.transform.localScale = Vector3.MoveTowards(base.transform.localScale, current.value * Vector3.one, num2 * Time.deltaTime);
				}
			}
			else
			{
				current.callback?.Invoke();
				current = null;
			}
		}

		public void MoveTo(Vector3 pos, float duration)
		{
			AnimAction animAction = new AnimAction();
			animAction.type = AnimActionType.Move;
			animAction.duration = duration;
			animAction.target_pos = pos;
			sequence.Enqueue(animAction);
		}

		public void ScaleTo(float value, float duration)
		{
			AnimAction animAction = new AnimAction();
			animAction.type = AnimActionType.Size;
			animAction.duration = duration;
			animAction.value = value;
			sequence.Enqueue(animAction);
		}

		public void Callback(float duration, UnityAction callback)
		{
			AnimAction animAction = new AnimAction();
			animAction.type = AnimActionType.None;
			animAction.duration = duration;
			animAction.callback = callback;
			sequence.Enqueue(animAction);
		}

		public void Clear()
		{
			target = null;
			timer = 0f;
			sequence.Clear();
		}

		public static AnimFX Create(GameObject target)
		{
			AnimFX animFX = target.GetComponent<AnimFX>();
			if (animFX == null)
			{
				animFX = target.AddComponent<AnimFX>();
			}
			animFX.Clear();
			animFX.target = target;
			return animFX;
		}
	}
}
