using UnityEngine;
using System.Collections;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using System.IO;

public class ScoreScreenHandler : PlayerBase {

    [System.Serializable]
    public struct TextBox {
        public Text[] textBoxes;
    }

    public TextBox[] scoreBoxes;
    public Text displayTimer;
    public RawImage previewImage;
    public float timer;

    void Start() {
        for (var i = 0; i < 2; i++) {
            scoreBoxes[i].textBoxes[0].text = PlayerPref.playerSettings[i].playerScore.perfect.ToString();
            scoreBoxes[i].textBoxes[1].text = PlayerPref.playerSettings[i].playerScore.miss.ToString();
            scoreBoxes[i].textBoxes[2].text = PlayerPref.playerSettings[i].playerScore.maxCombo.ToString();
            scoreBoxes[i].textBoxes[3].text = PlayerPref.playerSettings[i].playerScore.score.ToString();
        }

        DirectoryInfo directory = new DirectoryInfo(PlayerPref.songs[PlayerPref.songIndex].path);
        FileInfo[] temp = directory.GetFiles("*.PNG");

        using (WWW image = new WWW("file:///" + temp[0].FullName)) {
            while (!image.isDone) ;
            previewImage.texture = image.texture;
        }

        InputBase.players[0] = this;
        InputBase.players[1] = this;
        timer = Time.time + 10f;
    }

    void Update() {
        if (Input.GetKeyDown(KeyCode.Escape) || timer < Time.time)
            SceneManager.LoadScene(SceneIndex.menu);
        else
            displayTimer.text = Mathf.Floor(timer - Time.time).ToString();
    }

    public override void BeatInput(int inputValue, int beat) {
        if (beat == 2) {
            SceneManager.LoadScene(SceneIndex.menu);
        }
    }
}
