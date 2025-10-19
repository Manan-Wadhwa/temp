using System;
using UnityEngine;

namespace TcgEngine
{
	public class ApiTool : MonoBehaviour
	{
		public static T JsonToObject<T>(string json)
		{
			try
			{
				return JsonUtility.FromJson<T>(json);
			}
			catch (Exception)
			{
			}
			return (T)Activator.CreateInstance(typeof(T));
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

		public static int ParseInt(string int_str, int default_val = 0)
		{
			if (!int.TryParse(int_str, out var result))
			{
				return default_val;
			}
			return result;
		}
	}
}
