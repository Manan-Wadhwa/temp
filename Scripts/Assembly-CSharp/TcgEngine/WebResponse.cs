namespace TcgEngine
{
	public struct WebResponse
	{
		public bool success;

		public long status;

		public string data;

		public string error;

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

		public T GetData<T>()
		{
			return WebTool.JsonToObject<T>(data);
		}

		public string GetError()
		{
			ErrorResponse errorResponse = WebTool.JsonToObject<ErrorResponse>(data);
			if (errorResponse != null)
			{
				return errorResponse.error;
			}
			return error;
		}
	}
}
