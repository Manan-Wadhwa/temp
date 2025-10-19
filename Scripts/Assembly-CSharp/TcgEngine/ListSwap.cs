using System.Collections.Generic;

namespace TcgEngine
{
	public class ListSwap<T>
	{
		public List<T> swap1 = new List<T>();

		public List<T> swap2 = new List<T>();

		public List<T> Get()
		{
			swap1.Clear();
			return swap1;
		}

		public List<T> GetOther(List<T> skip)
		{
			if (skip == swap1)
			{
				swap2.Clear();
				return swap2;
			}
			swap1.Clear();
			return swap1;
		}

		public void Clear()
		{
			swap1.Clear();
			swap2.Clear();
		}
	}
}
