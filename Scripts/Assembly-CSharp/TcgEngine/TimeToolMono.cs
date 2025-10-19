using System.Collections;
using UnityEngine;

namespace TcgEngine
{
	public class TimeToolMono : MonoBehaviour
	{
		private static TimeToolMono _instance;

		public static TimeToolMono Inst
		{
			get
			{
				if (_instance == null)
				{
					_instance = new GameObject("TimeTool").AddComponent<TimeToolMono>();
				}
				return _instance;
			}
		}

		public Coroutine StartRoutine(IEnumerator routine)
		{
			return StartCoroutine(routine);
		}

		public void StopRoutine(Coroutine routine)
		{
			StopCoroutine(routine);
		}
	}
}
