using UnityEngine;
using System.Collections;
using System.IO;

public class MainPlayerController : MonoBehaviour {

    public StepchartMover[] stepcharts;
    public InputBase inputBase;

    public AudioSource songPlayer;
    public float cRealTime;
    public float offset;
    public string dataPath;

    void Start() {
        for (var i = 0; i < stepcharts.Length; i++)
            if (PlayerPref.playerSettings[i].life > 0) {
                stepcharts[i].InitialiseStepchart(i);
                stepcharts[i].playerManager = this;
            } else {
                stepcharts[i].gameObject.SetActive(false);
                if (inputBase.currentGameMode == InputBase.GameMode.Single)
                    stepcharts[i].stepchartBuilder.sequenceZoneToMeasure[i].gameObject.SetActive(false);
            }

        string endExt = ".wav";
        dataPath = Application.dataPath;

#if UNITY_ANDROID
        dataPath = Application.persistentDataPath;
        endExt = ".mp3";
#endif

        WWW song = new WWW("file:///" + Path.Combine(Path.Combine(dataPath, "Songs"), PlayerPref.songs[PlayerPref.songIndex].name + endExt));

        while (!song.isDone) ;
        songPlayer.clip = song.GetAudioClip(false);

        songPlayer.pitch = PlayerPref.prefRush;
        songPlayer.Play();
        offset += Time.realtimeSinceStartup;
    }

    void Update() {
        cRealTime = Time.realtimeSinceStartup - offset;
    }
}
