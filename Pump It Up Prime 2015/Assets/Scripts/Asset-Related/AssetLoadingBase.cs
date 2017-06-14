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

    public Data data;
    public bool justCreateDirectory;
    public ImageGroup[] imageScreenGroup;

    void Awake() {
        if (justCreateDirectory)
            CreateDirectory();

        data.dataGroups = new DataGroup[imageScreenGroup.Length];
        DirectoryInfo directory = new DirectoryInfo(Path.Combine(Application.dataPath, "AssetDatabase"));
        LoadGraphicFromDirectory(directory.GetDirectories()[SceneManager.GetActiveScene().buildIndex - 1], 0,1);
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

    public void LoadGraphicFromDirectory(DirectoryInfo directoryInfo, int imageIndex,float scaleValue) {
        DirectoryInfo[] directories = directoryInfo.GetDirectories();

        FileInfo[] scaleData = directoryInfo.GetFiles("*.txt");

        if (scaleData.Length > 0) 
            scaleValue = float.Parse(File.ReadAllText(scaleData[0].FullName));
        
        if (directories.Length > 0) {
            for (var i = 0; i < imageScreenGroup.Length; i++)
                LoadGraphicFromDirectory(directories[i], i,scaleValue);
        } else {
            FileInfo[] imageFiles = directoryInfo.GetFiles("*.png");
            data.dataGroups[imageIndex].uiScaleValue = scaleValue;
            data.dataGroups[imageIndex].dataBits = new DataBit[imageFiles.Length];

            for (var i = 0; i < imageFiles.Length; i++) {
                using (WWW image = new WWW("file:///" + imageFiles[i].FullName)) {
                    while (!image.isDone) ;
                    data.dataGroups[imageIndex].dataBits[i] = new DataBit(image.texture);
                    Destroy(image.texture);
                }
            }         
        }
    }

    public void LoadDataFromDatabase(RawImage screen, int itemToImport) {
        //if (data[currentState].dataGroups[currentScreen].dataBits[itemToImport].sound)
        //   actionSound.clip = data[currentState].dataGroups[currentScreen].dataBits[itemToImport].sound;

        if (data.dataGroups[int.Parse(screen.name)].dataBits.Length > itemToImport)
            if (data.dataGroups[int.Parse(screen.name)].dataBits[itemToImport].image)
                screen.texture = data.dataGroups[int.Parse(screen.name)].dataBits[itemToImport].image;

        screen.SetNativeSize();
        screen.transform.localScale = Vector3.one / data.dataGroups[int.Parse(screen.name)].uiScaleValue;
    }
}
