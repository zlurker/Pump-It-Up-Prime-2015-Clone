using UnityEngine;
using System.Collections;
using UnityEditor;
using System;
using System.IO;

public class StepchartReader : MonoBehaviour {

    public GameObject[] beatArrows;

    public GameObject[] longBeatMid;
    public GameObject[] longBeatEnd;

    public string fileName;
    public float speed;

    public StepchartMover stepchartMover;

    StreamReader stepchart;
    StreamReader readBeats;

    GameObject[] longBeatStartData;

    void Start() {
        //stepchart = File.OpenText(Path.Combine(Application.dataPath, fileName));
        CreateStepchart();
    }

    // Update is called once per frame
    void Update() {

    }

    public void CreateStepchart() {
        stepchart = File.OpenText(Path.Combine(Application.dataPath, fileName));
        readBeats = File.OpenText(Path.Combine(Application.dataPath, fileName));

        string currentBeat = "";
        string currentRow = "";
        float beatPosition = 0;
        float numberOfRows = 0;

        int debugBeats = 1;
        int lastBeat = 0;
        longBeatStartData = new GameObject[10];

        while (!(currentRow = readBeats.ReadLine()).Contains(","))
            numberOfRows++;


        while ((currentBeat = stepchart.ReadLine()) != ";") { //We can read more than one stream at the same time.
            if (!currentBeat.Contains(",")) {
                lastBeat++;
                for (var e = 0; e < currentBeat.Length; e++) {
                    char beat = currentBeat[e];

                    GameObject inst = null;
                    switch (char.ConvertFromUtf32(beat)) {
                        case "1":
                        case "2":
                            inst = Instantiate(beatArrows[e], new Vector2(e, -beatPosition), Quaternion.identity) as GameObject;
                            longBeatStartData[e] = inst;
                            inst.transform.parent = stepchartMover.transform;
                            break;

                        case "3":
                            float dist =0;

                            inst = Instantiate(longBeatEnd[e], new Vector2(e, -beatPosition), Quaternion.identity) as GameObject;
                            dist = inst.transform.position.y - longBeatStartData[e].transform.position.y;

                            GameObject temp = Instantiate(longBeatMid[e], new Vector2(e, -beatPosition - (dist/2)), Quaternion.identity) as GameObject;                           
                            temp.transform.localScale = new Vector2(2,dist/((temp.transform.GetComponentInChildren<SpriteRenderer>().bounds.extents.y)*2));

                            inst.transform.parent = stepchartMover.transform;
                            temp.transform.parent = stepchartMover.transform;
                            break;
                    }                        
                }
                beatPosition += ((speed * 4) / numberOfRows);
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
        stepchartMover.endBpm = debugBeats *4;
        stepchartMover.totalDist = (debugBeats * 4) * speed;
        Debug.Log("Stepchart Deciphered");
    }

    public void ClearStepchart() {
        foreach (Transform beat in stepchartMover.transform)
            DestroyImmediate(beat.gameObject);
    }
}

[CustomEditor(typeof(StepchartReader))]
public class StepchartEditor : Editor {

    public override void OnInspectorGUI() {
        DrawDefaultInspector();

        StepchartReader toCreateChart = target as StepchartReader;

        if (GUILayout.Button("Create Stepchart")) {
            toCreateChart.CreateStepchart();
        }

        if (GUILayout.Button("Clear Stepchart")) {
            toCreateChart.ClearStepchart();
        }

    }
}
