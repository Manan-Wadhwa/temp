using System;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.Networking;

namespace TcgEngine
{
	public class WebTool
	{
		public static T JsonToObject<T>(string json)
		{
			T result = (T)Activator.CreateInstance(typeof(T));
			try
			{
				result = JsonUtility.FromJson<T>(json);
			}
			catch (Exception)
			{
			}
			return result;
		}

		public static T[] JsonToArray<T>(string json)
		{
			new ListJson<T>().list = new T[0];
			try
			{
				return JsonUtility.FromJson<ListJson<T>>("{ \"list\": " + json + "}").list;
			}
			catch (Exception)
			{
			}
			return new T[0];
		}

		public static string ToJson(object data)
		{
			return JsonUtility.ToJson(data);
		}

		public static int Parse(string int_str, int default_val = 0)
		{
			if (!int.TryParse(int_str, out var result))
			{
				return default_val;
			}
			return result;
		}

		public static async Task<WebResponse> SendRequest(string url)
		{
			return await SendRequest(WebRequest.Create(url));
		}

		public static async Task<WebResponse> SendRequest(UnityWebRequest request)
		{
			try
			{
				UnityWebRequestAsyncOperation asyncOp = request.SendWebRequest();
				while (!asyncOp.isDone)
				{
					await TimeTool.Delay(200);
				}
			}
			catch (Exception)
			{
			}
			if (request.result != UnityWebRequest.Result.Success)
			{
				Debug.Log(request.error);
			}
			WebResponse response = WebRequest.GetResponse(request);
			request.Dispose();
			return response;
		}
	}
}
