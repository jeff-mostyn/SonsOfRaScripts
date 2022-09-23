using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Constants : MonoBehaviour {

	// ---------------------------------- misc ----------------------------------------
	public const string statFilePath = "/data/history/";
	public const string settingsFilePath = "/data/";
	public const string conquestFilePath = "/data/conquest/";

	// ---------------------------------- enumerations --------------------------------
	public enum gameScenes {mainMenu, mapSelect, patronSelect, patronSelectConquest, mapDunes, mapSenet,
		mapValley, mapValleyAlt, load, postGame, conquestPostGame, discordSplash, conquest, conquest1, lobby,
		random, arcade, mapSenetAlt, mapDunesAlt};
	public enum blessingType { basic1, basic2, special, ultimate};
	public enum unitType { spearman, shieldbearer, archer, catapult, embalmPriest, mummy, huntress, avatar };
	public enum damageSource { unit, tower, blessing, sync }
	public enum towerType { archerTower, sunTower, obelisk, stasisTower};
	public enum expansionType { mine, temple, barracks };
	public enum patrons { Ra, Isis, Anubis, Sekhmet, Set, Amalgam };
	public enum radialCodes { top, mid, bot, none};
    public enum ButtonActions { confirm, back, unlock, expansion, rotateR, rotateL, menuConfirm, controls, confirmSpace, leftStick, clearPC };
	public enum MapScenes { mapDunes, mapSenet, mapValley1 };

    // ---------------------------------- dictionaries --------------------------------
    public static Dictionary<gameScenes, string> sceneNames = new Dictionary<gameScenes, string> {
		{gameScenes.mainMenu, "Menu_Main" },
		{gameScenes.mapSelect, "Menu_MapSelect" },
		{gameScenes.patronSelect, "Menu_PatronSelect" },
		{gameScenes.patronSelectConquest, "Menu_PatronSelectSingle" },
		{gameScenes.mapDunes, "Map_Dunes" },
		{gameScenes.mapDunesAlt, "Map_Dunes_Alt" },
		{gameScenes.mapSenet, "Map_Senet" },
		{gameScenes.mapSenetAlt, "Map_Senet_Alt" },
		{gameScenes.mapValley, "Map_Valley" },
		{gameScenes.mapValleyAlt, "Map_Valley_Alt" },
		{gameScenes.load, "Loading" },
		{gameScenes.postGame, "Menu_PostGame" },
		{gameScenes.discordSplash, "Menu_DiscordSplash" },
		{gameScenes.conquest, "Conquest" },
		{gameScenes.conquest1, "Conquest_1" },
		{gameScenes.conquestPostGame, "Menu_ConquestPostGame" },
        {gameScenes.lobby, "Menu_Online" },
		{gameScenes.arcade, "Menu_Arcade" }
	};

    public static Dictionary<string, gameScenes> sceneCodes = new Dictionary<string, gameScenes> {
        {"Menu_Main", gameScenes.mainMenu},
        {"Menu_MapSelect", gameScenes.mapSelect},
        {"Menu_PatronSelect", gameScenes.patronSelect},
        {"Menu_PatronSelectSingle", gameScenes.patronSelectConquest },
        {"Map_Dunes", gameScenes.mapDunes},
        {"Map_Dunes_Alt", gameScenes.mapDunesAlt},
        {"Map_Senet", gameScenes.mapSenet},
        {"Map_Senet_Alt", gameScenes.mapSenetAlt},
        {"Map_Valley", gameScenes.mapValley},
        {"Map_Valley_Alt", gameScenes.mapValleyAlt},
        {"Loading", gameScenes.load},
        {"Menu_PostGame", gameScenes.postGame},
        {"Menu_DiscordSplash", gameScenes.discordSplash },
        {"Conquest", gameScenes.conquest},
        {"Conquest_1", gameScenes.conquest1},
        {"Menu_ConquestPostGame", gameScenes.conquestPostGame},
        {"Menu_Online", gameScenes.lobby },
		{"Menu_Arcade", gameScenes.arcade }
	};

	public static Dictionary<gameScenes, bool> isTwoLaneMap = new Dictionary<gameScenes, bool> {
		{gameScenes.mapDunes, false },
		{gameScenes.mapDunesAlt, false },
		{gameScenes.mapSenet, true },
		{gameScenes.mapSenetAlt, true },
		{gameScenes.mapValley, false },
		{gameScenes.mapValleyAlt, false },
	};

	public static Dictionary<gameScenes, string> mapNames = new Dictionary<gameScenes, string> {
		{gameScenes.mapDunes, "Map_Dunes" },
		{gameScenes.mapDunesAlt, "Map_Dunes_Alt" },
		{gameScenes.mapSenet, "Map_Senet" },
		{gameScenes.mapSenetAlt, "Map_Senet_Alt" },
		{gameScenes.mapValley, "Map_Valley" },
		{gameScenes.mapValleyAlt, "Map_Valley_Alt" },
	};

	public static Dictionary<string, Lang.language> apiLanguageMapping = new Dictionary<string, Lang.language>() {
		{"schinese", Lang.language.SChinese },
		{"english", Lang.language.English },
		{"japanese", Lang.language.Japanese },
		{"koreana", Lang.language.Korean },
		{"russian", Lang.language.Russian },
		{"spanish", Lang.language.Spanish },
		{"latam", Lang.language.Spanish },
		{"french", Lang.language.French },
	};

	// ----------------------------- demo mode ------------------------------------
	public static Dictionary<gameScenes, bool> MapAvailableInDemo = new Dictionary<gameScenes, bool> {
		{gameScenes.mapDunes, true },
		{gameScenes.mapSenet, true },
		{gameScenes.mapValley, true },
		{gameScenes.mapValleyAlt, false }
	};

	public static Dictionary<patrons, bool> PatronAvailableInDemo = new Dictionary<patrons, bool> {
		{patrons.Ra, true },
		{patrons.Isis, true },
		{patrons.Anubis, true },
		{patrons.Sekhmet, false },
		{patrons.Set, false }
	};

	// ------------------------------- radial menu --------------------------------
	public static Dictionary<int, float> RADIAL_ZONE_ANGLES = new Dictionary<int, float> {
		{ 0, 0f },
		{ 1, -165.0f },
		{ 2, -135.0f },
		{ 3, -105.0f },
		{ 4, -75.0f },
		{ 5, -45.0f },
		{ 6, -15.0f },
		{ 7, 15.0f },
		{ 8, 45.0f },
		{ 9, 75.0f },
		{ 10, 105.0f },
		{ 11, 135.0f },
		{ 12, 165.0f }
	};
	public static int RADIAL_ZONE_COUNT = 12;
	public static float RADIAL_ANGLE_RANGE_DEG = 15.0f;

	public static float RADIAL_TOP3_MIN = RADIAL_ZONE_ANGLES[1] - RADIAL_ANGLE_RANGE_DEG;  // flipped sign because negative
	public static float RADIAL_TOP3_MAX = RADIAL_ZONE_ANGLES[4] + RADIAL_ANGLE_RANGE_DEG;  // flipped sign because negative

	public static float RADIAL_MID3_MIN = RADIAL_ZONE_ANGLES[5] - RADIAL_ANGLE_RANGE_DEG;  // flipped sign because negative
	public static float RADIAL_MID3_MAX = RADIAL_ZONE_ANGLES[8] + RADIAL_ANGLE_RANGE_DEG;

	public static float RADIAL_BOT3_MIN = RADIAL_ZONE_ANGLES[9] - RADIAL_ANGLE_RANGE_DEG;
	public static float RADIAL_BOT3_MAX = RADIAL_ZONE_ANGLES[12] + RADIAL_ANGLE_RANGE_DEG;

	public static float RADIAL_P1_TOP = 120f;
	public static float RADIAL_P1_MID = 0f;
	public static float RADIAL_P1_BOT = -120f;
	public static float RADIAL_P2_TOP = 60f;
	public static float RADIAL_P2_MID = 180f;
	public static float RADIAL_P2_BOT = -60f;

	public static float RADIAL_DEFAULT_ALPHA = 0f;
	public static float RADIAL_SELECTED_ALPHA = 100f;

	public static float RADIAL_BUTTONPROMPT_DEFAULT_ALPHA = 20f;
	public static float RADIAL_BUTTONPROMPT_ACTIVE_ALPHA = 100f;

	public static Vector3 SELETECTED_ICON_SCALE_FACTOR = new Vector3(1.6f, 1.6f, 1.6f);
	public static Vector3 ONES = new Vector3(1f, 1f, 1f);

	// ---------------------------------- effects -----------------------------------
	public static int STEREO_PAN_MAX_Z_POSITION = 15;

    // --------------- Currency --------------------
    public static int MINE_GP5 = 175;
	public static int TEMPLE_FP5 = 5;

	// -------------- Stats ----------------
	public static float TRAINING_GROUND_ATK_BOOST = 0.80f;
	public static float TRAINING_GROUND_ARMOR_BOOST = 0.30f;

	// --------------- Online ------------------
	public static int ping_bad = 300, ping_fair = 120, ping_good = 50;

	// --------------------- Achievements ------------------------------
	public static int Anubis_Mummy_Spawn_count = 30;
	public static int Sekhmet_BattleHardened_count = 20;
	public static int Isis_Block_Stacks_count = 15;
	public static int Ra_Solar_Flare_Kills_Count = 30;
}
