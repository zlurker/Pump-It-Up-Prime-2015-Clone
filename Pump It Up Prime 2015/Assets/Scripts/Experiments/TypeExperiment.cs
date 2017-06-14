using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;
using UnityEditor;

[ExecuteInEditMode]
public class TypeExperiment : MonoBehaviour {

    public struct Variable {
        public FieldInfo variable;
        public FieldInfo member;
        public FieldInfo arrayIndex;
    }

    public int no;
    public bool hi;
    public NoteSkinLoader nope;
    public float wtfisthis;
    public int screwu;

    public int testthisshit;

    FieldInfo[] fields;
    public string variableName;

    void Update() {
        if (!Application.isPlaying) {

            char[] delimiters = { '[' };
            string[] temp = variableName.Split(delimiters);

            foreach (string tem in temp) {
                Debug.Log(tem);
            }

            fields = GetType().GetFields();
            for (var i = 0; i < fields.Length; i++)
                if (variableName == fields[i].Name) {
                    return;
                }

            Debug.LogError("Variable doesn't exist");
        }
    }

    void NestedEquation(Object main, Object nested) {
        //main = fields[1];
        //fields[0].GetValue
    }

    void VariableNameParsing() {
        List<int> openingBraces = new List<int>();
        List<int> closingBraces = new List<int>();

        for (int i = 0; i < variableName.Length; i++) {
            switch (variableName[i]) {
                case '[':
                    openingBraces.Add(i);
                    break;

                case ']':
                    closingBraces.Add(-1);
                    break;
            }
        }

        for (int i = 0; i < openingBraces.Count; i++) {
            //openingBraces[i]
        }
    }
}

