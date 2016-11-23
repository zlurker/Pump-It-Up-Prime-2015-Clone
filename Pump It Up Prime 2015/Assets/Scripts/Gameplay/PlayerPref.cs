using UnityEngine;
using System.Collections;
using System.Collections.Generic;

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
    public float prefSpeed;
    public float life;
}

public static class PlayerPref {

    public static int sceneValueOffset;
    //Global Settings  
    public static bool songsRegisted;
    public static string[] songs;
    public static int songIndex;
    public static float prefRush;
    //Future DP Settings
    public static PlayerSettings[] playerSettings;
}
