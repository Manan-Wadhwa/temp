using TcgEngine.Client;
using TcgEngine.FX;
using UnityEngine;

namespace TcgEngine
{
	public class FXTool : MonoBehaviour
	{
		public static GameObject DoFX(GameObject fx_prefab, Vector3 pos, float duration = 5f)
		{
			if (fx_prefab != null)
			{
				GameObject obj = Object.Instantiate(fx_prefab, pos, GetFXRotation());
				Object.Destroy(obj, duration);
				return obj;
			}
			return null;
		}

		public static GameObject DoSnapFX(GameObject fx_prefab, Transform snap_target)
		{
			return DoSnapFX(fx_prefab, snap_target, Vector3.zero);
		}

		public static GameObject DoSnapFX(GameObject fx_prefab, Transform snap_target, Vector3 offset)
		{
			if (fx_prefab != null && snap_target != null)
			{
				GameObject obj = Object.Instantiate(fx_prefab, snap_target.transform.position + snap_target.transform.up * 2f, GetFXRotation());
				SnapFX snapFX = obj.AddComponent<SnapFX>();
				snapFX.target = snap_target;
				snapFX.offset = offset;
				Object.Destroy(obj, 5f);
				return obj;
			}
			return null;
		}

		private static Quaternion GetFXRotation()
		{
			GameBoard gameBoard = GameBoard.Get();
			return Quaternion.LookRotation((gameBoard != null) ? gameBoard.transform.forward : Vector3.forward, Vector3.up);
		}
	}
}
