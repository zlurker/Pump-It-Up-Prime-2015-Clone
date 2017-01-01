using UnityEngine;
using System.Collections;

public class Expnasion2 : Root {

	// Use this for initialization
	void Start () {
	
	}
	
	// Update is called once per frame
	void Update () {
        if (add) {
            add = false;
            Addition();
        }
    }
}
