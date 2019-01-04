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
    public GameObject pass2Obj;
    public GameObject objBGM;
    IEnumerator routine=null;

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
        objBGM = GameObject.Find("BGMManager");
        startObj = GameObject.Find("StartButton");
        nameObj = GameObject.Find("InputField"); nameObj.SetActive(false);
        //スライダーの現在位置をセーブされていた位置にする。
        GameObject.Find("SliderBGM").GetComponent<Slider>().value = PlayerPrefs.GetFloat("BGMVolume", 0.8f);
        GameObject.Find("SliderSE").GetComponent<Slider>().value = PlayerPrefs.GetFloat("SEVolume", 0.8f);
        //BGM再生
        DontDestroyOnLoad(objBGM);//BGMマネージャーのオブジェクトはタイトル画面で作ってゲーム終了までそれを使用。
        objBGM.GetComponent<AudioSource>().volume = PlayerPrefs.GetFloat("BGMVolume", 0.8f);
        objBGM.GetComponent<BGMManager>().bgmChange(true, 0);//BGMManager内部変数の初期化
    }

    // Update is called once per frame
    void Update()
    {
        timeCount++;
    }

    public void PushStartButton()
    {
        if (routine != null) { StopCoroutine(routine); }
        routine = null;
        routine = ButtonComeBack(60);
        StartCoroutine(routine);
        nameObj.SetActive(true);
        startObj.SetActive(false);
        selectObj.SetActive(false);
        pass2Obj.SetActive(false);
    }

    public void PushDecideButton()
    {
        string scenarioName, scenarioPass,dataFolderPath;
        scenarioName = GameObject.Find("InputField").GetComponent<InputField>().text;
        scenarioPass = GameObject.Find("InputFieldPass").GetComponent<InputField>().text;
        if (Application.platform == RuntimePlatform.WindowsPlayer || Application.platform == RuntimePlatform.WindowsEditor)
        {
            dataFolderPath = @GetComponent<Utility>().GetAppPath() + objBGM.GetComponent<BGMManager>().folderChar + scenarioName + ".zip";
        }
        else
        {
            dataFolderPath = @GetComponent<Utility>().GetAppPath().Substring(0, @GetComponent<Utility>().GetAppPath().Length - 25) + objBGM.GetComponent<BGMManager>().folderChar + scenarioName + ".zip";
        }
        ZipMake(dataFolderPath, scenarioPass);
        PlayerPrefs.SetString("進行中シナリオ",dataFolderPath);
        GetComponent<Utility>().StartCoroutine("LoadSceneCoroutine", "MapScene");
    }

    public void PushSelectButton()
    {

        if (routine != null) { StopCoroutine(routine); }
        routine = null;
        routine = ButtonComeBack(60);
        StartCoroutine(routine);
        nameObj.SetActive(false);
        startObj.SetActive(false);
        selectObj.SetActive(false);
        GetComponent<GracesGames.SimpleFileBrowser.Scripts.FileOpenManager>().GetFilePathWithKey("進行中シナリオ");
    }

    private IEnumerator ButtonComeBack(int time)
    {
        for (int i = 0; i < time; i++) { yield return null; }
        startObj.SetActive(true);
        selectObj.SetActive(true);
    }

    public void PushJumpButton()
    {
        Application.OpenURL("http://www1366uj.sakura.ne.jp/public_html/scenario/upload/upload.cgi");
    }

    private void ZipMake(string scenarioName,string scenarioPass)
    {
        string str="[END]";
        string file = @GetComponent<Utility>().GetAppPath() + objBGM.GetComponent<BGMManager>().folderChar + "[system]mapdata[system].txt";
        string file2 = @GetComponent<Utility>().GetAppPath() + objBGM.GetComponent<BGMManager>().folderChar + "[system]password[system].txt";
        //先に[system]mapdata.txtと[system]password.txtを一時的に書き出しておく。

        str = ",,,,,,,,,,,[system]PC版スタート地点[system].txt\r\n,,,,,,,,,,,[system]導入シーン(導入は発生条件なしで作るのがお勧め).txt\r\n[END]";
        System.IO.File.WriteAllText(file, str);
        System.IO.File.WriteAllText(file2, scenarioPass);

        //作成するZIP書庫のパス
        string zipPath = scenarioName;
        //ZipFileオブジェクトの作成
        ICSharpCode.SharpZipLib.Zip.ZipFile zf =
            ICSharpCode.SharpZipLib.Zip.ZipFile.Create(zipPath);
        zf.Password = Secret.SecretString.zipPass;
        //mapdataファイルだけ入れておく()
        zf.BeginUpdate();
        zf.Add(file, "[system]mapdata[system].txt");
        zf.Add(file2, "[system]password[system].txt");
        zf.CommitUpdate();

        //閉じる
        zf.Close();

        //一時的に書きだした[system]mapdata.txtを消去する。
        System.IO.File.Delete(file);
        System.IO.File.Delete(file2);
    }

    public void PushPasswordButton()
    {
        string pass;
        string text="";
        pass=pass2Obj.GetComponent<InputField>().text;
        try
        {
            //閲覧するエントリ
            string extractFile = "[system]password[system].txt";

            //ZipFileオブジェクトの作成
            ICSharpCode.SharpZipLib.Zip.ZipFile zf =
                new ICSharpCode.SharpZipLib.Zip.ZipFile(PlayerPrefs.GetString("進行中シナリオ", ""));
            zf.Password = Secret.SecretString.zipPass;
            //展開するエントリを探す
            ICSharpCode.SharpZipLib.Zip.ZipEntry ze = zf.GetEntry(extractFile);

            try
            {
                if (ze != null)
                {
                    //閲覧するZIPエントリのStreamを取得
                    System.IO.Stream reader = zf.GetInputStream(ze);
                    //文字コードを指定してStreamReaderを作成
                    System.IO.StreamReader sr = new System.IO.StreamReader(
                        reader, System.Text.Encoding.GetEncoding("UTF-8"));
                    // テキストを取り出す
                    text = sr.ReadToEnd();
                    //閉じる
                    sr.Close();
                    reader.Close();
                }
            }
            catch { }

            //閉じる
            zf.Close();
        }
        catch
        {
            GameObject.Find("InputFieldPass2Guide").GetComponent<Text>().text = "シナリオファイルに異常があります。";
            return;
        }
        if (text=="" || text == pass) { GetComponent<Utility>().StartCoroutine("LoadSceneCoroutine", "MapScene"); }
        else { GameObject.Find("InputFieldPass2Guide").GetComponent<Text>().text="パスワードが違います。"; return; }        
    }

}