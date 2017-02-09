using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System.IO;
using UnityEngine.SceneManagement;

public class AssetLoadingBase : MonoBehaviour {

    // Use this for initialization
    public RawImage[] imageScreens;

    void Awake() {
        AssetDatabase.data.dataGroups = new DataGroup[imageScreens.Length];
        DirectoryInfo directory = new DirectoryInfo(Path.Combine(Application.dataPath, "AssetDatabase"));
        LoadGraphicFromDirectory(directory.GetDirectories()[SceneManager.GetActiveScene().buildIndex - 1], 0);     
    }


    public void LoadGraphicFromDirectory(DirectoryInfo directoryInfo, int imageIndex) {
        DirectoryInfo[] directories = directoryInfo.GetDirectories();

        if (directories.Length > 0) {
            for (var i = 0; i < imageScreens.Length; i++)
                LoadGraphicFromDirectory(directories[i], i);
        } else {
            FileInfo[] imageFiles = directoryInfo.GetFiles("*.png");
            FileInfo[] soundFiles = directoryInfo.GetFiles("*.wav");
            Debug.Log(imageIndex);

            AssetDatabase.data.dataGroups[imageIndex].dataBits = new List<DataBit>();

            for (var i = 0; i < imageFiles.Length; i++) {
                using (WWW image = new WWW("file:///" + imageFiles[i].FullName)) {
                    while (!image.isDone) ;
                    AssetDatabase.data.dataGroups[imageIndex].dataBits.Add(new DataBit(image.texture));
                    Destroy(image.texture);
                }
                Debug.Log(imageIndex + " " + i + " " + " " + imageFiles[i].Name);
            }
        }
    }

    public Texture LoadDataFromDatabase(int currentScreen, int itemToImport) {
        //if (AssetDatabase.data[currentState].dataGroups[currentScreen].dataBits[itemToImport].sound)
        //   actionSound.clip = AssetDatabase.data[currentState].dataGroups[currentScreen].dataBits[itemToImport].sound;

        if (AssetDatabase.data.dataGroups[currentScreen].dataBits[itemToImport].image)
            return AssetDatabase.data.dataGroups[currentScreen].dataBits[itemToImport].image;

        return null;
    }
}
