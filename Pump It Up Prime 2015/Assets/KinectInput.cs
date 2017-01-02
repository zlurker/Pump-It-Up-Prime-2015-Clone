using UnityEngine;
using System.Collections;

public class KinectInput : InputBase {

    public int prevIndex;
    float timer;
    float duration = 1;
    public static bool inMenu;

    void OnCollisionStay(Collision coll) {
        if (coll.transform.CompareTag("DancePad")) {

            int padIndex = int.Parse(coll.transform.name);

            if (Time.time >= timer || !inMenu) {
                InputProcessor(2, padIndex);
                prevIndex = padIndex;
                timer = Time.time + duration;
                //Debug.Log(coll.transform.name);
            }
        }
    }
}
