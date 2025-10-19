using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

namespace TcgEngine.UI
{
	public class PackPanel : UIPanel
	{
		[Header("Packs")]
		public ScrollRect scroll_rect;

		public RectTransform scroll_content;

		public CardGrid grid_content;

		public GameObject pack_prefab;

		private List<GameObject> pack_list = new List<GameObject>();

		private static PackPanel instance;

		protected override void Awake()
		{
			base.Awake();
			instance = this;
			for (int i = 0; i < grid_content.transform.childCount; i++)
			{
				UnityEngine.Object.Destroy(grid_content.transform.GetChild(i).gameObject);
			}
		}

		protected override void Start()
		{
			base.Start();
		}

		protected override void Update()
		{
			base.Update();
		}

		public async void ReloadUserPack()
		{
			await Authenticator.Get().LoadUserData();
			RefreshPacks();
		}

		private void RefreshAll()
		{
			RefreshPacks();
			RefreshStarterDeck();
		}

		public void RefreshPacks()
		{
			UserData userData = Authenticator.Get().UserData;
			foreach (GameObject item in pack_list)
			{
				UnityEngine.Object.Destroy(item.gameObject);
			}
			pack_list.Clear();
			foreach (PackData item2 in PackData.GetAllAvailable())
			{
				GameObject gameObject = UnityEngine.Object.Instantiate(pack_prefab, grid_content.transform);
				PackUI componentInChildren = gameObject.GetComponentInChildren<PackUI>();
				componentInChildren.SetPack(item2, userData.GetPackQuantity(item2.id));
				componentInChildren.onClick = (UnityAction<PackUI>)Delegate.Combine(componentInChildren.onClick, new UnityAction<PackUI>(OnClickPack));
				componentInChildren.onClickRight = (UnityAction<PackUI>)Delegate.Combine(componentInChildren.onClickRight, new UnityAction<PackUI>(OnClickPack));
				pack_list.Add(gameObject);
			}
		}

		private void RefreshStarterDeck()
		{
			UserData userData = Authenticator.Get().UserData;
			if (userData.cards.Length == 0 || userData.rewards.Length == 0)
			{
				StarterDeckPanel.Get().Show();
			}
		}

		public void OnClickPack(PackUI pack)
		{
			PackZoomPanel.Get().ShowPack(pack.GetPack());
		}

		public void OnClickCardRight(PackUI pack)
		{
			PackZoomPanel.Get().ShowPack(pack.GetPack());
		}

		public void OnClickOpenPacks()
		{
			MainMenu.Get().FadeToScene("OpenPack");
		}

		public override void Show(bool instant = false)
		{
			base.Show(instant);
			RefreshAll();
		}

		public static PackPanel Get()
		{
			return instance;
		}
	}
}
