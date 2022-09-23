using System.Collections;
using System.Collections.Generic;
using System;
using UnityEngine;
using UnityEngine.UI;
using System.Linq;

public class resolutionSelector : menuOptionSelector
{
	public Resolution[] supportedResolutions;
    public int selectedResolution;
    public int selectionIndex, maxIndex, minIndex;

    new void Awake() {
		base.Awake();
	}

	new void Update() {
		base.Update();
	}

	public override void Initialize() {
		//Debug.Log("current resolution: " + Screen.currentResolution.ToString());
		supportedResolutions = Screen.resolutions;
		//Debug.Log("supported resolutions count: " + supportedResolutions.Length);

		List<Resolution> supportedResolutionsList = supportedResolutions.ToList();

		//foreach(Resolution r in supportedResolutionsList) {
		//	Debug.Log(r.ToString());
		//}

		//selectionIndex = supportedResolutionsList.IndexOf(Screen.currentResolution);
		Resolution presetRes = Screen.currentResolution;
		presetRes.height = SettingsManager.Instance.GetResolution(1);
		presetRes.width = SettingsManager.Instance.GetResolution(0);
		selectionIndex = supportedResolutionsList.IndexOf(presetRes);
		if (selectionIndex == -1) {
			List<Resolution> likeResolutions = supportedResolutionsList.FindAll(r => r.width == Screen.currentResolution.width && r.height == Screen.currentResolution.height);
			// Debug.Log("like resolution count: " + likeResolutions.Count);
			if (likeResolutions.Count > 0) {
				Resolution closest = likeResolutions.OrderBy(item => Math.Abs(Screen.currentResolution.refreshRate - item.refreshRate)).First();
				// Debug.Log("closest: " + closest.ToString());
				selectedResolution = supportedResolutionsList.IndexOf(closest);
				selectionIndex = selectedResolution;
				Screen.SetResolution(closest.width, closest.height, true);
			}
			else {
				Screen.SetResolution(supportedResolutionsList[supportedResolutionsList.Count - 1].width,
					supportedResolutionsList[supportedResolutionsList.Count - 1].height,
					true, supportedResolutionsList[supportedResolutionsList.Count - 1].refreshRate);
			}
		}
		else {
			selectedResolution = selectionIndex;
		}
		supportedResolutions = supportedResolutionsList.ToArray();

		UpdateText();
	}

	public override void IncrementOption() {
        if (selectionIndex + 1 <= (supportedResolutions.Length - 1) && CanSwitchOptions())
        {
            int indexChange = FindNextUniqueResolution(1);
            Debug.Log(indexChange);

            //do nothing if change is 0
            if (indexChange != 0) {
                selectionIndex += indexChange;
                selectedResolution = selectionIndex;
                UpdateText();
            }
        }
        StartCoroutine(arrowScaler(rightArrow));
        ResetSwitchCooldown();
    }

	public override void DecrementOption() {
        if (selectionIndex - 1 >= 0 && CanSwitchOptions()) {
            int indexChange = FindNextUniqueResolution(-1);
            Debug.Log(indexChange);

            //do nothing if change is 0
            if (indexChange != 0){
                selectionIndex += indexChange;
                selectedResolution = selectionIndex;
                UpdateText();
            }
        }
        StartCoroutine(arrowScaler(leftArrow));
        ResetSwitchCooldown();
    }

    int FindNextUniqueResolution(int incVal) {
        bool uniqueRes = false;
        int i = incVal;

        while(!uniqueRes) {
            //check if resolution width is same as perviously selected
            if(supportedResolutions[selectionIndex + i].width == supportedResolutions[selectionIndex].width) {
                //If width is the same, check if height is the same
                if(supportedResolutions[selectionIndex + i].height == supportedResolutions[selectionIndex].height) {
                    //both are the same
                    uniqueRes = false;
                }
                else {
                    //height is different, unique value
                    uniqueRes = true;
                }
            }
            else {
                //width is different, unique value
                uniqueRes = true;
            }

            //increment i value for next loop
            if(!uniqueRes) i += incVal;

            //if i goes out of range, end loop and say no change
            if(selectionIndex + i > (supportedResolutions.Length - 1) || selectionIndex + i < 0) {
                uniqueRes = true;
                i = 0;
            }
        }

        return i;
    }

	public void UpdateText() {
        displayText.SetText(supportedResolutions[selectedResolution].width.ToString() + "x" + supportedResolutions[selectedResolution].height.ToString());
	}

	public override T GetValue<T>() {
		return (T)Convert.ChangeType(selectedResolution, typeof(T));
	}
}
