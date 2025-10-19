using System.Collections.Generic;
using TcgEngine.UI;
using UnityEngine;

namespace TcgEngine.Client
{
	public class PackCard : MonoBehaviour
	{
		public float move_speed = 5f;

		public float flip_speed = 10f;

		public SpriteRenderer cardback;

		public CardUI card_ui;

		public GameObject new_card;

		[Header("FX")]
		public GameObject card_flip_fx;

		public GameObject card_rare_flip_fx;

		public AudioClip card_flip_audio;

		public AudioClip card_rare_flip_audio;

		private CardData icard;

		private VariantData variant;

		private Vector3 target;

		private Quaternion rtarget;

		private bool revealed;

		private bool removed;

		private bool is_new;

		private float timer;

		private static List<PackCard> card_list = new List<PackCard>();

		private void Awake()
		{
			card_list.Add(this);
		}

		private void OnDestroy()
		{
			card_list.Remove(this);
		}

		private void Update()
		{
			base.transform.position = Vector3.MoveTowards(base.transform.position, target, move_speed * Time.deltaTime);
			if (revealed)
			{
				timer += Time.deltaTime;
				base.transform.rotation = Quaternion.Slerp(base.transform.rotation, rtarget, flip_speed * Time.deltaTime);
			}
			if (removed && timer > 4f)
			{
				Object.Destroy(base.gameObject);
			}
		}

		public void SetCard(PackData pack, CardData card, VariantData variant)
		{
			icard = card;
			this.variant = variant;
			if (cardback != null)
			{
				cardback.sprite = pack.cardback_img;
			}
			card_ui.SetCard(card, variant);
			new_card?.SetActive(value: false);
			UserData userData = Authenticator.Get().GetUserData();
			is_new = !userData.HasCard(icard.id, variant.id);
		}

		public void SetTarget(Vector3 pos)
		{
			target = pos;
			rtarget = Quaternion.Euler(0f, 180f, 0f);
			base.transform.rotation = rtarget;
		}

		public void Reveal()
		{
			if (!revealed)
			{
				revealed = true;
				rtarget = Quaternion.Euler(0f, 0f, 0f);
				new_card?.SetActive(is_new);
				if (icard != null && icard.rarity.rank >= 3)
				{
					FXTool.DoFX(card_rare_flip_fx, base.transform.position);
					AudioTool.Get().PlaySFX("pack_open", card_rare_flip_audio);
				}
				else
				{
					FXTool.DoFX(card_flip_fx, base.transform.position);
					AudioTool.Get().PlaySFX("pack_open", card_flip_audio);
				}
			}
		}

		public void Remove()
		{
			if (!removed)
			{
				removed = true;
				timer = 0f;
				target = Vector3.up * 10f;
			}
		}

		public void OnMouseDown()
		{
			if (!GameUI.IsOverUILayer("UI"))
			{
				Reveal();
			}
		}

		public bool IsRevealed()
		{
			if (revealed)
			{
				return timer > 0.5f;
			}
			return false;
		}

		public static List<PackCard> GetAll()
		{
			return card_list;
		}
	}
}
