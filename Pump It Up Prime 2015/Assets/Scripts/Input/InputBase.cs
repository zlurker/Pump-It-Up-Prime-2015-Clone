using UnityEngine;
using System.Collections;

public class InputBase : MonoBehaviour {

    public enum GameMode {
        Single, Double
    }

    public GameMode currentGameMode;
    public StepchartMover[] players;
    public int activePlayerIndex;

    protected void InputProcessor(int givenInput, int givenBeat) {
        switch (currentGameMode) {
            case GameMode.Single:
                if (givenBeat > 4) {
                    givenBeat -= 5;
                    players[1].BeatInput(givenInput, givenBeat);
                } else
                    players[0].BeatInput(givenInput, givenBeat);
                break;
            case GameMode.Double:
                players[activePlayerIndex].BeatInput(givenInput, givenBeat);
                break;
        }
    }
}
