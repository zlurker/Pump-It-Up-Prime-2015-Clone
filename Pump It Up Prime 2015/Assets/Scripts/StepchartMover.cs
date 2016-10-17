using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

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
    public struct ScrollData {
        public float beat;
        public float scroll;
        public float time;
        public float dist;

        public ScrollData(float givenBeat, float givenScroll, float givenTime, float givenDist) {
            beat = givenBeat;
            scroll = givenScroll;
            time = givenTime;
            dist = givenDist;
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

    public StepchartReader stepchartBuilder;
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
    public List<ScrollData> scrollData;
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

    float prevBeat;
    float prevDist;

    int currentBpm;
    int currentBeat;
    int currentSpeed;
    int currentScroll;

    public float prevSpeed;

    int combo;

    float endLongBeatTime;
    int longBeatsActive;
    int[] beatsActive = new int[10];
    public int[] holdingDown = new int[10];

    public float iniLength;
    public SpriteRenderer sprite;
    public Transform[] legs;

    public Text points;
    public float totalPoints;

    void Start() {
        //for (var i = 0; i < legs.Length; i++)
        //KinectManager.Instance.legs[i] = legs[i];
        stepchartBuilder.songName = PlayerPref.songName;
        stepchartBuilder.speed = PlayerPref.prefSpeed;
        song.clip = PlayerPref.song;

        stepchartBuilder.CreateTimingData();
        stepchartBuilder.CreateStepchart();

        rush = PlayerPref.prefRush;
        longBeatsActive = 0;
        currentBeat = 0;
        currentSpeed = 0;
        currentBpm = 0;
        currentScroll = 0;
        prevBeat = 0;
        prevSpeed = 0;

        song.pitch = rush;
        bpm = bpmData[0].bpm;
        bpm *= rush;
        endTime = (endBpm / bpm) * 60;
        song.Play();

        endBpm = scrollData[scrollData.Count - 1].beat;
        totalDist = scrollData[scrollData.Count - 1].dist;
        offset = PlayerPref.songOffset;
        offset += Time.realtimeSinceStartup;
    }

    void Update() {
        cRealTime = Time.realtimeSinceStartup - offset;

        while (currentBpm < bpmData.Count && bpmData[currentBpm].time / rush < cRealTime) { //Bpm changer
            ChangeBpm(bpmData[currentBpm].bpm, bpmData[currentBpm].beat);
            currentBpm++;
        }

        while (currentSpeed < speedData.Count && speedData[currentSpeed].time / rush < cRealTime) { //Speed changer
            if (currentSpeed - 1 > -1)
                prevSpeed = speedData[currentSpeed - 1].speed;
            currentSpeed++;
        }

        while (currentScroll < scrollData.Count - 1 && scrollData[currentScroll].time / rush < cRealTime) {
            currentScroll++;

            endBpm = scrollData[currentScroll].beat - scrollData[currentScroll - 1].beat;
            endTime = (endBpm / bpm) * 60;
            prevBeat = scrollData[currentScroll - 1].beat;
            prevDist = scrollData[currentScroll - 1].dist;

            totalDist = scrollData[currentScroll].dist - prevDist;
        }


        if (currentSpeed - 1 > 0)
            ChangeSpeed(speedData[currentSpeed - 1].speed, speedData[currentSpeed - 1].time / rush, speedData[currentSpeed - 1].timeForChange / rush);

        transform.position = new Vector2(2, (prevDist + (((cRealTime - dOffset - ((prevBeat / bpm) * 60)) / endTime) * (totalDist))) * transform.localScale.y); //Movement

        if (Input.GetKeyDown(KeyCode.Escape))
            SceneManager.LoadScene("Menu");

        #region Judgement
        // --------------------------------- Everything below is judgement/ input-----------------------------------------------------//
        if (currentBeat < beats.Count)
            if ((beats[currentBeat].beatTiming + (allowanceTime)) / rush <= cRealTime) { //Considered as Late.     
                BeatScore(-1);
                currentBeat++;
            } //when player can start tapping.
        #endregion
    }

    #region Stepchart Effects
    void ChangeSpeed(float speedToChange, float startTime, float givenTime) {
        if (cRealTime > startTime + givenTime) {
            if (transform.localScale.y != speedToChange) {
                transform.localScale = new Vector3(1, speedToChange);
                float scaleValue = 0;
                scaleValue = iniLength / sprite.bounds.extents.y;

                foreach (Transform child in transform) {
                    if (child != sprite.transform && child.tag != "LongBeat")
                        child.localScale = new Vector2(2.5f, 2.5f * scaleValue);
                }
            }
        } else {
            transform.localScale = new Vector2(1, prevSpeed + ((speedToChange - prevSpeed) * ((cRealTime - startTime) / givenTime)));
            float scaleValue = 0;
            scaleValue = iniLength / sprite.bounds.extents.y;

            foreach (Transform child in transform) {
                if (child != sprite.transform && child.tag != "LongBeat")
                    child.localScale = new Vector2(2.5f, 2.5f * scaleValue);
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
    #endregion

    public void BeatInput(int inputValue, int beat) {
        if (currentBeat < beats.Count)
        if ((beats[currentBeat].beatTiming - allowanceTime) / rush <= cRealTime)
            if (beats[currentBeat].beats[beat] - inputValue <= 0) {
                beats[currentBeat].beats[beat] = 0;

                int tempBeatValue = 0;

                foreach (int beatValue in beats[currentBeat].beats)
                    tempBeatValue += beatValue;

                if (tempBeatValue == 0) {
                    BeatScore(1);
                    currentBeat++;
                }
            }

    }

    void BeatScore(int givenCombo) {
        grade.Stop();
        grade.Play();

        if (givenCombo > 0) {
            if (combo < 0)
                combo = 0;
            else
                combo++;

            totalPoints += 1000;
            gradeT.text = "PERFECT";

            string tempPoints = totalPoints.ToString();

            if (10 - tempPoints.Length > 0) {
                for (var i = 0; i < 10 - tempPoints.Length; i++) {
                    tempPoints = "0" + tempPoints;
                }
            }

            points.text = tempPoints;
        } else {
            if (combo > 0)
                combo = 0;
            else
                combo--;

            gradeT.text = "MISS";
        }

        comboT.text = Mathf.Abs(combo).ToString();
    }
}




