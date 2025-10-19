using System;
using System.Collections;
using System.Threading.Tasks;
using UnityEngine;

namespace TcgEngine
{
	public class TimeTool
	{
		public static void WaitFor(float time, Action callback)
		{
			StartCoroutine(WaitForRun(time, callback));
		}

		public static void WaitUntil(Func<bool> condition, Action callback)
		{
			StartCoroutine(WaitUntilRun(condition, callback));
		}

		private static IEnumerator WaitForRun(float time, Action callback)
		{
			yield return new WaitForSeconds(time);
			callback?.Invoke();
		}

		private static IEnumerator WaitUntilRun(Func<bool> condition, Action callback)
		{
			yield return new WaitUntil(condition);
			callback?.Invoke();
		}

		public static Coroutine StartCoroutine(IEnumerator routine)
		{
			return TimeToolMono.Inst.StartCoroutine(routine);
		}

		public static void StopCoroutine(Coroutine routine)
		{
			TimeToolMono.Inst.StopCoroutine(routine);
		}

		public static async Task Delay(int miliseconds)
		{
			await Task.Delay(miliseconds);
		}
	}
}
