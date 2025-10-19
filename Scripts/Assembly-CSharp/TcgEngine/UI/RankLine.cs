using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

namespace TcgEngine.UI
{
	public class RankLine : MonoBehaviour
	{
		public Text ranking;

		public Text player;

		public Text elo_txt;

		public Text winrate_txt;

		public Image highlight;

		public UnityAction<string> onClick;

		private string username;

		private void Start()
		{
			highlight.enabled = false;
		}

		public void SetLine(UserData udata, int ranking, bool highlight)
		{
			username = udata.username;
			this.ranking.text = ranking.ToString();
			player.text = username;
			elo_txt.text = udata.elo.ToString();
			int num = Mathf.RoundToInt((float)udata.victories * 100f / (float)Mathf.Max(udata.matches, 1));
			winrate_txt.text = num + "%";
			this.highlight.enabled = highlight;
			base.gameObject.SetActive(value: true);
		}

		public void Hide()
		{
			base.gameObject.SetActive(value: false);
		}

		public string GetUsername()
		{
			return username;
		}

		public void OnClick()
		{
			onClick?.Invoke(username);
		}
	}
}
