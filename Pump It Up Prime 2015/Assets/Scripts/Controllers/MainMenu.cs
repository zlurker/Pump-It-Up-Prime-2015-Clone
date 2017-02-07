using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class MainMenu : MonoBehaviour {

    [System.Serializable]
    public struct SecretCodes {
        public int[] keyValue;
        [HideInInspector]
        public int[] playersAtValue;
    }

    public enum MenuState {
        ChannelSelect, SelectSong, SelectSongLevel
    }

    public SecretCodes[] codes;
    public string path;
    public Text dataPath;
    public Text songTitle;
    public RawImage previewImage;
    //public Text currSpeed;
    public Text currRush;

    public GameObject[] playerMenu;
    public Text[] currSpeed;
    public Text[] currLevel;
    public AudioSource previewSong;
    public MenuState menuState;

    public RawImage video;
    public string videoPath;
    WWW startUpClip;

    void Start() {
        for (var i = 0; i < codes.Length; i++)
            codes[i].playersAtValue = new int[2];

        startUpClip = new WWW("file:///" + Path.Combine(Application.dataPath, videoPath));

        MovieTexture instance = startUpClip.movie;
        while (!instance.isReadyToPlay) ;
        video.texture = instance;
        instance.Play();
        instance.loop = true;

        path = Application.dataPath;

        dataPath.text = "Put song folder here: " + path;

        if (!PlayerPref.songsRegisted) {
            PlayerPref.currentChannel = 0;
            PlayerPref.channels = new Channel[5];

            PlayerPref.channels[0].channelName = "ALLTUNES";
            PlayerPref.channels[1].channelName = "WORLD MUSIC";
            PlayerPref.channels[2].channelName = "K-POP";
            PlayerPref.channels[3].channelName = "J-POP";
            PlayerPref.channels[4].channelName = "FULL SONG";

            for (var i = 0; i < PlayerPref.channels.Length; i++)
                PlayerPref.channels[i].references = new List<int>();

            PlayerPref.songs = new List<SongData>();
            LoadSongsFromDirectory(new DirectoryInfo(Path.Combine(Application.dataPath, "Songs")));

            PlayerPref.playerSettings = new PlayerSettings[2];
            PlayerPref.songsRegisted = true;
            PlayerPref.prefRush = 1;

            for (var i = 0; i < 2; i++) {
                PlayerPref.playerSettings[i].prefSpeed = 2;
            }

            PlayerPref.playerSettings[0].life = 5;
            PlayerPref.playerSettings[1].life = 5;

            for (var i = 0; i < PlayerPref.channels.Length; i++)
                for (var j = 0; j < PlayerPref.channels[i].references.Count; j++)
                    Debug.Log(PlayerPref.channels[i].channelName + " - " + PlayerPref.songs[PlayerPref.channels[i].references[j]].name);

        }

        for (var i = 0; i < 2; i++)
            PlayerPref.playerSettings[i].playerScore = new float[7];

        InputBase.currentGameMode = InputBase.GameMode.Single;

        ChangeMusicMenu(0);
        RefreshUI();

        KinectManager.ChangeFeetSize(0.05f);
    }

    void Update() {
        if (previewSong.time > PlayerPref.songs[PlayerPref.songIndex].previewEnd)
            previewSong.Pause();
        else
            previewSong.volume = (PlayerPref.songs[PlayerPref.songIndex].previewEnd - previewSong.time) / 5;
    }

    public void Replay() {
        previewSong.time = PlayerPref.songs[PlayerPref.songIndex].previewStart;
    }

    public void ChangeRush(float value) {
        if (PlayerPref.prefRush + value > 0.7f && PlayerPref.prefRush + value < 1.6f)
            PlayerPref.prefRush += value;
        RefreshUI();
    }

    public void ChangeMusicMenu(int value) {

        if (PlayerPref.songIndex + value > -1 && PlayerPref.songIndex + value < PlayerPref.songs.Count) {
            PlayerPref.songIndex += value;

            if (value != 0)
                for (var i = 0; i < 2; i++)
                    PlayerPref.playerSettings[i].currentSongLevel = 0;

            DirectoryInfo directory = new DirectoryInfo(PlayerPref.songs[PlayerPref.songIndex].path);
            FileInfo[] temp = directory.GetFiles("*.wav");

            Destroy(previewSong.clip);
            using (WWW song = new WWW("file:///" + temp[0].FullName)) {
                while (!song.isDone) ;
                previewSong.clip = song.GetAudioClip(false);
            }

            previewSong.volume = 1;

            previewSong.pitch = PlayerPref.prefRush;
            previewSong.Play();

            previewSong.time = PlayerPref.songs[PlayerPref.songIndex].previewStart;
        }
        RefreshUI();
    }

    public void RefreshUI() {
        songTitle.text = PlayerPref.songs[PlayerPref.songIndex].name;
        currRush.text = PlayerPref.prefRush.ToString();

        DirectoryInfo directory = new DirectoryInfo(PlayerPref.songs[PlayerPref.songIndex].path);
        FileInfo[] temp = directory.GetFiles("*.PNG");

        using (WWW image = new WWW("file:///" + temp[0].FullName)) {
            while (!image.isDone) ;
            previewImage.texture = image.texture;
        }

        for (var i = 0; i < 2; i++) {

            if (PlayerPref.playerSettings[i].life == 0 || menuState != MenuState.SelectSongLevel)
                playerMenu[i].SetActive(false);
            else
                playerMenu[i].SetActive(true);

            currSpeed[i].text = PlayerPref.playerSettings[i].prefSpeed.ToString();
            currLevel[i].text = PlayerPref.songs[PlayerPref.songIndex].levels[PlayerPref.playerSettings[i].currentSongLevel];
        }
    }

    public void LoadLevel() {
        SceneManager.LoadScene(SceneIndex.gameplayLevel);
    }

    public void KillPlayer(int player) {
        if (PlayerPref.playerSettings[player].life > 0)
            PlayerPref.playerSettings[player].life = 0;
        else
            PlayerPref.playerSettings[player].life = 5;
        RefreshUI();
    }

    void LoadSongsFromDirectory(DirectoryInfo directoryInfo) {
        FileInfo[] stepcharts = directoryInfo.GetFiles("*.ssc");
        DirectoryInfo[] directories = directoryInfo.GetDirectories();

        for (var i = 0; i < stepcharts.Length; i++) {
            PlayerPref.songs.Add(ReadStepchartLevelData(stepcharts[i].FullName, directoryInfo.FullName));
        }

        foreach (DirectoryInfo directory in directories)
            LoadSongsFromDirectory(directory);
    }

    SongData ReadStepchartLevelData(string songPath, string rootPath) {
        SongData instance = new SongData();
        StreamReader stepchart;
        string tempStr;
        string level = "";

        instance.path = rootPath;
        instance.name = songPath.Substring(path.Length + 12, songPath.Length - path.Length - 12 - 4);
        instance.levels = new List<string>();
        stepchart = File.OpenText(songPath);

        PlayerPref.channels[0].references.Add(PlayerPref.songs.Count);

        while ((tempStr = stepchart.ReadLine()) != null) {

            if (tempStr.Contains("#TITLE:"))
                instance.name = tempStr.Substring(7, tempStr.Length - 1 - 7);

            if (tempStr.Contains("#GENRE:"))
                for (var i = 0; i < PlayerPref.channels.Length; i++)
                    if (PlayerPref.channels[i].channelName == tempStr.Substring(7, tempStr.Length - 1 - 7))
                        PlayerPref.channels[i].references.Add(PlayerPref.songs.Count);

            if (tempStr.Contains("#SAMPLESTART:"))
                instance.previewStart = float.Parse(tempStr.Substring(13, tempStr.Length - 1 - 13));

            if (tempStr.Contains("#SAMPLELENGTH:"))
                instance.previewEnd = instance.previewStart + float.Parse(tempStr.Substring(14, tempStr.Length - 1 - 14));

            if (tempStr.Contains("pump-single"))
                level = "S";
            else if (tempStr.Contains("pump-double") || tempStr.Contains("pump-routine"))
                level = "D";

            if (tempStr.Contains("#METER:")) {
                level += tempStr.Remove(0, 7);
                level = level.Remove(level.Length - 1, 1);
                instance.levels.Add(level);
            }
        }

        stepchart.Close();
        return instance;
    }
}

