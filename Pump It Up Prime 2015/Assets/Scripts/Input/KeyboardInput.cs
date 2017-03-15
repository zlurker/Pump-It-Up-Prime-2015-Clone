using UnityEngine;
using System.Collections;
using UnityEngine.SceneManagement;

public class KeyboardInput : InputBase {

    public KeyCode[] keyboardInputs;

    void Update() {
        for (var i = 0; i < keyboardInputs.Length; i++) {
            if (Input.GetKeyDown(keyboardInputs[i]))
                InputProcessor(2, i);

            if (Input.GetKey(keyboardInputs[i]))
                InputProcessor(1, i);
        }

        if (Input.GetKeyDown(KeyCode.Escape))
            if (players[0] is StepchartMover)
                ExitLevel();
            else if (players[0] is SubMenuController)
                (players[0] as SubMenuController).AdvancedMenuOption();
        
        if (Input.GetKeyDown(KeyCode.F1)) 
            SceneManager.LoadScene(SceneIndex.startUpScreen);
        
    }

}
