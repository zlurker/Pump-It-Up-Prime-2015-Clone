using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;

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
        public float time;

        public SpeedData(float givenBeat, float givenSpeed, float givenTimeForChange, float givenTime) {
            beat = givenBeat;
            speed = givenSpeed;
            timeForChange = givenTimeForChange;
            time = givenTime;
        }
    }

    [System.Serializable]
    public struct BeatsInfo {
        public float beatTiming;
        public int[] beats;

        public BeatsInfo(float givenBeatTiming, int[] givenBeats) {
            beatTiming = givenBeatTiming;
            beats = givenBeats;
        }

    }
   
    
    public float offset;
    public float rush;
    public AudioSource song;
    public Animation grade;
    public Text comboT;
    public List<BPMData> bpmData;
    public List<SpeedData> speedData;
    public List<BeatsInfo> beats;

    [HideInInspector] public float endBpm;
    [HideInInspector] public float totalDist;
    float bpm;
    float cRealTime;
    float endTime;
    float dOffset;
    float timerForGrade;

    int currentBpm;
    int currentBeat;
    int currentSpeed;

    void Start() {
        offset += Time.realtimeSinceStartup;
        currentBeat = 1;
        currentBpm = 0;
        bpm *= rush;
        song.pitch = rush;
        bpm = bpmData[0].bpm;
        endTime = (endBpm / bpm) * 60;
        song.Play();
    }

    void Update() {
        cRealTime = Time.realtimeSinceStartup - offset;

        if (currentBeat < beats.Count)
            if (beats[currentBeat].beatTiming / rush < cRealTime) { //Beat checker.
                currentBeat++;
                grade.Stop();
                grade.Play();

                comboT.text = currentBeat.ToString();
                Debug.Log("Beat " + currentBeat);
            }

        if (currentBpm < bpmData.Count)
            if (bpmData[currentBpm].time / rush < cRealTime) { //Bpm changer
                ChangeBpm(bpmData[currentBpm].bpm, bpmData[currentBpm].beat);
                currentBpm++;
            }

        //if (currentSpeed < speedData.Count)
            //if (speedData[currentBpm]. / rush < cRealTime) { //Bpm changer
              //  currentSpeed++;
            //}
        transform.position = new Vector2(2, ((cRealTime - dOffset) / endTime) * (totalDist * transform.localScale.y));
    }

    void ChangeBpm(float bpmToChange, float currentBeat) {
        float tempOffset = 0;
        bpmToChange *= rush;

        tempOffset = (cRealTime - dOffset) - ((currentBeat / bpm) * 60); //Finds the offset of current bpm.
        dOffset += (cRealTime - dOffset) - (((currentBeat / bpmToChange) * 60) + tempOffset); //Adds the offset value that will offset time to transition bpm.

        endTime = (endBpm / bpmToChange) * 60; //Changes ending time.
        bpm = bpmToChange; //Changes the BPM.
    }
}

