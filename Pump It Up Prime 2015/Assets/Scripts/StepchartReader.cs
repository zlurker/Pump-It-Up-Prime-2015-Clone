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
    public float unitSpaceBetweenBeats;

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
        longBeatStartData = new GameObject[5];

        while (!(currentRow = readBeats.ReadLine()).Contains(","))
            numberOfRows++;
        

        while ((currentBeat = stepchart.ReadLine()) != ";") { //We can read more than one stream at the same time.
            if (!currentBeat.Contains(",")) {
                for (var e = 0; e < currentBeat.Length; e++) {
                    char beat = currentBeat[e];

                    GameObject inst = null;
                    switch (char.ConvertFromUtf32(beat)) {
                        case "1":
                        case "2":                       
                            inst = Instantiate(beatArrows[e], new Vector2(e, -beatPosition), Quaternion.identity) as GameObject;
                            longBeatStartData[e] = inst;
                            break;

                        case "3":
                            inst = Instantiate(longBeatEnd[e], new Vector2(e, -beatPosition), Quaternion.identity) as GameObject;
                            break;
                    }
                    //inst.name = inst.name + numberOfRows.ToString();
                }
                
                beatPosition += ((unitSpaceBetweenBeats *4) / numberOfRows);
            } else {
                numberOfRows = 0;
                while (!(currentRow = readBeats.ReadLine()).Contains(",") && currentRow != ";") 
                    numberOfRows++;
                debugBeats++;
                
            }
        }

        stepchart.Close();
        readBeats.Close();
        Debug.Log("Number of 4-beats: " + debugBeats);
        Debug.Log("Stepchart Deciphered");
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

    }
}
