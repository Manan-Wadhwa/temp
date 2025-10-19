using UnityEngine.Events;

namespace TcgEngine.FX
{
	public class AnimMatAction
	{
		public AnimMatActionType type;

		public string target_name;

		public float target_value;

		public float duration = 1f;

		public UnityAction callback;
	}
}
