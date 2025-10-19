using System;
using TcgEngine.Client;
using UnityEngine.Events;
using UnityEngine.UI;

namespace TcgEngine.UI
{
	public class LoadPanel : UIPanel
	{
		public Text load_txt;

		private static LoadPanel instance;

		protected override void Awake()
		{
			base.Awake();
			instance = this;
			if (load_txt != null)
			{
				load_txt.text = "";
			}
		}

		protected override void Start()
		{
			base.Start();
			GameClient gameClient = GameClient.Get();
			gameClient.onConnectGame = (UnityAction)Delegate.Combine(gameClient.onConnectGame, new UnityAction(OnConnect));
			GameClient gameClient2 = GameClient.Get();
			gameClient2.onPlayerReady = (UnityAction<int>)Delegate.Combine(gameClient2.onPlayerReady, new UnityAction<int>(OnReady));
			GameClient gameClient3 = GameClient.Get();
			gameClient3.onGameStart = (UnityAction)Delegate.Combine(gameClient3.onGameStart, new UnityAction(OnStart));
			SetLoadText("Connecting to server...");
		}

		private void OnConnect()
		{
			SetLoadText("Sending player data...");
		}

		private void OnStart()
		{
			SetLoadText("");
		}

		private void OnReady(int player_id)
		{
			if (player_id == GameClient.Get().GetPlayerID())
			{
				SetLoadText("Waiting for other player...");
			}
		}

		private void SetLoadText(string text)
		{
			if (IsOnline() && load_txt != null)
			{
				load_txt.text = text;
			}
		}

		public bool IsOnline()
		{
			return GameClient.game_settings.IsOnline();
		}

		public static LoadPanel Get()
		{
			return instance;
		}
	}
}
