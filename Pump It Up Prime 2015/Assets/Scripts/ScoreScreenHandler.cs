using UnityEngine;
using System.Collections;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class ScoreScreenHandler : MonoBehaviour {

    public Text perfect;
    public Text miss;
    public Text maxCombo;
    public Text score;

    void Start() {
        perfect.text = PlayerPref.playerSettings[0].playerScore.perfect.ToString();
        miss.text = PlayerPref.playerSettings[0].playerScore.miss.ToString();
        maxCombo.text = PlayerPref.playerSettings[0].playerScore.maxCombo.ToString();
        score.text = PlayerPref.playerSettings[0].playerScore.score.ToString();
    }

    void Update() {
        if (Input.GetKeyDown(KeyCode.Escape))
            SceneManager.LoadScene(0 + PlayerPref.sceneValueOffset);
    }

}
