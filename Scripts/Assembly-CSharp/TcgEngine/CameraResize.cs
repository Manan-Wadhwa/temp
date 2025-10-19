using UnityEngine;

namespace TcgEngine
{
	[RequireComponent(typeof(Camera))]
	public class CameraResize : MonoBehaviour
	{
		private Camera cam;

		private int sheight;

		private int swidth;

		private void Start()
		{
			cam = GetComponent<Camera>();
			sheight = Screen.height;
			swidth = Screen.width;
			UpdateSize();
		}

		private void Update()
		{
			if (sheight != Screen.height || swidth != Screen.width)
			{
				sheight = Screen.height;
				swidth = Screen.width;
				UpdateSize();
			}
		}

		public void UpdateSize()
		{
			float num = (float)Screen.width / (float)Screen.height;
			float aspectRatio = GetAspectRatio();
			if (Mathf.Approximately(num, aspectRatio))
			{
				cam.rect = new Rect(0f, 0f, 1f, 1f);
			}
			else if (num > aspectRatio)
			{
				float num2 = aspectRatio / num;
				float x = (1f - num2) / 2f;
				cam.rect = new Rect(x, 0f, num2, 1f);
			}
			else
			{
				float num3 = num / aspectRatio;
				float y = (1f - num3) / 2f;
				cam.rect = new Rect(0f, y, 1f, num3);
			}
		}

		public static float GetAspectMin()
		{
			return 1.6f;
		}

		public static float GetAspectMax()
		{
			return 1.7777778f;
		}

		public static float GetCamSizeMin()
		{
			return 4.5f;
		}

		public static float GetCamSizeMax()
		{
			return 5f;
		}

		public static float GetAspectRatio()
		{
			float aspectMax = GetAspectMax();
			float aspectMin = GetAspectMin();
			return Mathf.Clamp((float)Screen.width / (float)Screen.height, aspectMin, aspectMax);
		}

		public static float GetAspectValue()
		{
			float aspectMax = GetAspectMax();
			float aspectMin = GetAspectMin();
			return (GetAspectRatio() - aspectMin) / (aspectMax - aspectMin);
		}
	}
}
