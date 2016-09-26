using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class StepchartMover : MonoBehaviour {

    [System.Serializable]
    public struct BPMData {
        public float beat;
        public float bpm;
        public float time;

        public BPMData(float givenBeat, float givenBPM, float calculatedTime) {
            beat = givenBeat;
            bpm = givenBPM;
            time = calculatedTime;
        }
    }

    [System.Serializable]
    public struct SpeedData {
        public float beat;
        public float speed;
        public float timeForChange;

        public SpeedData(float givenBeat, float givenSpeed, float givenTimeForChange) {
            beat = givenBeat;
            speed = givenSpeed;
            timeForChange = givenTimeForChange;
        }
    }

    public float bpm;
    public float endBpm;
    public float offset;    
    public float totalDist;
    public float rush;
    public AudioSource song;
    public List<BPMData> bpmData;
    public List<SpeedData> speedData; 

    float cRealTime;
    float endTime;
    float dOffset;
    
	void Start () {
        offset += Time.realtimeSinceStartup;      
        bpm *= rush;
        song.pitch = rush;
        endTime = (endBpm / bpm) * 60;
        song.Play();
	}
	
	void Update () {
        cRealTime = Time.realtimeSinceStartup - offset;
        transform.position = new Vector2(2, ((cRealTime - dOffset) / endTime) * (totalDist* transform.localScale.y));
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

