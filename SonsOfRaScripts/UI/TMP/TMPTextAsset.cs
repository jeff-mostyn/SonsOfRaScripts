using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public abstract class TMPTextAsset : MonoBehaviour
{
    [System.Serializable]
    public struct LanguageToFont {
        public Lang.language l;
        public TMP_FontAsset f;
    }

    public List<LanguageToFont> fontMapping;
    private TextMeshProUGUI TextField;

    // Start is called before the first frame update
    protected void SetFont() {
        TextField = GetComponentInChildren<TextMeshProUGUI>();
        try {
            TextField.font = fontMapping.Find(map => map.l == SettingsManager.Instance.language).f;
        }
        catch {
            Debug.Log("name " + gameObject.name);
        }
    }

    protected void SetText(string txt) {
        try {
            TextField.SetText(txt);
        }
        catch {
            Debug.Log(txt);
        }
    }
}
