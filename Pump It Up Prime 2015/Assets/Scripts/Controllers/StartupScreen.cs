using UnityEngine;
using System.Collections;
using UnityEngine.UI;
using System.IO;
using UnityEngine.SceneManagement;

public class StartupScreen : PlayerBase {

    public RawImage video;
    public string path;
    WWW startUpClip;

    void Start() {
        InputBase.players[0] = this;
        InputBase.players[1] = this;
        startUpClip = new WWW("file:///" + Path.Combine(Application.dataPath,path));

        MovieTexture instance = startUpClip.movie;
        while (!instance.isReadyToPlay) ;
        video.texture = instance;
        instance.Play();
        instance.loop = true;
    }

    public override void BeatInput(int inputValue, int beat) {
        if (beat == 2) 
            SceneManager.LoadScene(SceneIndex.menu);       
    }
}
