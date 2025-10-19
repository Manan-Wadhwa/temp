using System;
using System.Collections;
using System.Collections.Generic;
using TcgEngine.Client;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.Events;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace TcgEngine.UI
{
	public class GameUI : MonoBehaviour
	{
		public Canvas game_canvas;

		public Canvas panel_canvas;

		public Canvas top_canvas;

		public UIPanel menu_panel;

		public Text quit_btn;

		[Header("Turn Area")]
		public Text turn_count;

		public Text turn_timer;

		public Button end_turn_button;

		public Animator timeout_animator;

		public AudioClip timeout_audio;

		private float selector_timer;

		private float end_turn_timer;

		private int prev_time_val;

		private static GameUI instance;

		private void Awake()
		{
			instance = this;
			if (game_canvas.worldCamera == null)
			{
				game_canvas.worldCamera = Camera.main;
			}
			if (panel_canvas.worldCamera == null)
			{
				panel_canvas.worldCamera = Camera.main;
			}
			if (top_canvas.worldCamera == null)
			{
				top_canvas.worldCamera = Camera.main;
			}
		}

		private void Start()
		{
			GameClient gameClient = GameClient.Get();
			gameClient.onGameStart = (UnityAction)Delegate.Combine(gameClient.onGameStart, new UnityAction(OnGameStart));
			GameClient gameClient2 = GameClient.Get();
			gameClient2.onNewTurn = (UnityAction<int>)Delegate.Combine(gameClient2.onNewTurn, new UnityAction<int>(OnNewTurn));
			LoadPanel.Get().Show(instant: true);
			BlackPanel.Get().Show(instant: true);
			BlackPanel.Get().Hide();
			if (quit_btn != null)
			{
				quit_btn.text = (GameClient.game_settings.IsOnlinePlayer() ? "Resign" : "Quit");
			}
		}

		private void Update()
		{
			Game gameData = GameClient.Get().GetGameData();
			bool flag = gameData == null || gameData.state == GameState.Connecting;
			bool visible = !flag && !GameClient.Get().IsReady();
			ConnectionPanel.Get().SetVisible(visible);
			if (Input.GetKeyDown(KeyCode.Escape))
			{
				menu_panel.Toggle();
			}
			if (!GameClient.Get().IsReady())
			{
				return;
			}
			bool flag2 = GameClient.Get().IsYourTurn();
			LoadPanel.Get().SetVisible(flag && !gameData.HasStarted());
			end_turn_button.interactable = flag2 && end_turn_timer > 1f;
			end_turn_timer += Time.deltaTime;
			selector_timer += Time.deltaTime;
			turn_count.text = "Turn " + gameData.turn_count;
			turn_timer.enabled = gameData.turn_timer > 0f;
			turn_timer.text = Mathf.RoundToInt(gameData.turn_timer).ToString();
			turn_timer.enabled = gameData.turn_timer < 999f;
			if (gameData.state == GameState.Play && gameData.turn_timer > 0f)
			{
				gameData.turn_timer -= Time.deltaTime;
			}
			if (gameData.state == GameState.Play)
			{
				int num = Mathf.RoundToInt(gameData.turn_timer);
				int num2 = 10;
				if (num < prev_time_val && num <= num2)
				{
					PulseFX();
				}
				prev_time_val = num;
			}
			foreach (SelectorPanel item in SelectorPanel.GetAll())
			{
				bool flag3 = item.ShouldShow();
				if (flag3 != item.IsVisible() && selector_timer > 1f)
				{
					selector_timer = 0f;
					item.SetVisible(flag3);
					if (flag3)
					{
						AbilityData ability = AbilityData.Get(gameData.selector_ability_id);
						Card card = gameData.GetCard(gameData.selector_caster_uid);
						item.Show(ability, card);
					}
				}
			}
			if (!flag2)
			{
				SelectorPanel.HideAll();
			}
		}

		private void PulseFX()
		{
			timeout_animator?.SetTrigger("pulse");
			AudioTool.Get().PlaySFX("time", timeout_audio, 1f);
		}

		private void OnGameStart()
		{
		}

		private void OnNewTurn(int player_id)
		{
			CardSelector.Get().Hide();
			SelectTargetUI.Get().Hide();
		}

		public void OnClickNextTurn()
		{
			GameClient.Get().EndTurn();
			end_turn_timer = 0f;
		}

		public void OnClickRestart()
		{
			SceneManager.LoadScene(SceneManager.GetActiveScene().name);
		}

		public void OnClickMenu()
		{
			menu_panel.Show();
		}

		public void OnClickBack()
		{
			menu_panel.Hide();
		}

		public void OnClickQuit()
		{
			bool num = GameClient.game_settings.IsOnlinePlayer();
			bool flag = GameClient.Get().HasEnded();
			if (num && !flag)
			{
				GameClient.Get().Resign();
			}
			else
			{
				StartCoroutine(QuitRoutine("Menu"));
			}
			menu_panel.Hide();
		}

		private IEnumerator QuitRoutine(string scene)
		{
			BlackPanel.Get().Show();
			AudioTool.Get().FadeOutMusic("music");
			AudioTool.Get().FadeOutSFX("ambience");
			AudioTool.Get().FadeOutSFX("ending_sfx");
			yield return new WaitForSeconds(1f);
			GameClient.Get().Disconnect();
			SceneNav.GoTo(scene);
		}

		public void OnClickSwapObserve()
		{
			int observerMode = ((GameClient.Get().GetPlayerID() == 0) ? 1 : 0);
			GameClient.Get().SetObserverMode(observerMode);
		}

		public static bool IsUIOpened()
		{
			if (!CardSelector.Get().IsVisible())
			{
				return EndGamePanel.Get().IsVisible();
			}
			return true;
		}

		public static bool IsOverUI()
		{
			PointerEventData pointerEventData = new PointerEventData(EventSystem.current);
			pointerEventData.position = new Vector2(Input.mousePosition.x, Input.mousePosition.y);
			List<RaycastResult> list = new List<RaycastResult>();
			EventSystem.current.RaycastAll(pointerEventData, list);
			return list.Count > 0;
		}

		public static bool IsOverUILayer(string sorting_layer)
		{
			return IsOverUILayer(SortingLayer.NameToID(sorting_layer));
		}

		public static bool IsOverUILayer(int sorting_layer)
		{
			PointerEventData pointerEventData = new PointerEventData(EventSystem.current);
			pointerEventData.position = new Vector2(Input.mousePosition.x, Input.mousePosition.y);
			List<RaycastResult> list = new List<RaycastResult>();
			EventSystem.current.RaycastAll(pointerEventData, list);
			int num = 0;
			foreach (RaycastResult item in list)
			{
				if (item.sortingLayer == sorting_layer)
				{
					num++;
				}
			}
			return num > 0;
		}

		public static bool IsOverRectTransform(Canvas canvas, RectTransform rect)
		{
			PointerEventData pointerEventData = new PointerEventData(EventSystem.current);
			pointerEventData.position = Input.mousePosition;
			List<RaycastResult> list = new List<RaycastResult>();
			canvas.GetComponent<GraphicRaycaster>().Raycast(pointerEventData, list);
			foreach (RaycastResult item in list)
			{
				if (item.gameObject.transform == rect || item.gameObject.transform.IsChildOf(rect))
				{
					return true;
				}
			}
			return false;
		}

		public static Vector2 MouseToRectPos(Canvas canvas, RectTransform rect, Vector2 screen_pos)
		{
			if (canvas.renderMode != RenderMode.ScreenSpaceOverlay && canvas.worldCamera != null)
			{
				RectTransformUtility.ScreenPointToLocalPointInRectangle(rect, screen_pos, canvas.worldCamera, out var localPoint);
				return localPoint;
			}
			Vector2 vector = screen_pos - new Vector2(rect.position.x, rect.position.y);
			return new Vector2(vector.x / rect.lossyScale.x, vector.y / rect.lossyScale.y);
		}

		public static Vector3 MouseToWorld(Vector2 mouse_pos, float distance = 10f)
		{
			return ((GameCamera.Get() != null) ? GameCamera.GetCamera() : Camera.main).ScreenToWorldPoint(new Vector3(mouse_pos.x, mouse_pos.y, distance));
		}

		public static string FormatNumber(int value)
		{
			return $"{value:#,0}";
		}

		public static GameUI Get()
		{
			return instance;
		}
	}
}
