using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class TitleManager : MonoBehaviour {

    private int timeCount;                                           //シーン開始からのフレーム数
    public GameObject FileBrowserPrefab;
    private GameObject nameObj;
    private GameObject startObj;
    public GameObject selectObj;

    // Use this for initialization
    void Start()
    {
        //PlayerPrefs.DeleteAll();
        if (Application.platform == RuntimePlatform.WindowsPlayer ||
Application.platform == RuntimePlatform.OSXPlayer ||
Application.platform == RuntimePlatform.LinuxPlayer)
        {
            Screen.SetResolution(900, 640, false);
        }
        selectObj = GameObject.Find("SelectButton");
        startObj = GameObject.Find("StartButton");
        nameObj = GameObject.Find("InputField");nameObj.SetActive(false);
        //スライダーの現在位置をセーブされていた位置にする。
        GameObject.Find("SliderBGM").GetComponent<Slider>().value = PlayerPrefs.GetFloat("BGMVolume", 0.8f);
        GameObject.Find("SliderSE").GetComponent<Slider>().value = PlayerPrefs.GetFloat("SEVolume", 0.8f);
        //BGM再生
        DontDestroyOnLoad(GameObject.Find("BGMManager"));//BGMマネージャーのオブジェクトはタイトル画面で作ってゲーム終了までそれを使用。
        GameObject.Find("BGMManager").GetComponent<AudioSource>().volume = PlayerPrefs.GetFloat("BGMVolume", 0.8f);
        GetComponent<Utility>().BGMPlay(Resources.Load<AudioClip>("TitleBGM"));
        GameObject.Find("BGMManager").GetComponent<BGMManager>().bgmChange(true, 0);//BGMManager内部変数の初期化
    }

    // Update is called once per frame
    void Update()
    {
        timeCount++;
    }

    public void PushStartButton()
    {
        nameObj.SetActive(true);
        startObj.SetActive(false);
        selectObj.SetActive(true);
    }

    public void PushDecideButton()
    {
        string scenarioName;
        scenarioName = GameObject.Find("InputField").GetComponent<InputField>().text;
        ZipMake(scenarioName);
        PlayerPrefs.SetString("進行中シナリオ", @GetComponent<Utility>().GetAppPath() + @"\" + scenarioName + ".zip");
        GetComponent<Utility>().StartCoroutine("LoadSceneCoroutine", "MapScene");
    }

    public void PushSelectButton()
    {
        nameObj.SetActive(false);
        startObj.SetActive(true);
        selectObj.SetActive(false);
        GetComponent<GracesGames.SimpleFileBrowser.Scripts.FileOpenManager>().GetFilePathWithKey("進行中シナリオ");
    }

    private void ZipMake(string scenarioName)
    {
        string str="[END]";
        string file = @GetComponent<Utility>().GetAppPath() + @"\" + "mapdata.txt";

        //先にmapdata.txtを一時的に書き出しておく。
        str = ",,,,,,,,,,,導入シーン(導入は発生条件なしで作るのがお勧め).txt\r\n[END]";
        System.IO.File.WriteAllText(file, str);

        //作成するZIP書庫のパス
        string zipPath = @GetComponent<Utility>().GetAppPath() + @"\" + scenarioName + ".zip";
        //ZipFileオブジェクトの作成
        ICSharpCode.SharpZipLib.Zip.ZipFile zf =
            ICSharpCode.SharpZipLib.Zip.ZipFile.Create(zipPath);

        //mapdataファイルだけ入れておく()
        zf.BeginUpdate();
        zf.Add(file, "mapdata.txt");
        zf.CommitUpdate();

        //閉じる
        zf.Close();

        //一時的に書きだしたmapdata.txtを消去する。
        System.IO.File.Delete(file);
    }

}