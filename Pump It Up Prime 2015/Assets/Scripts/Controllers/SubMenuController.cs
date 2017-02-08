using UnityEngine;
using System.Collections;
using UnityEngine.UI;
public class SubMenuController : PlayerBase {

    public MainMenu mainController;
    public int playerIndex;
    public GameObject advancedMenu;

    void Awake() {
        InputBase.players[playerIndex] = this;
    }

    public override void BeatInput(int inputValue, int beat) {
        if (PlayerPref.playerSettings[playerIndex].life > 0)
            if (inputValue > 1) {
                int value = 0;
                switch (beat) {
                    case 0:
                        value = -1;
                        break;

                    case 4:
                        value = 1;
                        break;

                    case 2:
                        if (PlayerPref.menuState == MenuState.SelectSongLevel)
                            mainController.LoadLevel();
                        else
                            PlayerPref.menuState += 1;
                        break;

                    case 1:
                    case 3:
                        if (PlayerPref.menuState > 0)
                            PlayerPref.menuState -= 1;
                        break;
                }

                if (value != 0)
                    switch (PlayerPref.menuState) {
                        case MenuState.ChannelSelect:
                            mainController.ChangeChannel(value);
                            break;

                        case MenuState.SelectSong:
                            mainController.ChangeMusicMenu(value);
                            break;

                        case MenuState.SelectSongLevel:
                            ChangeSongLevel(value);
                            break;
                    }

                SecretCodeChecker(beat);
                mainController.RefreshUI();
            }
    }

    public void SecretCodeChecker(int beat) {
        foreach (MainMenu.SecretCodes code in mainController.codes)
            if (beat == code.keyValue[code.playersAtValue[playerIndex]]) {
                if (code.playersAtValue[playerIndex] < code.keyValue.Length)
                    code.playersAtValue[playerIndex]++;

                if (!(code.playersAtValue[playerIndex] < code.keyValue.Length))
                    code.playersAtValue[playerIndex] = 0;

                //Debug.LogFormat("Player {0} is at {1}", playerIndex, code.playersAtValue[playerIndex]);
            } else
                code.playersAtValue[playerIndex] = 0;
    }


    public void ChangeSpeed(float value) {
        if (PlayerPref.playerSettings[playerIndex].prefSpeed + value > 0 && PlayerPref.playerSettings[playerIndex].prefSpeed + value < 7)
            PlayerPref.playerSettings[playerIndex].prefSpeed += value;
        mainController.RefreshUI();
    }

    public void ChangeSongLevel(int value) {
        if (PlayerPref.playerSettings[playerIndex].currentSongLevel + value > -1 && PlayerPref.playerSettings[playerIndex].currentSongLevel + value < PlayerPref.songs[PlayerPref.channels[PlayerPref.currentChannel].references[PlayerPref.currentChannelSong]].levels.Count)
            PlayerPref.playerSettings[playerIndex].currentSongLevel += value;
        mainController.RefreshUI();
    }

    public void ToogleAutoPlay() {
        PlayerPref.playerSettings[playerIndex].autoPlay = true ? !PlayerPref.playerSettings[playerIndex].autoPlay : PlayerPref.playerSettings[playerIndex].autoPlay;
    }

    public void AdvancedMenuOption() {
        if (!advancedMenu.activeInHierarchy)
            advancedMenu.SetActive(true);
        else
            advancedMenu.SetActive(false);
    }
}
