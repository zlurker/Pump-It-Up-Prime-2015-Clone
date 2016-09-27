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
        public GameObject[] beats;

        public BeatsInfo(float givenBeatTiming, GameObject[] givenBeats) {
            beatTiming = givenBeatTiming;
            beats = givenBeats;
        }

    }

    public float offset;
    public float rush;
    public AudioSource song;
    public Animation grade;
    public Text gradeT;
    public Text comboT;
    public KeyCode[] controls;
    public float allowanceTime;

    public List<BPMData> bpmData;
    public List<SpeedData> speedData;
    public List<BeatsInfo> beats;

    [HideInInspector]
    public float endBpm;
    [HideInInspector]
    public float totalDist;
    float bpm;
    float cRealTime;
    float endTime;
    float dOffset;
    float timerForLongBeat;

    int currentBpm;
    int currentBeat;
    int currentSpeed;
    int combo;

    int longBeatsActive;
    int[] beatsActive = new int[10];

    void Start() {
        offset += Time.realtimeSinceStartup;
        longBeatsActive = 0;
        currentBeat = 0;
        currentBpm = 0;
        bpm *= rush;
        song.pitch = rush;
        bpm = bpmData[0].bpm;
        endTime = (endBpm / bpm) * 60;
        song.Play();
    }

    void Update() {
        cRealTime = Time.realtimeSinceStartup - offset;

        if (currentBpm < bpmData.Count)
            if (bpmData[currentBpm].time / rush < cRealTime) { //Bpm changer
                ChangeBpm(bpmData[currentBpm].bpm, bpmData[currentBpm].beat);
                currentBpm++;
            }

        transform.position = new Vector2(2, ((cRealTime - dOffset) / endTime) * (totalDist * transform.localScale.y));

        // --------------------------------- Everything below is judgement/ input-----------------------------------------------------//
        if (currentBeat < beats.Count)
            if ((beats[currentBeat].beatTiming + (allowanceTime * 2)) / rush <= cRealTime) { //Considered as Late.                
                BeatScore(-1);
            } else if (((beats[currentBeat].beatTiming - allowanceTime) / rush <= cRealTime)) {//when player can start tapping.
                int numberOfBeatsLeft = 0;
                for (var i = 0; i < controls.Length; i++) {
                    if (Input.GetKeyDown(controls[i])) {
                        if (beats[currentBeat].beats[i])
                            Destroy(beats[currentBeat].beats[i]);
                    }
                    if (beats[currentBeat].beats[i]) {
                        if (beats[currentBeat].beats[i].name == "1")
                            numberOfBeatsLeft++;
                    }
                }

                if (numberOfBeatsLeft == 0) {
                    BeatScore(1);
                }
            }

        if (longBeatsActive > 0) {
            if (timerForLongBeat < cRealTime) {
                int longBeatsLeft = 0;
                timerForLongBeat = cRealTime + (0.1f / rush);

                for (var i = 0; i < beatsActive.Length; i++) {
                    if (beatsActive[i] == 1) {
                        longBeatsLeft++;
                        if (Input.GetKey(controls[i])) {
                            longBeatsLeft--;
                        }
                    }
                }
                if (longBeatsLeft != 0)
                    BeatScore(-1);
                else {
                    BeatScore(1);
                }
            }
        }
    }

    void ChangeBpm(float bpmToChange, float currentBeat) {
        float tempOffset = 0;
        bpmToChange *= rush;

        tempOffset = (cRealTime - dOffset) - ((currentBeat / bpm) * 60);
        dOffset += (cRealTime - dOffset) - (((currentBeat / bpmToChange) * 60) + tempOffset); //Adds the offset value that will offset time to transition bpm.

        endTime = (endBpm / bpmToChange) * 60; //Changes ending time.
        bpm = bpmToChange; //Changes the BPM.
    }

    void BeatScore(int givenCombo) {
        grade.Stop();
        grade.Play();

        if (givenCombo > 0) {
            gradeT.text = "PERFECT";
        } else {
            gradeT.text = "MISS";
        }

        combo++;
        comboT.text = combo.ToString();

        for (var i = 0; i < beats[currentBeat].beats.Length; i++) {
            if (beats[currentBeat].beats[i] != null) {
                if (beats[currentBeat].beats[i].name == "2") {
                    longBeatsActive++;
                    beatsActive[i] = 1;
                }

                if (beats[currentBeat].beats[i].name == "3") {
                    longBeatsActive--;
                    beatsActive[i] = 0;
                }
            }
        }
        currentBeat++;
    }
}




