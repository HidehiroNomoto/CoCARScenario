using System;
using UnityEngine;
using UnityEngine.UI;


namespace GracesGames.SimpleFileBrowser.Scripts
{
    public class FileOpenManager : MonoBehaviour
    {

        // Use the file browser prefab
        public GameObject FileBrowserPrefab;
        // Define a file extension
        public string[] FileExtensions;

        public bool PortraitMode;

        // Use this for initialization
        void Start()
        {
        }

        // Update is called once per frame
        void Update()
        {

        }

        // Open the file browser using boolean parameter so it can be called in GUI
        public void OpenFileBrowser(bool saving)
        {
            OpenFileBrowser(saving ? FileBrowserMode.Save : FileBrowserMode.Load);
        }

        // Open a file browser to save and load files
        private void OpenFileBrowser(FileBrowserMode fileBrowserMode)
        {
            // Create the file browser and name it
            GameObject fileBrowserObject = Instantiate(FileBrowserPrefab, transform);
            fileBrowserObject.name = "FileBrowser";
            // Set the mode to save or load
            FileBrowser fileBrowserScript = fileBrowserObject.GetComponent<FileBrowser>();
            fileBrowserScript.SetupFileBrowser(PortraitMode ? ViewMode.Portrait : ViewMode.Landscape);
            if (fileBrowserMode == FileBrowserMode.Save)
            {
                fileBrowserScript.SaveFilePanel("NoName", FileExtensions);
                // Subscribe to OnFileSelect event (call SaveFileUsingPath using path) 
                fileBrowserScript.OnFileSelect += SaveFileUsingPath;
            }
            else
            {
                fileBrowserScript.OpenFilePanel(FileExtensions);
                // Subscribe to OnFileSelect event (call LoadFileUsingPath using path) 
                fileBrowserScript.OnFileSelect += LoadFileUsingPath;
            }
        }

        //★ここを改造する
        private void SaveFileUsingPath(string path)
        {
            // Make sure path and _textToSave is not null or empty
            if (!String.IsNullOrEmpty(path))
            {
                //ファイルをアーカイブ化して保存
            }
            else
            {
                Debug.Log("Invalid path or empty file given");
            }
        }

        // Loads a file using a path★ここを改造する
        private void LoadFileUsingPath(string path)
        {
            if (GameObject.Find("BGMManager").GetComponent<BGMManager>().saveKey == "進行中シナリオ")
            {
                if (path != null && path.Length != 0)
                {
                    //フラグ情報の全消去（別シナリオのフラグが生きてると意図せぬバッティングなどバグの元）
                    //セーブデータを全部消す
                    PlayerPrefs.DeleteAll();

                    PlayerPrefs.SetString("進行中シナリオ",path);
                    GameObject.Find("TitleManager").GetComponent<TitleManager>().pass2Obj.SetActive(true);
                }
            }
        }

        public void GetFilePathWithKey(string key)
        {
            GameObject.Find("BGMManager").GetComponent<BGMManager>().saveKey = key;
            OpenFileBrowser(false);
        }
    }
}
