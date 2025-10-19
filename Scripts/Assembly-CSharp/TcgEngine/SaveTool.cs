using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using UnityEngine;

namespace TcgEngine
{
	[Serializable]
	public class SaveTool
	{
		public static T LoadFile<T>(string filename) where T : class
		{
			T result = null;
			string path = Application.persistentDataPath + "/" + filename;
			if (IsValidFilename(filename) && File.Exists(path))
			{
				FileStream fileStream = null;
				try
				{
					BinaryFormatter binaryFormatter = new BinaryFormatter();
					fileStream = File.Open(path, FileMode.Open);
					result = (T)binaryFormatter.Deserialize(fileStream);
					fileStream.Close();
				}
				catch (Exception ex)
				{
					Debug.Log("Error Loading Data " + ex);
					fileStream?.Close();
				}
			}
			return result;
		}

		public static void SaveFile<T>(string filename, T data) where T : class
		{
			if (IsValidFilename(filename))
			{
				FileStream fileStream = null;
				try
				{
					BinaryFormatter binaryFormatter = new BinaryFormatter();
					fileStream = File.Create(Application.persistentDataPath + "/" + filename);
					binaryFormatter.Serialize(fileStream, data);
					fileStream.Close();
				}
				catch (Exception ex)
				{
					Debug.Log("Error Saving Data " + ex);
					fileStream?.Close();
				}
			}
		}

		public static void DeleteFile(string filename)
		{
			string path = Application.persistentDataPath + "/" + filename;
			if (File.Exists(path))
			{
				File.Delete(path);
			}
		}

		public static List<string> GetAllSave(string extension = "")
		{
			List<string> list = new List<string>();
			string[] files = Directory.GetFiles(Application.persistentDataPath);
			foreach (string text in files)
			{
				if (text.EndsWith(extension))
				{
					string fileName = Path.GetFileName(text);
					if (!list.Contains(fileName))
					{
						list.Add(fileName);
					}
				}
			}
			return list;
		}

		public static bool DoesFileExist(string filename)
		{
			string path = Application.persistentDataPath + "/" + filename;
			if (IsValidFilename(filename))
			{
				return File.Exists(path);
			}
			return false;
		}

		public static bool IsValidFilename(string filename)
		{
			if (string.IsNullOrWhiteSpace(filename))
			{
				return false;
			}
			char[] invalidFileNameChars = Path.GetInvalidFileNameChars();
			foreach (char c in invalidFileNameChars)
			{
				if (filename.Contains(c.ToString()))
				{
					return false;
				}
			}
			return true;
		}
	}
}
