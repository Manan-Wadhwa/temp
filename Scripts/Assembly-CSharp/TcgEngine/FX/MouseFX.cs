using UnityEngine;

namespace TcgEngine.FX
{
	public class MouseFX : MonoBehaviour
	{
		public float speed = 20f;

		private void Start()
		{
		}

		private void Update()
		{
			Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
			new Plane(Vector3.forward, 0f).Raycast(ray, out var enter);
			Vector3 point = ray.GetPoint(enter);
			base.transform.position = Vector3.Lerp(base.transform.position, point, speed * Time.deltaTime);
		}
	}
}
