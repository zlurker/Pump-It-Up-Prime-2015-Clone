using UnityEngine;
using System.Collections;

public class KeyboardInput : InputBase {

    public KeyCode[] keyboardInputs;

    void Start() {

    }

    void Update() {
        for (var i = 0; i < keyboardInputs.Length; i++) {
            if (Input.GetKeyDown(keyboardInputs[i])) {
                InputProcessor(2, i);
                //Debug.Log(i);
            }

            if (Input.GetKey(keyboardInputs[i])) {
                InputProcessor(1, i);
                //Debug.Log(i);
            }
        }

        if (Input.GetKeyDown(KeyCode.Escape))
            players[0].ExitLevel();
    }

}
