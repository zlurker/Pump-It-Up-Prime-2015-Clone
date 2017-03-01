using UnityEngine;
using System.Collections;
using UnityEngine.UI;
public class SubMenuController : PlayerBase {


    public MainMenu mainController;
    public int playerIndex;
    public GameObject advancedMenu;

    public GameObject cmWindow;
    public RawImage currentMod;
    public RawImage levelBubble;

    void Awake() {
        InputBase.players[playerIndex] = this;
    }

    public override void BeatInput(int inputValue, int beat) {
        if (PlayerPref.playerSettings[playerIndex].life > 0)
            if (inputValue > 1) {
                switch (beat) {
                    case 0:
                        MenuElementNavigation(-1);
                        break;

                    case 4:
                        MenuElementNavigation(1);
                        break;

                    case 2:
                        MenuLayerNavigation(1);
                        break;

                    case 1:
                    case 3:
                        MenuLayerNavigation(-1);
                        break;
                }

                SecretCodeChecker(beat);
                mainController.RefreshUI(playerIndex);
            }
    }

    void MenuLayerNavigation(int value) {
        int leftoverValue;
        PlayerPref.menuIndexes[PlayerPref.currentPlayerLayer[playerIndex]].dataGroupPoint[PlayerPref.IndexCheck(PlayerPref.menuIndexes[PlayerPref.currentPlayerLayer[playerIndex]].dataGroupPoint.Length, playerIndex)] = PlayerPref.ScrollAnArray(PlayerPref.menuIndexes[PlayerPref.currentPlayerLayer[playerIndex]].dataGroupPoint[PlayerPref.IndexCheck(PlayerPref.menuIndexes[PlayerPref.currentPlayerLayer[playerIndex]].dataGroupPoint.Length, playerIndex)], value, PlayerPref.menuIndexes[PlayerPref.currentPlayerLayer[playerIndex]].indexDataGroup.Length, out leftoverValue);

        switch (leftoverValue) {
            case -1:
                PlayerPref.currentPlayerLayer[playerIndex] = PlayerPref.ScrollAnArray(PlayerPref.currentPlayerLayer[playerIndex], leftoverValue, PlayerPref.menuIndexes.Length);
                break;
            case 1:
                //Confirmation
                break;
        }


        mainController.RefreshUI(playerIndex);
    }

    void MenuElementNavigation(int value) {
        int currentDataGroup = PlayerPref.menuIndexes[PlayerPref.currentPlayerLayer[playerIndex]].dataGroupPoint[PlayerPref.IndexCheck(PlayerPref.menuIndexes[PlayerPref.currentPlayerLayer[playerIndex]].dataGroupPoint.Length, playerIndex)];
        int index = PlayerPref.IndexCheck(PlayerPref.menuIndexes[PlayerPref.currentPlayerLayer[playerIndex]].indexDataGroup[currentDataGroup].indexDataBit.Length, playerIndex);

        Debug.Log(PlayerPref.menuIndexes[PlayerPref.currentPlayerLayer[playerIndex]].indexDataGroup[currentDataGroup].indexDataBit.Length);
        PlayerPref.menuIndexes[PlayerPref.currentPlayerLayer[playerIndex]].indexDataGroup[currentDataGroup].indexDataBit[index] += value;
        mainController.RefreshUI(playerIndex);
    }

    public void SecretCodeChecker(int beat) {
        foreach (MainMenu.SecretCodes code in mainController.codes)
            if (beat == code.keyValue[code.playersAtValue[playerIndex]]) {
                if (code.playersAtValue[playerIndex] < code.keyValue.Length)
                    code.playersAtValue[playerIndex]++;

                if (!(code.playersAtValue[playerIndex] < code.keyValue.Length)) {
                    PlayerPref.currentPlayerLayer[playerIndex] = code.resultToPlayer;

                    code.playersAtValue[playerIndex] = 0;
                }
            } else
                code.playersAtValue[playerIndex] = 0;
    }


    public void ChangeSpeed(float value) {
        if (PlayerPref.playerSettings[playerIndex].prefSpeed + value > 0 && PlayerPref.playerSettings[playerIndex].prefSpeed + value < 7)
            PlayerPref.playerSettings[playerIndex].prefSpeed += value;
        mainController.RefreshUI(playerIndex);
    }

    public void ChangeSongLevel(int value) {
        if (PlayerPref.playerSettings[playerIndex].currentSongLevel + value > -1 && PlayerPref.playerSettings[playerIndex].currentSongLevel + value < PlayerPref.songs[PlayerPref.channels[PlayerPref.currentChannel].references[PlayerPref.currentChannelSong]].levels.Count)
            PlayerPref.playerSettings[playerIndex].currentSongLevel += value;
        mainController.RefreshUI(playerIndex);
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
