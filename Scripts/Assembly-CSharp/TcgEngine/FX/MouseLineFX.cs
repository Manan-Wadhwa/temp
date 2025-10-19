using System.Collections.Generic;
using TcgEngine.Client;
using UnityEngine;

namespace TcgEngine.FX
{
	public class MouseLineFX : MonoBehaviour
	{
		public GameObject dot_template;

		public float dot_spacing = 0.2f;

		private List<GameObject> dot_list = new List<GameObject>();

		private List<Vector3> points = new List<Vector3>();

		private void Start()
		{
			dot_template.SetActive(value: false);
		}

		private void Update()
		{
			if (GameClient.Get().IsReady())
			{
				RefreshLine();
				RefreshRender();
			}
		}

		private void RefreshLine()
		{
			points.Clear();
			Game gameData = GameClient.Get().GetGameData();
			BoardCard selected = PlayerControls.Get().GetSelected();
			bool flag = false;
			Vector3 vector = Vector3.zero;
			if (selected != null)
			{
				vector = selected.transform.position;
				flag = true;
			}
			HandCard drag = HandCard.GetDrag();
			if (drag != null)
			{
				vector = drag.transform.position;
				flag = drag.GetCardData().IsRequireTarget();
			}
			if (gameData.selector == SelectorType.SelectTarget && gameData.selector_player_id == GameClient.Get().GetPlayerID())
			{
				BoardCard boardCard = BoardCard.Get(gameData.selector_caster_uid);
				if (boardCard != null)
				{
					vector = boardCard.transform.position;
					flag = true;
				}
			}
			if (flag)
			{
				Vector3 vector2 = GameBoard.Get().RaycastMouseBoard();
				Vector3 normalized = (vector2 - vector).normalized;
				float magnitude = (vector2 - vector).magnitude;
				for (float num = 0f; num < magnitude; num += dot_spacing)
				{
					Vector3 item = vector + normalized * num;
					points.Add(item);
				}
			}
		}

		private void RefreshRender()
		{
			while (dot_list.Count < points.Count)
			{
				AddDot();
			}
			int num = 0;
			foreach (GameObject item in dot_list)
			{
				bool flag = false;
				if (num < points.Count)
				{
					Vector3 position = points[num];
					item.transform.position = position;
					flag = true;
				}
				if (item.activeSelf != flag)
				{
					item.SetActive(flag);
				}
				num++;
			}
		}

		public void AddDot()
		{
			GameObject gameObject = Object.Instantiate(dot_template, base.transform);
			gameObject.SetActive(value: true);
			dot_list.Add(gameObject);
		}
	}
}
