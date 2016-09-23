using UnityEngine;
using System.Collections;
using UnityEditor;

public class StepchartMover : MonoBehaviour {

    public float bpm;
    public float endBpm;
    public float offset;    
    public float totalDist;
    public float rush;
    public AudioSource song;

    float cRealTime;
    float endTime;
    float dOffset;

    public float beatNo;
    public float beatNo2;

    bool test;
    bool test2;

    float currentBeat;
    
	void Start () {
        offset += Time.realtimeSinceStartup;      
        bpm *= rush;
        song.pitch = rush;
        endTime = (endBpm / bpm) * 60;
        song.Play();
        Debug.Log(endTime);
	}
	
	void Update () {
        cRealTime = Time.realtimeSinceStartup - offset;
        transform.position = new Vector2(2, ((cRealTime - dOffset) / endTime) * totalDist);

        if (cRealTime > (7.5f/rush)&& !test){
            ChangeBpm(60, beatNo);
            test = true;
        }

        if (cRealTime > (17.5f/rush) && !test2) {
            ChangeBpm(160, beatNo2);
            test2 = true;
        }
        //if (cRealTime > ((beatNo/160) * 60) + ((10/100) * 60)) {
          //  ChangeBpm(160, beatNo2);
        //}
    }

    void ChangeBpm(float bpmToChange, float currentBeat) {
        float tempOffset = 0;
        bpmToChange *= rush;

        tempOffset = (cRealTime - dOffset) - ((currentBeat / bpm) * 60); //Finds the offset of current bpm.
        dOffset += (cRealTime - dOffset) -(((currentBeat / bpmToChange) * 60) + tempOffset); //Adds the offset value that will offset time to transition bpm.
        endTime = (endBpm / bpmToChange) * 60; //Changes ending time.
        bpm = bpmToChange; //Changes the BPM.
    }
}

