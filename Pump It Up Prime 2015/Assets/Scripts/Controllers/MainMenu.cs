using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class MainMenu : MonoBehaviour {

    public string path;
    public Text dataPath;
    public Text songTitle;
    //public Text currSpeed;
    public Text currRush;

    public GameObject[] playerMenu;
    public Text[] currSpeed;
    public Text[] currLevel;

    //string test = "#METER:15;";
    // Use this for initialization
    void Start() {
        //Debug.Log(test.Remove(0, 7));
        path = Application.dataPath;

#if UNITY_ANDROID
		path = Application.persistentDataPath;;
#endif

        dataPath.text = "Put stepchart files here: " + path;

        if (!PlayerPref.songsRegisted) {
            LoadSongsFromDirectory();

            PlayerPref.playerSettings = new PlayerSettings[2];
            PlayerPref.songsRegisted = true;
            PlayerPref.prefRush = 1;

            for (var i = 0; i < 2; i++) {
                PlayerPref.playerSettings[i].prefSpeed = 2;
            }

            PlayerPref.playerSettings[0].life = 5;
            //PlayerPref.playerSettings[1].life = 5;
        }

        for (var i = 0; i < 2; i++) {
            PlayerPref.playerSettings[i].playerScore.perfect = 0;
            PlayerPref.playerSettings[i].playerScore.miss = 0;
            PlayerPref.playerSettings[i].playerScore.maxCombo = 0;
            PlayerPref.playerSettings[i].playerScore.score = 0;
        }

        RefreshUI();
    }

    public void ChangeRush(float value) {
        if (PlayerPref.prefRush + value > 0.7f && PlayerPref.prefRush + value < 1.6f)
            PlayerPref.prefRush += value;
        RefreshUI();
    }

    public void ChangeMusicMenu(int value) {
        if (PlayerPref.songIndex + value > -1 && PlayerPref.songIndex + value < PlayerPref.songs.Length) {
            PlayerPref.songIndex += value;
            for (var i = 0; i < 2; i++)
                PlayerPref.playerSettings[i].currentSongLevel = 0;
        }
        RefreshUI();
    }

    public void RefreshUI() {
        songTitle.text = PlayerPref.songs[PlayerPref.songIndex].name;
        currRush.text = PlayerPref.prefRush.ToString();

        for (var i = 0; i < 2; i++) {
            if (PlayerPref.playerSettings[i].life == 0)
                playerMenu[i].SetActive(false);
            else
                playerMenu[i].SetActive(true);

            currSpeed[i].text = PlayerPref.playerSettings[i].prefSpeed.ToString();
            currLevel[i].text = PlayerPref.songs[PlayerPref.songIndex].levels[PlayerPref.playerSettings[i].currentSongLevel];
        }
    }

    public void LoadLevel() {
        SceneManager.LoadScene(1 + PlayerPref.sceneValueOffset);
    }

    public void KillPlayer(int player) {
        PlayerPref.playerSettings[player].life = 0;
        RefreshUI();
    }

    void LoadSongsFromDirectory() {
        DirectoryInfo stepchartDirectory = new DirectoryInfo(Path.Combine(path, "Stepcharts"));
        FileInfo[] stepcharts = stepchartDirectory.GetFiles("*.txt");
        PlayerPref.songs = new SongData[stepcharts.Length];

        for (var i = 0; i < PlayerPref.songs.Length; i++) {
            PlayerPref.songs[i].name = stepcharts[i].FullName.Substring(path.Length + 12, stepcharts[i].FullName.Length - path.Length - 12 - 4);

            PlayerPref.songs[i].levels = new List<string>();
            ReadStepchartLevelData(i, stepcharts[i].FullName);
        }
    }

    void ReadStepchartLevelData(int songIndex, string songPath) {
        StreamReader stepchart;
        string tempStr;
        string level = "";
        stepchart = File.OpenText(songPath);

        while ((tempStr = stepchart.ReadLine()) != null) {

            if (tempStr.Contains("pump-single"))
                level = "S";
            else if (tempStr.Contains("pump-double"))
                level = "D";

            if (tempStr.Contains("#METER:")) {
                level += tempStr.Remove(0, 7);
                level = level.Remove(level.Length - 1, 1);
                PlayerPref.songs[songIndex].levels.Add(level);
            }
        }

        stepchart.Close();
    }
}
