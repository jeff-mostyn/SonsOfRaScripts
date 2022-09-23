using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[Serializable()]
public class SettingsRecording
{
	public float musicVolume;
	public float fxVolume;
	public int resX, resY;
	public int qualitySetting;
	public FullScreenMode fsMode;
	public int frameRate;
	public int language;

	public void Initialize(int _resx, int _resy, FullScreenMode screenMode, Lang.language _language) {
		musicVolume = 50f;
		fxVolume = 50f;
		resX = _resx;
		resY = _resy;
		//qualitySetting = QualitySettings.GetQualityLevel();
		qualitySetting = 3;
		fsMode = screenMode;
		frameRate = (int)SettingsManager.targetFrameRates.Uncapped;
		language = (int)_language;
	}
}
