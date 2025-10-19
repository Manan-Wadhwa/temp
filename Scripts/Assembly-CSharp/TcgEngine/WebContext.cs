using System;
using System.IO;
using System.Net;
using System.Text;
using UnityEngine;

namespace TcgEngine
{
	public class WebContext
	{
		public HttpListenerContext http;

		public string method;

		public string token;

		public string path;

		public string data;

		public void SendResponse<T>(T value)
		{
			string value2 = WebTool.ToJson(value);
			SendResponse(value2);
		}

		public void SendResponse(ulong value)
		{
			SendResponse(value.ToString());
		}

		public void SendResponse(int value)
		{
			SendResponse(value.ToString());
		}

		public void SendResponse(bool value)
		{
			SendResponse(value.ToString());
		}

		public void SendResponse(string value)
		{
			byte[] bytes = Encoding.UTF8.GetBytes(value);
			SendResponse(bytes, 200);
		}

		public void SendError(string value)
		{
			byte[] bytes = Encoding.UTF8.GetBytes(value);
			SendResponse(bytes, 400);
		}

		public void SendResponse()
		{
			try
			{
				WriteHeader();
				http.Response.StatusCode = 200;
				http.Response.Close();
			}
			catch (Exception message)
			{
				Debug.Log(message);
			}
		}

		public void SendResponse(byte[] bytes, int code)
		{
			try
			{
				WriteHeader();
				http.Response.StatusCode = code;
				http.Response.OutputStream.Write(bytes, 0, bytes.Length);
				http.Response.Close();
			}
			catch (Exception message)
			{
				Debug.Log(message);
			}
		}

		private void WriteHeader()
		{
			http.Response.Headers.Add("Access-Control-Allow-Origin", "*");
			http.Response.Headers.Add("Access-Control-Allow-Methods", "GET,HEAD,OPTIONS,POST,PUT");
			http.Response.Headers.Add("Access-Control-Allow-Headers", "Origin, X-Requested-With, Content-Type, Accept, Authorization");
		}

		public T GetData<T>()
		{
			return WebTool.JsonToObject<T>(data);
		}

		public ulong GetInt64()
		{
			if (!ulong.TryParse(data, out var result))
			{
				return 0uL;
			}
			return result;
		}

		public int GetInt()
		{
			if (!int.TryParse(data, out var result))
			{
				return 0;
			}
			return result;
		}

		public bool GetBool()
		{
			if (!bool.TryParse(data, out var result))
			{
				return false;
			}
			return result;
		}

		public ulong GetClientID()
		{
			if (!ulong.TryParse(token, out var result))
			{
				return 0uL;
			}
			return result;
		}

		public string GetIP()
		{
			return http.Request.RemoteEndPoint.Address.ToString();
		}

		public string GetKey()
		{
			return token;
		}

		public bool IsKeyValid(string key)
		{
			return token == key;
		}

		public string GetQuery(string key)
		{
			try
			{
				return http.Request.QueryString.Get(key);
			}
			catch (Exception message)
			{
				Debug.Log(message);
			}
			return "";
		}

		public void Close()
		{
			try
			{
				http.Response.Close();
			}
			catch (Exception message)
			{
				Debug.Log(message);
			}
		}

		public static WebContext Create(HttpListenerContext http)
		{
			WebContext webContext = new WebContext();
			webContext.http = http;
			webContext.path = "";
			webContext.data = "";
			try
			{
				webContext.method = http.Request.HttpMethod;
				webContext.path = http.Request.RawUrl.Remove(0, 1);
				webContext.token = http.Request.Headers.Get("Authorization");
				if (http.Request.InputStream != null)
				{
					StreamReader streamReader = new StreamReader(http.Request.InputStream, http.Request.ContentEncoding);
					webContext.data = streamReader.ReadToEnd();
				}
			}
			catch (Exception message)
			{
				Debug.Log(message);
			}
			return webContext;
		}
	}
}
