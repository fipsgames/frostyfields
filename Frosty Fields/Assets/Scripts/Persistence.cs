using UnityEngine;
using util;
using System.Security.Cryptography;

public class Persistence : MonoBehaviour {

    public TextAsset levelAsset;

    //static string gameModeKey = "GameMode";

    // no need to save level def strings in player prefs
    //static string levelsKey = "Levels";
    static string customLevelsKey = "CustomLevels";
    static string currLevelKey = "CurrLevel";
    static string maxLevelKey = "MaxLevel";
    static string currCustomLevelKey = "CurrCustonLevel";
    static string mutedKey = "Muted";

    static string emptyLevel = "#;S;B;D;F;P;0-0";

    static string customLevelHashKeyPrefix = "CustomLevel_";
    static string tutorialPrefix = "Tutorial_";

    // HACK TO COPY ALL LEVELS INTO CREATOR MODE
    /*
    void Start()
    {
        SaveCustomLevels(LoadLevels());
    }
    */

    public int LoadCurrCustomLevel()
    {
        return (PlayerPrefs.GetInt(currCustomLevelKey, 1));
    }

    public void SaveCurrCustomlevel(int currCustomLevel)
    {
        PlayerPrefs.SetInt(currCustomLevelKey, currCustomLevel);
        //Logger.Log("SAVED CURR CUSTOM LEVEL: " + currCustomLevel);
    }

    public string LoadLevels(){
        // always load Levels from level asset, no need to save and load it in player prefs additonally
        //return (PlayerPrefs.GetString(levelsKey,levelAsset.text));
        //Logger.Log("loaded levels: " + levelAsset.text, this, true, true);
        return levelAsset.text;
    }


    public string LoadCustomLevels()
    {
        // HACK TO RESET CUSTOM LEVELS
        // return emptyLevel;
        return (PlayerPrefs.GetString(customLevelsKey, emptyLevel));
    }

    public void SaveCustomLevels(string customLevels)
    {
        PlayerPrefs.SetString(customLevelsKey, customLevels);
        //Logger.Log("SAVED CUSTOM LEVELS: " + customLevels);
    }

    public int LoadCurrLevel(){
        return (PlayerPrefs.GetInt(currLevelKey, 1));
    }
	
    public int LoadMaxLevel(){
        return (PlayerPrefs.GetInt(maxLevelKey, 1));
    }

    public void SaveCurrLevel(int currLevel){
        PlayerPrefs.SetInt(currLevelKey, currLevel);
        //Logger.Log("SAVED CURR LEVEL: " + currLevel);
    } 

    // only saves max level value if its higher than the one stored
    public void SaveMaxLevel(int maxLevel){
        if(maxLevel > LoadMaxLevel()) PlayerPrefs.SetInt(maxLevelKey, maxLevel);
        //Logger.Log("SAVED MAX LEVEL: " + LoadMaxLevel());
    } 

    public void SaveMaxLevelUnchecked(int maxLevel){
        PlayerPrefs.SetInt(maxLevelKey, maxLevel);
        //Logger.Log("SAVED MAX LEVEL: " + LoadMaxLevel());
    } 

    public string GetEmptyLevelString(){
        return emptyLevel;
    }

    public void SaveMuted(bool muted){
        PlayerPrefs.SetInt(mutedKey, muted?1:0);
        //Logger.Log("SAVED MUTED SOUND: " + muted);
    }

    public bool LoadMuted(){
        return PlayerPrefs.GetInt(mutedKey, 0) == 1;
    }

    public void AddCustomLevelSolved(string level){
        string hash = Util.GetHash(SHA256.Create(), level);
        ////Logger.Log("Adding HashKey: " + hash, this, true);
        PlayerPrefs.SetInt(customLevelHashKeyPrefix + hash, 1);
    }

    public bool GetCustomLevelSolved(string level){
        string hash = Util.GetHash(SHA256.Create(), level);
        bool solved = PlayerPrefs.HasKey(customLevelHashKeyPrefix + hash);
        ////Logger.Log("Level solved HashKey: " + solved, this, true);
        return solved;
    }

    public void AddTutorialFinished(string tutorial)
    {
        PlayerPrefs.SetInt(tutorialPrefix + tutorial, 1);
    }

    public void ResetTutorialFinished(string tutorial)
    {
        PlayerPrefs.SetInt(tutorialPrefix + tutorial, 0);
    }

    public bool GetTutorialFinished(string tutorial)
    {
        return PlayerPrefs.GetInt(tutorialPrefix + tutorial, 0) == 1;
    }
}
