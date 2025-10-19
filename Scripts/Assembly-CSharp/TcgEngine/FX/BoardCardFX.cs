using System;
using System.Collections;
using System.Collections.Generic;
using TcgEngine.Client;
using UnityEngine;
using UnityEngine.Events;

namespace TcgEngine.FX
{
	public class BoardCardFX : MonoBehaviour
	{
		public Material kill_mat;

		public string kill_mat_fade = "noise_fade";

		private BoardCard bcard;

		private ParticleSystem exhausted_fx;

		private Dictionary<StatusType, GameObject> status_fx_list = new Dictionary<StatusType, GameObject>();

		private void Awake()
		{
			bcard = GetComponent<BoardCard>();
			BoardCard boardCard = bcard;
			boardCard.onKill = (UnityAction)Delegate.Combine(boardCard.onKill, new UnityAction(OnKill));
		}

		private void Start()
		{
			GameClient gameClient = GameClient.Get();
			gameClient.onCardMoved = (UnityAction<Card, Slot>)Delegate.Combine(gameClient.onCardMoved, new UnityAction<Card, Slot>(OnMove));
			gameClient.onAttackStart = (UnityAction<Card, Card>)Delegate.Combine(gameClient.onAttackStart, new UnityAction<Card, Card>(OnAttack));
			gameClient.onAttackPlayerStart = (UnityAction<Card, Player>)Delegate.Combine(gameClient.onAttackPlayerStart, new UnityAction<Card, Player>(OnAttackPlayer));
			gameClient.onAbilityStart = (UnityAction<AbilityData, Card>)Delegate.Combine(gameClient.onAbilityStart, new UnityAction<AbilityData, Card>(OnAbilityStart));
			gameClient.onAbilityTargetCard = (UnityAction<AbilityData, Card, Card>)Delegate.Combine(gameClient.onAbilityTargetCard, new UnityAction<AbilityData, Card, Card>(OnAbilityEffect));
			gameClient.onAbilityEnd = (UnityAction<AbilityData, Card>)Delegate.Combine(gameClient.onAbilityEnd, new UnityAction<AbilityData, Card>(OnAbilityAfter));
			OnSpawn();
		}

		private void OnDestroy()
		{
			GameClient gameClient = GameClient.Get();
			gameClient.onCardMoved = (UnityAction<Card, Slot>)Delegate.Remove(gameClient.onCardMoved, new UnityAction<Card, Slot>(OnMove));
			gameClient.onAttackStart = (UnityAction<Card, Card>)Delegate.Remove(gameClient.onAttackStart, new UnityAction<Card, Card>(OnAttack));
			gameClient.onAttackPlayerStart = (UnityAction<Card, Player>)Delegate.Remove(gameClient.onAttackPlayerStart, new UnityAction<Card, Player>(OnAttackPlayer));
			gameClient.onAbilityStart = (UnityAction<AbilityData, Card>)Delegate.Remove(gameClient.onAbilityStart, new UnityAction<AbilityData, Card>(OnAbilityStart));
			gameClient.onAbilityTargetCard = (UnityAction<AbilityData, Card, Card>)Delegate.Remove(gameClient.onAbilityTargetCard, new UnityAction<AbilityData, Card, Card>(OnAbilityEffect));
			gameClient.onAbilityEnd = (UnityAction<AbilityData, Card>)Delegate.Remove(gameClient.onAbilityEnd, new UnityAction<AbilityData, Card>(OnAbilityAfter));
		}

		private void Update()
		{
			if (!GameClient.Get().IsReady())
			{
				return;
			}
			Card card = bcard.GetCard();
			foreach (CardStatus item in card.GetAllStatus())
			{
				StatusData statusData = StatusData.Get(item.type);
				if (statusData != null && !status_fx_list.ContainsKey(item.type) && statusData.status_fx != null)
				{
					GameObject gameObject = UnityEngine.Object.Instantiate(statusData.status_fx, base.transform);
					gameObject.transform.localPosition = Vector3.zero;
					status_fx_list[statusData.effect] = gameObject;
				}
			}
			List<StatusType> list = new List<StatusType>();
			foreach (KeyValuePair<StatusType, GameObject> item2 in status_fx_list)
			{
				if (!card.HasStatus(item2.Key))
				{
					list.Add(item2.Key);
					UnityEngine.Object.Destroy(item2.Value);
				}
			}
			foreach (StatusType item3 in list)
			{
				status_fx_list.Remove(item3);
			}
			if (exhausted_fx != null && !exhausted_fx.isPlaying && card.exhausted)
			{
				exhausted_fx.Play();
			}
			if (exhausted_fx != null && exhausted_fx.isPlaying && !card.exhausted)
			{
				exhausted_fx.Stop();
			}
		}

		private void OnSpawn()
		{
			CardData icard = bcard.GetCardData();
			AudioClip sound = ((icard?.spawn_audio != null) ? icard.spawn_audio : AssetData.Get().card_spawn_audio);
			AudioTool.Get().PlaySFX("card_spawn", sound);
			FXTool.DoFX((icard.spawn_fx != null) ? icard.spawn_fx : AssetData.Get().card_spawn_fx, base.transform.position);
			if (GameTool.IsURP())
			{
				bcard.card_sprite.material = kill_mat;
				FadeSetVal(bcard.card_sprite, 0f);
				FadeKill(bcard.card_sprite, 1f, 0.5f);
			}
			if (AssetData.Get().card_exhausted_fx != null)
			{
				GameObject gameObject = UnityEngine.Object.Instantiate(AssetData.Get().card_exhausted_fx, base.transform);
				gameObject.transform.localPosition = Vector3.zero;
				exhausted_fx = gameObject.GetComponent<ParticleSystem>();
			}
			TimeTool.WaitFor(1f, delegate
			{
				if (icard.idle_fx != null)
				{
					UnityEngine.Object.Instantiate(icard.idle_fx, base.transform).transform.localPosition = Vector3.zero;
				}
			});
		}

		private void OnKill()
		{
			StartCoroutine(KillRoutine());
		}

		private IEnumerator KillRoutine()
		{
			yield return new WaitForSeconds(0.5f);
			CardData cardData = bcard.GetCardData();
			FXTool.DoFX((cardData.death_fx != null) ? cardData.death_fx : AssetData.Get().card_destroy_fx, base.transform.position);
			AudioClip sound = ((cardData?.death_audio != null) ? cardData.death_audio : AssetData.Get().card_destroy_audio);
			AudioTool.Get().PlaySFX("card_spawn", sound);
			if (GameTool.IsURP())
			{
				FadeKill(bcard.card_sprite, 0f, 0.5f);
			}
		}

		private void FadeSetVal(SpriteRenderer render, float val)
		{
			render.material = kill_mat;
			render.material.SetFloat(kill_mat_fade, val);
		}

		private void FadeKill(SpriteRenderer render, float val, float duration)
		{
			AnimMatFX.Create(render.gameObject, render.material).SetFloat(kill_mat_fade, val, duration);
		}

		private void OnMove(Card card, Slot slot)
		{
			AudioTool.Get().PlaySFX("card_move", AssetData.Get().card_move_audio);
		}

		private void OnAttack(Card attacker, Card target)
		{
			Card card = bcard.GetCard();
			CardData cardData = bcard.GetCardData();
			if (attacker == null || target == null)
			{
				return;
			}
			if (card.uid == attacker.uid)
			{
				BoardCard boardCard = BoardCard.Get(target.uid);
				if (boardCard != null)
				{
					ChargeInto(boardCard);
					if (!attacker.HasStatus(StatusType.Intimidate))
					{
						DamageFX(target, attacker, base.transform);
					}
					FXTool.DoSnapFX((cardData.attack_fx != null) ? cardData.attack_fx : AssetData.Get().card_attack_fx, base.transform);
					AudioClip sound = ((cardData?.attack_audio != null) ? cardData.attack_audio : AssetData.Get().card_attack_audio);
					AudioTool.Get().PlaySFX("card_attack", sound);
				}
			}
			if (card.uid == target.uid && (target.CardData.IsCharacter() || card == target))
			{
				DamageFX(attacker, target, base.transform);
			}
		}

		private void OnAttackPlayer(Card attacker, Player player)
		{
			if (attacker != null && player != null && bcard.GetCard().uid == attacker.uid)
			{
				bool opponent = player.player_id != GameClient.Get().GetPlayerID();
				CardData cardData = bcard.GetCardData();
				BoardSlotPlayer boardSlotPlayer = BoardSlotPlayer.Get(opponent);
				ChargeIntoPlayer(boardSlotPlayer);
				AudioClip sound = ((cardData?.attack_audio != null) ? cardData.attack_audio : AssetData.Get().card_attack_audio);
				AudioTool.Get().PlaySFX("card_attack", sound);
				int attack = bcard.GetCard().GetAttack();
				DamageFX(boardSlotPlayer.transform, attack);
			}
		}

		private void DamageFX(Card attacker, Card target, Transform target_trans, float delay = 0.5f)
		{
			if (!target.HasStatus(StatusType.Invincibility))
			{
				int attack = attacker.GetAttack();
				attack = Mathf.Max(attack - target.GetStatusValue(StatusType.Armor), 0);
				DamageFX(target_trans, attack, delay);
			}
		}

		private void DamageFX(Transform target, int value, float delay = 0.5f)
		{
			TimeTool.WaitFor(delay, delegate
			{
				FXTool.DoFX(AssetData.Get().damage_fx, target.position).GetComponent<DamageFX>().SetValue(value);
			});
		}

		private void ChargeInto(BoardCard target)
		{
			if (target != null)
			{
				ChargeInto(target.gameObject);
				CardData icard = target.GetCardData();
				TimeTool.WaitFor(0.25f, delegate
				{
					GameObject fx_prefab = (icard.damage_fx ? icard.damage_fx : AssetData.Get().card_damage_fx);
					AudioClip sound = (icard.damage_audio ? icard.damage_audio : AssetData.Get().card_damage_audio);
					FXTool.DoFX(fx_prefab, target.transform.position);
					AudioTool.Get().PlaySFX("card_hit", sound);
				});
			}
		}

		private void ChargeIntoPlayer(BoardSlotPlayer target)
		{
			if (target != null)
			{
				ChargeInto(target.gameObject);
				TimeTool.WaitFor(0.25f, delegate
				{
					FXTool.DoFX(AssetData.Get().player_damage_fx, target.transform.position);
					AudioClip player_damage_audio = AssetData.Get().player_damage_audio;
					AudioTool.Get().PlaySFX("card_hit", player_damage_audio);
				});
			}
		}

		private void ChargeInto(GameObject target)
		{
			if (!(target != null))
			{
				return;
			}
			int current_order = bcard.card_sprite.sortingOrder;
			Vector3 vector = target.transform.position - base.transform.position;
			_ = target.transform.position - vector.normalized * 1f;
			Vector3 position = base.transform.position;
			bcard.SetOrder(current_order + 10);
			AnimFX animFX = AnimFX.Create(base.gameObject);
			animFX.MoveTo(position - vector.normalized * 0.5f, 0.3f);
			animFX.MoveTo(target.transform.position, 0.1f);
			animFX.MoveTo(position, 0.3f);
			animFX.Callback(0f, delegate
			{
				if (bcard != null)
				{
					bcard.SetOrder(current_order);
				}
			});
		}

		private void OnAbilityStart(AbilityData iability, Card caster)
		{
			if (iability != null && caster != null && caster.uid == bcard.GetCardUID())
			{
				FXTool.DoSnapFX(iability.caster_fx, bcard.transform);
				AudioTool.Get().PlaySFX("ability", iability.cast_audio);
			}
		}

		private void OnAbilityAfter(AbilityData iability, Card caster)
		{
			if (iability != null && caster != null)
			{
				_ = caster.uid == bcard.GetCardUID();
			}
		}

		private void OnAbilityEffect(AbilityData iability, Card caster, Card target)
		{
			if (iability != null && caster != null && target != null)
			{
				if (target.uid == bcard.GetCardUID())
				{
					FXTool.DoSnapFX(iability.target_fx, bcard.transform);
					AudioTool.Get().PlaySFX("ability_effect", iability.target_audio);
				}
				if (caster.uid == bcard.GetCardUID() && iability.charge_target && caster.CardData.IsBoardCard())
				{
					BoardCard target2 = BoardCard.Get(target.uid);
					ChargeInto(target2);
				}
			}
		}
	}
}
