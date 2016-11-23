using UnityEngine;
using System.Collections;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class ScoreScreenHandler : MonoBehaviour {

    [System.Serializable]
    public struct TextBox {
        public Text[] textBoxes;
    }

    public TextBox[] scoreBoxes;

    void Start() {
        for (var i = 0; i < 2; i++) {
            scoreBoxes[i].textBoxes[0].text = PlayerPref.playerSettings[i].playerScore.perfect.ToString();
            scoreBoxes[i].textBoxes[1].text = PlayerPref.playerSettings[i].playerScore.miss.ToString();
            scoreBoxes[i].textBoxes[2].text = PlayerPref.playerSettings[i].playerScore.maxCombo.ToString();
            scoreBoxes[i].textBoxes[3].text = PlayerPref.playerSettings[i].playerScore.score.ToString();
        }
    }

    void Update() {
        if (Input.GetKeyDown(KeyCode.Escape))
            SceneManager.LoadScene(0 + PlayerPref.sceneValueOffset);
    }

}
