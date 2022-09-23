public class TMPText_Settings : TMPTextAsset
{
    public Lang.settingsText text;

    void Start() {
        SetFont();
        SetText(Lang.SettingsText[text][SettingsManager.Instance.language]);
    }
}
