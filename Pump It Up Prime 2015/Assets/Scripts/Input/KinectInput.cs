using UnityEngine;
using System.Collections;

public class KinectInput : InputBase {

    public int prevIndex;
    private float timer = 0;
    float duration = 2;

    void OnCollisionStay(Collision coll) {

        if (coll.transform.CompareTag("DancePad")) {

            int padIndex = int.Parse(coll.transform.name);

            if (players[0])
                if (Time.time > timer || !(players[0] is SubMenuController)) {
                    InputProcessor(2, padIndex);
                    prevIndex = padIndex;

                    if (players[0] is SubMenuController)
                        timer = Time.time + duration;
                }
        }
    }
}
