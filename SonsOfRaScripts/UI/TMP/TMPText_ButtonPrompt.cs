public class TMPText_ButtonPrompt : TMPTextAsset
{
	public Lang.buttonPrompts buttonPrompt;
    
    void Start() {
        SetFont();
        SetText(Lang.ButtonPrompts[buttonPrompt][SettingsManager.Instance.language]);
    }
}
