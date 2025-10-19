using TcgEngine.Client;
using UnityEngine;
using UnityEngine.UI;

namespace TcgEngine.UI
{
	public class LevelUI : MonoBehaviour
	{
		[Header("Level")]
		public LevelData level;

		[Header("UI")]
		public Text title;

		public Text subtitle;

		public DeckDisplay deck;

		public GameObject completed;

		private void Start()
		{
			GetComponent<Button>().onClick.AddListener(OnClick);
			completed.SetActive(value: false);
			if (level != null)
			{
				SetLevel(level);
			}
			else
			{
				Hide();
			}
		}

		public void SetLevel(LevelData level)
		{
			this.level = level;
			RefreshLevel();
		}

		public void RefreshLevel()
		{
			if (level != null)
			{
				title.text = level.title;
				subtitle.text = "LEVEL " + level.level;
				deck.SetDeck(level.player_deck);
				base.gameObject.SetActive(value: true);
				UserData userData = Authenticator.Get().GetUserData();
				if (userData != null)
				{
					completed.SetActive(userData.HasReward(level.id));
				}
			}
		}

		public void Hide()
		{
			base.gameObject.SetActive(value: false);
		}

		public void OnClick()
		{
			if (level != null)
			{
				GameClient.game_settings.level = level.id;
				GameClient.game_settings.scene = level.scene;
				GameClient.player_settings.deck = new UserDeckData(level.player_deck);
				GameClient.ai_settings.deck = new UserDeckData(level.ai_deck);
				GameClient.ai_settings.ai_level = level.ai_level;
				MainMenu.Get().StartGame(GameType.Adventure, GameMode.Casual);
			}
		}
	}
}
