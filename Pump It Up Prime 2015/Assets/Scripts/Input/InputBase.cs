using UnityEngine;
using System.Collections;
using UnityEngine.SceneManagement;

public class InputBase : MonoBehaviour {

    public enum GameMode {
        Single, Double
    }

    public static GameMode currentGameMode;
    public static PlayerBase[] players= new PlayerBase[2];
    public static int activePlayerIndex;

    void Awake() {
        DontDestroyOnLoad(gameObject);
    }

    protected void InputProcessor(int givenInput, int givenBeat) {
        switch (currentGameMode) {
            case GameMode.Single:
                int playerIndex = 0;

                if (givenBeat > 4) {
                    givenBeat -= 5;
                    playerIndex = 1;
                }
                if (players[playerIndex])
                    players[playerIndex].BeatInput(givenInput, givenBeat);
                break;
            case GameMode.Double:
                if (players[activePlayerIndex])
                    players[activePlayerIndex].BeatInput(givenInput, givenBeat);
                break;
        }
    }

    public void ExitLevel() {
        SceneManager.LoadScene(SceneIndex.menu);
    }
}
