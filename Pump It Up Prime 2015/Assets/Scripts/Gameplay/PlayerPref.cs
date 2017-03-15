using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public struct SongData {
    public string name;
    public string path;

    public float previewStart;
    public float previewEnd;
    public List<LevelType> levels;
}

public struct LevelType {
    public StepType stepType;
    public string level;
}

public enum StepType {
    Single, Double
}

public struct Channel {
    public string channelName;
    public List<int> references;
}

public struct PlayerSettings {
    public float[] playerScore; // 0 -- perfect, 1 -- great, 2 -- good, 3 -- bad, 4 -- miss, 5 -- max combo, 6 -- score
    public int currentSongLevel;
    public float prefSpeed;
    public float life;
    public bool autoPlay;
}

public struct Data {
    public DataGroup[] dataGroups;
}

public struct DataGroup {
    public List<DataBit> dataBits;
    public float uiScaleValue;
}

public struct DataBit {

    public DataBit(Texture text) {
        image = text;
        //sound = clip;
    }

    public Texture image;
    //public AudioClip sound;
}

public struct IndexData {
    public IndexDataGroup[] indexDataGroup; //Contains multiple group of index data
    public int[] dataGroupPoint;
}

public struct IndexDataGroup {
    public int[] indexDataBit; //Contain a group of index data
}

public static class PlayerPref {
    public static int sceneValueOffset;
    //Global Settings  
    public static bool songsRegisted;
    public static Channel[] channels;
    public static List<SongData> songs;
    public static int currSong;

    public static float prefRush;
    //SP settings
    public static PlayerSettings[] playerSettings;
    public static IndexData[] menuIndexes;
    public static int[] currentPlayerLayer;

    public static int ProcessRawIndex(int currValue, int length) {
        if (currValue < 0)
            return length - 1;
        if (!(currValue < length))
            return 0;

        return currValue;
    }

    public static int ScrollAnArray(int currValue, int valueToAdd, int length) {
        if (currValue + valueToAdd > -1 && currValue + valueToAdd < length)
            return currValue + valueToAdd;

        return currValue;
    }

    public static int ScrollAnArray(int currValue, int valueToAdd, int length, out int leftover) {
        leftover = 0;
        if (currValue + valueToAdd > -1 && currValue + valueToAdd < length)
            return currValue + valueToAdd;

        leftover = valueToAdd;
        return currValue;
    }

    public static int IndexCheck(int length, int playerIndex) {
        if (length > 1)
            return playerIndex;

        return 0;
    }

}

public static class AssetDatabase {
    public static Data data;
    public static NoteskinType[] noteskins;
    public static Sprite seqZone;
    public static Sprite healthbar;
}

public static class SceneIndex { //All the sceneindexs
    public static int setUp = 0;
    public static int startUpScreen = 1;
    public static int menu = 2;
    public static int gameplayLevel = 3;
    public static int scoreScreen = 4;
}
