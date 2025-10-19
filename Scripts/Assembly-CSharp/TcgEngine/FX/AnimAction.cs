using UnityEngine;
using UnityEngine.Events;

namespace TcgEngine.FX
{
	public class AnimAction
	{
		public AnimActionType type;

		public Vector3 target_pos;

		public float value;

		public float duration = 1f;

		public UnityAction callback;
	}
}
