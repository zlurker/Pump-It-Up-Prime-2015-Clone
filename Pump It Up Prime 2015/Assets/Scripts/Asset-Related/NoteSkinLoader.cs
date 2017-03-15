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

    string noteskinData;

    void Awake() {
        AssetDatabase.noteskins = new NoteskinType[5];
        noteskinData = Path.Combine(Path.Combine(Application.dataPath, "AssetDatabase"), "Noteskins");
        DirectoryInfo directory = new DirectoryInfo(noteskinData);
        DirectoryInfo[] noteSkinDirectories = directory.GetDirectories();
        LoadNoteSkinFromDirectory(noteSkinDirectories[0]);
    }

    void LoadNoteSkinFromDirectory(DirectoryInfo directoryInfo) {
        DirectoryInfo[] directories = directoryInfo.GetDirectories();

        AssetDatabase.seqZone = LoadSpriteFromNoteskin(directories[0].GetFiles("*.png"), new Vector2(0.5f, 0.5f));

        for (var i = 0; i < AssetDatabase.noteskins.Length; i++) {
            AssetDatabase.noteskins[i].tapNote = LoadSpriteFromNoteskin(directories[i + 1].GetFiles("0*"), new Vector2(0.5f, 0.5f));
            AssetDatabase.noteskins[i].holdNote = LoadSpriteFromNoteskin(directories[i + 1].GetFiles("1*"), new Vector2(0.5f, 1f));
            AssetDatabase.noteskins[i].endHoldNote = LoadSpriteFromNoteskin(directories[i + 1].GetFiles("2*"), new Vector2(0.5f, 0.5f));
        }
    }

    Sprite LoadSpriteFromNoteskin(FileInfo[] files, Vector2 pivot) {
        if (files.Length > 0) {
            using (WWW image = new WWW("file:///" + files[0].FullName)) {
                while (!image.isDone) ;
                return Sprite.Create(image.texture, new Rect(0, 0, image.texture.width, image.texture.height), pivot);
            }
        }
        return null;
    }
}


