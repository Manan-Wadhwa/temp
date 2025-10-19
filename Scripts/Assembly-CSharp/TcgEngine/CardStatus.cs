using System;
using System.Collections.Generic;

namespace TcgEngine
{
	[Serializable]
	public class CardStatus
	{
		public StatusType type;

		public int value;

		public int duration = 1;

		public bool permanent = true;

		[NonSerialized]
		private StatusData data;

		public StatusData StatusData
		{
			get
			{
				if (data == null || data.effect != type)
				{
					data = StatusData.Get(type);
				}
				return data;
			}
		}

		public StatusData Data => StatusData;

		public CardStatus()
		{
		}

		public CardStatus(StatusType type, int value, int duration)
		{
			this.type = type;
			this.value = value;
			this.duration = duration;
			permanent = duration == 0;
		}

		public static CardStatus CloneNew(CardStatus copy)
		{
			return new CardStatus(copy.type, copy.value, copy.duration)
			{
				permanent = copy.permanent
			};
		}

		public static void Clone(CardStatus source, CardStatus dest)
		{
			dest.type = source.type;
			dest.value = source.value;
			dest.duration = source.duration;
			dest.permanent = source.permanent;
		}

		public static void CloneList(List<CardStatus> source, List<CardStatus> dest)
		{
			for (int i = 0; i < source.Count; i++)
			{
				if (i < dest.Count)
				{
					Clone(source[i], dest[i]);
				}
				else
				{
					dest.Add(CloneNew(source[i]));
				}
			}
			if (dest.Count > source.Count)
			{
				dest.RemoveRange(source.Count, dest.Count - source.Count);
			}
		}
	}
}
