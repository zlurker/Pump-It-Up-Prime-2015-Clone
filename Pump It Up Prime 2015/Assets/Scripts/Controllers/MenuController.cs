using UnityEngine;
using System.Collections;
using System.IO;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class MenuController : MonoBehaviour {

    public string path;
    public Text dataPath;
    public Text songTitle;
    //public Text currSpeed;
    public Text currRush;

    public Text[] currSpeed;

    // Use this for initialization
    void Start() {
        path = Application.dataPath;

#if UNITY_ANDROID
		path = Application.persistentDataPath;;
#endif

        dataPath.text = "Put stepchart files here: " + path;

        if (!PlayerPref.songsRegisted) {
            PlayerPref.playerSettings = new PlayerSettings[2];
            PlayerPref.songsRegisted = true;

            DirectoryInfo stepchartDirectory = new DirectoryInfo(Path.Combine(path, "Stepcharts"));
            FileInfo[] stepcharts = stepchartDirectory.GetFiles("*.txt");

            PlayerPref.songs = new string[stepcharts.Length];

            for (var i = 0; i < PlayerPref.songs.Length; i++) {
                PlayerPref.songs[i] = stepcharts[i].FullName.Substring(path.Length + 12, stepcharts[i].FullName.Length - path.Length - 12 - 4);//Song's name.
            }

            PlayerPref.prefRush = 1;

            for (var i = 0; i < 2; i++)
                PlayerPref.playerSettings[i].prefSpeed = 2;
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
        }
        RefreshUI();
    }

    public void RefreshUI() {
        songTitle.text = PlayerPref.songs[PlayerPref.songIndex];

        currRush.text = PlayerPref.prefRush.ToString();

        for (var i = 0; i < 2; i++)
            currSpeed[i].text = PlayerPref.playerSettings[i].prefSpeed.ToString();
    }

    public void LoadLevel() {
        SceneManager.LoadScene(1 + PlayerPref.sceneValueOffset);
    }
}
