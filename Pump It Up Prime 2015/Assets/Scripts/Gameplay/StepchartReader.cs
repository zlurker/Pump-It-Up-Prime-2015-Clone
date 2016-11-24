using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;
using System.IO;

#if UNITY_EDITOR
using UnityEditor;
#endif

public class StepchartReader : MonoBehaviour {

    public Transform[] sequenceZoneToMeasure;
    public bool invertStepchart;
    public GameObject[] beatArrows;
    public GameObject[] longBeatMid;
    public GameObject[] longBeatEnd;

    public string fileName;
    public string timingData;
    public string songName;
    public float speed;
    public float beatScale;
    public float screenSizeMultiplier;
    public StepchartMover stepchartMover;
    public InputBase input;
    StreamReader stepchart;
    StreamReader readBeats;
    StreamReader timeData;

    GameObject[] longBeatStartData;
    bool[] toCreateLongBeatData;

    string dataPath;

    public void CreateStepchart() {
        stepchart = File.OpenText(Path.Combine(Path.Combine(dataPath, "Stepcharts"), PlayerPref.songs[PlayerPref.songIndex] + ".txt"));
        readBeats = File.OpenText(Path.Combine(Path.Combine(dataPath, "Stepcharts"), PlayerPref.songs[PlayerPref.songIndex] + ".txt"));

        stepchartMover.beats = new List<StepchartMover.BeatsInfo>();

        string currentBeat = "";
        string currentRow = "";
        float beatPosition = 0;
        float numberOfRows = 0;

        int debugBeats = 1;
        int lastBeat = 0;
        int currentScroll = 0;
        longBeatStartData = new GameObject[10];
        toCreateLongBeatData = new bool[10];
        stepchartMover.lanesInfo = new StepchartMover.LaneInfo[10];
        float currentPos = 0;

        stepchartMover.transform.position = sequenceZoneToMeasure[stepchartMover.index].position;

        for (var i = 0; i < stepchartMover.lanesInfo.Length; i++)
            stepchartMover.lanesInfo[i].beatPositions = new List<int>();

        while (!stepchart.ReadLine().Contains("#NOTES:")) ;
        while (!readBeats.ReadLine().Contains("#NOTES:")) ;

        while (!(currentRow = readBeats.ReadLine()).Contains(","))
            numberOfRows++;

        while ((currentBeat = stepchart.ReadLine()) != ";") { //We can read more than one stream at the same time.
            if (!currentBeat.Contains(",")) {
                lastBeat++;

                bool toCreateData = false;
                int[] tempBeatHolder = new int[currentBeat.Length];
                int activeLongBeats = 0;

                while (stepchartMover.scrollData.Count > currentScroll && stepchartMover.scrollData[currentScroll].beat < beatPosition) {
                    currentScroll++;
                }

                if (currentScroll > 0) {
                    currentPos = stepchartMover.scrollData[currentScroll - 1].dist + ((beatPosition - stepchartMover.scrollData[currentScroll - 1].beat) * speed * stepchartMover.scrollData[currentScroll - 1].scroll);
                }

                for (var e = 0; e < currentBeat.Length; e++) {

                    char beat = currentBeat[e];
                    GameObject inst = null;

                    switch (char.ConvertFromUtf32(beat)) {
                        case "F": //F is fake
                            inst = Instantiate(beatArrows[e], new Vector2(sequenceZoneToMeasure[stepchartMover.index].position.x - (2 * beatScale) + (e * beatScale), -currentPos), Quaternion.identity) as GameObject;
                            inst.transform.localScale = new Vector2(2 * beatScale, 2 * beatScale);
                            longBeatStartData[e] = inst;
                            inst.transform.parent = stepchartMover.transform;
                            inst.name = char.ConvertFromUtf32(beat);
                            break;

                        case "1":
                        case "X":
                        case "Y":
                            inst = Instantiate(beatArrows[e], new Vector2(sequenceZoneToMeasure[stepchartMover.index].position.x - (2 * beatScale) + (e * beatScale), -currentPos), Quaternion.identity) as GameObject;
                            inst.transform.localScale = new Vector2(2 * beatScale, 2 * beatScale);
                            longBeatStartData[e] = inst;
                            inst.transform.parent = stepchartMover.transform;
                            inst.name = char.ConvertFromUtf32(beat);
                            tempBeatHolder[e] = 2;
                            toCreateData = true;
                            stepchartMover.lanesInfo[e].beatPositions.Add(stepchartMover.beats.Count);
                            break;

                        case "2":
                        case "x":
                        case "y":
                            inst = Instantiate(beatArrows[e], new Vector2(sequenceZoneToMeasure[stepchartMover.index].position.x - (2 * beatScale) + (e * beatScale), -currentPos), Quaternion.identity) as GameObject;
                            inst.transform.localScale = new Vector2(2 * beatScale, 2 * beatScale);
                            longBeatStartData[e] = inst;
                            inst.transform.parent = stepchartMover.transform;
                            inst.name = char.ConvertFromUtf32(beat);
                            toCreateLongBeatData[e] = true;
                            toCreateData = true;
                            break;

                        case "3":
                            float dist = 0;

                            inst = Instantiate(longBeatEnd[e], new Vector2(sequenceZoneToMeasure[stepchartMover.index].position.x - (2 * beatScale) + (e * beatScale), -currentPos), Quaternion.identity) as GameObject;
                            inst.transform.localScale = new Vector2(2 * beatScale, 2 * beatScale);
                            dist = inst.transform.position.y - longBeatStartData[e].transform.position.y;

                            GameObject temp = Instantiate(longBeatMid[e], new Vector2(sequenceZoneToMeasure[stepchartMover.index].position.x - (2 * beatScale) + (e * beatScale), -currentPos - (dist / 2)), Quaternion.identity) as GameObject;
                            temp.transform.localScale = new Vector2(2 * beatScale, dist / ((temp.transform.GetComponentInChildren<SpriteRenderer>().bounds.extents.y) * 2));

                            inst.transform.parent = stepchartMover.transform;
                            temp.transform.parent = stepchartMover.transform;
                            inst.name = char.ConvertFromUtf32(beat);
                            tempBeatHolder[e] = 1;
                            activeLongBeats++;
                            toCreateLongBeatData[e] = false;
                            toCreateData = true;
                            stepchartMover.lanesInfo[e].beatPositions.Add(stepchartMover.beats.Count);
                            break;
                    }
                }

                for (var i = 0; i < toCreateLongBeatData.Length; i++)
                    if (toCreateLongBeatData[i]) {
                        tempBeatHolder[i] = 1;
                        activeLongBeats++;
                        stepchartMover.lanesInfo[i].beatPositions.Add(stepchartMover.beats.Count);
                    }


                if (toCreateData || activeLongBeats > 0)
                    stepchartMover.beats.Add(new StepchartMover.BeatsInfo(ReadTimeFromBPM(beatPosition), tempBeatHolder));

                beatPosition += 4 / numberOfRows;
            } else {
                numberOfRows = 0;
                while (!(currentRow = readBeats.ReadLine()).Contains(",") && currentRow != ";")
                    numberOfRows++;
                debugBeats++;
            }
        }

        if (stepchartMover.beats[stepchartMover.beats.Count - 1].beats.Length == 10) {
            input.activePlayerIndex = stepchartMover.index;
            input.currentGameMode = InputBase.GameMode.Double;
            sequenceZoneToMeasure[0].position = new Vector3(2.7f, 0, 0);
            sequenceZoneToMeasure[1].position = new Vector3(9f, 0, 0);
            

            stepchartMover.transform.position = sequenceZoneToMeasure[0].position;
        }

        Debug.Log("LastBeat: " + lastBeat);
        stepchart.Close();
        readBeats.Close();
        Debug.Log("Number of 4-beats: " + debugBeats);
        currentPos = stepchartMover.scrollData[currentScroll - 1].dist + ((beatPosition - stepchartMover.scrollData[currentScroll - 1].beat) * speed * stepchartMover.scrollData[currentScroll - 1].scroll);
        stepchartMover.scrollData.Add(new StepchartMover.ScrollData(debugBeats * 4, 0, ReadTimeFromBPM(debugBeats * 4), currentPos));
        stepchartMover.beatScale = 2 * beatScale;
        Debug.Log("Stepchart Deciphered");
        Debug.Log(sequenceZoneToMeasure[0].position);
        Debug.Log(sequenceZoneToMeasure[1].position);
    }

    public void ClearStepchart() {
        foreach (Transform beat in stepchartMover.transform)
            if (beat.name != "Dummy")
                DestroyImmediate(beat.gameObject);
    }

    public void CreateTimingData() {
        dataPath = Application.dataPath;
        speed *= screenSizeMultiplier;

        timeData = File.OpenText(Path.Combine(Path.Combine(dataPath, "Stepcharts"), PlayerPref.songs[PlayerPref.songIndex] + ".txt"));

        stepchartMover.bpmData = new List<StepchartMover.BPMData>();
        stepchartMover.speedData = new List<StepchartMover.SpeedData>();

        string tempStr = "";
        int[] equalPos = new int[3];

        while (!timeData.ReadLine().Contains("#OFFSET:")) ;
        while ((tempStr = timeData.ReadLine()) != ";") {
            stepchartMover.offset = -(float.Parse(tempStr));
        }

        float prevBpm = 0;
        float prevBeat = 0;
        float cummalativeTime = 0;

        while (!timeData.ReadLine().Contains("#BPMS:")) ;
        while ((tempStr = timeData.ReadLine()) != ";") { //Reading bpms/warp
            for (var i = 0; i < tempStr.Length; i++) {
                if (char.ConvertFromUtf32(tempStr[i]) == "=")
                    equalPos[0] = i;
            }
            float beat = float.Parse(tempStr.Substring(0, equalPos[0]));
            float bpm = float.Parse(tempStr.Substring(equalPos[0] + 1, tempStr.Length - 1 - equalPos[0]));

            if (prevBpm > 0)
                cummalativeTime += ((beat - prevBeat) * 60) / prevBpm;

            stepchartMover.bpmData.Add(new StepchartMover.BPMData(beat, bpm, cummalativeTime));

            prevBpm = bpm;
            prevBeat = beat;
        }

        while (!timeData.ReadLine().Contains("#WARPS:")) ;
        while ((tempStr = timeData.ReadLine()) != ";") { //Reading warp
            for (var i = 0; i < tempStr.Length; i++) {
                if (char.ConvertFromUtf32(tempStr[i]) == "=")
                    equalPos[0] = i;
            }

            float beat = float.Parse(tempStr.Substring(0, equalPos[0]));
            float warp = float.Parse(tempStr.Substring(equalPos[0] + 1, tempStr.Length - 1 - equalPos[0]));
            float bpm = 0;
            int j = 0;

            for (j = 0; j < stepchartMover.bpmData.Count; j++) {

                if (beat < stepchartMover.bpmData[j].beat) {
                    break;
                }
                if (stepchartMover.bpmData[j].bpm > 0)
                    bpm = stepchartMover.bpmData[j].bpm;
            }

            float startWarpTiming = ReadTimeFromBPM(beat);
            float timingDifference = ReadTimeFromBPM(beat + warp) - startWarpTiming;

            int f = j;

            stepchartMover.bpmData.Insert(j, new StepchartMover.BPMData(beat, 0, startWarpTiming));

            for (j += 1; j < stepchartMover.bpmData.Count; j++) {
                StepchartMover.BPMData inst = stepchartMover.bpmData[j];
                inst.time -= timingDifference;

                if (inst.time <= stepchartMover.bpmData[f].time) {
                    inst.time = stepchartMover.bpmData[f].time;

                    if (inst.bpm > 0) {
                        bpm = inst.bpm;
                        inst.bpm = 0;
                    }
                }

                stepchartMover.bpmData[j] = inst;
            }

            for (f += 1; f < stepchartMover.bpmData.Count; f++) {
                if (beat + warp < stepchartMover.bpmData[f].beat)
                    break;
            }

            stepchartMover.bpmData.Insert(f, new StepchartMover.BPMData(beat + warp, bpm, startWarpTiming));
        }

        while (!timeData.ReadLine().Contains("#SPEEDS:")) ;
        while ((tempStr = timeData.ReadLine()) != ";") { //Reading speed
            int posInEA = 0;
            for (var i = 0; i < tempStr.Length; i++) {
                if (char.ConvertFromUtf32(tempStr[i]) == "=") {
                    equalPos[posInEA] = i;
                    posInEA++;
                }
            }
            float speedBeat = float.Parse(tempStr.Substring(0, equalPos[0]));
            float timeAllowed = 0;

            timeAllowed = float.Parse(tempStr.Substring(equalPos[1] + 1, equalPos[2] - equalPos[1] - 1));

            if (tempStr.Substring(equalPos[2] + 1, 1) == "0")
                timeAllowed = ReadTimeFromBPM(speedBeat + timeAllowed) - ReadTimeFromBPM(speedBeat);

            stepchartMover.speedData.Add(new StepchartMover.SpeedData(speedBeat, float.Parse(tempStr.Substring(equalPos[0] + 1, equalPos[1] - equalPos[0] - 1)), timeAllowed, ReadTimeFromBPM(speedBeat)));
        }


        while (!timeData.ReadLine().Contains("#SCROLLS:")) ;

        int scrollIndex = 0;

        while ((tempStr = timeData.ReadLine()) != ";") { //Reading Scrolls
            for (var i = 0; i < tempStr.Length; i++) {
                if (char.ConvertFromUtf32(tempStr[i]) == "=")
                    equalPos[0] = i;
            }
            float beat = float.Parse(tempStr.Substring(0, equalPos[0]));
            float scroll = float.Parse(tempStr.Substring(equalPos[0] + 1, tempStr.Length - 1 - equalPos[0]));

            float dist = 0;

            if (scrollIndex > 0)
                dist = ((beat - stepchartMover.scrollData[scrollIndex - 1].beat) * stepchartMover.scrollData[scrollIndex - 1].scroll * speed) + stepchartMover.scrollData[scrollIndex - 1].dist;

            stepchartMover.scrollData.Add(new StepchartMover.ScrollData(beat, scroll, ReadTimeFromBPM(beat), dist));
            scrollIndex++;
        }
        timeData.Close();
    }

    float ReadTimeFromBPM(float currentBeat) {
        float beat = 0;
        float bpm = 0;
        float time = 0;

        foreach (StepchartMover.BPMData bpmInst in stepchartMover.bpmData) {
            if (bpmInst.beat > currentBeat)
                break;

            beat = bpmInst.beat;
            bpm = bpmInst.bpm;
            time = bpmInst.time;
        }

        if (bpm > 0)
            return time + (((currentBeat - beat) / bpm) * 60);
        return time;
    }
}

#if UNITY_EDITOR
[CustomEditor(typeof(StepchartReader))]
public class StepchartEditor : Editor {

    public override void OnInspectorGUI() {
        DrawDefaultInspector();

        StepchartReader toCreateChart = target as StepchartReader;

        if (GUILayout.Button("Create Stepchart"))
            //toCreateChart.CreateStepchart();

            if (GUILayout.Button("Clear Stepchart"))
                toCreateChart.ClearStepchart();

        if (GUILayout.Button("Create Timing Data"))
            toCreateChart.CreateTimingData();
    }
}
#endif
