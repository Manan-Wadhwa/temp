using System;
using System.Collections.Generic;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

namespace TcgEngine
{
	public static class GameTool
	{
		private const string uid_chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789";

		private static Random random = new Random();

		public static string GenerateRandomID(int min = 9, int max = 15)
		{
			int num = random.Next(min, max);
			string text = "";
			for (int i = 0; i < num; i++)
			{
				text += "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789"[random.Next("ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789".Length - 1)];
			}
			return text;
		}

		public static int GenerateRandomInt()
		{
			return random.Next(int.MinValue, int.MaxValue);
		}

		public static ulong GenerateRandomUInt64()
		{
			long num = (uint)random.Next(int.MinValue, int.MaxValue);
			uint num2 = (uint)random.Next(int.MinValue, int.MaxValue);
			return (ulong)((num << 32) | num2);
		}

		public static List<T> PickXRandom<T>(List<T> source, List<T> dest, int x)
		{
			if (source.Count <= x || x <= 0)
			{
				return source;
			}
			if (dest.Count > 0)
			{
				dest.Clear();
			}
			for (int i = 0; i < x; i++)
			{
				int index = random.Next(source.Count);
				dest.Add(source[index]);
				source.RemoveAt(index);
			}
			return dest;
		}

		public static void CloneList(List<string> source, List<string> dest)
		{
			for (int i = 0; i < source.Count; i++)
			{
				if (i < dest.Count)
				{
					dest[i] = source[i];
				}
				else
				{
					dest.Add(source[i]);
				}
			}
			if (dest.Count > source.Count)
			{
				dest.RemoveRange(source.Count, dest.Count - source.Count);
			}
		}

		public static void CloneListRef<T>(List<T> source, List<T> dest) where T : class
		{
			for (int i = 0; i < source.Count; i++)
			{
				if (i < dest.Count)
				{
					dest[i] = source[i];
				}
				else
				{
					dest.Add(source[i]);
				}
			}
			if (dest.Count > source.Count)
			{
				dest.RemoveRange(source.Count, dest.Count - source.Count);
			}
		}

		public static void CloneListRefNull<T>(List<T> source, ref List<T> dest) where T : class
		{
			if (source == null)
			{
				dest = null;
				return;
			}
			if (dest == null)
			{
				dest = new List<T>();
			}
			CloneListRef(source, dest);
		}

		public static bool IsMobile()
		{
			return false;
		}

		public static bool IsURP()
		{
			if (GraphicsSettings.defaultRenderPipeline is UniversalRenderPipelineAsset)
			{
				return true;
			}
			return false;
		}
	}
}
