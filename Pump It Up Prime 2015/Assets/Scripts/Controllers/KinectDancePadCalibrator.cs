using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using UnityEngine.SceneManagement;
using System.IO;

public class KinectDancePadCalibrator : NoteSkinLoader {

    string padDataPath;
    public float delayTimer;
    public Vector3[] padData = new Vector3[10];
    public InputField dancepadToCalibrate;
    public Text status;

    // Use this for initialization
    void Start() {
        padDataPath = Path.Combine(Application.dataPath, "DancePadData.txt");

        if (!File.Exists(padDataPath))
            File.CreateText(padDataPath).Close();
        else
            SceneManager.LoadScene(SceneIndex.startUpScreen);

        ReadCurrentPadData();
    }

    public void WriteCurrentPadData() {
        StreamWriter writer = new StreamWriter(File.OpenWrite(padDataPath));

        foreach (Vector3 pad in padData) {
            writer.WriteLine(pad.x.ToString());
            writer.WriteLine(pad.y.ToString());
            writer.WriteLine(pad.z.ToString());
        }

        writer.Close();
        UpdatePadPosition();
    }

    public void ReadCurrentPadData() {
        string temp = "";
        int currentPadData = 0;

        StreamReader reader = File.OpenText(padDataPath);

        while ((temp = reader.ReadLine()) != null) {
            padData[currentPadData].x = float.Parse(temp);
            padData[currentPadData].y = float.Parse(reader.ReadLine());
            padData[currentPadData].z = float.Parse(reader.ReadLine());

            currentPadData++;
        }

        reader.Close();
        UpdatePadPosition();
    }

    public void CalibratePosition() {
        int dancePad;
        if (int.TryParse(dancepadToCalibrate.text, out dancePad))
            if (-1 < dancePad && dancePad < padData.Length) {
                status.text = string.Format("Calibrating in {0} seconds...", delayTimer);
                StartCoroutine(DelayTimer(dancePad));
            } else
                status.text = "Invalid number assigned";
        else
            status.text = "Infomation invalid";
    }

    IEnumerator DelayTimer(int dancePad) {
        yield return new WaitForSeconds(delayTimer);
        status.text = string.Format("Calibrated.");
        padData[dancePad] = KinectManager.currentUserPosition; //Need to make this work.
        padData[dancePad].y = 0;
        UpdatePadPosition();
    }

    public void UpdatePadPosition() {
        GameObject[] pads = GameObject.FindGameObjectsWithTag("DancePad");

        foreach (GameObject pad in pads) {
            pad.transform.position = padData[int.Parse(pad.name)];
        }
    }
}
