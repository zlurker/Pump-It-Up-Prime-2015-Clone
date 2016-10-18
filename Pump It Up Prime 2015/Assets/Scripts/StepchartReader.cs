using UnityEngine;
using System.Collections;
using System.Collections.Generic;
//using UnityEditor;
using System;
using System.IO;

public class StepchartReader : MonoBehaviour {

    public GameObject[] beatArrows;
    public GameObject[] longBeatMid;
    public GameObject[] longBeatEnd;

    public string fileName;
    public string timingData;
    public string songName;
    public float speed;

    public StepchartMover stepchartMover;

    StreamReader stepchart;
    StreamReader readBeats;
    StreamReader timeData;

    GameObject[] longBeatStartData;
    bool[] toCreateLongBeatData;

    string dataPath;

    public void CreateStepchart() {
        stepchart = File.OpenText(Path.Combine(Path.Combine(dataPath, "Stepcharts"), songName + fileName));
        readBeats = File.OpenText(Path.Combine(Path.Combine(dataPath, "Stepcharts"), songName + fileName));

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
        float currentPos = 0;

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
                        case "1":
                        case "F": //F is fake
                        case "X":
                        case "Y":
                            inst = Instantiate(beatArrows[e], new Vector2(e * 1.25f, -currentPos), Quaternion.identity) as GameObject;
                            longBeatStartData[e] = inst;
                            inst.transform.parent = stepchartMover.transform;
                            inst.name = char.ConvertFromUtf32(beat);
                            tempBeatHolder[e] = 2;

                            if (char.ConvertFromUtf32(beat) != "F")
                                toCreateData = true;
                            break;

                        case "2":
                        case "x":
                        case "y":
                            inst = Instantiate(beatArrows[e], new Vector2(e * 1.25f, -currentPos), Quaternion.identity) as GameObject;
                            longBeatStartData[e] = inst;
                            inst.transform.parent = stepchartMover.transform;
                            inst.name = char.ConvertFromUtf32(beat);
                            tempBeatHolder[e] = 1;
                            toCreateLongBeatData[e] = true;
                            toCreateData = true;
                            break;

                        case "3":
                            float dist = 0;

                            inst = Instantiate(longBeatEnd[e], new Vector2(e * 1.25f, -currentPos), Quaternion.identity) as GameObject;
                            dist = inst.transform.position.y - longBeatStartData[e].transform.position.y;

                            GameObject temp = Instantiate(longBeatMid[e], new Vector2(e * 1.25f, -currentPos - (dist / 2)), Quaternion.identity) as GameObject;
                            temp.transform.localScale = new Vector2(2.5f, dist / ((temp.transform.GetComponentInChildren<SpriteRenderer>().bounds.extents.y) * 2));

                            inst.transform.parent = stepchartMover.transform;
                            temp.transform.parent = stepchartMover.transform;
                            inst.name = char.ConvertFromUtf32(beat);
                            tempBeatHolder[e] = 1;
                            toCreateLongBeatData[e] = false;
                            toCreateData = true;
                            break;
                    }
                }

                for (var i = 0; i < toCreateLongBeatData.Length; i++)
                    if (toCreateLongBeatData[i]) {
                        tempBeatHolder[i] = 1;
                        activeLongBeats++;
                    }


                if (toCreateData || activeLongBeats > 0) {
                    stepchartMover.beats.Add(new StepchartMover.BeatsInfo(ReadTimeFromBPM(beatPosition), tempBeatHolder));
                }

                //Need to create a formula for distance pertaining to scrolls.
                //beatposition is correct. Contains data for current beat. Need to use this to 
                //make beatposition with scroll inside?

                beatPosition += 4 / numberOfRows;
            } else {
                numberOfRows = 0;
                while (!(currentRow = readBeats.ReadLine()).Contains(",") && currentRow != ";")
                    numberOfRows++;
                debugBeats++;

            }
        }

        Debug.Log("LastBeat: " + lastBeat);
        stepchart.Close();
        readBeats.Close();
        Debug.Log("Number of 4-beats: " + debugBeats);
        currentPos = stepchartMover.scrollData[currentScroll - 1].dist + ((beatPosition - stepchartMover.scrollData[currentScroll - 1].beat) * speed * stepchartMover.scrollData[currentScroll - 1].scroll);
        stepchartMover.scrollData.Add(new StepchartMover.ScrollData(debugBeats * 4, 0, ReadTimeFromBPM(debugBeats * 4), currentPos));
        Debug.Log("Stepchart Deciphered");
    }

    public void ClearStepchart() {
        foreach (Transform beat in stepchartMover.transform)
            if (beat.name != "Dummy")
                DestroyImmediate(beat.gameObject);
    }

    public void CreateTimingData() {
        dataPath = Application.dataPath;
#if UNITY_ANDROID
        dataPath = Application.persistentDataPath;
#endif
        timeData = File.OpenText(Path.Combine(Path.Combine(dataPath, "Stepcharts"), songName + timingData));

        stepchartMover.bpmData = new List<StepchartMover.BPMData>();
        stepchartMover.speedData = new List<StepchartMover.SpeedData>();

        string tempStr = "";
        int[] equalPos = new int[3];

        float prevBpm = 0;
        float prevBeat = 0;
        float cummalativeTime = 0;

        while ((tempStr = timeData.ReadLine()) != ";") { //Reading bpms
            for (var i = 0; i < tempStr.Length; i++) {
                if (char.ConvertFromUtf32(tempStr[i]) == "=")
                    equalPos[0] = i;
            }
            float beat = float.Parse(tempStr.Substring(0, equalPos[0]));
            float bpm = float.Parse(tempStr.Substring(equalPos[0] + 1, tempStr.Length - 1 - equalPos[0]));
            if (prevBpm != 0)
                cummalativeTime += ((beat - prevBeat) * 60) / prevBpm; //(beat)

            stepchartMover.bpmData.Add(new StepchartMover.BPMData(beat, bpm, cummalativeTime));

            prevBpm = bpm;
            prevBeat = beat;
        }

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

        int scrollIndex = 0;

        while ((tempStr = timeData.ReadLine()) != ";") { //Reading Scrolls
            for (var i = 0; i < tempStr.Length; i++) {
                if (char.ConvertFromUtf32(tempStr[i]) == "=")
                    equalPos[0] = i;
            }
            float beat = float.Parse(tempStr.Substring(0, equalPos[0]));
            float scroll = float.Parse(tempStr.Substring(equalPos[0] + 1, tempStr.Length - 1 - equalPos[0]));

            float dist = 0;

            if (scrollIndex > 0) //if anything is wrong, might be problem with this.
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
        return time + (((currentBeat - beat) / bpm) * 60);
    }
}

/*[CustomEditor(typeof(StepchartReader))]
public class StepchartEditor : Editor {

    public override void OnInspectorGUI() {
        DrawDefaultInspector();

        StepchartReader toCreateChart = target as StepchartReader;

        if (GUILayout.Button("Create Stepchart"))
            toCreateChart.CreateStepchart();

        if (GUILayout.Button("Clear Stepchart"))
            toCreateChart.ClearStepchart();

        if (GUILayout.Button("Create Timing Data"))
            toCreateChart.CreateTimingData();

    }
}*/
