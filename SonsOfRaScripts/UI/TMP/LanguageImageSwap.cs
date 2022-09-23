using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class LanguageImageSwap : MonoBehaviour
{
    [System.Serializable]
    public struct LanguageToImage {
        public Lang.language l;
        public Sprite s;
    }

    public List<LanguageToImage> imageMapping;
    private Image image;

	private void Start() {
		image = GetComponent<Image>();
		SetImage();
	}

	protected void SetImage() {
        try {
			image.sprite = imageMapping.Find(map => map.l == SettingsManager.Instance.language).s;
        }
        catch {
            Debug.Log("name " + gameObject.name);
        }
    }
}
