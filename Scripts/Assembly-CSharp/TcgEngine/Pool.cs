using System.Collections.Generic;

namespace TcgEngine
{
	public class Pool<T> where T : new()
	{
		private HashSet<T> in_use = new HashSet<T>();

		private Stack<T> available = new Stack<T>();

		public int Count => in_use.Count;

		public int CountAvailable => available.Count;

		public int CountCapacity => in_use.Count + available.Count;

		public T Create()
		{
			if (available.Count > 0)
			{
				T val = available.Pop();
				in_use.Add(val);
				return val;
			}
			T val2 = new T();
			in_use.Add(val2);
			return val2;
		}

		public void Dispose(T elem)
		{
			in_use.Remove(elem);
			available.Push(elem);
		}

		public void DisposeAll()
		{
			foreach (T item in in_use)
			{
				available.Push(item);
			}
			in_use.Clear();
		}

		public void Clear()
		{
			in_use.Clear();
			available.Clear();
		}

		public HashSet<T> GetAllActive()
		{
			return in_use;
		}
	}
}
