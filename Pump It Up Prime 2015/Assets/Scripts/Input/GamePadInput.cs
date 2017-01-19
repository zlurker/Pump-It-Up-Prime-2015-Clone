using UnityEngine;
using System.Collections;

public class GamePadInput : InputBase {

    void Start() {

    }

    void Update() {
        for (var i = 0; i < 5; i++) {
            if (i != 2) {
                if (Input.GetButtonDown(i.ToString()))
                    InputProcessor(2, i);

                if (Input.GetButton(i.ToString()))
                    InputProcessor(1, i);
            }
        }
    }
}
