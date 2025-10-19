using System;
using System.Collections.Generic;
using TcgEngine.Client;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

namespace TcgEngine.UI
{
	public class ChatUI : MonoBehaviour
	{
		public bool is_opponent;

		[Header("Display Box")]
		public ChatBubble chat_bubble;

		public AudioClip chat_audio;

		[Header("Write Box")]
		public UIPanel chat_field_area;

		public InputField chat_field;

		private string chat_msg;

		private float chat_timer;

		private static List<ChatUI> ui_list = new List<ChatUI>();

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
			GameClient gameClient = GameClient.Get();
			gameClient.onChatMsg = (UnityAction<int, string>)Delegate.Combine(gameClient.onChatMsg, new UnityAction<int, string>(OnChat));
			RefreshChat();
		}

		private void Update()
		{
			if (!GameClient.Get().IsReady())
			{
				return;
			}
			int id = (is_opponent ? GameClient.Get().GetOpponentPlayerID() : GameClient.Get().GetPlayerID());
			if (GameClient.Get().GetGameData().GetPlayer(id) == null)
			{
				return;
			}
			if (chat_field_area != null && !is_opponent && Input.GetKeyDown(KeyCode.Return))
			{
				if (chat_field_area.IsVisible())
				{
					if (!string.IsNullOrWhiteSpace(chat_field.text))
					{
						SendChat(chat_field.text);
					}
					chat_field.text = "";
					chat_field_area.Hide();
					GUI.FocusControl(null);
				}
				else
				{
					chat_field_area.Show();
				}
				chat_field.ActivateInputField();
				chat_field.Select();
			}
			chat_timer += Time.deltaTime;
			if (chat_timer > 5f)
			{
				chat_msg = null;
			}
		}

		private void SendChat(string msg)
		{
			GameClient.Get().SendChatMsg(msg);
		}

		private void RefreshChat()
		{
			chat_bubble.Hide();
			if (!string.IsNullOrWhiteSpace(chat_msg))
			{
				chat_bubble.SetLine(chat_msg, 5f);
			}
		}

		private void OnChat(int chat_player_id, string msg)
		{
			if ((is_opponent ? GameClient.Get().GetOpponentPlayerID() : GameClient.Get().GetPlayerID()) == chat_player_id)
			{
				chat_msg = msg;
				chat_timer = 0f;
				AudioTool.Get().PlaySFX("chat", chat_audio);
				RefreshChat();
			}
		}

		public void OnClickSend()
		{
			if (chat_field_area != null && !string.IsNullOrWhiteSpace(chat_field.text))
			{
				SendChat(chat_field.text);
				chat_field.text = "";
				chat_field_area.Hide();
				GUI.FocusControl(null);
			}
		}

		public static ChatUI Get(bool opponent)
		{
			foreach (ChatUI item in ui_list)
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
