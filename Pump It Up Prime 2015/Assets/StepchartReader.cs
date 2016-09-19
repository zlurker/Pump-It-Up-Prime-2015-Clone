using UnityEngine;
using System.Collections;
using UnityEditor;
using System;
using System.IO;

public class StepchartReader : MonoBehaviour {

    public GameObject[] beatArrows;


    public string fileName;
    StreamReader stepchart;
    StreamReader readBeats;


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

        while ((currentBeat = stepchart.ReadLine()) != ";") { //We can read more than one stream at the same time.
            if (!currentBeat.Contains(",") && numberOfRows != 0) {
                for (var e = 0; e < currentBeat.Length; e++) {
                    char beat = currentBeat[e];
                    
                    switch (char.ConvertFromUtf32(beat)) {
                        case "1":
                            GameObject inst = Instantiate(beatArrows[e], new Vector2(e, -beatPosition), Quaternion.identity) as GameObject;
                            inst.name = inst.name + numberOfRows.ToString();
                            break;
                        case "2":
                            break;
                        case "3":
                            break;
                    }
                }

                beatPosition += (4 / numberOfRows);
            } else {
                numberOfRows = 0;
                while (!(currentRow = readBeats.ReadLine()).Contains(",") && currentRow != ";") { 
                    numberOfRows++;
                }
            }
        }

        stepchart.Close();
        readBeats.Close();
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
