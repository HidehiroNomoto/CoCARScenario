using System;
using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;

public class MapScene : MonoBehaviour
{
    Sprite mapImage;
    private float intervalTime = 0.0f;
    private int width = 640;
    private int height = 640;
    private double longitude;
    private double latitude;
    private double longitudeMap;
    private double latitudeMap;
    private int zoom = 16;
    private float targetX=0;
    private float targetY=0;
    List<string> mapData=new List<string>();
    private bool sceneChange = false;
    GameObject mapImageObj;
    public GameObject objIvent;
    GameObject objBGM;
    List<GameObject> objIB = new List<GameObject>();
    GameObject parentObject;
    public int selectNum = -1;
    string _FILE_HEADER;
    public InputField[] inputField=new InputField[14];
    public GameObject objCCB;
    public AudioClip errorSE;
    public GameObject FirstPlace;

    void Start()
    {
        for (int i = 0; i < 12; i++) { inputField[i] = GameObject.Find("InputField" + i.ToString()).GetComponent<InputField>(); }
        parentObject = GameObject.Find("Content");
        _FILE_HEADER = PlayerPrefs.GetString("進行中シナリオ","");                      //ファイル場所の頭
        longitude=PlayerPrefs.GetFloat("longitude",135.768738f); latitude = PlayerPrefs.GetFloat("latitude", 35.010348f);
        mapImageObj = GameObject.Find("mapImage").gameObject as GameObject;
        objBGM= GameObject.Find("BGMManager").gameObject as GameObject;
        LoadMapData("[system]mapdata.txt");
        GetMap();
    }

    void Update()
    {
    }

    public void GetMap()
    {
        //マップを取得
        StartCoroutine(GetStreetViewImage(latitude, longitude, zoom));
    }


    private IEnumerator GetStreetViewImage(double latitude, double longitude, double zoom)
    {
        string url="";
        if (Application.platform == RuntimePlatform.IPhonePlayer) { url = "http://maps.googleapis.com/maps/api/staticmap?center=" + latitude + "," + longitude + "&zoom=" + zoom + "&size=" + width + "x" + height + Secret.SecretString.iPhoneKey; }
        if (Application.platform == RuntimePlatform.Android) { url = "http://maps.googleapis.com/maps/api/staticmap?center=" + latitude + "," + longitude + "&zoom=" + zoom + "&size=" + width + "x" + height + Secret.SecretString.androidKey; }
        if(Application.platform == RuntimePlatform.WindowsEditor || Application.platform == RuntimePlatform.WindowsPlayer || Application.platform == RuntimePlatform.OSXEditor || Application.platform == RuntimePlatform.OSXPlayer) { url = "http://maps.googleapis.com/maps/api/staticmap?center=" + latitude + "," + longitude + "&zoom=" + zoom + "&size=" + width + "x" + height + Secret.SecretString.androidKey; ; }
        WWW www = new WWW(url);
        yield return www;
        //マップの画像をTextureからspriteに変換して貼り付ける
        mapImage = Sprite.Create(www.texture, new Rect(0, 0, 640, 640), Vector2.zero);
        mapImageObj.GetComponent<Image>().sprite=mapImage;

        //地図の中心の緯度経度を保存
        longitudeMap = longitude;
        latitudeMap = latitude;

        //targetの位置を中心に
        targetX = 0;targetY = 0;
    }

    //目次ファイルを読み込む。
    private void LoadMapData(string path)
    {
        string[] strs;
        try
        {
        //閲覧するエントリ
        string extractFile = path;

        //ZipFileオブジェクトの作成
        ICSharpCode.SharpZipLib.Zip.ZipFile zf =
            new ICSharpCode.SharpZipLib.Zip.ZipFile(PlayerPrefs.GetString("進行中シナリオ",""));
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
                    string text = sr.ReadToEnd();

                    // 読み込んだ目次テキストファイルからstring配列を作成する
                    mapData.AddRange(text.Split('\n'));
                    //閉じる
                    sr.Close();
                    reader.Close();
                    mapData.RemoveAt(mapData.Count - 1);//最終行は[END]なので除去。
                    //イベントをボタンとして一覧に放り込む。
                    for (int i = 0; i < mapData.Count; i++)
                    {
                        objIB.Add(Instantiate(objIvent) as GameObject);
                        objIB[i].transform.SetParent(parentObject.transform, false);
                        objIB[i].GetComponentInChildren<Text>().text = MapDataToButton(mapData[i]);
                        objIB[i].GetComponent<IventButton>().buttonNum = i;
                        ScenarioFileCheck(i,zf);
                    }
                }
                else
                {
                    GetComponent<Utility>().StartCoroutine("LoadSceneCoroutine", "TitleScene");
                }
            }
            catch{ }

            ze = zf.GetEntry("[system]Command1PC版スタート地点.txt");
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
                    string text = sr.ReadToEnd();

                    // 読み込んだ目次テキストファイルからstring配列を作成する
                    strs = text.Split('\n');
                    strs = strs[1].Substring(12).Replace("\r", "").Replace("\n", "").Split(',');
                    //閉じる
                    sr.Close();
                    reader.Close();
                    latitude = Convert.ToDouble(strs[0]); longitude = Convert.ToDouble(strs[1]);
                    objIB[0].GetComponentInChildren<Text>().text = "[system]PC版スタート地点　緯:" + latitude.ToString() + ",経:" + longitude.ToString();
                }
            }
            catch { }

        //閉じる
        zf.Close();
        }
        catch
        {
            GetComponent<Utility>().StartCoroutine("LoadSceneCoroutine", "TitleScene");
        }
    }

    public void IventAddButton()
    {
        //追加ボタンが押されたらイベントボタンを追加する。
            objIB.Insert(selectNum+1, Instantiate(objIvent) as GameObject);
            objIB[selectNum+1].transform.SetParent(parentObject.transform, false);
            objIB[selectNum+1].GetComponent<IventButton>().buttonNum = selectNum+1;
            objIB[selectNum+1].GetComponent<Transform>().SetSiblingIndex(selectNum+1);
            for (int i = selectNum+2; i < objIB.Count; i++) { objIB[i].GetComponent<IventButton>().buttonNum++; }//追加分の後ろはボタン番号が１増える。
            mapData.Insert(selectNum+1,"");
    }

    public void IventDeleteButton()
    {
        if (selectNum > 0)
        {
            Destroy(objIB[selectNum]);
            objIB.RemoveAt(selectNum);
            for (int i = selectNum; i < objIB.Count; i++) { objIB[i].GetComponent<IventButton>().buttonNum--; }//削除分の後ろはボタン番号が１減る。
            if (mapData.Count - 1 >= selectNum) { mapData.RemoveAt(selectNum); }
            selectNum = -1;
        }
        else
        {
            AudioSource bgm = GameObject.Find("BGMManager").GetComponent<AudioSource>(); bgm.loop = false; bgm.clip = errorSE; bgm.Play();
        }
    }

    public void IventCreateButton()
    {
        string[] strs;
        if (selectNum > 0)
        {
            try
            {
                InputDecideButton();
                MakeMapDataFile();
                strs = mapData[selectNum].Replace("\r", "").Replace("\n", "").Split(',');
                objBGM.GetComponent<BGMManager>().chapterName = strs[11];
                GetComponent<Utility>().StartCoroutine("LoadSceneCoroutine", "NovelScene");
            }
            catch
            {
            }
        }
    }

    public void SetIvent()
    {
        string[] strs;
        try
        {
            if (selectNum > 0)
            {
                FirstPlace.SetActive(false);
                strs = mapData[selectNum].Replace("\r","").Replace("\n","").Split(',');
                inputField[0].text = strs[11].Substring(0, strs[11].Length - 4);
                inputField[1].text = strs[0];
                inputField[2].text = strs[1];
                inputField[3].text = strs[2];
                inputField[4].text = strs[3];
                inputField[5].text = strs[4];
                inputField[6].text = strs[5];
                inputField[7].text = strs[6];
                inputField[8].text = strs[7];
                inputField[9].text = strs[8];
                inputField[10].text = strs[9];
                inputField[11].text = strs[10];
                latitude = Convert.ToDouble(strs[0]); longitude = Convert.ToDouble(strs[1]);
            }
            if (selectNum == 0)
            {
                FirstPlace.SetActive(true);

                    //閲覧するエントリ
                    string extractFile = "[system]Command1PC版スタート地点.txt";

                    //ZipFileオブジェクトの作成
                    ICSharpCode.SharpZipLib.Zip.ZipFile zf =
                        new ICSharpCode.SharpZipLib.Zip.ZipFile(PlayerPrefs.GetString("進行中シナリオ", ""));
                    zf.Password = Secret.SecretString.zipPass;
                    //展開するエントリを探す
                    ICSharpCode.SharpZipLib.Zip.ZipEntry ze = zf.GetEntry(extractFile);

                        if (ze != null)
                        {
                            //閲覧するZIPエントリのStreamを取得
                            System.IO.Stream reader = zf.GetInputStream(ze);
                            //文字コードを指定してStreamReaderを作成
                            System.IO.StreamReader sr = new System.IO.StreamReader(
                                reader, System.Text.Encoding.GetEncoding("UTF-8"));
                            // テキストを取り出す
                            string text = sr.ReadToEnd();

                            // 読み込んだ目次テキストファイルからstring配列を作成する
                            strs=text.Split('\n');
                    strs = strs[1].Substring(12).Replace("\r","").Replace("\n","").Split(',');
                            //閉じる
                            sr.Close();
                            reader.Close();

                    //（未）を外す
                    mapData[selectNum] = ",,,,,,,,,,,[system]PC版スタート地点.txt";
                }
                        else
                        {
                    strs = new string[2];
                            strs[0] = "35.010348"; strs[1] = "135.768738";
                    mapData[selectNum] = ",,,,,,,,,,,[system]PC版スタート地点.txt";
                }

                //閉じる
                zf.Close();

                inputField[12].text = strs[0];
                inputField[13].text = strs[1];
                latitude = Convert.ToDouble(strs[0]); longitude = Convert.ToDouble(strs[1]);
            }
        }
        catch
        {
        }
    }

    //インプットフィールドの入力を受け取る関数
    public void InputDecideButton()
    {
        try
        {
            if (selectNum > 0)
            {
                if (mapData.Count <= selectNum) { for (int i = mapData.Count; i <= selectNum; i++) { mapData.Add(""); } }//mapDataの要素数をselectNumが越えたら配列の要素数を合わせて増やす。中身は空でOK。（イベント追加されるとmapData.Count以上の番号を持つイベントができるため）
                mapData[selectNum] = inputField[1].text + "," + inputField[2].text + "," + inputField[3].text + "," + inputField[4].text + "," + inputField[5].text + "," + inputField[6].text + "," + inputField[7].text + "," + inputField[8].text + "," + inputField[9].text + "," + inputField[10].text + "," + inputField[11].text + "," + inputField[0].text + ".txt\n";
                objIB[selectNum].GetComponentInChildren<Text>().text = MapDataToButton(mapData[selectNum]);

                //ファイルチェックして（未）をつける
                //ZipFileオブジェクトの作成
                ICSharpCode.SharpZipLib.Zip.ZipFile zf =
                    new ICSharpCode.SharpZipLib.Zip.ZipFile(PlayerPrefs.GetString("進行中シナリオ", ""));
                zf.Password = Secret.SecretString.zipPass;
                ScenarioFileCheck(selectNum, zf);
                zf.Close();
                latitude = Convert.ToDouble(inputField[1].text); longitude = Convert.ToDouble(inputField[2].text);
            }
            else if (selectNum == 0)
            {
                //座標を突っ込むだけのイベントファイルを作成。内容は座標設定→マップワンス
                string str = "";//イベントファイルの１行目はファイル名入れない
                string str2= "[system]Command1PC版スタート地点.txt\r\n";//一行目はファイル名を示す部分。
                                                          //ZIP書庫のパス
                string zipPath = PlayerPrefs.GetString("進行中シナリオ", "");
                //書庫に追加するファイルのパス
                string file = @GetComponent<Utility>().GetAppPath() + @"\[system]PC版スタート地点.txt";
                string file2 = @GetComponent<Utility>().GetAppPath() + @"\[system]Command1PC版スタート地点.txt";

                //先にテキストファイルを一時的に書き出しておく。
                str = str + System.IO.Path.GetFileName(file2);
                str2 = str2 + "PlaceChange:" + inputField[12].text + "," + inputField[13].text + "\r\nBackText:シナリオ初期データ設定中,false\r\nMap:Once\r\n[END]";

                System.IO.File.WriteAllText(file, str);
                System.IO.File.WriteAllText(file2, str2);
                //ZipFileオブジェクトの作成
                ICSharpCode.SharpZipLib.Zip.ZipFile zf =
                    new ICSharpCode.SharpZipLib.Zip.ZipFile(zipPath);
                zf.Password = Secret.SecretString.zipPass;
                //ZipFileの更新を開始
                zf.BeginUpdate();

                //ZIP内のエントリの名前を決定する 
                string f = System.IO.Path.GetFileName(file);
                string f2= System.IO.Path.GetFileName(file2);
                //ZIP書庫に一時的に書きだしておいたファイルを追加する
                zf.Add(file, f);
                zf.Add(file2, f2);
                //ZipFileの更新をコミット
                zf.CommitUpdate();

                //閉じる
                zf.Close();

                //一時的に書きだしたファイルを消去する。
                try
                {
                    System.IO.File.Delete(file);
                    System.IO.File.Delete(file2);
                }
                catch { }
                //（未）を外す
                latitude = Convert.ToDouble(inputField[12].text); longitude = Convert.ToDouble(inputField[13].text);
                objIB[selectNum].GetComponentInChildren<Text>().text = "[system]PC版スタート地点　緯:" + latitude.ToString() + ",経:" + longitude.ToString();
            }
            else
            {
                AudioSource bgm = GameObject.Find("BGMManager").GetComponent<AudioSource>(); bgm.loop = false; bgm.clip = errorSE; bgm.Play();
            }
            try { GetMap(); } catch { }
        }
        catch { }
    }

    //mapDataをボタン表示用に成型する関数
    public string MapDataToButton(string mapdata)
    {
        string[] strs;
        try
        {
            strs = mapdata.Replace("\r", "").Replace("\n", "").Split(',');
            if (strs[11].Length>4) { strs[11] = strs[11].Substring(0,strs[11].Length-4); }
            if (strs[0].Length > 0) { strs[0] = " 緯:" + strs[0]; }
            if (strs[1].Length > 0) { strs[1] = " 経:" + strs[1]; }
            if (strs[2].Length > 0) { strs[2] = " " + strs[2] + "月"; }
            if (strs[3].Length > 0) { strs[3] = strs[3] + "日"; }
            if (strs[4].Length > 0) { strs[4] = strs[4] + "時"; }
            if (strs[5].Length > 0) { strs[5] = strs[5] + "分"; }
            if (strs[6].Length > 0) { strs[6] = "～" + strs[6] + "月"; }
            if (strs[7].Length > 0) { strs[7] = strs[7] + "日"; }
            if (strs[8].Length > 0) { strs[8] = strs[8] +"時"; }
            if (strs[9].Length > 0) { strs[9] = strs[9] + "分"; }
            if (strs[10].Length > 0) { strs[10] = " " + strs[10]; }
            return strs[11] + strs[0] + strs[1] + strs[2] + strs[3] + strs[4] + strs[5] + strs[6] + strs[7] + strs[8] + strs[9] + strs[10];
                }
        catch
        {
            return "";
        }
    }

    //[system]mapdata.txtファイルを書き出す関数
    public void MakeMapDataFile()
    {
        try
        {
            string str = "";
            //ZIP書庫のパス
            string zipPath = PlayerPrefs.GetString("進行中シナリオ", "");
            //書庫に追加するファイルのパス
            string file = @GetComponent<Utility>().GetAppPath() + @"\" + "[system]mapdata.txt";

            //先に[system]mapdata.txtを一時的に書き出しておく。
            for (int i = 0; i < mapData.Count; i++) { if (mapData[i].Replace("\n", "").Replace("\r", "") == "") { continue; } str = str + mapData[i].Replace("\n", "").Replace("\r", "") + "\r\n"; }
            str = str + "[END]";
            System.IO.File.WriteAllText(file, str);

            //ZipFileオブジェクトの作成
            ICSharpCode.SharpZipLib.Zip.ZipFile zf =
                new ICSharpCode.SharpZipLib.Zip.ZipFile(zipPath);
            zf.Password = Secret.SecretString.zipPass;
            //ZipFileの更新を開始
            zf.BeginUpdate();

            //ZIP内のエントリの名前を決定する 
            string f = System.IO.Path.GetFileName(file);
            //ZIP書庫に一時的に書きだしておいたファイルを追加する
            zf.Add(file, f);

            //ZipFileの更新をコミット
            zf.CommitUpdate();

            //閉じる
            zf.Close();

            //一時的に書きだした[system]mapdata.txtを消去する。
            System.IO.File.Delete(file);
        }
        catch { }
    }

    public void ScenarioFileCheck(int num, ICSharpCode.SharpZipLib.Zip.ZipFile zf)
    {
        string[] strs;
        strs = mapData[num].Replace("\r", "").Replace("\n", "").Split(',');//strs[11]がシナリオパス
        try
        {
            //展開するエントリを探す
            ICSharpCode.SharpZipLib.Zip.ZipEntry ze = zf.GetEntry(strs[11]);

            //参照先のファイルがあるか調べる。なかったら(未)をつける。
            if (ze == null)
            {
                objIB[num].GetComponentInChildren<Text>().text = "(未)" + objIB[num].GetComponentInChildren<Text>().text;
            }
        }
        catch { }
    }
}


