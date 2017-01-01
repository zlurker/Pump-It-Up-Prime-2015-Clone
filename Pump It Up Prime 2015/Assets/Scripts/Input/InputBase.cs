using UnityEngine;
using System.Collections;
using UnityEngine.SceneManagement;

public class InputBase : MonoBehaviour {

    public enum GameMode {
        Single, Double
    }

    public static GameMode currentGameMode;
    public static PlayerBase[] players;
    public static int activePlayerIndex;

    void Awake() {
        DontDestroyOnLoad(gameObject);
        players = new PlayerBase[2];
        PlayerPref.sceneValueOffset++;
        ExitLevel();
    }

    protected void InputProcessor(int givenInput, int givenBeat) {
        switch (currentGameMode) {
            case GameMode.Single:
                if (givenBeat > 4) {
                    givenBeat -= 5;
                    players[1].BeatInput(givenInput, givenBeat);
                } else {
                    players[0].BeatInput(givenInput, givenBeat);
                }
                break;
            case GameMode.Double:
                players[activePlayerIndex].BeatInput(givenInput, givenBeat);
                break;
        }
    }

    protected void ExitLevel() {
        SceneManager.LoadScene("Menu");
    }
}
