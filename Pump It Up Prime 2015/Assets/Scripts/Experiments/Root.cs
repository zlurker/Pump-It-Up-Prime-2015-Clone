using UnityEngine;
using System.Collections;

public class Root : MonoBehaviour {

    public static int test;
    public bool add;

    protected void Addition() {
        test++;
        Debug.Log(test);
    }
}
