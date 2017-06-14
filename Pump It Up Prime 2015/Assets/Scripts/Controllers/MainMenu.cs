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
    public struct UIElements {
        public GameObject[] ui;
    }

    [System.Serializable]
    public struct MenuInterfaceGroup {
        public int menuInterfaceBits;
        public UIElements[] uiElements;
    }

    [System.Serializable]
    public struct MenuInterface {
        public MenuInterfaceGroup[] menuInterfaceGroup;
        public bool closesWhenNotInLayer;
        public int numberOfPlayerIndexes;
    }

    public enum NumberModifier {
        Set, Add
    }

    public SecretCodes[] codes;
    public SpeedMod[] speedModifier;
    public MenuInterface[] menuInterface;

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
    float currPreviewEnd;

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

            //Needs to be more "Dynamic"
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
        KinectManager.ChangeFeetSize(0.05f);
    }

    void Update() {
        if (previewSong.time > currPreviewEnd)
            previewSong.Pause();
        else
            previewSong.volume = (currPreviewEnd - previewSong.time) / 5;
    }

    public void ChangeRush(float value) {
        if (PlayerPref.prefRush + value > 0.7f && PlayerPref.prefRush + value < 1.6f)
            PlayerPref.prefRush += value;
    }

    public void RefreshUI(int player) {
        int[] dataGroupPoint = PlayerPref.menuIndexes[PlayerPref.currentPlayerLayer[player]].dataGroupPoint;
        int valueInst = dataGroupPoint[PlayerPref.IndexCheck(dataGroupPoint.Length, player)];
        int[] lengthChecker = new int[valueInst + 1];

        for (var i = 0; i < lengthChecker.Length; i++)
            lengthChecker[i] = PlayerPref.menuIndexes[PlayerPref.currentPlayerLayer[player]].indexDataGroup[i].indexDataBit[PlayerPref.IndexCheck(PlayerPref.menuIndexes[PlayerPref.currentPlayerLayer[player]].indexDataGroup[i].indexDataBit.Length, player)];
        //Debug.LogFormat("i is {0} and valued at {1}", i, lengthChecker[i]);

        PlayerPref.menuIndexes[PlayerPref.currentPlayerLayer[player]].indexDataGroup[valueInst].indexDataBit[PlayerPref.IndexCheck(PlayerPref.menuIndexes[PlayerPref.currentPlayerLayer[player]].indexDataGroup[valueInst].indexDataBit.Length, player)] = PlayerPref.ProcessRawIndex(lengthChecker[lengthChecker.Length - 1], LengthData(PlayerPref.currentPlayerLayer[player], lengthChecker));

        for (var i = 0; i < lengthChecker.Length; i++)
            lengthChecker[i] = PlayerPref.menuIndexes[PlayerPref.currentPlayerLayer[player]].indexDataGroup[i].indexDataBit[PlayerPref.IndexCheck(PlayerPref.menuIndexes[PlayerPref.currentPlayerLayer[player]].indexDataGroup[i].indexDataBit.Length, player)];

        int currentDataGroup = PlayerPref.menuIndexes[PlayerPref.currentPlayerLayer[player]].dataGroupPoint[PlayerPref.IndexCheck(PlayerPref.menuIndexes[PlayerPref.currentPlayerLayer[player]].dataGroupPoint.Length, player)];
        int index = PlayerPref.menuIndexes[PlayerPref.currentPlayerLayer[player]].indexDataGroup[currentDataGroup].indexDataBit[PlayerPref.IndexCheck(PlayerPref.menuIndexes[PlayerPref.currentPlayerLayer[player]].indexDataGroup[currentDataGroup].indexDataBit.Length, player)];

        //-------Debug
        Debug.Log(PlayerPref.currentPlayerLayer[player] + " " + currentDataGroup + " " + index);
        //-------EndDebug

        switch (currentDataGroup) {
            case 0:
                LoadDataFromDatabase(channelImage, lengthChecker[0]);
                //channelImage.texture = AssetDatabase.data.dataGroups[0].dataBits[PlayerPref.currentChannel].image;
                break;

            case 1:
                LoadSongData(PlayerPref.channels[lengthChecker[0]].references[lengthChecker[1]]);
                break;

            case 2:
                currRush.text = PlayerPref.prefRush.ToString();

                for (var i = 0; i < 2; i++)
                    if (PlayerPref.playerSettings[i].life == 0)
                        playerMenu[i].SetActive(false);
                    else
                        playerMenu[i].SetActive(true);

                currSpeed[player].text = PlayerPref.playerSettings[player].prefSpeed.ToString();
                currLevel[player].text = PlayerPref.songs[PlayerPref.channels[lengthChecker[0]].references[lengthChecker[1]]].levels[lengthChecker[2]].level;
                PlayerPref.playerSettings[player].currentSongLevel = lengthChecker[2];
                LoadDataFromDatabase(players[player].levelBubble, (int)PlayerPref.songs[PlayerPref.channels[lengthChecker[0]].references[lengthChecker[1]]].levels[lengthChecker[2]].stepType);
                break;

                //case MenuState.Confirmation:
                //LoadLevel();
                // break;
        }

        LayerCheck(PlayerPref.currentPlayerLayer);

        foreach (MenuInterfaceGroup menuInterfaceGroup in menuInterface[PlayerPref.currentPlayerLayer[player]].menuInterfaceGroup)
            foreach (GameObject ui in menuInterfaceGroup.uiElements[PlayerPref.IndexCheck(menuInterfaceGroup.uiElements.Length, player)].ui)
                ui.SetActive(false);

        foreach (GameObject ui in menuInterface[PlayerPref.currentPlayerLayer[player]].menuInterfaceGroup[valueInst].uiElements[PlayerPref.IndexCheck(menuInterface[PlayerPref.currentPlayerLayer[player]].menuInterfaceGroup[valueInst].uiElements.Length, player)].ui)
            ui.SetActive(true);
        //for (int i = 0; i < 2; i++)
        // for (int j = 0; j < menuInterface[PlayerPref.currentPlayerLayer[i]].menuInterfaceGroup[currentDataGroup].uiElements.Length; j++)
        // menuInterface[PlayerPref.currentPlayerLayer[i]].menuInterfaceGroup[currentDataGroup].uiElements[j].SetActive(true);
    }

    public void LoadSongData(int referenceValue) {

        DirectoryInfo directory = new DirectoryInfo(PlayerPref.songs[referenceValue].path);
        FileInfo[] temp = directory.GetFiles("*.wav");

        Destroy(previewSong.clip);
        using (WWW song = new WWW("file:///" + temp[0].FullName)) {
            while (!song.isDone) ;
            previewSong.clip = song.GetAudioClip(false);
        }

        previewSong.volume = 1;

        previewSong.pitch = PlayerPref.prefRush;
        previewSong.Play();

        previewSong.time = PlayerPref.songs[referenceValue].previewStart;

        temp = directory.GetFiles("*.PNG");

        Destroy(previewImage.texture);

        using (WWW image = new WWW("file:///" + temp[0].FullName)) {
            while (!image.isDone) ;
            previewImage.texture = image.texture;
        }

        currPreviewEnd = PlayerPref.songs[referenceValue].previewEnd;
        songTitle.text = PlayerPref.songs[referenceValue].name;
        PlayerPref.currSong = referenceValue;
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
                        lengthInstance = PlayerPref.songs[PlayerPref.channels[indexes[0]].references[indexes[1]]].levels.Count;
                        break;
                }
                break;
        }
        return lengthInstance;
    }

    public void LoadLevel() {
        PlayerPref.menuIndexes[PlayerPref.currentPlayerLayer[0]].dataGroupPoint[0] = 1;
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

            if (tempStr.Contains("#SAMPLELENGTH:")) //{
                instance.previewEnd = instance.previewStart + float.Parse(tempStr.Substring(14, tempStr.Length - 1 - 14));
                //instance.previewStart += tempOffset * instance.previewEnd;
            //}

            if (tempStr.Contains("pump-single"))
                level.stepType = StepType.Single;
            else if (tempStr.Contains("pump-double") || tempStr.Contains("pump-routine"))
                level.stepType = StepType.Double;

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

    void LayerCheck(int[] layersInUse) {
        bool[] layerCheck = new bool[menuInterface.Length];

        foreach (int layers in layersInUse)
            layerCheck[layers] = true;

        for (int i = 0; i < layerCheck.Length; i++)
            if (!layerCheck[i] && menuInterface[i].closesWhenNotInLayer)
                foreach (MenuInterfaceGroup menuInterfaceGroup in menuInterface[i].menuInterfaceGroup)
                    foreach (UIElements uiElement in menuInterfaceGroup.uiElements)
                        foreach (GameObject ui in uiElement.ui)
                            ui.SetActive(false);
    }
}

