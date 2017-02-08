using UnityEngine;
using System.Collections;
using System.IO;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class MainPlayerController : MonoBehaviour {

    public StepchartMover[] stepcharts;

    public AudioSource songPlayer;
    public float cRealTime;
    public float offset;
    public string dataPath;

    public RawImage previewImage;
    public CursorLockMode cursorMode;

    public RawImage video;
    public string path;
    WWW startUpClip;
    MovieTexture instance;

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

        DirectoryInfo directory = new DirectoryInfo(PlayerPref.songs[PlayerPref.channels[PlayerPref.currentChannel].references[PlayerPref.currentChannelSong]].path);
        FileInfo[] temp = directory.GetFiles("*.wav");

        WWW song = new WWW("file:///" + temp[0].FullName);

        while (!song.isDone) ;
        songPlayer.clip = song.GetAudioClip(false);
        temp = directory.GetFiles("*.PNG");

        using (WWW image = new WWW("file:///" + temp[0].FullName)) {
            while (!image.isDone) ;
            previewImage.texture = image.texture;
        }

        temp = directory.GetFiles("*.ogv");

        if (temp.Length > 0)
            path = temp[0].FullName;
        else
            path = Path.Combine(Application.dataPath, path);

        startUpClip = new WWW("file:///" + path);

        instance = startUpClip.movie;
        while (!instance.isReadyToPlay) ;
        previewImage.texture = instance;
        instance.loop = true;

        songPlayer.pitch = PlayerPref.prefRush;
        instance.Play();
        songPlayer.Play();
        offset += Time.realtimeSinceStartup;
        KinectManager.ChangeFeetSize(0.2f);
    }

    void Update() {
        cRealTime = Time.realtimeSinceStartup - offset;

        if (!songPlayer.isPlaying)
            SceneManager.LoadScene(SceneIndex.scoreScreen);
    }
}
