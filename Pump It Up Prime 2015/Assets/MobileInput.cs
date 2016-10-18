using UnityEngine;
using System.Collections;

public class MobileInput : MonoBehaviour {

    public StepchartMover stepchart;

	void Start () {
	
	}
	
	// Update is called once per frame
	void Update () {
	
	}

    public void Tap(int key) {
        stepchart.BeatInput(key, 2);
    }
}
