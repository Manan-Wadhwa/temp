using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

namespace TcgEngine.UI
{
	public class FriendLine : MonoBehaviour
	{
		public Text username;

		public Image avatar;

		public Image online_img;

		public Sprite online_sprite;

		public Sprite offline_sprite;

		public Button accept_btn;

		public Button reject_btn;

		public Button watch_btn;

		public Button challenge_btn;

		public UnityAction<FriendLine> onClick;

		public UnityAction<FriendLine> onClickAccept;

		public UnityAction<FriendLine> onClickReject;

		public UnityAction<FriendLine> onClickWatch;

		public UnityAction<FriendLine> onClickChallenge;

		private FriendData fdata;

		private Sprite default_avat;

		private void Awake()
		{
			default_avat = avatar.sprite;
			if (accept_btn != null)
			{
				accept_btn.onClick.AddListener(delegate
				{
					onClickAccept?.Invoke(this);
				});
			}
			if (reject_btn != null)
			{
				reject_btn.onClick.AddListener(delegate
				{
					onClickReject?.Invoke(this);
				});
			}
			if (watch_btn != null)
			{
				watch_btn.onClick.AddListener(delegate
				{
					onClickWatch?.Invoke(this);
				});
			}
			if (challenge_btn != null)
			{
				challenge_btn.onClick.AddListener(delegate
				{
					onClickChallenge?.Invoke(this);
				});
			}
		}

		public void SetLine(FriendData user, bool online, bool is_request = false)
		{
			fdata = user;
			username.text = user.username;
			avatar.sprite = default_avat;
			if (avatar != null)
			{
				AvatarData avatarData = AvatarData.Get(user.avatar);
				if (avatarData != null)
				{
					avatar.sprite = avatarData.avatar;
				}
			}
			if (online_img != null)
			{
				online_img.sprite = (online ? online_sprite : offline_sprite);
			}
			if (watch_btn != null)
			{
				watch_btn.gameObject.SetActive(online && !is_request);
			}
			if (challenge_btn != null)
			{
				challenge_btn.gameObject.SetActive(online && !is_request);
			}
			if (accept_btn != null)
			{
				accept_btn.gameObject.SetActive(is_request);
			}
			if (reject_btn != null)
			{
				reject_btn.gameObject.SetActive(is_request);
			}
			base.gameObject.SetActive(value: true);
		}

		public void Hide()
		{
			base.gameObject.SetActive(value: false);
		}

		public void OnClick()
		{
			onClick?.Invoke(this);
		}

		public FriendData GetFriend()
		{
			return fdata;
		}
	}
}
