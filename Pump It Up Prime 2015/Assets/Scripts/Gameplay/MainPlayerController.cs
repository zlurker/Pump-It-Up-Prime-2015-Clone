using UnityEngine;
using System.Collections;
using System.IO;
using UnityEngine.UI;

public class MainPlayerController : MonoBehaviour {

    public StepchartMover[] stepcharts;

    public AudioSource songPlayer;
    public float cRealTime;
    public float offset;
    public string dataPath;

    public RawImage previewImage;

    void Start() {
        for (var i = 0; i < stepcharts.Length; i++)
            if (PlayerPref.playerSettings[i].life > 0) {
                stepcharts[i].playerManager = this;
                stepcharts[i].InitialiseStepchart(i);
            } else {
                stepcharts[i].gameObject.SetActive(false);
                if (InputBase.currentGameMode == InputBase.GameMode.Single)
                    stepcharts[i].stepchartBuilder.sequenceZoneToMeasure[i].gameObject.SetActive(false);
            }

        dataPath = Application.dataPath;

#if UNITY_ANDROID
        dataPath = Application.persistentDataPath;
        endExt = ".mp3";
#endif
        DirectoryInfo directory = new DirectoryInfo(PlayerPref.songs[PlayerPref.songIndex].path);
        FileInfo[] temp = directory.GetFiles("*.wav");

        WWW song = new WWW("file:///" + temp[0].FullName);

        while (!song.isDone) ;
        songPlayer.clip = song.GetAudioClip(false);
        temp = directory.GetFiles("*.PNG");

        using (WWW image = new WWW("file:///" + temp[0].FullName)) {
            while (!image.isDone) ;
            previewImage.texture = image.texture;
        }

        songPlayer.pitch = PlayerPref.prefRush;
        songPlayer.Play();
        offset += Time.realtimeSinceStartup;
    }

    void Update() {
        cRealTime = Time.realtimeSinceStartup - offset;
    }
}
