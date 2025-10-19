using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Runtime.Serialization.Formatters.Binary;
using System.Text;
using System.Threading.Tasks;
using Unity.Collections;
using Unity.Netcode;
using UnityEngine;

namespace TcgEngine
{
	public class NetworkTool
	{
		public static byte[] Serialize<T>(T obj) where T : class
		{
			try
			{
				BinaryFormatter binaryFormatter = new BinaryFormatter();
				MemoryStream memoryStream = new MemoryStream();
				binaryFormatter.Serialize(memoryStream, obj);
				byte[] result = memoryStream.ToArray();
				memoryStream.Close();
				return result;
			}
			catch (Exception ex)
			{
				Debug.LogError("Serialization error: " + ex.Message);
				return new byte[0];
			}
		}

		public static T Deserialize<T>(byte[] bytes) where T : class
		{
			try
			{
				BinaryFormatter binaryFormatter = new BinaryFormatter();
				MemoryStream memoryStream = new MemoryStream();
				memoryStream.Write(bytes, 0, bytes.Length);
				memoryStream.Seek(0L, SeekOrigin.Begin);
				T result = (T)binaryFormatter.Deserialize(memoryStream);
				memoryStream.Close();
				return result;
			}
			catch (Exception ex)
			{
				Debug.LogError("Deserialization error: " + ex.Message);
				return null;
			}
		}

		public static byte[] NetSerialize<T>(T obj, int size = 128) where T : INetworkSerializable, new()
		{
			if (obj == null)
			{
				return new byte[0];
			}
			try
			{
				FastBufferWriter fastBufferWriter = new FastBufferWriter(size, Allocator.Temp, TcgNetwork.MsgSizeMax);
				fastBufferWriter.WriteNetworkSerializable(in obj);
				byte[] result = fastBufferWriter.ToArray();
				fastBufferWriter.Dispose();
				return result;
			}
			catch (Exception ex)
			{
				Debug.LogError("Serialization error: " + ex.Message);
				return new byte[0];
			}
		}

		public static T NetDeserialize<T>(byte[] bytes) where T : INetworkSerializable, new()
		{
			if (bytes == null || bytes.Length == 0)
			{
				return default(T);
			}
			try
			{
				FastBufferReader fastBufferReader = new FastBufferReader(bytes, Allocator.Temp);
				fastBufferReader.ReadNetworkSerializable(out T value);
				fastBufferReader.Dispose();
				return value;
			}
			catch (Exception ex)
			{
				Debug.LogError("Deserialization error: " + ex.Message);
				return default(T);
			}
		}

		public static void NetSerializeArray<TS>(BufferSerializer<TS> serializer, ref string[] array) where TS : IReaderWriter
		{
			if (serializer.IsReader)
			{
				int value = 0;
				serializer.SerializeValue(ref value, default(FastBufferWriter.ForPrimitives));
				array = new string[value];
				for (int i = 0; i < value; i++)
				{
					string s = "";
					serializer.SerializeValue(ref s);
					array[i] = s;
				}
			}
			if (serializer.IsWriter)
			{
				int value2 = array.Length;
				serializer.SerializeValue(ref value2, default(FastBufferWriter.ForPrimitives));
				for (int j = 0; j < value2; j++)
				{
					serializer.SerializeValue(ref array[j]);
				}
			}
		}

		public static void NetSerializeArray<T, TS>(BufferSerializer<TS> serializer, ref T[] array) where T : INetworkSerializable, new() where TS : IReaderWriter
		{
			if (serializer.IsReader)
			{
				int value = 0;
				serializer.SerializeValue(ref value, default(FastBufferWriter.ForPrimitives));
				array = new T[value];
				for (int i = 0; i < value; i++)
				{
					T value2 = new T();
					serializer.SerializeValue(ref value2, default(FastBufferWriter.ForNetworkSerializable));
					array[i] = value2;
				}
			}
			if (serializer.IsWriter)
			{
				int value3 = array.Length;
				serializer.SerializeValue(ref value3, default(FastBufferWriter.ForPrimitives));
				for (int j = 0; j < value3; j++)
				{
					serializer.SerializeValue(ref array[j], default(FastBufferWriter.ForNetworkSerializable));
				}
			}
		}

		public static byte[] SerializeInt32(int data)
		{
			return BitConverter.GetBytes(data);
		}

		public static int DeserializeInt32(byte[] bytes)
		{
			if (bytes != null && bytes.Length != 0)
			{
				return BitConverter.ToInt32(bytes, 0);
			}
			return 0;
		}

		public static byte[] SerializeUInt64(ulong data)
		{
			return BitConverter.GetBytes(data);
		}

		public static ulong DeserializeUInt64(byte[] bytes)
		{
			if (bytes != null && bytes.Length != 0)
			{
				return BitConverter.ToUInt64(bytes, 0);
			}
			return 0uL;
		}

		public static byte[] SerializeString(string data)
		{
			if (data != null)
			{
				return Encoding.UTF8.GetBytes(data);
			}
			return new byte[0];
		}

		public static string DeserializeString(byte[] bytes)
		{
			if (bytes != null)
			{
				return Encoding.UTF8.GetString(bytes);
			}
			return null;
		}

		public static string SerializeToString<T>(T obj) where T : class
		{
			return Convert.ToBase64String(Serialize(obj));
		}

		public static T DeserializeFromString<T>(string str) where T : class
		{
			return Deserialize<T>(Convert.FromBase64String(str));
		}

		public static void SerializeObject<T, T1>(BufferSerializer<T> serializer, ref T1 data) where T : IReaderWriter where T1 : class
		{
			string s = "";
			if (serializer.IsWriter)
			{
				s = SerializeToString(data);
			}
			serializer.SerializeValue(ref s, oneByteChars: true);
			if (serializer.IsReader)
			{
				data = DeserializeFromString<T1>(s);
			}
		}

		public static void SerializeDictionary<T, T1, T2>(BufferSerializer<T> serializer, ref Dictionary<T1, T2> data) where T : IReaderWriter where T1 : unmanaged, IComparable, IConvertible, IComparable<T1>, IEquatable<T1> where T2 : unmanaged, IComparable, IConvertible, IComparable<T2>, IEquatable<T2>
		{
			int value = ((data != null) ? data.Count : 0);
			serializer.SerializeValue(ref value, default(FastBufferWriter.ForPrimitives));
			if (serializer.IsWriter)
			{
				foreach (KeyValuePair<T1, T2> datum in data)
				{
					T1 value2 = datum.Key;
					T2 value3 = datum.Value;
					serializer.SerializeValue(ref value2, default(FastBufferWriter.ForPrimitives));
					serializer.SerializeValue(ref value3, default(FastBufferWriter.ForPrimitives));
				}
			}
			if (serializer.IsReader)
			{
				data = new Dictionary<T1, T2>();
				for (int i = 0; i < value; i++)
				{
					T1 value4 = new T1();
					T2 value5 = new T2();
					serializer.SerializeValue(ref value4, default(FastBufferWriter.ForPrimitives));
					serializer.SerializeValue(ref value5, default(FastBufferWriter.ForPrimitives));
					data.Add(value4, value5);
				}
			}
		}

		public static void SerializeDictionaryEnum<T, T1, T2>(BufferSerializer<T> serializer, ref Dictionary<T1, T2> data) where T : IReaderWriter where T1 : unmanaged, Enum where T2 : unmanaged, IComparable, IConvertible, IComparable<T2>, IEquatable<T2>
		{
			int value = ((data != null) ? data.Count : 0);
			serializer.SerializeValue(ref value, default(FastBufferWriter.ForPrimitives));
			if (serializer.IsWriter)
			{
				foreach (KeyValuePair<T1, T2> datum in data)
				{
					T1 value2 = datum.Key;
					T2 value3 = datum.Value;
					serializer.SerializeValue(ref value2, default(FastBufferWriter.ForEnums));
					serializer.SerializeValue(ref value3, default(FastBufferWriter.ForPrimitives));
				}
			}
			if (serializer.IsReader)
			{
				data = new Dictionary<T1, T2>();
				for (int i = 0; i < value; i++)
				{
					T1 value4 = new T1();
					T2 value5 = new T2();
					serializer.SerializeValue(ref value4, default(FastBufferWriter.ForEnums));
					serializer.SerializeValue(ref value5, default(FastBufferWriter.ForPrimitives));
					data.Add(value4, value5);
				}
			}
		}

		public static void SerializeDictionary<T, T2>(BufferSerializer<T> serializer, ref Dictionary<string, T2> data) where T : IReaderWriter where T2 : unmanaged, IComparable, IConvertible, IComparable<T2>, IEquatable<T2>
		{
			int value = ((data != null) ? data.Count : 0);
			serializer.SerializeValue(ref value, default(FastBufferWriter.ForPrimitives));
			if (serializer.IsWriter)
			{
				foreach (KeyValuePair<string, T2> datum in data)
				{
					string s = datum.Key;
					T2 value2 = datum.Value;
					serializer.SerializeValue(ref s);
					serializer.SerializeValue(ref value2, default(FastBufferWriter.ForPrimitives));
				}
			}
			if (serializer.IsReader)
			{
				data = new Dictionary<string, T2>();
				for (int i = 0; i < value; i++)
				{
					string s2 = "";
					T2 value3 = new T2();
					serializer.SerializeValue(ref s2);
					serializer.SerializeValue(ref value3, default(FastBufferWriter.ForPrimitives));
					data.Add(s2, value3);
				}
			}
		}

		public static void SerializeDictionary<T>(BufferSerializer<T> serializer, ref Dictionary<string, string> data) where T : IReaderWriter
		{
			int value = ((data != null) ? data.Count : 0);
			serializer.SerializeValue(ref value, default(FastBufferWriter.ForPrimitives));
			if (serializer.IsWriter)
			{
				foreach (KeyValuePair<string, string> datum in data)
				{
					string s = datum.Key;
					string s2 = datum.Value;
					serializer.SerializeValue(ref s);
					serializer.SerializeValue(ref s2);
				}
			}
			if (serializer.IsReader)
			{
				data = new Dictionary<string, string>();
				for (int i = 0; i < value; i++)
				{
					string s3 = "";
					string s4 = "";
					serializer.SerializeValue(ref s3);
					serializer.SerializeValue(ref s4);
					data.Add(s3, s4);
				}
			}
		}

		public static void SerializeDictionaryNetObject<T, T2>(BufferSerializer<T> serializer, ref Dictionary<string, T2> data) where T : IReaderWriter where T2 : INetworkSerializable, new()
		{
			int value = ((data != null) ? data.Count : 0);
			serializer.SerializeValue(ref value, default(FastBufferWriter.ForPrimitives));
			if (serializer.IsWriter)
			{
				foreach (KeyValuePair<string, T2> datum in data)
				{
					string s = datum.Key;
					T2 value2 = datum.Value;
					serializer.SerializeValue(ref s);
					serializer.SerializeNetworkSerializable(ref value2);
				}
			}
			if (serializer.IsReader)
			{
				data = new Dictionary<string, T2>();
				for (int i = 0; i < value; i++)
				{
					string s2 = "";
					T2 value3 = new T2();
					serializer.SerializeValue(ref s2);
					serializer.SerializeNetworkSerializable(ref value3);
					data.Add(s2, value3);
				}
			}
		}

		public static void SerializeDictionaryNetObject<T, T1, T2>(BufferSerializer<T> serializer, ref Dictionary<T1, T2> data) where T : IReaderWriter where T1 : unmanaged, IComparable, IConvertible, IComparable<T1>, IEquatable<T1> where T2 : INetworkSerializable, new()
		{
			int value = ((data != null) ? data.Count : 0);
			serializer.SerializeValue(ref value, default(FastBufferWriter.ForPrimitives));
			if (serializer.IsWriter)
			{
				foreach (KeyValuePair<T1, T2> datum in data)
				{
					T1 value2 = datum.Key;
					T2 value3 = datum.Value;
					serializer.SerializeValue(ref value2, default(FastBufferWriter.ForPrimitives));
					serializer.SerializeNetworkSerializable(ref value3);
				}
			}
			if (serializer.IsReader)
			{
				data = new Dictionary<T1, T2>();
				for (int i = 0; i < value; i++)
				{
					T1 value4 = new T1();
					T2 value5 = new T2();
					serializer.SerializeValue(ref value4, default(FastBufferWriter.ForPrimitives));
					serializer.SerializeNetworkSerializable(ref value5);
					data.Add(value4, value5);
				}
			}
		}

		public static void SerializeDictionaryObject<T, T2>(BufferSerializer<T> serializer, ref Dictionary<string, T2> data) where T : IReaderWriter where T2 : class, new()
		{
			int value = ((data != null) ? data.Count : 0);
			serializer.SerializeValue(ref value, default(FastBufferWriter.ForPrimitives));
			if (serializer.IsWriter)
			{
				foreach (KeyValuePair<string, T2> datum in data)
				{
					string s = datum.Key;
					T2 data2 = datum.Value;
					serializer.SerializeValue(ref s);
					SerializeObject(serializer, ref data2);
				}
			}
			if (serializer.IsReader)
			{
				data = new Dictionary<string, T2>();
				for (int i = 0; i < value; i++)
				{
					string s2 = "";
					T2 data3 = new T2();
					serializer.SerializeValue(ref s2);
					SerializeObject(serializer, ref data3);
					data.Add(s2, data3);
				}
			}
		}

		public static void SerializeDictionaryObject<T, T1, T2>(BufferSerializer<T> serializer, ref Dictionary<T1, T2> data) where T : IReaderWriter where T1 : unmanaged, IComparable, IConvertible, IComparable<T1>, IEquatable<T1> where T2 : class, new()
		{
			int value = ((data != null) ? data.Count : 0);
			serializer.SerializeValue(ref value, default(FastBufferWriter.ForPrimitives));
			if (serializer.IsWriter)
			{
				foreach (KeyValuePair<T1, T2> datum in data)
				{
					T1 value2 = datum.Key;
					T2 data2 = datum.Value;
					serializer.SerializeValue(ref value2, default(FastBufferWriter.ForPrimitives));
					SerializeObject(serializer, ref data2);
				}
			}
			if (serializer.IsReader)
			{
				data = new Dictionary<T1, T2>();
				for (int i = 0; i < value; i++)
				{
					T1 value3 = new T1();
					T2 data3 = new T2();
					serializer.SerializeValue(ref value3, default(FastBufferWriter.ForPrimitives));
					SerializeObject(serializer, ref data3);
					data.Add(value3, data3);
				}
			}
		}

		public static void SerializeDictionaryEnumObject<T, T1, T2>(BufferSerializer<T> serializer, ref Dictionary<T1, T2> data) where T : IReaderWriter where T1 : unmanaged, Enum where T2 : class, new()
		{
			int value = ((data != null) ? data.Count : 0);
			serializer.SerializeValue(ref value, default(FastBufferWriter.ForPrimitives));
			if (serializer.IsWriter)
			{
				foreach (KeyValuePair<T1, T2> datum in data)
				{
					T1 value2 = datum.Key;
					T2 data2 = datum.Value;
					serializer.SerializeValue(ref value2, default(FastBufferWriter.ForEnums));
					SerializeObject(serializer, ref data2);
				}
			}
			if (serializer.IsReader)
			{
				data = new Dictionary<T1, T2>();
				for (int i = 0; i < value; i++)
				{
					T1 value3 = new T1();
					T2 data3 = new T2();
					serializer.SerializeValue(ref value3, default(FastBufferWriter.ForEnums));
					SerializeObject(serializer, ref data3);
					data.Add(value3, data3);
				}
			}
		}

		public static ushort Hash16(string string_id)
		{
			return (ushort)string_id.GetHashCode();
		}

		public static uint Hash32(string string_id)
		{
			return (uint)string_id.GetHashCode();
		}

		public static ulong Hash64(string string_id)
		{
			string text = string_id.Substring(0, string_id.Length / 2);
			string text2 = string_id.Substring(string_id.Length / 2);
			return ((ulong)(uint)text.GetHashCode() << 32) | (uint)text2.GetHashCode();
		}

		public static IPAddress ResolveDns(string url)
		{
			IPAddress[] hostAddresses = Dns.GetHostAddresses(url);
			if (hostAddresses != null && hostAddresses.Length != 0)
			{
				return hostAddresses[0];
			}
			return null;
		}

		public static string HostToIP(string host)
		{
			if (IPAddress.TryParse(host, out var address))
			{
				return address.ToString();
			}
			IPAddress iPAddress = ResolveDns(host);
			if (iPAddress != null)
			{
				return iPAddress.ToString();
			}
			return "";
		}

		public static string GetLocalIp()
		{
			IPAddress[] addressList = Dns.GetHostEntry(Dns.GetHostName()).AddressList;
			foreach (IPAddress iPAddress in addressList)
			{
				if (iPAddress.AddressFamily == AddressFamily.InterNetwork)
				{
					return iPAddress.ToString();
				}
			}
			return "";
		}

		public static async Task<string> GetOnlineIp()
		{
			WebResponse webResponse = await WebTool.SendRequest("https://api.ipify.org");
			if (webResponse.success)
			{
				return webResponse.data;
			}
			return null;
		}
	}
}
