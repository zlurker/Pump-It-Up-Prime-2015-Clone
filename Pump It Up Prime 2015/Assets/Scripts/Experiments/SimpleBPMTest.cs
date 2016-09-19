using UnityEngine;
using System.Collections;

public class SimpleBPMTest : MonoBehaviour {

    public float bpm;
    public float endBpm;

    public float offset;
    
    float endTime;
    float initialPos;
	
	void Start () {
        initialPos = transform.position.y;
        offset += Time.realtimeSinceStartup;
        endTime = (endBpm / bpm) * 60;
        
	}
	
	void Update () {
        transform.position = new Vector2(2, initialPos - (initialPos * ((Time.realtimeSinceStartup - offset) / endTime)));
	}
}
