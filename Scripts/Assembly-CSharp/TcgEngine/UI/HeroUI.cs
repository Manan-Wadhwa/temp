using System.Collections.Generic;
using TcgEngine.Client;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace TcgEngine.UI
{
	public class HeroUI : MonoBehaviour
	{
		public bool opponent;

		public GameObject power_area;

		public Button power_button;

		public Image power_image;

		public GameObject power_mana_slot;

		public Text power_mana;

		public Material active_mat;

		public Material inactive_mat;

		private bool focus;

		private static List<HeroUI> ui_list = new List<HeroUI>();

		private void Awake()
		{
			ui_list.Add(this);
		}

		private void OnDestroy()
		{
			ui_list.Remove(this);
		}

		private void Start()
		{
			power_area.SetActive(value: false);
			if (power_button != null)
			{
				power_button.onClick.AddListener(OnClickPower);
			}
			EventTrigger component = power_area.GetComponent<EventTrigger>();
			EventTrigger.Entry entry = new EventTrigger.Entry();
			entry.eventID = EventTriggerType.PointerEnter;
			entry.callback.AddListener(delegate
			{
				OnEnterMouse();
			});
			EventTrigger.Entry entry2 = new EventTrigger.Entry();
			entry2.eventID = EventTriggerType.PointerExit;
			entry2.callback.AddListener(delegate
			{
				OnExitMouse();
			});
			component.triggers.Add(entry);
			component.triggers.Add(entry2);
		}

		private void Update()
		{
			if (!GameClient.Get().IsReady())
			{
				return;
			}
			Game gameData = GameClient.Get().GetGameData();
			Player player = GetPlayer();
			Card hero = player.hero;
			if (hero != null)
			{
				AbilityData ability = hero.GetAbility(AbilityTrigger.Activate);
				if (ability != null)
				{
					power_image.sprite = hero.CardData.GetBoardArt(hero.VariantData);
					power_image.material = ((!hero.exhausted) ? active_mat : inactive_mat);
					power_mana_slot?.SetActive(gameData.IsPlayerTurn(player) && !hero.exhausted);
					power_mana.text = ability.mana_cost.ToString();
				}
				if (power_button != null)
				{
					power_button.interactable = ability != null && !hero.exhausted && gameData.IsPlayerTurn(player);
				}
				if (hero != null && !power_area.activeSelf)
				{
					power_area.SetActive(value: true);
				}
			}
		}

		public void OnClickPower()
		{
			Game gameData = GameClient.Get().GetGameData();
			Player player = GameClient.Get().GetPlayer();
			Card hero = player.hero;
			AbilityData abilityData = hero?.GetAbility(AbilityTrigger.Activate);
			if (abilityData != null && !opponent)
			{
				if (!hero.exhausted && !player.CanPayAbility(hero, abilityData))
				{
					WarningText.ShowNoMana();
				}
				else if (gameData.IsPlayerActionTurn(player) && gameData.CanCastAbility(hero, abilityData))
				{
					GameClient.Get().CastAbility(hero, abilityData);
				}
			}
		}

		private void OnEnterMouse()
		{
			focus = true;
		}

		private void OnExitMouse()
		{
			focus = false;
		}

		private void OnDisable()
		{
			focus = false;
		}

		public bool IsFocus()
		{
			return focus;
		}

		public int GetPlayerID()
		{
			if (!opponent)
			{
				return GameClient.Get().GetPlayerID();
			}
			return GameClient.Get().GetOpponentPlayerID();
		}

		public Player GetPlayer()
		{
			return GameClient.Get().GetGameData().GetPlayer(GetPlayerID());
		}

		public Card GetCard()
		{
			return GetPlayer().hero;
		}

		public static HeroUI GetFocus()
		{
			foreach (HeroUI item in ui_list)
			{
				if (item.IsFocus())
				{
					return item;
				}
			}
			return null;
		}

		public static HeroUI Get(bool opponent)
		{
			foreach (HeroUI item in ui_list)
			{
				if (item.opponent == opponent)
				{
					return item;
				}
			}
			return null;
		}

		public static HeroUI Get(int player_id)
		{
			return Get(player_id != GameClient.Get().GetPlayerID());
		}
	}
}
