using System;
using TcgEngine.Client;
using UnityEngine.Events;

namespace TcgEngine.UI
{
	public class ChoiceSelector : SelectorPanel
	{
		public ChoiceSelectorChoice[] choices;

		private Card caster;

		private AbilityData ability;

		private static ChoiceSelector instance;

		protected override void Awake()
		{
			base.Awake();
			instance = this;
		}

		protected override void Start()
		{
			base.Start();
			ChoiceSelectorChoice[] array = choices;
			foreach (ChoiceSelectorChoice obj in array)
			{
				obj.onClick = (UnityAction<int>)Delegate.Combine(obj.onClick, new UnityAction<int>(OnClickChoice));
			}
		}

		protected override void Update()
		{
			base.Update();
			Game gameData = GameClient.Get().GetGameData();
			if (gameData != null && gameData.selector == SelectorType.None)
			{
				Hide();
			}
		}

		public void RefreshPanel()
		{
			if (ability == null)
			{
				return;
			}
			ChoiceSelectorChoice[] array = choices;
			for (int i = 0; i < array.Length; i++)
			{
				array[i].Hide();
			}
			Game gameData = GameClient.Get().GetGameData();
			GameClient.Get().GetPlayer();
			int num = 0;
			AbilityData[] chain_abilities = ability.chain_abilities;
			foreach (AbilityData abilityData in chain_abilities)
			{
				if (abilityData != null && num < choices.Length)
				{
					ChoiceSelectorChoice obj = choices[num];
					obj.SetChoice(num, abilityData);
					obj.SetInteractable(gameData.CanSelectAbility(caster, abilityData));
					num++;
				}
			}
		}

		public void OnClickChoice(int index)
		{
			if (GameClient.Get().GetGameData().selector == SelectorType.SelectorChoice)
			{
				GameClient.Get().SelectChoice(index);
				Hide();
			}
			else
			{
				Hide();
			}
		}

		public void OnClickCancel()
		{
			GameClient.Get().CancelSelection();
			Hide();
		}

		public override void Show(AbilityData iability, Card caster)
		{
			this.caster = caster;
			ability = iability;
			Show();
		}

		public override void Show(bool instant = false)
		{
			base.Show(instant);
			RefreshPanel();
		}

		public override bool ShouldShow()
		{
			Game gameData = GameClient.Get().GetGameData();
			int playerID = GameClient.Get().GetPlayerID();
			if (gameData.selector == SelectorType.SelectorChoice)
			{
				return gameData.selector_player_id == playerID;
			}
			return false;
		}

		public static ChoiceSelector Get()
		{
			return instance;
		}
	}
}
