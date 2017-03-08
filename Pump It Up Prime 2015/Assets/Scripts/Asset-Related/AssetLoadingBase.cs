using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System.IO;
using UnityEngine.SceneManagement;

public class AssetLoadingBase : MonoBehaviour {

    [System.Serializable]
    public struct ImageGroup {
        public RawImage[] imageScreens;
    }

    public bool justCreateDirectory;
    public ImageGroup[] imageScreenGroup;

    void Awake() {
        if (justCreateDirectory)
            CreateDirectory();

        AssetDatabase.data.dataGroups = new DataGroup[imageScreenGroup.Length];
        DirectoryInfo directory = new DirectoryInfo(Path.Combine(Application.dataPath, "AssetDatabase"));
        LoadGraphicFromDirectory(directory.GetDirectories()[SceneManager.GetActiveScene().buildIndex - 1], 0);
        AutoLoadAssets();
    }

    void AutoLoadAssets() {
        for (var i = 0; i < imageScreenGroup.Length; i++)
            for (var j = 0; j < imageScreenGroup[i].imageScreens.Length; j++) {
                imageScreenGroup[i].imageScreens[j].name = i.ToString();
                LoadDataFromDatabase(imageScreenGroup[i].imageScreens[j], 0);
            }
    }

    public void CreateDirectory() {
        DirectoryInfo directory = new DirectoryInfo(Path.Combine(Application.dataPath, "AssetDatabase")).GetDirectories()[SceneManager.GetActiveScene().buildIndex - 1];

        for (var i = 0; i < imageScreenGroup.Length; i++) {
            string destination = Path.Combine(directory.FullName, i.ToString() + " - " + imageScreenGroup[i].imageScreens[0].gameObject.name);
            DirectoryInfo[] tempDir;
            if ((tempDir = directory.GetDirectories(i.ToString() + "*")).Length > 0) {
                if (destination != tempDir[0].FullName)
                    tempDir[0].MoveTo(destination);
            } else
                directory.CreateSubdirectory(destination);
        }
    }

    public void LoadGraphicFromDirectory(DirectoryInfo directoryInfo, int imageIndex) {
        DirectoryInfo[] directories = directoryInfo.GetDirectories();

        if (directories.Length > 0) {
            for (var i = 0; i < imageScreenGroup.Length; i++)
                LoadGraphicFromDirectory(directories[i], i);
        } else {
            FileInfo[] imageFiles = directoryInfo.GetFiles("*.png");

            AssetDatabase.data.dataGroups[imageIndex].dataBits = new List<DataBit>();

            for (var i = 0; i < imageFiles.Length; i++) {
                using (WWW image = new WWW("file:///" + imageFiles[i].FullName)) {
                    while (!image.isDone) ;
                    AssetDatabase.data.dataGroups[imageIndex].dataBits.Add(new DataBit(image.texture));
                    Destroy(image.texture);
                }
            }

            imageFiles = directoryInfo.GetFiles("*.txt");

            if (imageFiles.Length > 0) {
                File.ReadAllText(imageFiles[0].FullName);
                AssetDatabase.data.dataGroups[imageIndex].uiScaleValue = float.Parse(File.ReadAllText(imageFiles[0].FullName));
            } else {
                AssetDatabase.data.dataGroups[imageIndex].uiScaleValue = 1;
            }
        }
    }

    public void LoadDataFromDatabase(RawImage screen, int itemToImport) {
        //if (AssetDatabase.data[currentState].dataGroups[currentScreen].dataBits[itemToImport].sound)
        //   actionSound.clip = AssetDatabase.data[currentState].dataGroups[currentScreen].dataBits[itemToImport].sound;

        if (AssetDatabase.data.dataGroups[int.Parse(screen.name)].dataBits.Count > itemToImport)
            if (AssetDatabase.data.dataGroups[int.Parse(screen.name)].dataBits[itemToImport].image)
                screen.texture = AssetDatabase.data.dataGroups[int.Parse(screen.name)].dataBits[itemToImport].image;

        screen.SetNativeSize();
        screen.transform.localScale = Vector3.one / AssetDatabase.data.dataGroups[int.Parse(screen.name)].uiScaleValue;
    }
}
