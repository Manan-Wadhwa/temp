using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine.Networking;

namespace TcgEngine
{
	public class WebRequest
	{
		public const string METHOD_GET = "GET";

		public const string METHOD_POST = "POST";

		public const string METHOD_PATCH = "PATCH";

		public const string METHOD_DELETE = "DELETE";

		public const int timeout = 10;

		public static UnityWebRequest Create(string url)
		{
			UnityWebRequest unityWebRequest = new UnityWebRequest(url, "GET");
			unityWebRequest.SetRequestHeader("Content-Type", "application/json");
			unityWebRequest.downloadHandler = new DownloadHandlerBuffer();
			unityWebRequest.timeout = 10;
			return unityWebRequest;
		}

		public static UnityWebRequest Create(string url, string method, string json_data, string token)
		{
			UnityWebRequest unityWebRequest = new UnityWebRequest(url, method);
			unityWebRequest.SetRequestHeader("Content-Type", "application/json");
			if (token != null)
			{
				unityWebRequest.SetRequestHeader("Authorization", token);
			}
			unityWebRequest.downloadHandler = new DownloadHandlerBuffer();
			unityWebRequest.timeout = 10;
			if (method != "GET" && !string.IsNullOrEmpty(json_data))
			{
				UploadHandler uploadHandler = new UploadHandlerRaw(Encoding.UTF8.GetBytes(json_data));
				uploadHandler.contentType = "application/json";
				unityWebRequest.uploadHandler = uploadHandler;
			}
			return unityWebRequest;
		}

		public static UnityWebRequest CreateRaw(string url, string method, string contentType, byte[] data, string token)
		{
			UnityWebRequest unityWebRequest = new UnityWebRequest(url, method);
			unityWebRequest.SetRequestHeader("Content-Type", contentType);
			if (token != null)
			{
				unityWebRequest.SetRequestHeader("Authorization", token);
			}
			unityWebRequest.downloadHandler = new DownloadHandlerBuffer();
			unityWebRequest.timeout = 10;
			if (method != "GET" && !string.IsNullOrEmpty(contentType))
			{
				UploadHandler uploadHandler = new UploadHandlerRaw(data);
				uploadHandler.contentType = contentType;
				unityWebRequest.uploadHandler = uploadHandler;
			}
			return unityWebRequest;
		}

		public static UnityWebRequest CreateHeader(string url)
		{
			return UnityWebRequest.Head(url);
		}

		public static UnityWebRequest CreateTexture(string url)
		{
			UnityWebRequest texture = UnityWebRequestTexture.GetTexture(url);
			texture.SetRequestHeader("Content-Type", "image/png");
			return texture;
		}

		public static UnityWebRequest CreateImageUploadForm(string url, string path, byte[] data, string token)
		{
			List<IMultipartFormSection> list = new List<IMultipartFormSection>();
			list.Add(new MultipartFormDataSection("path", path, "text"));
			list.Add(new MultipartFormFileSection("data", data, "file.png", "image/png"));
			UnityWebRequest unityWebRequest = UnityWebRequest.Post(url, list);
			if (token != null)
			{
				unityWebRequest.SetRequestHeader("Authorization", token);
			}
			unityWebRequest.timeout = 200;
			return unityWebRequest;
		}

		public static WebResponse GetResponse(UnityWebRequest request)
		{
			WebResponse result = new WebResponse
			{
				success = (request.responseCode >= 200 && request.responseCode < 300),
				status = request.responseCode,
				error = request.error,
				data = ""
			};
			if (request.downloadHandler != null)
			{
				result.data = request.downloadHandler.text;
			}
			return result;
		}

		public static HeadResponse GetHeadResponse(UnityWebRequest request)
		{
			HeadResponse obj = new HeadResponse
			{
				success = (request.responseCode >= 200 && request.responseCode < 300),
				status = request.responseCode
			};
			string responseHeader = request.GetResponseHeader("Content-Type");
			DateTime.TryParse(request.GetResponseHeader("Last-Modified"), out var result);
			int.TryParse(request.GetResponseHeader("Content-Length"), out var result2);
			obj.content_type = responseHeader;
			obj.last_edit = result;
			obj.size = result2;
			return obj;
		}
	}
}
