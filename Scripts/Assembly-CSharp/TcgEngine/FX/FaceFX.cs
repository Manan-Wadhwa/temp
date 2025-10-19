using TcgEngine.Client;
using UnityEngine;

namespace TcgEngine.FX
{
	public class FaceFX : MonoBehaviour
	{
		public FaceType type;

		private void Start()
		{
			Vector3 up = GameBoard.Get().transform.up;
			if (type == FaceType.FaceCamera)
			{
				GameCamera gameCamera = GameCamera.Get();
				if (gameCamera != null)
				{
					Vector3 forward = gameCamera.transform.forward;
					base.transform.rotation = Quaternion.LookRotation(forward, up);
				}
			}
			if (type == FaceType.FaceCameraCenter)
			{
				GameCamera gameCamera2 = GameCamera.Get();
				if (gameCamera2 != null)
				{
					Vector3 vector = base.transform.position - gameCamera2.transform.position;
					base.transform.rotation = Quaternion.LookRotation(vector.normalized, up);
				}
			}
			if (type == FaceType.FaceBoard)
			{
				GameBoard gameBoard = GameBoard.Get();
				if (gameBoard != null)
				{
					Vector3 forward2 = gameBoard.transform.forward;
					base.transform.rotation = Quaternion.LookRotation(forward2, up);
				}
			}
		}
	}
}
