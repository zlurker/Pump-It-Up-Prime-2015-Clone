using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;
using System.IO;


public class StepchartReader : MonoBehaviour {

    public Vector2 targetedAspect;
    public Transform[] sequenceZoneToMeasure;
    public SpriteRenderer[] seqZones;
    public bool invertStepchart;
    public SpriteRenderer[] beatArrows;
    public SpriteRenderer[] longBeatMid;
    public SpriteRenderer[] longBeatEnd;
    public static float[] originalLongBeatLength;

    public string fileName;
    public string timingData;
    public string songName;
    public float speed;
    public float beatScale;
    public float screenSizeMultiplier;
    public StepchartMover stepchartMover;
    int currentLevel;
    StreamReader stepchart;
    StreamReader readBeats;
    StreamReader timeData;

    GameObject[] longBeatStartData;
    bool[] toCreateLongBeatData;

    public Vector2 AspectScale(Texture targetTexture, Vector3 tAspect) {
        Vector2 inst = Vector2.zero;
        inst.x = targetTexture.width / tAspect.x;
        inst.y = targetTexture.height / tAspect.y;

        //Debug.Log(targetTexture.width);
        //Debug.Log(currScale.x / inst.x + " " + currScale.y / inst.y);

        return new Vector2(1 / inst.x, 1 / inst.y);
    }

    public void CreateStepchart(string path) {
        originalLongBeatLength = new float[10];
        for (var i = 0; i < AssetDatabase.noteskins.Length; i++) {
            beatArrows[i].sprite = AssetDatabase.noteskins[i].tapNote;
            beatArrows[i].transform.localScale = AspectScale(AssetDatabase.noteskins[i].tapNote.texture, targetedAspect);
            stepchartMover.beatScale = beatArrows[i].transform.localScale.x;

            longBeatMid[i].sprite = AssetDatabase.noteskins[i].holdNote;
            longBeatMid[i].transform.localScale = AspectScale(AssetDatabase.noteskins[i].holdNote.texture, targetedAspect);
            longBeatMid[i].transform.localScale = new Vector2(longBeatMid[i].transform.localScale.x, 1f);
            originalLongBeatLength[i] = longBeatMid[i].bounds.extents.y;
            originalLongBeatLength[i+5] = longBeatMid[i].bounds.extents.y;

            longBeatEnd[i].sprite = AssetDatabase.noteskins[i].endHoldNote;
            longBeatEnd[i].transform.localScale = AspectScale(AssetDatabase.noteskins[i].endHoldNote.texture, targetedAspect);
        }

        Vector2 seqZoneTarAspect = targetedAspect;
        seqZoneTarAspect.x *= 5;

        foreach (SpriteRenderer seqZone in seqZones) {
            seqZone.sprite = AssetDatabase.seqZone;
            seqZone.transform.localScale = AspectScale(AssetDatabase.seqZone.texture, seqZoneTarAspect);
        }

        stepchart = File.OpenText(path);
        readBeats = File.OpenText(path);

        stepchartMover.beats = new List<StepchartMover.BeatsInfo>();

        int sequenceZoneToUse = 0;

        sequenceZoneToUse = stepchartMover.index;

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
        currentLevel = 0;

        //stepchartMover.transform.position = sequenceZoneToMeasure[stepchartMover.index].position;

        for (var i = 0; i < stepchartMover.lanesInfo.Length; i++)
            stepchartMover.lanesInfo[i].beatPositions = new List<int>();

        while (currentLevel != PlayerPref.playerSettings[sequenceZoneToUse].currentSongLevel + 1) {
            readBeats.ReadLine();
            if (stepchart.ReadLine().Contains("#STEPSTYPE:"))
                currentLevel++;
        }

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

                List<GameObject> destroyOnPerf = new List<GameObject>();
                for (var e = 0; e < currentBeat.Length; e++) {

                    char beat = currentBeat[e];
                    GameObject inst = null;

                    switch (char.ConvertFromUtf32(beat)) {
                        case "F": //F is fake
                            inst = Instantiate(beatArrows[e].gameObject, new Vector2(sequenceZoneToMeasure[sequenceZoneToUse].position.x - (2 * beatScale) + (e * beatScale), -currentPos), Quaternion.identity) as GameObject;
                            //inst.transform.localScale = new Vector2(2 * beatScale, 2 * beatScale);
                            longBeatStartData[e] = inst;
                            inst.transform.parent = stepchartMover.transform;
                            inst.name = char.ConvertFromUtf32(beat);
                            break;

                        case "1":
                        case "X":
                        case "Y":
                        case "Z":
                            inst = Instantiate(beatArrows[e].gameObject, new Vector2(sequenceZoneToMeasure[sequenceZoneToUse].position.x - (2 * beatScale) + (e * beatScale), -currentPos), Quaternion.identity) as GameObject;
                            //inst.transform.localScale = new Vector2(2 * beatScale, 2 * beatScale);
                            longBeatStartData[e] = inst;
                            inst.transform.parent = stepchartMover.transform;
                            inst.name = char.ConvertFromUtf32(beat);
                            tempBeatHolder[e] = 2;
                            toCreateData = true;
                            stepchartMover.lanesInfo[e].beatPositions.Add(stepchartMover.beats.Count);
                            destroyOnPerf.Add(inst);
                            break;

                        case "2":
                        case "x":
                        case "y":
                        case "z":
                            //inst = Instantiate(beatArrows[e].gameObject, new Vector2(sequenceZoneToMeasure[sequenceZoneToUse].position.x - (2 * beatScale) + (e * beatScale), -currentPos), Quaternion.identity) as GameObject;
                            //inst.name = char.ConvertFromUtf32(beat);
                            toCreateLongBeatData[e] = true;
                            toCreateData = true;
                            break;

                        case "3":
                            float dist = 0;

                            inst = Instantiate(longBeatEnd[e].gameObject, new Vector2(sequenceZoneToMeasure[sequenceZoneToUse].position.x - (2 * beatScale) + (e * beatScale), -currentPos), Quaternion.identity) as GameObject;
                            //inst.transform.localScale = new Vector2(2 * beatScale, 2 * beatScale);
                            dist = inst.transform.position.y - stepchartMover.beats[stepchartMover.beats.Count - 1].allignWithSeqZone[e].transform.position.y;


                            stepchartMover.beats[stepchartMover.beats.Count - 1].scaleDownToSeqZone[e].transform.position = new Vector2(stepchartMover.beats[stepchartMover.beats.Count - 1].scaleDownToSeqZone[e].transform.position.x, -currentPos);
                            //GameObject temp = Instantiate(longBeatMid[e].transform.parent.gameObject, new Vector2(sequenceZoneToMeasure[sequenceZoneToUse].position.x - (2 * beatScale) + (e * beatScale), -currentPos), Quaternion.identity) as GameObject;
                            stepchartMover.beats[stepchartMover.beats.Count - 1].scaleDownToSeqZone[e].transform.localScale = new Vector2(1, dist / (originalLongBeatLength[e] * 2));

                            inst.transform.parent = stepchartMover.transform;
                            //temp.transform.parent = stepchartMover.transform;
                            destroyOnPerf.Add(stepchartMover.beats[stepchartMover.beats.Count - 1].scaleDownToSeqZone[e]);
                            destroyOnPerf.Add(stepchartMover.beats[stepchartMover.beats.Count - 1].allignWithSeqZone[e]);
                            destroyOnPerf.Add(inst);

                            inst.name = char.ConvertFromUtf32(beat);
                            tempBeatHolder[e] = 1;
                            activeLongBeats++;
                            toCreateLongBeatData[e] = false;
                            toCreateData = true;
                            stepchartMover.lanesInfo[e].beatPositions.Add(stepchartMover.beats.Count);
                            break;
                    }
                }

                GameObject[] allignInst = new GameObject[10];
                GameObject[] scaleDownInst = new GameObject[10];

                for (var i = 0; i < toCreateLongBeatData.Length; i++)
                    if (toCreateLongBeatData[i]) {
                        tempBeatHolder[i] = 1;
                        activeLongBeats++;
                        stepchartMover.lanesInfo[i].beatPositions.Add(stepchartMover.beats.Count);

                        Debug.Log(stepchartMover.beats.Count + " " + i);
                        if (stepchartMover.beats.Count == 0 || !stepchartMover.beats[stepchartMover.beats.Count - 1].allignWithSeqZone[i]) {
                            allignInst[i] = Instantiate(beatArrows[i].gameObject, new Vector2(sequenceZoneToMeasure[sequenceZoneToUse].position.x - (2 * beatScale) + (i * beatScale), -currentPos), Quaternion.identity);
                            allignInst[i].transform.parent = stepchartMover.transform;
                        } else
                            allignInst[i] = stepchartMover.beats[stepchartMover.beats.Count - 1].allignWithSeqZone[i];

                        if (stepchartMover.beats.Count == 0 || !stepchartMover.beats[stepchartMover.beats.Count - 1].scaleDownToSeqZone[i]) {
                            scaleDownInst[i] = Instantiate(longBeatMid[i].transform.parent.gameObject, new Vector2(sequenceZoneToMeasure[sequenceZoneToUse].position.x - (2 * beatScale) + (i * beatScale), -currentPos), Quaternion.identity);
                            scaleDownInst[i].transform.parent = stepchartMover.transform;
                        } else
                            scaleDownInst[i] = stepchartMover.beats[stepchartMover.beats.Count - 1].scaleDownToSeqZone[i];
                    }


                if (toCreateData || activeLongBeats > 0)
                    stepchartMover.beats.Add(new StepchartMover.BeatsInfo(ReadTimeFromBPM(beatPosition), tempBeatHolder, destroyOnPerf, allignInst, scaleDownInst));

                beatPosition += 4 / numberOfRows;
            } else {
                numberOfRows = 0;
                while (!(currentRow = readBeats.ReadLine()).Contains(",") && currentRow != ";")
                    numberOfRows++;
                debugBeats++;
            }
        }

        if (stepchartMover.beats[stepchartMover.beats.Count - 1].beats.Length == 10) {
            InputBase.activePlayerIndex = stepchartMover.index;
            stepchartMover.transform.position += sequenceZoneToMeasure[2].position - sequenceZoneToMeasure[sequenceZoneToUse].position;
            InputBase.currentGameMode = InputBase.GameMode.Double;
        }

        stepchart.Close();
        readBeats.Close();
        //Debug.Log("Number of 4-beats: " + debugBeats);
        currentPos = stepchartMover.scrollData[currentScroll - 1].dist + ((beatPosition - stepchartMover.scrollData[currentScroll - 1].beat) * speed * stepchartMover.scrollData[currentScroll - 1].scroll);
        stepchartMover.scrollData.Add(new StepchartMover.ScrollData(debugBeats * 4, 0, ReadTimeFromBPM(debugBeats * 4), currentPos));


        for (var i = 0; i < stepchartMover.bpmData.Count; i++) {
            StepchartMover.BPMData returnInst = stepchartMover.bpmData[i];
            foreach (StepchartMover.DelayData delayInst in stepchartMover.delayData) {
                if (delayInst.beat >= stepchartMover.bpmData[i].beat)
                    break;

                returnInst.time += delayInst.delay;
            }
            stepchartMover.bpmData[i] = returnInst;
        }
        Debug.Log("Stepchart Deciphered");
    }

    public void ClearStepchart() {
        foreach (Transform beat in stepchartMover.transform)
            if (beat.name != "Dummy")
                DestroyImmediate(beat.gameObject);
    }

    public void CreateTimingData(string path) {
        speed *= screenSizeMultiplier;

        StreamReader timeDataTemp = File.OpenText(path);
        timeData = File.OpenText(path);

        stepchartMover.bpmData = new List<StepchartMover.BPMData>();
        stepchartMover.delayData = new List<StepchartMover.DelayData>();
        stepchartMover.speedData = new List<StepchartMover.SpeedData>();

        string tempStr = "";
        int[] equalPos = new int[3];
        currentLevel = 0;

        while (currentLevel != PlayerPref.playerSettings[stepchartMover.index].currentSongLevel + 1) {
            timeDataTemp.ReadLine();
            if (timeData.ReadLine().Contains("#STEPSTYPE:"))
                currentLevel++;
        }

        while (!(tempStr = timeData.ReadLine()).Contains("#OFFSET:")) ;
        tempStr = tempStr.Remove(0, 8);
        tempStr = tempStr.Remove(tempStr.Length - 1, 1);
        stepchartMover.offset = -(float.Parse(tempStr));//-(float.Parse(tempStr));

        float prevBpm = 0;
        float prevBeat = 0;
        float cummalativeTime = 0;

        while (!(tempStr = timeData.ReadLine()).Contains("#BPMS:")) ;
        tempStr = tempStr.Remove(0, 6);

        while (tempStr != ";") { //Reading bpms/warp
            for (var i = 0; i < tempStr.Length; i++)
                if (char.ConvertFromUtf32(tempStr[i]) == "=")
                    equalPos[0] = i;

            float beat = float.Parse(tempStr.Substring(0, equalPos[0]));
            float bpm = float.Parse(tempStr.Substring(equalPos[0] + 1, tempStr.Length - 1 - equalPos[0]));

            if (prevBpm > 0)
                cummalativeTime += ((beat - prevBeat) * 60) / prevBpm;

            stepchartMover.bpmData.Add(new StepchartMover.BPMData(beat, bpm, cummalativeTime));

            prevBpm = bpm;
            prevBeat = beat;
            tempStr = timeData.ReadLine();
        }

        while (!(tempStr = timeDataTemp.ReadLine()).Contains("#WARPS:")) {
            //Debug.Log(tempStr);
        }
        //Debug.Log(tempStr);
        tempStr = tempStr.Remove(0, 7);
        while (tempStr != ";") {
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
            tempStr = timeDataTemp.ReadLine();
        }

        timeDataTemp.Close();

        while (!(tempStr = timeData.ReadLine()).Contains("#DELAYS:")) ;
        tempStr = tempStr.Remove(0, 8);

        while (tempStr != ";") {
            for (var i = 0; i < tempStr.Length; i++)
                if (char.ConvertFromUtf32(tempStr[i]) == "=")
                    equalPos[0] = i;

            float beat = float.Parse(tempStr.Substring(0, equalPos[0]));
            float delay = float.Parse(tempStr.Substring(equalPos[0] + 1, tempStr.Length - 1 - equalPos[0]));
            //float temp = 0;
            //float tempTime = ReadTimeFromBPM(beat,out temp);
            // Debug.Log("BPM: " +temp);
            stepchartMover.delayData.Add(new StepchartMover.DelayData(beat, delay, ReadTimeFromBPM(beat)));
            tempStr = timeData.ReadLine();
        }

        /*while (!(tempStr = timeData.ReadLine()).Contains("#STOPS:")) ;
        tempStr = tempStr.Remove(0, 7);

        while (tempStr != ";") {
            for (var i = 0; i < tempStr.Length; i++)
                if (char.ConvertFromUtf32(tempStr[i]) == "=")
                    equalPos[0] = i;

            float beat = float.Parse(tempStr.Substring(0, equalPos[0]));
            float delay = float.Parse(tempStr.Substring(equalPos[0] + 1, tempStr.Length - 1 - equalPos[0]));

            //for (var i = 0;)

            stepchartMover.delayData.Add(new StepchartMover.DelayData(beat, delay, ReadTimeFromBPM(beat)));
            tempStr = timeData.ReadLine();
        }*/

        while (!(tempStr = timeData.ReadLine()).Contains("#SPEEDS:")) ;
        tempStr = tempStr.Remove(0, 8);
        while (tempStr != ";") { //Reading speed
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
            tempStr = timeData.ReadLine();
        }


        while (!(tempStr = timeData.ReadLine()).Contains("#SCROLLS:")) ;
        tempStr = tempStr.Remove(0, 9);
        int scrollIndex = 0;

        while (tempStr != ";") { //Reading Scrolls
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
            tempStr = timeData.ReadLine();
        }
        timeData.Close();

        Debug.Log("Timing Data Generated");
        CreateStepchart(path);
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

        foreach (StepchartMover.DelayData delayInst in stepchartMover.delayData) {
            if (delayInst.beat > currentBeat)
                break;

            time += delayInst.delay;
        }

        if (bpm > 0)
            return time + (((currentBeat - beat) / bpm) * 60);

        return time;
    }
}
