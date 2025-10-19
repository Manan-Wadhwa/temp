using System.Collections.Generic;
using UnityEngine;

namespace TcgEngine
{
	public class AudioTool : MonoBehaviour
	{
		private static AudioTool instance;

		private Dictionary<string, AudioSource> channels_sfx = new Dictionary<string, AudioSource>();

		private Dictionary<string, AudioSource> channels_music = new Dictionary<string, AudioSource>();

		private Dictionary<string, float> channels_volume = new Dictionary<string, float>();

		private Dictionary<string, float> tchannels_volume = new Dictionary<string, float>();

		[HideInInspector]
		public float master_vol = 1f;

		[HideInInspector]
		public float sfx_vol = 1f;

		[HideInInspector]
		public float music_vol = 1f;

		private void Awake()
		{
			LoadPrefs();
			RefreshVolume();
		}

		private void Update()
		{
			foreach (KeyValuePair<string, AudioSource> item in channels_music)
			{
				if (item.Value.isPlaying)
				{
					float num = tchannels_volume[item.Key];
					float current2 = channels_volume[item.Key];
					current2 = Mathf.MoveTowards(current2, num, 0.5f * Time.deltaTime);
					channels_volume[item.Key] = current2;
					item.Value.volume = current2 * music_vol;
					if (current2 < 0.01f && num < 0.01f)
					{
						StopMusic(item.Key);
					}
				}
			}
			foreach (KeyValuePair<string, AudioSource> item2 in channels_sfx)
			{
				if (item2.Value.isPlaying)
				{
					float num2 = tchannels_volume[item2.Key];
					float current4 = channels_volume[item2.Key];
					current4 = Mathf.MoveTowards(current4, num2, 0.5f * Time.deltaTime);
					channels_volume[item2.Key] = current4;
					item2.Value.volume = current4 * sfx_vol;
					if (current4 < 0.01f && num2 < 0.01f)
					{
						StopSFX(item2.Key);
					}
				}
			}
		}

		public void PlaySFX(string channel, AudioClip sound, float vol = 0.6f, bool priority = true, bool loop = false)
		{
			if (!string.IsNullOrEmpty(channel) && !(sound == null))
			{
				AudioSource audioSource = GetChannel(channel);
				channels_volume[channel] = vol;
				tchannels_volume[channel] = vol;
				if (audioSource == null)
				{
					audioSource = CreateChannel(channel);
					channels_sfx[channel] = audioSource;
				}
				if ((bool)audioSource && (priority || !audioSource.isPlaying))
				{
					audioSource.clip = sound;
					audioSource.volume = vol * sfx_vol;
					audioSource.loop = loop;
					audioSource.Play();
				}
			}
		}

		public void PlayMusic(string channel, AudioClip music, float vol = 0.3f, bool loop = true)
		{
			if (!string.IsNullOrEmpty(channel) && !(music == null))
			{
				AudioSource audioSource = GetMusicChannel(channel);
				channels_volume[channel] = vol;
				tchannels_volume[channel] = vol;
				if (audioSource == null)
				{
					audioSource = CreateChannel(channel);
					channels_music[channel] = audioSource;
				}
				if ((bool)audioSource && (!audioSource.isPlaying || audioSource.clip != music))
				{
					audioSource.clip = music;
					audioSource.volume = vol * music_vol;
					audioSource.loop = loop;
					audioSource.Play();
				}
			}
		}

		public void PlaySFX(string channel, AudioClip[] sounds, float vol = 0.6f, bool priority = true, bool loop = false)
		{
			if (sounds != null && sounds.Length != 0)
			{
				AudioClip sound = sounds[Random.Range(0, sounds.Length)];
				PlaySFX(channel, sound, vol, priority, loop);
			}
		}

		public void PlayMusic(string channel, AudioClip[] musics, float vol = 0.6f, bool priority = true, bool loop = false)
		{
			if (musics != null && musics.Length != 0)
			{
				AudioClip music = musics[Random.Range(0, musics.Length)];
				PlayMusic(channel, music, vol, loop);
			}
		}

		public void StopSFX(string channel)
		{
			if (!string.IsNullOrEmpty(channel))
			{
				AudioSource channel2 = GetChannel(channel);
				if ((bool)channel2)
				{
					channel2.Stop();
				}
			}
		}

		public void StopMusic(string channel)
		{
			if (!string.IsNullOrEmpty(channel))
			{
				AudioSource musicChannel = GetMusicChannel(channel);
				if ((bool)musicChannel)
				{
					musicChannel.Stop();
				}
			}
		}

		public void FadeOutMusic(string channel)
		{
			if (tchannels_volume.ContainsKey(channel))
			{
				tchannels_volume[channel] = 0f;
			}
		}

		public void FadeOutSFX(string channel)
		{
			if (tchannels_volume.ContainsKey(channel))
			{
				tchannels_volume[channel] = 0f;
			}
		}

		public void SetMasterVolume(float value)
		{
			master_vol = value;
			RefreshVolume();
			SavePrefs();
		}

		public void SetMusicVolume(float value)
		{
			music_vol = value;
			RefreshVolume();
			SavePrefs();
		}

		public void SetSFXVolume(float value)
		{
			sfx_vol = value;
			RefreshVolume();
			SavePrefs();
		}

		public void LoadPrefs()
		{
			master_vol = PlayerPrefs.GetFloat("audio_master_volume", 1f);
			music_vol = PlayerPrefs.GetFloat("audio_music_volume", 1f);
			sfx_vol = PlayerPrefs.GetFloat("audio_sfx_volume", 1f);
		}

		public void SavePrefs()
		{
			PlayerPrefs.SetFloat("audio_master_volume", master_vol);
			PlayerPrefs.SetFloat("audio_music_volume", music_vol);
			PlayerPrefs.SetFloat("audio_sfx_volume", sfx_vol);
		}

		public void RefreshVolume()
		{
			AudioListener.volume = master_vol;
			foreach (KeyValuePair<string, AudioSource> item in channels_sfx)
			{
				if (item.Value != null)
				{
					float num = (channels_volume.ContainsKey(item.Key) ? channels_volume[item.Key] : 0.8f);
					item.Value.volume = num * sfx_vol;
				}
			}
			foreach (KeyValuePair<string, AudioSource> item2 in channels_music)
			{
				if (item2.Value != null)
				{
					float num2 = (channels_volume.ContainsKey(item2.Key) ? channels_volume[item2.Key] : 0.4f);
					item2.Value.volume = num2 * music_vol;
				}
			}
		}

		public bool IsMusicPlaying(string channel)
		{
			AudioSource musicChannel = GetMusicChannel(channel);
			if (musicChannel != null)
			{
				return musicChannel.isPlaying;
			}
			return false;
		}

		public AudioSource CreateChannel(string channel, int priority = 128)
		{
			if (string.IsNullOrEmpty(channel))
			{
				return null;
			}
			GameObject obj = new GameObject("AudioChannel-" + channel);
			obj.transform.SetParent(base.transform);
			AudioSource audioSource = obj.AddComponent<AudioSource>();
			audioSource.playOnAwake = false;
			audioSource.loop = false;
			audioSource.priority = priority;
			return audioSource;
		}

		public AudioSource GetChannel(string channel)
		{
			if (channels_sfx.ContainsKey(channel))
			{
				return channels_sfx[channel];
			}
			return null;
		}

		public AudioSource GetMusicChannel(string channel)
		{
			if (channels_music.ContainsKey(channel))
			{
				return channels_music[channel];
			}
			return null;
		}

		public bool DoesChannelExist(string channel)
		{
			return channels_sfx.ContainsKey(channel);
		}

		public bool DoesMusicChannelExist(string channel)
		{
			return channels_music.ContainsKey(channel);
		}

		public float GetMasterVolume()
		{
			return master_vol;
		}

		public float GetSFXVolume()
		{
			return sfx_vol;
		}

		public float GetMusicVolume()
		{
			return music_vol;
		}

		public static AudioTool Get()
		{
			if (instance == null)
			{
				GameObject obj = new GameObject("AudioSystem");
				instance = obj.AddComponent<AudioTool>();
				Object.DontDestroyOnLoad(obj);
			}
			return instance;
		}
	}
}
