using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UpwardsInheritanceTest : MonoBehaviour {
    [System.Serializable]
    public class Class2:Class1 {
        public int bullshit;
        public bool testshit;
    }

    [System.Serializable]
    public class Class1 {
        public int test;
    }

    public Class1 testing;
    public Class2 testing2;
	// Use this for initialization
	void Start () {
        testing2 = (Class2)testing;        
	}
	
	// Update is called once per frame
	void Update () {
		
	}
}
