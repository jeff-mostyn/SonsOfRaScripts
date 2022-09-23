public class TMPText_Menus : TMPTextAsset
{
	public Lang.menuText menuText;
    
    void Start() {
        SetFont();
        SetText(Lang.MenuText[menuText][SettingsManager.Instance.language]);  
    }
}
