using UnityEngine;
using UnityEngine.UI;

namespace TcgEngine.UI
{
	public class TraitUI : MonoBehaviour
	{
		public TraitData trait;

		public Image bg;

		public Text text;

		private void Start()
		{
		}

		public void SetCard(Card card)
		{
			bool flag = card.HasTrait(trait);
			int traitValue = card.GetTraitValue(trait);
			text.text = traitValue.ToString();
			bg.enabled = flag;
			text.enabled = flag;
		}

		public void SetCard(CardData card)
		{
			bool flag = card.HasTrait(trait);
			int stat = card.GetStat(trait.id);
			text.text = stat.ToString();
			bg.enabled = flag;
			text.enabled = flag;
		}
	}
}
