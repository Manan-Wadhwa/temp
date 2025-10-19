using System;
using TcgEngine.Client;
using TcgEngine.UI;
using UnityEngine;
using UnityEngine.Events;

namespace TcgEngine.FX
{
	public class GameBoardFX : MonoBehaviour
	{
		private void Start()
		{
			GameClient gameClient = GameClient.Get();
			gameClient.onNewTurn = (UnityAction<int>)Delegate.Combine(gameClient.onNewTurn, new UnityAction<int>(OnNewTurn));
			gameClient.onCardPlayed = (UnityAction<Card, Slot>)Delegate.Combine(gameClient.onCardPlayed, new UnityAction<Card, Slot>(OnPlayCard));
			gameClient.onAbilityStart = (UnityAction<AbilityData, Card>)Delegate.Combine(gameClient.onAbilityStart, new UnityAction<AbilityData, Card>(OnAbility));
			gameClient.onSecretTrigger = (UnityAction<Card, Card>)Delegate.Combine(gameClient.onSecretTrigger, new UnityAction<Card, Card>(OnSecret));
			gameClient.onValueRolled = (UnityAction<int>)Delegate.Combine(gameClient.onValueRolled, new UnityAction<int>(OnRoll));
		}

		private void OnNewTurn(int player_id)
		{
			AudioTool.Get().PlaySFX("turn", AssetData.Get().new_turn_audio);
			FXTool.DoFX(AssetData.Get().new_turn_fx, Vector3.zero);
		}

		private void OnPlayCard(Card card, Slot slot)
		{
			int playerID = GameClient.Get().GetPlayerID();
			if (card != null)
			{
				CardData cardData = CardData.Get(card.card_id);
				if (cardData.type == CardType.Spell)
				{
					FXTool.DoFX((playerID == card.player_id) ? AssetData.Get().play_card_fx : AssetData.Get().play_card_other_fx, Vector3.zero).GetComponentInChildren<CardUI>().SetCard(cardData, card.VariantData);
					AudioClip sound = ((cardData.spawn_audio != null) ? cardData.spawn_audio : AssetData.Get().card_spawn_audio);
					AudioTool.Get().PlaySFX("card_spell", sound);
				}
				if (cardData.type == CardType.Secret)
				{
					FXTool.DoFX((playerID == card.player_id) ? AssetData.Get().play_secret_fx : AssetData.Get().play_secret_other_fx, Vector3.zero);
					AudioClip sound2 = ((cardData.spawn_audio != null) ? cardData.spawn_audio : AssetData.Get().card_spawn_audio);
					AudioTool.Get().PlaySFX("card_spell", sound2);
				}
			}
		}

		private void OnAbility(AbilityData iability, Card caster)
		{
			if (iability != null)
			{
				FXTool.DoFX(iability.board_fx, Vector3.zero);
			}
		}

		private void OnSecret(Card secret, Card triggerer)
		{
			CardData cardData = CardData.Get(secret.card_id);
			if (cardData?.attack_audio != null)
			{
				AudioTool.Get().PlaySFX("card_secret", cardData.attack_audio);
			}
		}

		private void OnRoll(int value)
		{
			DiceRollFX diceRollFX = FXTool.DoFX(AssetData.Get().dice_roll_fx, Vector3.zero)?.GetComponent<DiceRollFX>();
			if (diceRollFX != null)
			{
				diceRollFX.value = value;
			}
		}
	}
}
