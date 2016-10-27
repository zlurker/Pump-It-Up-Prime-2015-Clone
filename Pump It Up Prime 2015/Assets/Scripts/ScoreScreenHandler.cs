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
        perfect.text = PlayerPref.playerScore.perfect.ToString();
        miss.text = PlayerPref.playerScore.miss.ToString();
        maxCombo.text = PlayerPref.playerScore.maxCombo.ToString();
        score.text = PlayerPref.playerScore.score.ToString();
    }

    void Update() {
        if (Input.GetKeyDown(KeyCode.Escape))
            SceneManager.LoadScene(0 + PlayerPref.sceneValueOffset);
    }

}
