using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.Events;
using UnityEngine.UI;

namespace TcgEngine.UI
{
	public class PackUI : MonoBehaviour, IPointerClickHandler, IEventSystemHandler
	{
		public Image pack_img;

		public Text pack_title;

		public Text pack_quantity;

		public Image quantity_bar;

		public UnityAction<PackUI> onClick;

		public UnityAction<PackUI> onClickRight;

		private PackData pack;

		private void Awake()
		{
		}

		public void SetPack(PackData pack)
		{
			this.pack = pack;
			if (pack != null)
			{
				if (pack_title != null)
				{
					pack_title.enabled = true;
					pack_title.text = pack.title;
				}
				pack_img.enabled = true;
				pack_img.sprite = pack.pack_img;
			}
			if (pack_quantity != null)
			{
				pack_quantity.enabled = false;
			}
			if (quantity_bar != null)
			{
				quantity_bar.enabled = false;
			}
		}

		public void SetPack(PackData pack, int quantity)
		{
			SetPack(pack);
			if (pack_quantity != null)
			{
				pack_quantity.enabled = quantity > 0;
				pack_quantity.text = quantity.ToString();
			}
			if (quantity_bar != null)
			{
				quantity_bar.enabled = quantity > 0;
			}
		}

		public void Hide()
		{
			pack = null;
			pack_img.enabled = false;
			if (pack_title != null)
			{
				pack_title.enabled = false;
			}
			if (pack_quantity != null)
			{
				pack_quantity.enabled = false;
			}
			if (quantity_bar != null)
			{
				quantity_bar.enabled = false;
			}
		}

		public void OnPointerClick(PointerEventData eventData)
		{
			if (eventData.button == PointerEventData.InputButton.Left && onClick != null)
			{
				onClick(this);
			}
			if (eventData.button == PointerEventData.InputButton.Right && onClickRight != null)
			{
				onClickRight(this);
			}
		}

		public PackData GetPack()
		{
			return pack;
		}
	}
}
