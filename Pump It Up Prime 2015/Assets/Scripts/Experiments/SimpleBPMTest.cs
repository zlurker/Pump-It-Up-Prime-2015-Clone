using UnityEngine;
using System.Collections;

public class SimpleBPMTest : MonoBehaviour {

    public float bpm;
    public float endBpm;

    public float offset;
    public float totalDist;
    
    float endTime;
    float initialPos;
    public AudioSource song;

	void Start () {
        initialPos = transform.position.y;
        offset += Time.realtimeSinceStartup;
        endTime = (endBpm / bpm) * 60;
        song.Play();      
	}
	
	void Update () {
        transform.position = new Vector2(2, ((Time.realtimeSinceStartup - offset) / endTime) * totalDist);
	}
}
