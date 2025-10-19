using System;
using System.Collections.Generic;
using TcgEngine.Client;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

namespace TcgEngine.UI
{
	public class PlayerUI : MonoBehaviour
	{
		public bool is_opponent;

		public Text pname;

		public AvatarUI avatar;

		public IconBar mana_bar;

		public Text hp_txt;

		public Text hp_max_txt;

		public Animator[] secrets;

		public GameObject dead_fx;

		public AudioClip dead_audio;

		public Sprite avatar_dead;

		private bool killed;

		private float timer;

		private static List<PlayerUI> ui_list = new List<PlayerUI>();

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
			pname.text = "";
			hp_txt.text = "";
			hp_max_txt.text = "";
			for (int i = 0; i < secrets.Length; i++)
			{
				secrets[i].gameObject.SetActive(value: false);
			}
			AvatarUI avatarUI = avatar;
			avatarUI.onClick = (UnityAction<AvatarData>)Delegate.Combine(avatarUI.onClick, new UnityAction<AvatarData>(OnClickAvatar));
			GameClient gameClient = GameClient.Get();
			gameClient.onSecretTrigger = (UnityAction<Card, Card>)Delegate.Combine(gameClient.onSecretTrigger, new UnityAction<Card, Card>(OnSecretTrigger));
		}

		private void Update()
		{
			if (!GameClient.Get().IsReady())
			{
				return;
			}
			Player player = GetPlayer();
			if (player != null)
			{
				pname.text = player.username;
				mana_bar.value = player.mana;
				mana_bar.max_value = player.mana_max;
				hp_txt.text = player.hp.ToString();
				hp_max_txt.text = "/" + player.hp_max;
				AvatarData avatarData = AvatarData.Get(player.avatar);
				if (avatar != null && avatarData != null && !killed)
				{
					avatar.SetAvatar(avatarData);
				}
			}
			timer += Time.deltaTime;
			if (timer > 0.4f)
			{
				timer = 0f;
				SlowUpdate();
			}
		}

		private void SlowUpdate()
		{
			Player player = GetPlayer();
			if (player == null)
			{
				return;
			}
			for (int i = 0; i < secrets.Length; i++)
			{
				bool flag = i < player.cards_secret.Count;
				bool activeSelf = secrets[i].gameObject.activeSelf;
				if (flag != activeSelf)
				{
					secrets[i].gameObject.SetActive(flag);
				}
				if (flag && !activeSelf)
				{
					secrets[i].SetTrigger("appear");
				}
				if (!flag && activeSelf)
				{
					secrets[i].Rebind();
				}
			}
		}

		public void Kill()
		{
			killed = true;
			avatar.SetImage(avatar_dead);
			AudioTool.Get().PlaySFX("fx", dead_audio);
			FXTool.DoFX(dead_fx, avatar.transform.position);
		}

		private void OnClickAvatar(AvatarData avatar)
		{
			Game gameData = GameClient.Get().GetGameData();
			int playerID = GameClient.Get().GetPlayerID();
			if (gameData.selector == SelectorType.SelectTarget && playerID == gameData.selector_player_id)
			{
				GameClient.Get().SelectPlayer(GetPlayer());
			}
		}

		private void OnSecretTrigger(Card secret, Card triggerer)
		{
			Player player = GetPlayer();
			int num = player.cards_secret.Count - 1;
			if (player.player_id == secret.player_id && num >= 0 && num < secrets.Length)
			{
				secrets[num].SetTrigger("reveal");
			}
		}

		public Player GetPlayer()
		{
			int id = (is_opponent ? GameClient.Get().GetOpponentPlayerID() : GameClient.Get().GetPlayerID());
			return GameClient.Get().GetGameData().GetPlayer(id);
		}

		public static PlayerUI Get(bool opponent)
		{
			foreach (PlayerUI item in ui_list)
			{
				if (item.is_opponent == opponent)
				{
					return item;
				}
			}
			return null;
		}
	}
}
