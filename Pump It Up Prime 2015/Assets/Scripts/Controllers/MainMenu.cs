using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class MainMenu : AssetLoadingBase {

    [System.Serializable]
    public struct SecretCodes {
        public int[] keyValue;
        public int resultToPlayer;
        [HideInInspector]
        public int[] playersAtValue;
    }

    [System.Serializable]
    public struct SpeedMod {
        public float value;
        public NumberModifier modifier;
    }

    [System.Serializable]
    public struct MenuInterfaceGroup {
        public int menuInterfaceBits;
        public GameObject[] uiElements;
    }

    [System.Serializable]
    public struct MenuInterface {
        public MenuInterfaceGroup[] menuInterfaceGroup;
        public bool layerClosesOtherLayers;
        public int numberOfPlayerIndexes;
    }

    public enum NumberModifier {
        Set, Add
    }

    public SecretCodes[] codes;
    public SpeedMod[] speedModifier;
    public MenuInterface[] menuInterface;
    public RawImage[] dynaMainMenuScreeens;

    public SubMenuController[] players;
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
    public RawImage[] currMod;
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
            //PlayerPref.menuState = MenuState.SelectSong;
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

            for (var i = 0; i < 2; i++)
                PlayerPref.playerSettings[i].prefSpeed = 2;

            PlayerPref.playerSettings[0].life = 5;
            PlayerPref.playerSettings[1].life = 5;

            PlayerPref.menuIndexes = new IndexData[menuInterface.Length];

            for (var i = 0; i < menuInterface.Length; i++) {
                PlayerPref.menuIndexes[i].indexDataGroup = new IndexDataGroup[menuInterface[i].menuInterfaceGroup.Length];
                PlayerPref.menuIndexes[i].dataGroupPoint = new int[menuInterface[i].numberOfPlayerIndexes];
                for (var j = 0; j < menuInterface[i].menuInterfaceGroup.Length; j++)
                    PlayerPref.menuIndexes[i].indexDataGroup[j].indexDataBit = new int[menuInterface[i].menuInterfaceGroup[j].menuInterfaceBits];
                //Debug.Log(menuInterface[i].menuInterfaceGroup[j].menuInterfaceBit.Length + " " + menuInterface[i].menuInterfaceGroup[j].menuInterfaceBit[0].name);
            }
        }
        PlayerPref.currentPlayerLayer = new int[2];

        for (var i = 0; i < 2; i++) {
            PlayerPref.playerSettings[i].playerScore = new float[7];
            RefreshUI(i);
        }

        InputBase.currentGameMode = InputBase.GameMode.Single;

        ChangeMusicMenu(0);


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
    }

    public void RefreshUI(int player) {
        int[] dataGroupPoint = PlayerPref.menuIndexes[PlayerPref.currentPlayerLayer[player]].dataGroupPoint;
        int valueInst = dataGroupPoint[dataGroupPoint[PlayerPref.IndexCheck(dataGroupPoint.Length, player)]];
        int[] lengthChecker = new int[valueInst+1];

        for (var i = 0; i < lengthChecker.Length; i++) {
            lengthChecker[i] = PlayerPref.menuIndexes[PlayerPref.currentPlayerLayer[player]].indexDataGroup[i].indexDataBit[PlayerPref.IndexCheck(PlayerPref.menuIndexes[PlayerPref.currentPlayerLayer[player]].indexDataGroup[i].indexDataBit.Length, player)];
            Debug.LogFormat("i is {0} and valued at {1}", i, lengthChecker[i]);
        }
        PlayerPref.menuIndexes[PlayerPref.currentPlayerLayer[player]].indexDataGroup[valueInst].indexDataBit[PlayerPref.IndexCheck(PlayerPref.menuIndexes[PlayerPref.currentPlayerLayer[player]].indexDataGroup[valueInst].indexDataBit.Length, player)] = PlayerPref.ProcessRawIndex(lengthChecker[lengthChecker.Length - 1], LengthData(PlayerPref.currentPlayerLayer[player], lengthChecker));

        int currentDataGroup = PlayerPref.menuIndexes[PlayerPref.currentPlayerLayer[player]].dataGroupPoint[PlayerPref.IndexCheck(PlayerPref.menuIndexes[PlayerPref.currentPlayerLayer[player]].dataGroupPoint.Length, player)];
        int index = PlayerPref.IndexCheck(PlayerPref.menuIndexes[PlayerPref.currentPlayerLayer[player]].indexDataGroup[currentDataGroup].indexDataBit.Length, player);
        Debug.Log(PlayerPref.currentPlayerLayer[player] + " " + PlayerPref.menuIndexes[PlayerPref.currentPlayerLayer[player]].dataGroupPoint[PlayerPref.IndexCheck(PlayerPref.menuIndexes[PlayerPref.currentPlayerLayer[player]].dataGroupPoint.Length, player)] + " " + PlayerPref.menuIndexes[PlayerPref.currentPlayerLayer[player]].indexDataGroup[currentDataGroup].indexDataBit[index]);

        switch (PlayerPref.menuIndexes[0].dataGroupPoint[0]) {
            case 0:
                channelImage.texture = AssetDatabase.data.dataGroups[0].dataBits[PlayerPref.currentChannel].image;
                break;

            case 1:
            case 2:
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
                    if (PlayerPref.playerSettings[i].life == 0)
                        playerMenu[i].SetActive(false);
                    else
                        playerMenu[i].SetActive(true);

                    currSpeed[i].text = PlayerPref.playerSettings[i].prefSpeed.ToString();
                    currLevel[i].text = PlayerPref.songs[PlayerPref.channels[PlayerPref.currentChannel].references[PlayerPref.currentChannelSong]].levels[PlayerPref.playerSettings[i].currentSongLevel].level;
                    players[i].levelBubble.texture = LoadDataFromDatabase(int.Parse(players[i].levelBubble.name), (int)PlayerPref.songs[PlayerPref.channels[PlayerPref.currentChannel].references[PlayerPref.currentChannelSong]].levels[PlayerPref.playerSettings[i].currentSongLevel].stepType);
                }
                break;

                //case MenuState.Confirmation:
                //LoadLevel();
                // break;
        }
        CheckUIElements(menuInterface[0].menuInterfaceGroup[PlayerPref.menuIndexes[0].dataGroupPoint[0]].uiElements);
    }

    int LengthData(int currScreen, int[] indexes) {
        int lengthInstance = 0;
        switch (currScreen) {
            case 0:
                switch (indexes.Length) {
                    case 1:
                        lengthInstance = PlayerPref.channels.Length;
                        break;
                    case 2:
                        lengthInstance = PlayerPref.channels[indexes[0]].references.Count;
                        break;
                    case 3:
                        lengthInstance = PlayerPref.songs[PlayerPref.channels[indexes[0]].references[1]].levels.Count;
                        break;
                }
                break;
        }
        return lengthInstance;
    }

    public void LoadLevel() {
        SceneManager.LoadScene(SceneIndex.gameplayLevel);
    }

    public void TooglePlayer(int player) {
        if (PlayerPref.playerSettings[player].life > 0)
            PlayerPref.playerSettings[player].life = 0;
        else
            PlayerPref.playerSettings[player].life = 5;
        RefreshUI(player);
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
        LevelType level = new LevelType();
        StreamReader stepchart;
        string tempStr;

        instance.path = rootPath;
        instance.name = songPath.Substring(path.Length + 12, songPath.Length - path.Length - 12 - 4);
        instance.levels = new List<LevelType>();
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
                level.stepType = StepType.Single;
            else if (tempStr.Contains("pump-double") || tempStr.Contains("pump-routine")) {
                level.stepType = StepType.Double;
            }

            if (tempStr.Contains("#METER:")) {
                level.level += tempStr.Remove(0, 7);
                level.level = level.level.Remove(level.level.Length - 1, 1);
                instance.levels.Add(level);
                level.level = "";
            }
        }

        stepchart.Close();
        return instance;
    }


    void CheckUIElements(GameObject[] uiElements) {
        for (var i = 0; i < menuInterface.Length; i++)
            for (var j = 0; j < menuInterface[i].menuInterfaceGroup.Length; j++)
                for (var k = 0; k < menuInterface[i].menuInterfaceGroup[j].uiElements.Length; k++)
                    menuInterface[i].menuInterfaceGroup[j].uiElements[k].SetActive(false);

        for (var i = 0; i < uiElements.Length; i++)
            uiElements[i].SetActive(true);
    }
}

