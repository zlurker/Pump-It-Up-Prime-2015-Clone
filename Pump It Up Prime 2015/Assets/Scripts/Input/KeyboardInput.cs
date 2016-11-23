using UnityEngine;
using System.Collections;

public class KeyboardInput : InputBase {

    public KeyCode[] keyboardInputs;
    public StepchartMover stepchart;
    // Use this for initialization
    void Start() {

    }

    // Update is called once per frame
    void Update() {
        for (var i = 0; i < keyboardInputs.Length; i++) {
            if (Input.GetKeyDown(keyboardInputs[i]))
                InputProcessor(2, i);

            if (Input.GetKey(keyboardInputs[i]))
                InputProcessor(1, i);
        }

        if (Input.GetKeyDown(KeyCode.Escape))
            stepchart.ExitLevel();
    }

}
