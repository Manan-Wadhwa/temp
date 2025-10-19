using TcgEngine.Client;
using UnityEngine;

namespace TcgEngine.FX
{
	public class AimTargetFX : MonoBehaviour
	{
		public GameObject fx;

		private void Start()
		{
		}

		private void Update()
		{
			bool flag = false;
			HandCard drag = HandCard.GetDrag();
			if (drag != null && drag.GetCard().CardData.IsRequireTarget())
			{
				flag = true;
			}
			if (fx.activeSelf != flag)
			{
				fx.SetActive(flag);
			}
			if (flag)
			{
				Vector3 position = GameBoard.Get().RaycastMouseBoard();
				base.transform.position = position;
			}
		}
	}
}
