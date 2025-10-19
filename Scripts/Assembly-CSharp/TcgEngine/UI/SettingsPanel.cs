using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

namespace TcgEngine.UI
{
	public class SettingsPanel : UIPanel
	{
		public string tab_group;

		public SliderDrag master_vol;

		public SliderDrag music_vol;

		public SliderDrag sfx_vol;

		public SliderDrag quality;

		public SliderDrag resolution;

		public Toggle windowed;

		public Text master_vol_txt;

		public Text music_vol_txt;

		public Text sfx_vol_txt;

		public Text quality_txt;

		public Text resolution_txt;

		public static HashSet<string> reso_hash = new HashSet<string>();

		public static List<Resolution> resolutions = new List<Resolution>();

		private bool refreshing;

		private static SettingsPanel instance;

		protected override void Awake()
		{
			base.Awake();
			instance = this;
		}

		protected override void Start()
		{
			base.Start();
			master_vol.minValue = 0f;
			master_vol.maxValue = 100f;
			music_vol.minValue = 0f;
			music_vol.maxValue = 100f;
			sfx_vol.minValue = 0f;
			sfx_vol.maxValue = 100f;
			quality.minValue = 0f;
			resolution.minValue = 0f;
			SliderDrag sliderDrag = master_vol;
			sliderDrag.onValueChanged = (UnityAction)Delegate.Combine(sliderDrag.onValueChanged, new UnityAction(RefreshText));
			SliderDrag sliderDrag2 = music_vol;
			sliderDrag2.onValueChanged = (UnityAction)Delegate.Combine(sliderDrag2.onValueChanged, new UnityAction(RefreshText));
			SliderDrag sliderDrag3 = sfx_vol;
			sliderDrag3.onValueChanged = (UnityAction)Delegate.Combine(sliderDrag3.onValueChanged, new UnityAction(RefreshText));
			SliderDrag sliderDrag4 = quality;
			sliderDrag4.onValueChanged = (UnityAction)Delegate.Combine(sliderDrag4.onValueChanged, new UnityAction(RefreshText));
			SliderDrag sliderDrag5 = resolution;
			sliderDrag5.onValueChanged = (UnityAction)Delegate.Combine(sliderDrag5.onValueChanged, new UnityAction(RefreshText));
			SliderDrag sliderDrag6 = master_vol;
			sliderDrag6.onEndDrag = (UnityAction)Delegate.Combine(sliderDrag6.onEndDrag, new UnityAction(OnChangeAudio));
			SliderDrag sliderDrag7 = music_vol;
			sliderDrag7.onEndDrag = (UnityAction)Delegate.Combine(sliderDrag7.onEndDrag, new UnityAction(OnChangeAudio));
			SliderDrag sliderDrag8 = sfx_vol;
			sliderDrag8.onEndDrag = (UnityAction)Delegate.Combine(sliderDrag8.onEndDrag, new UnityAction(OnChangeAudio));
			SliderDrag sliderDrag9 = quality;
			sliderDrag9.onEndDrag = (UnityAction)Delegate.Combine(sliderDrag9.onEndDrag, new UnityAction(OnChangeQuality));
			SliderDrag sliderDrag10 = resolution;
			sliderDrag10.onEndDrag = (UnityAction)Delegate.Combine(sliderDrag10.onEndDrag, new UnityAction(OnChangeResolution));
			windowed.onValueChanged.AddListener(OnChangeWindowed);
			Resolution[] array = Screen.resolutions;
			for (int i = 0; i < array.Length; i++)
			{
				Resolution item = array[i];
				string item2 = item.width + "x" + item.height;
				if (!reso_hash.Contains(item2))
				{
					resolutions.Add(item);
					reso_hash.Add(item2);
				}
			}
			quality.maxValue = QualitySettings.names.Length - 1;
			resolution.maxValue = resolutions.Count - 1;
			foreach (TabButton item3 in TabButton.GetAll(tab_group))
			{
				item3.onClick = (UnityAction)Delegate.Combine(item3.onClick, new UnityAction(OnClickTab));
			}
		}

		private void RefreshPanel()
		{
			refreshing = true;
			master_vol.value = AudioTool.Get().master_vol * 100f;
			music_vol.value = AudioTool.Get().music_vol * 100f;
			sfx_vol.value = AudioTool.Get().sfx_vol * 100f;
			int qualityLevel = QualitySettings.GetQualityLevel();
			int resolutionIndex = GetResolutionIndex();
			bool isOn = !Screen.fullScreen;
			quality.value = qualityLevel;
			resolution.value = resolutionIndex;
			windowed.isOn = isOn;
			refreshing = false;
			RefreshText();
		}

		private void RefreshText()
		{
			master_vol_txt.text = master_vol.value.ToString();
			music_vol_txt.text = music_vol.value.ToString();
			sfx_vol_txt.text = sfx_vol.value.ToString();
			int num = Mathf.RoundToInt(quality.value);
			quality_txt.text = QualitySettings.names[num];
			resolution_txt.text = "";
			int index = Mathf.RoundToInt(this.resolution.value);
			if (resolutions.Count > 0)
			{
				Resolution resolution = resolutions[index];
				string text = resolution.width + "x" + resolution.height + " " + Screen.currentResolution.refreshRate + "Hz";
				resolution_txt.text = text;
			}
		}

		private void OnChangeAudio()
		{
			if (!refreshing)
			{
				AudioTool.Get().master_vol = master_vol.value / 100f;
				AudioTool.Get().sfx_vol = sfx_vol.value / 100f;
				AudioTool.Get().music_vol = music_vol.value / 100f;
				AudioTool.Get().RefreshVolume();
				AudioTool.Get().SavePrefs();
				RefreshText();
			}
		}

		private void OnChangeQuality()
		{
			if (!refreshing)
			{
				QualitySettings.SetQualityLevel(Mathf.RoundToInt(quality.value));
				RefreshText();
			}
		}

		private void OnChangeResolution()
		{
			if (!refreshing && resolutions.Count > 0)
			{
				int index = Mathf.RoundToInt(this.resolution.value);
				Resolution resolution = resolutions[index];
				Screen.SetResolution(resolution.width, resolution.height, !windowed.isOn);
				RefreshText();
			}
		}

		private void OnChangeWindowed(bool val)
		{
			OnChangeResolution();
		}

		private void OnClickTab()
		{
			Hide();
		}

		public void OnClickOK()
		{
			Hide();
		}

		private int GetResolutionIndex()
		{
			int num = 99999;
			int result = 0;
			for (int i = 0; i < resolutions.Count; i++)
			{
				Resolution resolution = resolutions[i];
				int num2 = Mathf.Abs(resolution.height - Screen.height) + Mathf.Abs(resolution.width - Screen.width);
				if (num2 < num)
				{
					num = num2;
					result = i;
				}
			}
			return result;
		}

		public override void Show(bool instant = false)
		{
			base.Show(instant);
			RefreshPanel();
		}

		public override void Hide(bool instant = false)
		{
			base.Hide(instant);
		}

		public static SettingsPanel Get()
		{
			return instance;
		}
	}
}
