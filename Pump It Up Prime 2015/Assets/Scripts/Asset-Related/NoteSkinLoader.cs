using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;

[System.Serializable]
public struct NoteskinType {
    public Sprite tapNote;
    public Sprite holdNote;
    public Sprite endHoldNote;
}

public class NoteSkinLoader : MonoBehaviour {

    public int selectedSkin;
    public Vector2 targetedAspect;
    protected NoteskinType[] noteskins;
    string noteskinData;

    void Awake() {
        noteskins = new NoteskinType[5];
        noteskinData = Path.Combine(Path.Combine(Application.dataPath, "AssetDatabase"), "Noteskins");
        DirectoryInfo directory = new DirectoryInfo(noteskinData);
        DirectoryInfo[] noteSkinDirectories = directory.GetDirectories();
        LoadNoteSkinFromDirectory(noteSkinDirectories[selectedSkin]);
    }

    void LoadNoteSkinFromDirectory(DirectoryInfo directoryInfo) {
        DirectoryInfo[] directories = directoryInfo.GetDirectories();

        for (var i = 0; i < noteskins.Length; i++) {
            noteskins[i].tapNote = LoadSpriteFromNoteskin(directories[i + 1].GetFiles("0*"));
            noteskins[i].holdNote = LoadSpriteFromNoteskin(directories[i + 1].GetFiles("1*"));
            noteskins[i].endHoldNote = LoadSpriteFromNoteskin(directories[i + 1].GetFiles("2*"));
        }
    }

    Sprite LoadSpriteFromNoteskin(FileInfo[] files) {
        if (files.Length > 0) {
            using (WWW image = new WWW("file:///" + files[0].FullName)) {
                while (!image.isDone) ;
                return Sprite.Create(image.texture, new Rect(0, 0, image.texture.width,image.texture.height), new Vector2(0, 0));
            }
        }
        return null;
    }

}


