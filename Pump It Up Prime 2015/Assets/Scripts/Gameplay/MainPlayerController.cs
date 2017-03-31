using UnityEngine;
using System.Collections;
using System.IO;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

[System.Serializable]
public struct UIInstance {
    public RawImage health;
    public GameObject healthFrame;
    public RawImage stageNumber;
    public Animation grade;
    public SpriteRenderer gradeT;
    public SpriteRenderer comboGraphic;
    public Text comboT;
    public GameObject sequenceZone;
    public RawImage healthCover;
    public float healthbarPivotSide;
    public RectTransform healthbarPivot;
}

public class MainPlayerController : AssetLoadingBase {

    public StepchartMover[] stepcharts;
    public UIInstance[] uis;

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
        for (var i = 0; i < stepcharts.Length; i++) {
            InputBase.players[i] = stepcharts[i];
            if (PlayerPref.playerSettings[i].life > 0) {
                stepcharts[i].gameObject.SetActive(true);
                stepcharts[i].playerManager = this;
                stepcharts[i].InitialiseStepchart(i);

                if (InputBase.currentGameMode == InputBase.GameMode.Double)
                    stepcharts[i].uiToUse = uis[2];
                else
                    stepcharts[i].uiToUse = uis[i];

                stepcharts[i].InitialiseUI();
            }
        }


        dataPath = Application.dataPath;

        DirectoryInfo directory = new DirectoryInfo(PlayerPref.songs[PlayerPref.currSong].path);
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
