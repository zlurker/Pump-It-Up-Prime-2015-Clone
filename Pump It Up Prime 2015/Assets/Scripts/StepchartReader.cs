using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using System;
using System.IO;

public class StepchartReader : MonoBehaviour {

    public GameObject[] beatArrows;

    public GameObject[] longBeatMid;
    public GameObject[] longBeatEnd;

    public string fileName;
    public string timingData;
    public float speed;

    public StepchartMover stepchartMover;


    StreamReader stepchart;
    StreamReader readBeats;

    StreamReader timeData;

    GameObject[] longBeatStartData;

    void Start() {
        //CreateStepchart();
    }

    public void CreateStepchart() {
        stepchart = File.OpenText(Path.Combine(Application.dataPath, fileName));
        readBeats = File.OpenText(Path.Combine(Application.dataPath, fileName));

        stepchartMover.beats = new List<StepchartMover.BeatsInfo>();

        string currentBeat = "";
        string currentRow = "";
        float beatPosition = 0;
        float numberOfRows = 0;

        int debugBeats = 1;
        int lastBeat = 0;
        longBeatStartData = new GameObject[10];

        float timeInst = 0;
        float prevBeat = 0;
        float currBpm = 0;
        int bpmScaling = 0;

        while (!(currentRow = readBeats.ReadLine()).Contains(","))
            numberOfRows++;


        while ((currentBeat = stepchart.ReadLine()) != ";") { //We can read more than one stream at the same time.
            if (!currentBeat.Contains(",")) {
                lastBeat++;

                bool toCreateData = false;
                int[] tempBeatHolder = new int[currentBeat.Length];

                

                for (var e = 0; e < currentBeat.Length; e++) {
                    char beat = currentBeat[e];

                    tempBeatHolder[e] = int.Parse(char.ConvertFromUtf32(beat));

                    GameObject inst = null;
                    switch (char.ConvertFromUtf32(beat)) {
                        case "1":
                        case "2":
                            inst = Instantiate(beatArrows[e], new Vector2(e, -beatPosition * speed), Quaternion.identity) as GameObject;
                            longBeatStartData[e] = inst;
                            inst.transform.parent = stepchartMover.transform;
                            toCreateData = true;
                            break;

                        case "3":
                            float dist = 0;

                            inst = Instantiate(longBeatEnd[e], new Vector2(e, -beatPosition * speed), Quaternion.identity) as GameObject;
                            dist = inst.transform.position.y - longBeatStartData[e].transform.position.y;

                            GameObject temp = Instantiate(longBeatMid[e], new Vector2(e, (-beatPosition * speed) - (dist / 2)), Quaternion.identity) as GameObject;
                            temp.transform.localScale = new Vector2(2, dist / ((temp.transform.GetComponentInChildren<SpriteRenderer>().bounds.extents.y) * 2));

                            inst.transform.parent = stepchartMover.transform;
                            temp.transform.parent = stepchartMover.transform;
                            toCreateData = true;
                            break;
                    }
                }

                if (toCreateData) {
                    stepchartMover.beats.Add(new StepchartMover.BeatsInfo(timeInst + (((beatPosition - prevBeat)/currBpm) *60), tempBeatHolder));
                }

                if (stepchartMover.bpmData.Count > bpmScaling) {
                    if (stepchartMover.bpmData[bpmScaling].beat < beatPosition) {//when bpm changes
                        timeInst = stepchartMover.bpmData[bpmScaling].time;
                        prevBeat = stepchartMover.bpmData[bpmScaling].beat;
                        currBpm = stepchartMover.bpmData[bpmScaling].bpm;
                        bpmScaling++;
                    }
                }

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
        stepchartMover.endBpm = debugBeats * 4;
        stepchartMover.totalDist = (debugBeats * 4) * speed;
        Debug.Log("Stepchart Deciphered");
    }

    public void ClearStepchart() {
        foreach (Transform beat in stepchartMover.transform)
            DestroyImmediate(beat.gameObject);
    }

    public void CreateTimingData() {
        timeData = File.OpenText(Path.Combine(Application.dataPath, timingData));

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
            stepchartMover.speedData.Add(new StepchartMover.SpeedData(float.Parse(tempStr.Substring(0, equalPos[0])), float.Parse(tempStr.Substring(equalPos[0] + 1, equalPos[1] - equalPos[0] - 1)), float.Parse(tempStr.Substring(equalPos[1] + 1, equalPos[2] - equalPos[1] - 1)), 1));
        }
        timeData.Close();
    }
}

[CustomEditor(typeof(StepchartReader))]
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
}
