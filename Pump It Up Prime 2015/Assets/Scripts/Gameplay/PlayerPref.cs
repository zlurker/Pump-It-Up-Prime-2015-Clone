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

public struct PlayerSettings {
    public float[] playerScore; // 0 -- perfect, 1 -- great, 2 -- good, 3 -- bad, 4 -- miss, 5 -- max combo, 6 -- score
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
    public static int startUpScreen = 1;
    public static int menu = 2;
    public static int gameplayLevel = 3;
    public static int scoreScreen = 4;
}
