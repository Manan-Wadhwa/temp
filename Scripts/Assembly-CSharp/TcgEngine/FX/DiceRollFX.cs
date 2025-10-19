using UnityEngine;

namespace TcgEngine.FX
{
	public class DiceRollFX : MonoBehaviour
	{
		public int value;

		[Header("Anim")]
		public Transform dice;

		public float roll_speed = 20f;

		public float roll_duration = 1f;

		public AudioClip start_audio;

		public AudioClip end_audio;

		private Vector3[] dir;

		private bool ended;

		private float timer;

		private float x;

		private float y;

		private float z;

		private void Start()
		{
			dir = new Vector3[6];
			dir[0] = Vector3.forward;
			dir[1] = Vector3.up;
			dir[2] = Vector3.right;
			dir[3] = Vector3.left;
			dir[4] = Vector3.down;
			dir[5] = Vector3.back;
			AudioTool.Get().PlaySFX("dice", start_audio);
		}

		private void Update()
		{
			timer += Time.deltaTime;
			if (!ended)
			{
				if (timer < roll_duration)
				{
					x += 5f * Time.deltaTime;
					y += 7f * Time.deltaTime;
					dice.Rotate(x * roll_speed, y * roll_speed, z * roll_speed, Space.Self);
				}
				else
				{
					ended = true;
					timer = 0f;
					AudioTool.Get().PlaySFX("dice", end_audio);
				}
			}
			if (ended)
			{
				if (value >= 1 && value <= dir.Length)
				{
					Vector3 forward = dir[value - 1];
					Vector3 upwards = ((forward.y > forward.z) ? Vector3.back : Vector3.up);
					Quaternion b = Quaternion.LookRotation(forward, upwards);
					dice.localRotation = Quaternion.Slerp(dice.localRotation, b, roll_speed * Time.deltaTime);
				}
				if (timer > 1f)
				{
					Object.Destroy(base.gameObject);
				}
			}
		}
	}
}
