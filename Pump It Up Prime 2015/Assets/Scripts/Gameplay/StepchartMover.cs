using UnityEngine;
using System.Collections;
using System.IO;
using System.Collections.Generic;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class StepchartMover : PlayerBase {

    #region Data Structures
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
    public struct DelayData {
        public float beat;
        public float delay;
        public float time;

        public DelayData(float givenBeat, float givenDelay, float calculatedTime) {
            beat = givenBeat;
            delay = givenDelay;
            time = calculatedTime;
        }
    }

    [System.Serializable]
    public struct WarpInfo {
        public float beat;
        public float warp;

        public WarpInfo(float givenBeat, float givenWarp) {
            beat = givenBeat;
            warp = givenWarp;
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
        public List<GameObject> destroyOnPerf;
        public GameObject[] allignWithSeqZone;
        public GameObject[] scaleDownToSeqZone;

        public BeatsInfo(float givenBeatTiming, int[] givenBeats, List<GameObject> givenDestroyed, GameObject[] givenAllign, GameObject[] givenScaleDown) {
            beatTiming = givenBeatTiming;
            beats = givenBeats;
            destroyOnPerf = givenDestroyed;
            allignWithSeqZone = givenAllign;
            scaleDownToSeqZone = givenScaleDown;
        }
    }

    [System.Serializable]
    public struct LaneInfo {
        public List<int> beatPositions;
        public int currentBeatInLane;
    }

    #endregion

    public StepchartReader stepchartBuilder;
    public float offset;
    public float rush;
    public AudioSource song;
    public Sprite[] grades;

    public float allowanceTime;

    public List<BPMData> bpmData;
    public List<DelayData> delayData;
    public List<SpeedData> speedData;
    public List<ScrollData> scrollData;
    public List<BeatsInfo> beats;
    public List<WarpInfo> warps;
    public LaneInfo[] lanesInfo;
    public UIInstance uiToUse;

    [HideInInspector]
    public float endBpm;
    [HideInInspector]
    public float totalDist;
    [HideInInspector]
    public float beatScale;
    float bpm;
    float cRealTime;
    float endTime;
    float dOffset;

    float originalTime;

    float prevBeat;
    float prevDist;
    float prevOffset;

    int currentBpm;
    int currentDelay;
    int currentBeat;
    int currentSpeed;
    int currentScroll;

    public float prevSpeed;

    int combo;

    public float iniLength;
    public SpriteRenderer sprite;
    public Transform[] legs;

    public Text points;
    public float totalPoints;
    public MainPlayerController playerManager;
    public int index;
    public RectTransform healthPivot;

    float currHealth;
    float maxHealth = 100;
    SpeedData currentSpeedData;

    public void InitialiseStepchart(int playerIndex) {
        //for (var i = 0; i < legs.Length; i++)
        //KinectManager.Instance.legs[i] = legs[i];
        index = playerIndex;

        stepchartBuilder.speed = PlayerPref.playerSettings[playerIndex].prefSpeed;
        stepchartBuilder.stepchartMover = this;
        DirectoryInfo songFolder = new DirectoryInfo(PlayerPref.songs[PlayerPref.currSong].path);
        FileInfo[] stepchart = songFolder.GetFiles("*.ssc");

        stepchartBuilder.CreateTimingData(stepchart[0].FullName);
        //ChangeSpeed(speedData[currentSpeed].speed, 0, 0);
        rush = PlayerPref.prefRush;
        currentBeat = 0;
        currentSpeed = 0;
        currentBpm = 0;
        currentDelay = 0;
        currentScroll = 0;
        prevBeat = 0;
        prevSpeed = 0;

        bpm = bpmData[0].bpm;
        bpm *= rush;
        endTime = (endBpm / bpm) * 60;

        endBpm = scrollData[scrollData.Count - 1].beat;
        totalDist = scrollData[scrollData.Count - 1].dist;

        offset /= rush;

    }

    public void InitialiseUI() {
        uiToUse.stageNumber.gameObject.SetActive(true);
        uiToUse.sequenceZone.SetActive(true);
        uiToUse.healthFrame.gameObject.SetActive(true);
        uiToUse.health.gameObject.SetActive(true);
        uiToUse.grade.gameObject.SetActive(true);
        uiToUse.healthCover.gameObject.SetActive(true);

        // Debug.Log(new Vector3((uiToUse.healthbarPivotSide * uiToUse.health.rectTransform.rect.width) * uiToUse.health.rectTransform.localScale.x, 0) / 2);
        healthPivot = uiToUse.healthbarPivot;
        healthPivot.localPosition = uiToUse.health.rectTransform.localPosition + new Vector3((uiToUse.healthbarPivotSide * uiToUse.health.rectTransform.rect.width) * uiToUse.health.rectTransform.localScale.x, 0) / 2;
        uiToUse.health.transform.parent = healthPivot.transform;

        originalTime = playerManager.cRealTime - offset;
        currHealth = maxHealth / 2;
        PlayerHPHandler(0);
    }

    void Update() {
        cRealTime = playerManager.cRealTime - offset;

        #region Timing Checks

        while (currentBpm < bpmData.Count && bpmData[currentBpm].time / rush <= cRealTime) {
            ChangeBpm(bpmData[currentBpm].bpm, bpmData[currentBpm].beat);
            currentBpm++;
        }

        while (currentSpeed < speedData.Count && speedData[currentSpeed].time / rush <= cRealTime) {
            if (currentSpeed - 1 > -1)
                prevSpeed = speedData[currentSpeed - 1].speed;

            currentSpeedData = speedData[currentSpeed];
            currentSpeed++;
        }

        while (currentScroll < scrollData.Count - 1 && scrollData[currentScroll].time / rush <= cRealTime) {
            currentScroll++;

            endBpm = scrollData[currentScroll].beat - scrollData[currentScroll - 1].beat;
            endTime = (endBpm / bpm) * 60;
            prevBeat = scrollData[currentScroll - 1].beat;
            prevDist = scrollData[currentScroll - 1].dist;

            totalDist = scrollData[currentScroll].dist - prevDist;
        }

        while (currentDelay < delayData.Count && delayData[currentDelay].time / rush <= cRealTime) {
            originalTime = cRealTime - dOffset;
            dOffset += delayData[currentDelay].delay / rush;
            transform.position = new Vector2(transform.position.x, (prevDist + (((delayData[currentDelay].beat - prevBeat) / (scrollData[currentScroll].beat - prevBeat)) * (totalDist))) * transform.localScale.y);

            currentDelay++;
        }
        #endregion

        #region Stepchart Movement
        if (originalTime < cRealTime - dOffset) {
            if (cRealTime >= 0)
                ChangeSpeed(currentSpeedData.speed, currentSpeedData.time / rush, currentSpeedData.timeForChange / rush);

            transform.position = new Vector2(transform.position.x, (prevDist + (((cRealTime - dOffset - ((prevBeat / bpm) * 60)) / endTime) * (totalDist))) * transform.localScale.y);
        }
        #endregion

        #region Judgement
        if (PlayerPref.playerSettings[index].autoPlay) {
            while (currentBeat < beats.Count && (beats[currentBeat].beatTiming / rush <= cRealTime)) {
                for (var i = 0; i < beats[currentBeat].beats.Length; i++)
                    if (beats[currentBeat].beats[i] > 0)
                        BeatInput(2, i);

                currentBeat++;
            }

            for (var i = 0; i < beats[currentBeat].beats.Length; i++)
                BeatInput(0, i);
        }

        while (currentBeat < beats.Count && (beats[currentBeat].beatTiming + (allowanceTime)) / rush <= cRealTime) {
            float missedBeats = 0;
            for (var i = 0; i < beats[currentBeat].beats.Length; i++) {
                if (beats[currentBeat].beats[i] > 0) {
                    lanesInfo[i].currentBeatInLane++;
                    missedBeats++;
                }
            }

            if (missedBeats > 0)
                BeatScore(-1);

            currentBeat++;
        }

        if (!uiToUse.grade.isPlaying)
            uiToUse.grade.gameObject.SetActive(false);

        #endregion
    }

    #region Stepchart Effects
    void ChangeSpeed(float speedToChange, float startTime, float givenTime) {
        if (cRealTime >= startTime + givenTime) {
            if (transform.localScale.y != speedToChange) {
                transform.localScale = new Vector3(1, speedToChange);
                float scaleValue = 0;
                scaleValue = iniLength / sprite.bounds.extents.y;

                foreach (Transform child in transform) {
                    if (child != sprite.transform && child.tag != "LongBeat")
                        child.localScale = new Vector2(beatScale, beatScale * scaleValue);
                }
            }
        } else {
            transform.localScale = new Vector2(1, prevSpeed + ((speedToChange - prevSpeed) * ((cRealTime - startTime) / givenTime)));
            float scaleValue = 0;
            scaleValue = iniLength / sprite.bounds.extents.y;

            foreach (Transform child in transform) {
                if (child != sprite.transform && child.tag != "LongBeat")
                    child.localScale = new Vector2(beatScale, beatScale * scaleValue);
            }
        }
    }

    void ChangeBpm(float bpmToChange, float currentBeat) {
        float tempOffset = 0;
        bpmToChange *= rush;

        if (bpm > 0) {
            tempOffset = (cRealTime - dOffset) - ((currentBeat / bpm) * 60);
            prevOffset = tempOffset;
        } else
            tempOffset = prevOffset;

        if (bpmToChange > 0)
            dOffset += (cRealTime - dOffset) - (((currentBeat / bpmToChange) * 60) + tempOffset); //Adds the offset value that will offset time to transition bpm.

        endTime = (endBpm / bpmToChange) * 60; //Changes ending time.
        bpm = bpmToChange; //Changes the BPM.
    }
    #endregion

    #region Beat Handler
    public override void BeatInput(int inputValue, int beat) {
        if (isActiveAndEnabled) {
            if (currentBeat > 0) {
                if (beats[lanesInfo[beat].beatPositions[lanesInfo[beat].currentBeatInLane] - 1].allignWithSeqZone[beat])
                    beats[lanesInfo[beat].beatPositions[lanesInfo[beat].currentBeatInLane] - 1].allignWithSeqZone[beat].transform.position = new Vector2(beats[lanesInfo[beat].beatPositions[lanesInfo[beat].currentBeatInLane] - 1].allignWithSeqZone[beat].transform.position.x, 0);

                if (beats[lanesInfo[beat].beatPositions[lanesInfo[beat].currentBeatInLane] - 1].scaleDownToSeqZone[beat])
                    beats[lanesInfo[beat].beatPositions[lanesInfo[beat].currentBeatInLane] - 1].scaleDownToSeqZone[beat].transform.localScale = new Vector2(1, beats[lanesInfo[beat].beatPositions[lanesInfo[beat].currentBeatInLane] - 1].scaleDownToSeqZone[beat].transform.position.y / (StepchartReader.originalLongBeatLength[beat] * 2) / transform.localScale.y);
            }

            if (lanesInfo[beat].currentBeatInLane < lanesInfo[beat].beatPositions.Count)
                if (originalTime <= cRealTime) {
                    float tempOffset = 0;
                    if (beats[lanesInfo[beat].beatPositions[lanesInfo[beat].currentBeatInLane]].beats[beat] > 1)
                        tempOffset = allowanceTime;

                    if ((beats[lanesInfo[beat].beatPositions[lanesInfo[beat].currentBeatInLane]].beatTiming - tempOffset) / rush <= cRealTime) {

                        if (beats[lanesInfo[beat].beatPositions[lanesInfo[beat].currentBeatInLane]].beats[beat] - inputValue <= 0) {
                            beats[lanesInfo[beat].beatPositions[lanesInfo[beat].currentBeatInLane]].beats[beat] = 0;

                            int missedBeats = 0;

                            foreach (int beatValue in beats[lanesInfo[beat].beatPositions[lanesInfo[beat].currentBeatInLane]].beats)
                                missedBeats += beatValue;

                            if (!(missedBeats > 0)) {
                                BeatScore(4);
                                for (var i = 0; i < beats[lanesInfo[beat].beatPositions[lanesInfo[beat].currentBeatInLane]].destroyOnPerf.Count; i++)
                                    beats[lanesInfo[beat].beatPositions[lanesInfo[beat].currentBeatInLane]].destroyOnPerf[i].SetActive(false);
                            }

                            lanesInfo[beat].currentBeatInLane++;
                        }
                    }
                }
        }
    }

    void PlayerHPHandler(float value) {
        currHealth += value;

        if (currHealth < 0)
            currHealth = 0;
        if (currHealth > maxHealth)
            currHealth = maxHealth;

        healthPivot.localScale = new Vector2(currHealth / 100, 1);
        Rect instance = new Rect(0, 0, currHealth / 100, 1);
        uiToUse.health.uvRect = instance;
    }

    void BeatScore(int givenCombo) {
        uiToUse.grade.gameObject.SetActive(true);

        uiToUse.grade.Stop();
        uiToUse.grade.Play();

        Color temp;
        if (givenCombo > 0) {
            temp = Color.white;

            if (combo < 0)
                combo = 0;
            else
                combo++;

            if (combo > PlayerPref.playerSettings[index].playerScore[5])
                PlayerPref.playerSettings[index].playerScore[5] = combo;
        } else {
            temp = Color.red;

            if (combo > 0)
                combo = 0;
            else
                combo--;
        }

        uiToUse.comboT.color = temp;
        uiToUse.comboGraphic.color = temp;

        switch (givenCombo) {
            case -1:
                //gradeT.sprite = ;
                PlayerPref.playerSettings[index].playerScore[4]++;
                PlayerHPHandler(1 * combo);
                break;

            case 1:
                //gradeT.sprite = ;
                break;
            case 2:
                //gradeT.sprite = ;
                break;
            case 3:
                //gradeT.sprite = ;
                break;
            case 4:
                //gradeT.sprite = ;
                PlayerPref.playerSettings[index].playerScore[0]++;
                PlayerHPHandler(1);
                break;
        }

        uiToUse.gradeT.sprite = grades[givenCombo + 1];

        PlayerPref.playerSettings[index].playerScore[6] += 1000 * givenCombo;

        if (PlayerPref.playerSettings[index].playerScore[6] < 0)
            PlayerPref.playerSettings[index].playerScore[6] = 0;

        uiToUse.comboT.text = Mathf.Abs(combo).ToString();
        //comboTBG.text = Mathf.Abs(combo).ToString();
    }
    #endregion
}