using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

namespace TcgEngine.UI
{
	public class LeaderboardPanel : UIPanel
	{
		public RectTransform content;

		public RankLine line_template;

		public RankLine my_line;

		public float line_spacing = 80f;

		public Text test_text;

		private List<RankLine> lines = new List<RankLine>();

		private static LeaderboardPanel instance;

		protected override void Awake()
		{
			base.Awake();
			instance = this;
			RankLine rankLine = my_line;
			rankLine.onClick = (UnityAction<string>)Delegate.Combine(rankLine.onClick, new UnityAction<string>(OnClickLine));
			InitLines();
		}

		private void OnDestroy()
		{
		}

		private void InitLines()
		{
			for (int i = 0; i < content.transform.childCount; i++)
			{
				UnityEngine.Object.Destroy(content.transform.GetChild(i).gameObject);
			}
			int num = 100;
			for (int j = 0; j < num; j++)
			{
				RankLine item = AddLine(line_template, j);
				lines.Add(item);
			}
			content.sizeDelta = new Vector2(content.sizeDelta.x, (float)num * line_spacing + 20f);
		}

		private RankLine AddLine(RankLine template, int index)
		{
			Vector2 vector = Vector2.down * line_spacing;
			GameObject obj = UnityEngine.Object.Instantiate(template.gameObject, content);
			RectTransform component = obj.GetComponent<RectTransform>();
			RankLine component2 = obj.GetComponent<RankLine>();
			component.anchorMin = new Vector2(0.5f, 1f);
			component.anchorMax = new Vector2(0.5f, 1f);
			component.anchoredPosition = vector + Vector2.down * index * line_spacing;
			component2.onClick = (UnityAction<string>)Delegate.Combine(component2.onClick, new UnityAction<string>(OnClickLine));
			return component2;
		}

		private async void RefreshPanel()
		{
			my_line.Hide();
			foreach (RankLine line in lines)
			{
				line.Hide();
			}
			test_text.enabled = !Authenticator.Get().IsApi();
			if (!Authenticator.Get().IsApi())
			{
				return;
			}
			UserData udata = ApiClient.Get().UserData;
			int index = 0;
			string url = ApiClient.ServerURL + "/users";
			List<UserData> list = new List<UserData>(ApiTool.JsonToArray<UserData>((await ApiClient.Get().SendGetRequest(url)).data));
			list.Sort((UserData a, UserData b) => b.elo.CompareTo(a.elo));
			int num = 0;
			int num2 = 0;
			foreach (UserData item in list)
			{
				if (item.permission_level == 1 && item.matches != 0)
				{
					if (item.username == udata.username)
					{
						my_line.SetLine(item, index + 1, highlight: true);
					}
					if (index < lines.Count)
					{
						RankLine rankLine = lines[index];
						int num3 = ((num == item.elo) ? num2 : index);
						rankLine.SetLine(item, num3 + 1, item.username == udata.username);
						num = item.elo;
						num2 = num3;
					}
					index++;
				}
			}
		}

		private void OnClickLine(string username)
		{
		}

		public override void Show(bool instant = false)
		{
			base.Show(instant);
			RefreshPanel();
		}

		public void OnClickBack()
		{
			Hide();
		}

		public static LeaderboardPanel Get()
		{
			return instance;
		}
	}
}
