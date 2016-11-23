using UnityEngine;
using System.Collections;

public class SubMenuController : MonoBehaviour {

    public MenuController mainController;
    public int playerIndex;

    void Start () {
	
	}
	
    public void ChangeSpeed(float value) {
        if (PlayerPref.playerSettings[playerIndex].prefSpeed + value > 0 && PlayerPref.playerSettings[playerIndex].prefSpeed + value < 7)
            PlayerPref.playerSettings[playerIndex].prefSpeed += value;
        mainController.RefreshUI();
    }
}
