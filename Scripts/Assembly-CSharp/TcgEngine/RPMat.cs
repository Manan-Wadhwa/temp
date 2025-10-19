using UnityEngine;
using UnityEngine.UI;

namespace TcgEngine
{
	public class RPMat : MonoBehaviour
	{
		public Material mat_urp;

		private SpriteRenderer render;

		private Image image;

		private void Start()
		{
			render = GetComponent<SpriteRenderer>();
			image = GetComponent<Image>();
			if (render != null && GameTool.IsURP())
			{
				render.material = mat_urp;
			}
			if (image != null && GameTool.IsURP())
			{
				image.material = mat_urp;
			}
		}
	}
}
