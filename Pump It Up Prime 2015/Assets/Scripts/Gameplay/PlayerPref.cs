using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public struct SongData {
    public string name;
    public string path;

    public float previewStart;
    public float previewEnd;
    public List<string> levels;
}

public struct ScoreData {
    public float perfect;
    public float great;
    public float good;
    public float bad;
    public float miss;
    public float maxCombo;
    public float score;
}

public struct PlayerSettings {
    public ScoreData playerScore;
    public int currentSongLevel;
    public float prefSpeed;
    public float life;
    public bool autoPlay;
}

public static class PlayerPref {

    public static int sceneValueOffset;
    //Global Settings  
    public static bool songsRegisted;
    public static List<SongData> songs;
    public static int songIndex;
    public static float prefRush;
    //SP settings
    public static PlayerSettings[] playerSettings;
}

public static class SceneIndex { //All the sceneindexs
    public static int setUp = 0;
    public static int menu = 1;
    public static int gameplayLevel = 2;
    public static int scoreScreen = 3;
}
