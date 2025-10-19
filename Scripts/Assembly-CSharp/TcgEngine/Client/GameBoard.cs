using System.Collections;
using System.Collections.Generic;
using TcgEngine.UI;
using UnityEngine;
using UnityEngine.Events;

namespace TcgEngine.Client
{
	public class GameBoard : MonoBehaviour
	{
		public GameObject card_prefab;

		public UnityAction<Card> onCardSpawned;

		public UnityAction<Card> onCardKilled;

		private bool game_ended;

		private static GameBoard _instance;

		private void Awake()
		{
			_instance = this;
		}

		private void Start()
		{
		}

		private void Update()
		{
			if (!GameClient.Get().IsReady())
			{
				return;
			}
			Game gameData = GameClient.Get().GetGameData();
			List<BoardCard> all = BoardCard.GetAll();
			Player[] players = gameData.players;
			for (int i = 0; i < players.Length; i++)
			{
				foreach (Card item in players[i].cards_board)
				{
					BoardCard boardCard = BoardCard.Get(item.uid);
					if (item != null && boardCard == null)
					{
						SpawnNewCard(item);
					}
				}
			}
			for (int num = all.Count - 1; num >= 0; num--)
			{
				BoardCard boardCard2 = all[num];
				if (boardCard2 != null && HasBoardCard(boardCard2))
				{
					KillCard(boardCard2);
				}
			}
			if (!game_ended && gameData.state == GameState.GameEnded)
			{
				game_ended = true;
				EndGame();
			}
		}

		private void SpawnNewCard(Card card)
		{
			GameObject obj = Object.Instantiate(card_prefab, Vector3.zero, Quaternion.identity);
			obj.SetActive(value: true);
			obj.GetComponent<BoardCard>().SetCard(card);
			onCardSpawned?.Invoke(card);
		}

		private void KillCard(BoardCard card)
		{
			card.Kill();
			onCardKilled?.Invoke(card.GetCard());
		}

		private bool HasBoardCard(BoardCard bcard)
		{
			if (GameClient.Get().GetGameData().GetBoardCard(bcard.GetCardUID()) == null)
			{
				return !bcard.IsDead();
			}
			return false;
		}

		public void EndGame()
		{
			StartCoroutine(EndGameRun());
		}

		private IEnumerator EndGameRun()
		{
			Game data = GameClient.Get().GetGameData();
			Player player = data.GetPlayer(data.current_player);
			Player player2 = GameClient.Get().GetPlayer();
			bool win = player != null && player2.player_id == player.player_id;
			bool tied = player == null;
			AudioTool.Get().FadeOutMusic("music");
			yield return new WaitForSeconds(1f);
			if (win)
			{
				PlayerUI.Get(opponent: true).Kill();
			}
			if (!win && !tied)
			{
				PlayerUI.Get(opponent: false).Kill();
			}
			if (win && AssetData.Get().win_fx != null)
			{
				Object.Instantiate(AssetData.Get().win_fx, Vector3.zero, Quaternion.identity);
			}
			else if (tied && AssetData.Get().tied_fx != null)
			{
				Object.Instantiate(AssetData.Get().tied_fx, Vector3.zero, Quaternion.identity);
			}
			else if (tied && AssetData.Get().lose_fx != null)
			{
				Object.Instantiate(AssetData.Get().lose_fx, Vector3.zero, Quaternion.identity);
			}
			if (win)
			{
				AudioTool.Get().PlaySFX("ending_sfx", AssetData.Get().win_audio);
			}
			else
			{
				AudioTool.Get().PlaySFX("ending_sfx", AssetData.Get().defeat_audio);
			}
			if (win)
			{
				AudioTool.Get().PlayMusic("music", AssetData.Get().win_music, 0.4f, loop: false);
			}
			else
			{
				AudioTool.Get().PlayMusic("music", AssetData.Get().defeat_music, 0.4f, loop: false);
			}
			yield return new WaitForSeconds(2f);
			EndGamePanel.Get().Show(data.current_player);
		}

		public Vector3 RaycastMouseBoard()
		{
			Ray ray = GameCamera.Get().MouseToRay(Input.mousePosition);
			if (new Plane(base.transform.forward, 0f).Raycast(ray, out var enter))
			{
				return ray.GetPoint(enter);
			}
			return Vector3.zero;
		}

		public Vector3 GetAngles()
		{
			return base.transform.rotation.eulerAngles;
		}

		public static GameBoard Get()
		{
			return _instance;
		}
	}
}
