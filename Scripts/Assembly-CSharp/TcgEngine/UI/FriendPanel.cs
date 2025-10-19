using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

namespace TcgEngine.UI
{
	public class FriendPanel : UIPanel
	{
		public ScrollRect friend_scroll;

		public RectTransform friend_content;

		public FriendLine line_prefab;

		public InputField friend_input;

		public TabButton friends_tab;

		public TabButton requests_tab;

		public int online_duration = 10;

		public Text test_msg;

		public Text error;

		private List<FriendLine> friend_lines = new List<FriendLine>();

		private static FriendPanel instance;

		protected override void Awake()
		{
			base.Awake();
			instance = this;
			InitLines();
		}

		protected override void Start()
		{
			base.Start();
			TabButton tabButton = friends_tab;
			tabButton.onClick = (UnityAction)Delegate.Combine(tabButton.onClick, new UnityAction(RefreshPanel));
			TabButton tabButton2 = requests_tab;
			tabButton2.onClick = (UnityAction)Delegate.Combine(tabButton2.onClick, new UnityAction(RefreshPanel));
		}

		private void InitLines()
		{
			int num = 100;
			for (int i = 0; i < num; i++)
			{
				FriendLine friendLine = AddLine(line_prefab, i);
				friendLine.Hide();
				friend_lines.Add(friendLine);
			}
			friend_scroll.verticalNormalizedPosition = 1f;
		}

		private FriendLine AddLine(FriendLine template, int index)
		{
			GameObject obj = UnityEngine.Object.Instantiate(template.gameObject, friend_content);
			obj.GetComponent<RectTransform>();
			FriendLine component = obj.GetComponent<FriendLine>();
			component.onClick = (UnityAction<FriendLine>)Delegate.Combine(component.onClick, new UnityAction<FriendLine>(OnClickFriendLine));
			component.onClickAccept = (UnityAction<FriendLine>)Delegate.Combine(component.onClickAccept, new UnityAction<FriendLine>(OnClickFriendAccept));
			component.onClickReject = (UnityAction<FriendLine>)Delegate.Combine(component.onClickReject, new UnityAction<FriendLine>(OnClickFriendReject));
			component.onClickWatch = (UnityAction<FriendLine>)Delegate.Combine(component.onClickWatch, new UnityAction<FriendLine>(OnClickFriendWatch));
			component.onClickChallenge = (UnityAction<FriendLine>)Delegate.Combine(component.onClickChallenge, new UnityAction<FriendLine>(OnClickFriendChallenge));
			return component;
		}

		private async void RefreshPanel()
		{
			foreach (FriendLine friend_line in friend_lines)
			{
				friend_line.Hide();
			}
			if (test_msg != null)
			{
				test_msg.enabled = Authenticator.Get().IsTest();
			}
			if (!Authenticator.Get().IsApi())
			{
				return;
			}
			string url = ApiClient.ServerURL + "/users/friends/list";
			WebResponse webResponse = await ApiClient.Get().SendGetRequest(url);
			if (webResponse.success)
			{
				FriendResponse friendResponse = ApiTool.JsonToObject<FriendResponse>(webResponse.data);
				if (friends_tab.active)
				{
					SetFriends(friendResponse);
				}
				else if (requests_tab.active)
				{
					SetRequests(friendResponse);
				}
			}
		}

		private void SetFriends(FriendResponse contract_list)
		{
			DateTime dateTime = DateTime.Parse(contract_list.server_time).AddMinutes(-online_duration);
			int num = 0;
			FriendData[] friends = contract_list.friends;
			for (int i = 0; i < friends.Length; i++)
			{
				FriendData user = friends[i];
				if (num < friend_lines.Count)
				{
					friend_lines[num].SetLine(online: DateTime.TryParse(user.last_online_time, out var result) && result > dateTime, user: user);
				}
				num++;
			}
		}

		private void SetRequests(FriendResponse contract_list)
		{
			DateTime dateTime = DateTime.Parse(contract_list.server_time).AddMinutes(-10.0);
			int num = 0;
			FriendData[] friends_requests = contract_list.friends_requests;
			for (int i = 0; i < friends_requests.Length; i++)
			{
				FriendData user = friends_requests[i];
				if (num < friend_lines.Count)
				{
					friend_lines[num].SetLine(online: DateTime.TryParse(user.last_online_time, out var result) && result > dateTime, user: user, is_request: true);
				}
				num++;
			}
		}

		private async void AddFriend(string fuser)
		{
			FriendAddRequest friendAddRequest = new FriendAddRequest
			{
				username = fuser
			};
			string url = ApiClient.ServerURL + "/users/friends/add";
			string json_data = ApiTool.ToJson(friendAddRequest);
			WebResponse webResponse = await ApiClient.Get().SendPostRequest(url, json_data);
			if (webResponse.success)
			{
				RefreshPanel();
			}
			else
			{
				error.text = webResponse.error;
			}
		}

		private async void RemoveFriend(string fuser)
		{
			FriendAddRequest friendAddRequest = new FriendAddRequest
			{
				username = fuser
			};
			string url = ApiClient.ServerURL + "/users/friends/remove";
			string json_data = ApiTool.ToJson(friendAddRequest);
			WebResponse webResponse = await ApiClient.Get().SendPostRequest(url, json_data);
			if (webResponse.success)
			{
				RefreshPanel();
			}
			else
			{
				error.text = webResponse.error;
			}
		}

		public void OnClickBack()
		{
			Hide();
		}

		private void OnClickFriendLine(FriendLine user)
		{
		}

		private void OnClickFriendAccept(FriendLine user)
		{
			AddFriend(user.GetFriend().username);
		}

		private void OnClickFriendReject(FriendLine user)
		{
			RemoveFriend(user.GetFriend().username);
		}

		private void OnClickFriendWatch(FriendLine user)
		{
			FriendData friend = user.GetFriend();
			MainMenu.Get().StartObserve(friend.username);
		}

		private void OnClickFriendChallenge(FriendLine user)
		{
			FriendData friend = user.GetFriend();
			MainMenu.Get().StartChallenge(friend.username);
		}

		public void OnClickAddFriend()
		{
			string text = friend_input.text;
			if (!string.IsNullOrWhiteSpace(text))
			{
				error.text = "";
				AddFriend(text);
			}
		}

		public void OnClickRemoveFriend()
		{
			string text = friend_input.text;
			if (!string.IsNullOrWhiteSpace(text))
			{
				error.text = "";
				RemoveFriend(text);
			}
		}

		public override void Show(bool instant = false)
		{
			base.Show(instant);
			error.text = "";
			friend_input.text = "";
			friends_tab.Activate();
			RefreshPanel();
		}

		public static FriendPanel Get()
		{
			return instance;
		}
	}
}
