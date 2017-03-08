using UnityEngine;
using System.Collections;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using System.IO;

public class ScoreScreenHandler : PlayerBase {
    [System.Serializable]
    public struct GradeRequirements {
        public int grade;
        public float maxPercentageAllowed;
    }

    [System.Serializable]
    public struct GradingCiteria {
        public GradeRequirements[] requirements;
        public Sprite gradeImage;
    }

    [System.Serializable]
    public struct TextBox {
        public Text[] textBoxes;
    }

    public GradingCiteria[] gradingCiteria;
    public TextBox[] scoreBoxes;
    public SpriteRenderer[] grades;
    public Text displayTimer;
    public RawImage previewImage;
    public float timer;

    void Start() {
        for (var i = 0; i < 2; i++) {
            if (PlayerPref.playerSettings[i].life > 0) {
                for (var j = 0; j < PlayerPref.playerSettings[i].playerScore.Length; j++)
                    scoreBoxes[i].textBoxes[j].text = PlayerPref.playerSettings[i].playerScore[j].ToString();

                grades[i].sprite = gradingCiteria[TabulateGrade(i)].gradeImage;
            }
        }

        DirectoryInfo directory = new DirectoryInfo(PlayerPref.songs[PlayerPref.currSong].path);
        FileInfo[] temp = directory.GetFiles("*.PNG");

        using (WWW image = new WWW("file:///" + temp[0].FullName)) {
            while (!image.isDone) ;
            previewImage.texture = image.texture;
        }

        InputBase.players[0] = this;
        InputBase.players[1] = this;
        timer = Time.realtimeSinceStartup + 10f;
    }

    int TabulateGrade(int playerToTabulate) {
        float totalBeats = 0;
        int grade = 0;
        for (var i = 0; i < 6; i++)
            totalBeats += PlayerPref.playerSettings[playerToTabulate].playerScore[i];

        for (var i = 0; i < gradingCiteria.Length; i++) {
            foreach (GradeRequirements currentRequirement in gradingCiteria[i].requirements)
                if ((PlayerPref.playerSettings[playerToTabulate].playerScore[currentRequirement.grade] / totalBeats) * 100 > currentRequirement.maxPercentageAllowed)
                    return grade;

            grade = i;
        }
        return grade;
    }

    void Update() {
        if (Input.GetKeyDown(KeyCode.Escape) || timer < Time.realtimeSinceStartup)
            SceneManager.LoadScene(SceneIndex.menu);
        else
            displayTimer.text = Mathf.Floor(timer - Time.realtimeSinceStartup).ToString();
    }

    public override void BeatInput(int inputValue, int beat) {
        if (beat == 2) {
            SceneManager.LoadScene(SceneIndex.menu);
        }
    }
}
