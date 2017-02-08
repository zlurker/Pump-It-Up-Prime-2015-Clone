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

    [System.Serializable]
    public struct UIElements {
        public GameObject[] uiElements;
    }

    public SecretCodes[] codes;
    public UIElements[] ui;
    public string path;
    public string imageAssetPath;
    public Text dataPath;
    public Text songTitle;
    public RawImage previewImage;
    public RawImage channelImage;
    public Text currRush;

    public GameObject[] playerMenu;
    public Text[] currSpeed;
    public Text[] currLevel;
    public AudioSource previewSong;
    public AudioSource bgSound;
    public AudioSource actionSound;

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
            PlayerPref.menuState = MenuState.SelectSong;
            PlayerPref.channels = new Channel[6];

            PlayerPref.channels[0].channelName = "ALLTUNES";
            PlayerPref.channels[1].channelName = "WORLD MUSIC";
            PlayerPref.channels[2].channelName = "K-POP";
            PlayerPref.channels[3].channelName = "J-POP";
            PlayerPref.channels[4].channelName = "ORIGINAL";
            PlayerPref.channels[5].channelName = "FULL SONG";

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

            MenuData.channelImages = new List<Texture>();

            DirectoryInfo directory = new DirectoryInfo(Path.Combine(path, imageAssetPath));
            FileInfo[] temp = directory.GetFiles("CHANNEL*.PNG");

            for (var i = 0; i < temp.Length; i++) {
                using (WWW image = new WWW("file:///" + temp[i].FullName)) {
                    while (!image.isDone) ;
                    MenuData.channelImages.Add(image.texture);
                    Destroy(image.texture);
                }
            }
        }

        for (var i = 0; i < 2; i++)
            PlayerPref.playerSettings[i].playerScore = new float[7];

        InputBase.currentGameMode = InputBase.GameMode.Single;

        ChangeMusicMenu(0);
        RefreshUI();

        KinectManager.ChangeFeetSize(0.05f);
    }

    void Update() {
        if (previewSong.time > PlayerPref.songs[PlayerPref.channels[PlayerPref.currentChannel].references[PlayerPref.currentChannelSong]].previewEnd)
            previewSong.Pause();
        else
            previewSong.volume = (PlayerPref.songs[PlayerPref.channels[PlayerPref.currentChannel].references[PlayerPref.currentChannelSong]].previewEnd - previewSong.time) / 5;
    }

    public void ChangeRush(float value) {
        if (PlayerPref.prefRush + value > 0.7f && PlayerPref.prefRush + value < 1.6f)
            PlayerPref.prefRush += value;
        RefreshUI();
    }

    public void ChangeMusicMenu(int value) {

        if (PlayerPref.currentChannelSong + value > -1 && PlayerPref.currentChannelSong + value < PlayerPref.channels[PlayerPref.currentChannel].references.Count) {
            PlayerPref.currentChannelSong += value;

            if (value != 0)
                for (var i = 0; i < 2; i++)
                    PlayerPref.playerSettings[i].currentSongLevel = 0;

            DirectoryInfo directory = new DirectoryInfo(PlayerPref.songs[PlayerPref.channels[PlayerPref.currentChannel].references[PlayerPref.currentChannelSong]].path);
            FileInfo[] temp = directory.GetFiles("*.wav");

            Destroy(previewSong.clip);
            using (WWW song = new WWW("file:///" + temp[0].FullName)) {
                while (!song.isDone) ;
                previewSong.clip = song.GetAudioClip(false);
            }

            previewSong.volume = 1;

            previewSong.pitch = PlayerPref.prefRush;
            previewSong.Play();

            previewSong.time = PlayerPref.songs[PlayerPref.channels[PlayerPref.currentChannel].references[PlayerPref.currentChannelSong]].previewStart;
        }
        RefreshUI();
    }

    public void ChangeChannel(int value) {
        if (PlayerPref.currentChannel + value > -1 && PlayerPref.currentChannel + value < PlayerPref.channels.Length)
            PlayerPref.currentChannel += value;

        if (value != 0)
            for (var i = 0; i < 2; i++)
                PlayerPref.playerSettings[i].currentSongLevel = 0;

        PlayerPref.currentChannelSong = 0;
        previewSong.Pause();
        Destroy(previewSong.clip);
        //Debug.Log(PlayerPref.channels[PlayerPref.currentChannel].channelName);
    }

    public void RefreshUI() {
        switch (PlayerPref.menuState) {
            case MenuState.ChannelSelect:
                channelImage.texture = MenuData.channelImages[PlayerPref.currentChannel];
                break;

            case MenuState.SelectSong:
            case MenuState.SelectSongLevel:
                songTitle.text = PlayerPref.songs[PlayerPref.channels[PlayerPref.currentChannel].references[PlayerPref.currentChannelSong]].name;
                currRush.text = PlayerPref.prefRush.ToString();

                DirectoryInfo directory = new DirectoryInfo(PlayerPref.songs[PlayerPref.channels[PlayerPref.currentChannel].references[PlayerPref.currentChannelSong]].path);
                FileInfo[] temp = directory.GetFiles("*.PNG");

                Destroy(previewImage.texture);

                using (WWW image = new WWW("file:///" + temp[0].FullName)) {
                    while (!image.isDone) ;
                    previewImage.texture = image.texture;
                }

                for (var i = 0; i < 2; i++) {
                    if (PlayerPref.playerSettings[i].life == 0 || PlayerPref.menuState != MenuState.SelectSongLevel)
                        playerMenu[i].SetActive(false);
                    else
                        playerMenu[i].SetActive(true);

                    currSpeed[i].text = PlayerPref.playerSettings[i].prefSpeed.ToString();
                    currLevel[i].text = PlayerPref.songs[PlayerPref.channels[PlayerPref.currentChannel].references[PlayerPref.currentChannelSong]].levels[PlayerPref.playerSettings[i].currentSongLevel];
                }
                break;
        }
        CheckUIElements();
    }

    public void LoadLevel() {
        PlayerPref.menuState = MenuState.SelectSong;
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

    public void LoadImageFromPath() {

    }

    public void CheckUIElements() {
        int valueInst = 0;
        for (var i = 0; i < ui.Length; i++)
            if (i == (int)PlayerPref.menuState)
                valueInst = i;
            else
                for (var j = 0; j < ui[i].uiElements.Length; j++)
                    ui[i].uiElements[j].SetActive(false);

        for (var j = 0; j < ui[valueInst].uiElements.Length; j++)
            ui[valueInst].uiElements[j].SetActive(true);
    }
}

