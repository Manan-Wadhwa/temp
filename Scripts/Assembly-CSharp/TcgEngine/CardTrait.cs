using System;
using System.Collections.Generic;

namespace TcgEngine
{
	[Serializable]
	public class CardTrait
	{
		public string id;

		public int value;

		[NonSerialized]
		private TraitData data;

		public TraitData TraitData
		{
			get
			{
				if (data == null || data.id != id)
				{
					data = TraitData.Get(id);
				}
				return data;
			}
		}

		public TraitData Data => TraitData;

		public CardTrait(string id, int value)
		{
			this.id = id;
			this.value = value;
		}

		public CardTrait(TraitData trait, int value)
		{
			id = trait.id;
			this.value = value;
		}

		public static CardTrait CloneNew(CardTrait copy)
		{
			return new CardTrait(copy.id, copy.value);
		}

		public static void Clone(CardTrait source, CardTrait dest)
		{
			dest.id = source.id;
			dest.value = source.value;
		}

		public static void CloneList(List<CardTrait> source, List<CardTrait> dest)
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
