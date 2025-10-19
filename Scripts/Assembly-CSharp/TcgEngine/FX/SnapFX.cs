using UnityEngine;

namespace TcgEngine.FX
{
	public class SnapFX : MonoBehaviour
	{
		public Transform target;

		public Vector3 offset = Vector3.zero;

		private void Start()
		{
		}

		private void Update()
		{
			if (target == null)
			{
				Object.Destroy(base.gameObject);
			}
			else
			{
				base.transform.position = target.position + offset;
			}
		}
	}
}
