using System;
using UnityEngine;

namespace TcgEngine.Client
{
	public class GameCamera : MonoBehaviour
	{
		private float shake_timer;

		private float shake_intensity = 1f;

		private Camera cam;

		private Vector3 shake_vector = Vector3.zero;

		private Vector3 start_pos;

		private static GameCamera instance;

		private void Awake()
		{
			instance = this;
			start_pos = base.transform.position;
			cam = GetComponent<Camera>();
		}

		private void Update()
		{
			if (shake_timer > 0f)
			{
				shake_timer -= Time.deltaTime;
				shake_vector = new Vector3(Mathf.Cos(shake_timer * MathF.PI * 16f) * 0.02f, Mathf.Sin(shake_timer * MathF.PI * 12f) * 0.01f, 0f);
				base.transform.position = start_pos + shake_vector * shake_intensity;
			}
			else
			{
				base.transform.position = start_pos;
			}
		}

		public void Shake(float intensity = 1f, float duration = 1f)
		{
			shake_intensity = intensity;
			shake_timer = duration;
		}

		public Vector2 MouseToPercent(Vector3 mouse_pos)
		{
			float x = mouse_pos.x / (float)Screen.width;
			float y = mouse_pos.y / (float)Screen.height;
			return new Vector2(x, y);
		}

		public Ray MouseToRay(Vector3 mouse_pos)
		{
			return cam.ScreenPointToRay(mouse_pos);
		}

		public Vector3 MouseToWorld(Vector2 mouse_pos, float distance = 10f)
		{
			return cam.ScreenToWorldPoint(new Vector3(mouse_pos.x, mouse_pos.y, distance));
		}

		public static Camera GetCamera()
		{
			if (instance != null)
			{
				return instance.cam;
			}
			return null;
		}

		public static GameCamera Get()
		{
			return instance;
		}
	}
}
