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
        pass2Obj.SetActive(false);
    }

    public void PushDecideButton()
    {
        string scenarioName,scenarioPass;
        scenarioName = GameObject.Find("InputField").GetComponent<InputField>().text;
        scenarioPass= GameObject.Find("InputFieldPass").GetComponent<InputField>().text;
        ZipMake(scenarioName,scenarioPass);
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

    private void ZipMake(string scenarioName,string scenarioPass)
    {
        string str="[END]";
        string file = @GetComponent<Utility>().GetAppPath() + @"\" + "[system]mapdata.txt";
        string file2 = @GetComponent<Utility>().GetAppPath() + @"\" + "[system]password.txt";
        //先に[system]mapdata.txtと[system]password.txtを一時的に書き出しておく。

        str = ",,,,,,,,,,,導入シーン(導入は発生条件なしで作るのがお勧め).txt\r\n[END]";
        System.IO.File.WriteAllText(file, str);
        System.IO.File.WriteAllText(file2, scenarioPass);

        //作成するZIP書庫のパス
        string zipPath = @GetComponent<Utility>().GetAppPath() + @"\" + scenarioName + ".zip";
        //ZipFileオブジェクトの作成
        ICSharpCode.SharpZipLib.Zip.ZipFile zf =
            ICSharpCode.SharpZipLib.Zip.ZipFile.Create(zipPath);
        zf.Password = Secret.SecretString.zipPass;
        //mapdataファイルだけ入れておく()
        zf.BeginUpdate();
        zf.Add(file, "[system]mapdata.txt");
        zf.Add(file2, "[system]password.txt");
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
            string extractFile = "[system]password.txt";

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