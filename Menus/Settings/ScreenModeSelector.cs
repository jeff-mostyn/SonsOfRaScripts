using System.Collections;
using System.Collections.Generic;
using System;
using UnityEngine;
using UnityEngine.UI;
using System.Linq;

public class ScreenModeSelector : menuOptionSelector
{
    [SerializeField]private FullScreenMode[] screenModes = new FullScreenMode[3];
    public FullScreenMode SelectedMode;
    [SerializeField]private int SelectedModeIndex;
    [SerializeField] private int selectionIndex, maxIndex, minIndex;

    new void Awake()
    {
        base.Awake();
    }

    new void Update()
    {
        base.Update();
    }

    public override void Initialize()
    {
        screenModes[0] = FullScreenMode.ExclusiveFullScreen;
        screenModes[1] = FullScreenMode.FullScreenWindow;
        screenModes[2] = FullScreenMode.Windowed;

        SelectedMode = SettingsManager.Instance.GetFullscreen();
        SelectedModeIndex = Array.IndexOf(screenModes, SelectedMode);
        selectionIndex = SelectedModeIndex;
        UpdateText();
    }

    public override void IncrementOption() {        
        if (CanSwitchOptions())
        {
            if (selectionIndex + 1 <= (screenModes.Length - 1))
            {
                selectionIndex += 1;
                SelectedModeIndex = selectionIndex;                
                SelectedMode = screenModes[selectionIndex];
                UpdateText();
            }
            else
            {
                selectionIndex = 0;
                SelectedModeIndex = selectionIndex;                
                SelectedMode = screenModes[selectionIndex];
                UpdateText();
            }
        }
        Debug.Log("Selected mode: " + screenModes[SelectedModeIndex].ToString());
        StartCoroutine(arrowScaler(rightArrow));
        ResetSwitchCooldown();
    }

    public override void DecrementOption()
    {
        if (CanSwitchOptions())
        {
            if (selectionIndex - 1 >= 0)
            {
                selectionIndex -= 1;
                SelectedModeIndex = selectionIndex;                
                SelectedMode = screenModes[selectionIndex];
                UpdateText();
            }
            else
            {
                selectionIndex = screenModes.Length - 1;
                SelectedModeIndex = selectionIndex;                
                SelectedMode = screenModes[selectionIndex];
                UpdateText();
            }
        }
        Debug.Log("Selected mode: " + screenModes[SelectedModeIndex].ToString());
        StartCoroutine(arrowScaler(rightArrow));
        ResetSwitchCooldown();
    }

    public void UpdateText() {
        try {
            displayText.SetText(Lang.FullscreenOptions[SelectedMode][SettingsManager.Instance.language]);
        }
        catch { //in case it fails
			displayText.SetText(screenModes[SelectedModeIndex].ToString());
        }       
    }

    public FullScreenMode GetMode() {
        return SelectedMode;
    }

    public override T GetValue<T>()
    {
        return (T)Convert.ChangeType(SelectedModeIndex, typeof(T));
    }
}
