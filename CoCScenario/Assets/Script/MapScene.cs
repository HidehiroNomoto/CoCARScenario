using System;
using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;
using System.IO;

public class MapScene : MonoBehaviour
{
    Sprite mapImage;
    private float intervalTime = 0.0f;
    private int width = 2000;
    private int height = 2000;
    public double longitude;
    public double latitude;
    private double longitudeMap;
    private double latitudeMap;
    public int zoom = 16;
    private float targetX = 0;
    private float targetY = 0;
    public List<string> mapData = new List<string>();
    private bool sceneChange = false;
    GameObject mapImageObj;
    public GameObject objIvent;
    GameObject objBGM;
    public List<GameObject> objIB = new List<GameObject>();
    GameObject parentObject;
    public int selectNum = -1;
    string _FILE_HEADER;
    public InputField[] inputField = new InputField[14];
    public AudioClip errorSE;
    public GameObject FirstPlace;
    public GameObject IventMake;
    public int selectBefore = -1;
    public List<int> multiSelect = new List<int>();
    public bool URBool = false;
    public List<string> undoList = new List<string>();
    private int undoListNum = 0;
    private bool copyBool = false;
    public List<string> tmpList = new List<string>();//test
    private int time = 0;
    public int fallNum = 0;
    private int copynum = 0;
    public GameObject PLIventToggle;
    public GameObject objTarget;
    public string tests;

    void Start()
    {
        parentObject = GameObject.Find("Content");
        _FILE_HEADER = PlayerPrefs.GetString("進行中シナリオ", "");                      //ファイル場所の頭
        longitude = PlayerPrefs.GetFloat("longitude", 135.768738f); latitude = PlayerPrefs.GetFloat("latitude", 35.010348f);
        mapImageObj = GameObject.Find("mapImage").gameObject as GameObject;
        objBGM = GameObject.Find("BGMManager").gameObject as GameObject;
        LoadMapData("[system]mapdata[system].txt");
        try {
            GetStartPoint();
        } catch { }
        GetMap();
        //フォントの表示バグを修正するための処理（Unity固有のもの）
        Font.textureRebuilt += CallBackReMakeTextObject;
    }
    //フォントバグ対策のコールバック
    System.Action<Font> CallBackReMakeTextObject = (n) =>
    {
        Text[] objects;
        objects = FindObjectsOfType<Text>();
        for (int i = 0; i < objects.Length; i++) { objects[i].FontTextureChanged(); }
    };


    void Update()
    {
        bool textFlag = false;
        if (time % 36000 == 0) { System.IO.File.Copy(PlayerPrefs.GetString("進行中シナリオ", ""), "BackUp.zip", true); }
        time++;
        for (int x = 0; x < inputField.Length; x++) { if (inputField[x].isFocused) { textFlag = true; } }
        if (textFlag == false && (Input.GetKey(KeyCode.LeftControl) || Input.GetKey(KeyCode.RightControl)))
        {
            if (Input.GetKey(KeyCode.Z))
            {
                if (URBool == false) { UndoRedoButton(true); URBool = true; }
            }
            else if (Input.GetKey(KeyCode.Y))
            {
                if (URBool == false) { UndoRedoButton(false); URBool = true; }
            }
            else
            {
                URBool = false;
            }
        }
        else
        {
            URBool = false;
        }
        if (textFlag == false && (Input.GetKey(KeyCode.LeftControl) || Input.GetKey(KeyCode.RightControl)))
        {
            if (Input.GetKey(KeyCode.C))
            {
                if (copyBool == false) { CopyButton(); copyBool = true; }
            }
            else if (Input.GetKey(KeyCode.V))
            {
                if (copyBool == false) { PasteButton(); copyBool = true; }
            }
            else
            {
                copyBool = false;
            }
        }
        else
        {
            copyBool = false;
        }
    }

    public void GetStartPoint()
    {
        string[] strs;
        //閲覧するエントリ
        string extractFile = "[system]command1[system]PC版スタート地点[system].txt";

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
            strs = text.Split('\n');
            strs = strs[1].Substring(12).Replace("\r", "").Replace("\n", "").Split(',');
            //閉じる
            sr.Close();
            reader.Close();
        }
        else
        {
            strs = new string[2];
            strs[0] = "35.010348"; strs[1] = "135.768738";
        }
        //閉じる
        zf.Close();
        latitude = Convert.ToDouble(strs[0]); longitude = Convert.ToDouble(strs[1]);
    }

    public void UndoRedoButton(bool undoFlag)
    {
        string str;
        try
        {
            if (undoFlag == true)
            {
                str = undoList[undoListNum - 1];
                undoListNum--;
            }
            else
            {
                str = undoList[undoListNum + 1];
                undoListNum++;
            }
            mapData.Clear();
            for (int i = 0; i < objIB.Count; i++) { Destroy(objIB[i]); }
            objIB.Clear();

            // 読み込んだ目次テキストファイルからstring配列を作成する
            mapData.AddRange(str.Split('\n'));
            mapData.RemoveAt(mapData.Count - 1);//最後の行は空白なので消す
                                                //ZipFileオブジェクトの作成
            ICSharpCode.SharpZipLib.Zip.ZipFile zf =
                new ICSharpCode.SharpZipLib.Zip.ZipFile(PlayerPrefs.GetString("進行中シナリオ", ""));
            zf.Password = Secret.SecretString.zipPass;
            for (int i = 0; i < mapData.Count; i++)
            {
                objIB.Add(Instantiate(objIvent) as GameObject);
                objIB[i].transform.SetParent(parentObject.transform, false);
                objIB[i].GetComponentInChildren<Text>().text = MapDataToButton(mapData[i]);
                objIB[i].GetComponent<IventButton>().buttonNum = i;

                ScenarioFileCheck(i, zf);

            }
            zf.Close();
        }
        catch
        {
            if (undoFlag == true) { GameObject.Find("Error").GetComponent<Text>().text = "これ以上戻れません。"; }
            if (undoFlag == false) { GameObject.Find("Error").GetComponent<Text>().text = "これ以上進めません。"; }
            StartCoroutine(ErrorWait());
        }
        selectNum = -1;
    }

    public void GetMap()
    {
        //マップを取得
        StartCoroutine(GetStreetViewImage(latitude, longitude, zoom));
    }


    private IEnumerator GetStreetViewImage(double latitude, double longitude, double zoom)
    {
        string url = "";
 url = "https://map.yahooapis.jp/map/V1/static?" + Secret.SecretString.yahooKey + "&lat=" + latitude.ToString() + "&lon=" + longitude.ToString() + "&z=" + ((int)zoom+1).ToString() + "&width=" + width.ToString() + "&height=" + height.ToString(); 
        WWW www = new WWW(url);
        yield return www;
        //マップの画像をTextureからspriteに変換して貼り付ける
        mapImage = Sprite.Create(www.texture, new Rect(0, 0, 2000, 2000), Vector2.zero);
        mapImageObj.GetComponent<Image>().sprite = mapImage;

        //地図の中心の緯度経度を保存
        longitudeMap = longitude;
        latitudeMap = latitude;
        mapImageObj.GetComponent<RectTransform>().localPosition = new Vector3(-207, 110, 10f);
        //targetの位置を中心に
        targetX = 0; targetY = 0;
    }

    //目次ファイルを読み込む。
    private void LoadMapData(string path)
    {
        string str2;
        //string[] strs;
        try
        {
            //閲覧するエントリ
            string extractFile = path;

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
                        ScenarioFileCheck(i, zf);
                    }
                }
                else
                {
                    GetComponent<Utility>().StartCoroutine("LoadSceneCoroutine", "TitleScene");
                }
            }
            catch { }

            ze = zf.GetEntry("[system]command1[system]PC版スタート地点[system].txt");
            /*
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
                objIB[0].GetComponentInChildren<Text>().text = "PC版スタート地点　緯:" + latitude.ToString() + ",経:" + longitude.ToString();
            }
            }
            catch { }
            */

            //閉じる
            zf.Close();

            str2 = "";
            for (int i = 0; i < mapData.Count; i++) { if (mapData[i].Replace("\n", "").Replace("\r", "") == "") { continue; } str2 = str2 + mapData[i].Replace("\n", "").Replace("\r", "") + "\r\n"; }
            undoList.Add(str2);
            undoListNum = undoList.Count - 1;
        }
        catch
        {
            GetComponent<Utility>().StartCoroutine("LoadSceneCoroutine", "TitleScene");
        }
    }

    public void IventAddButton()
    {
        string str2;
        if (selectNum >= 0)//初期位置設定の前にイベントボタンを追加させない。selectNum==-1で追加すると追加できてしまうのでそのケースを排除。
        {
            //追加ボタンが押されたらイベントボタンを追加する。
            objIB.Insert(selectNum + 1, Instantiate(objIvent) as GameObject);
            objIB[selectNum + 1].transform.SetParent(parentObject.transform, false);
            objIB[selectNum + 1].GetComponent<IventButton>().buttonNum = selectNum + 1;
            objIB[selectNum + 1].GetComponent<Transform>().SetSiblingIndex(selectNum + 1);
            for (int i = selectNum + 2; i < objIB.Count; i++) { objIB[i].GetComponent<IventButton>().buttonNum++; }//追加分の後ろはボタン番号が１増える。
            mapData.Insert(selectNum + 1, ",,,,,,,,,,,[system]空イベント.txt");

            str2 = "";
            for (int i = 0; i < mapData.Count; i++) { if (mapData[i].Replace("\n", "").Replace("\r", "") == "") { continue; } str2 = str2 + mapData[i].Replace("\n", "").Replace("\r", "") + "\r\n"; }
            undoList.Add(str2);
            undoListNum = undoList.Count - 1;
        }
        else
        {
            GameObject.Find("Error").GetComponent<Text>().text = "追加先が選択されていません（選択されたイベントの後ろに追加します）。";
            StartCoroutine(ErrorWait());
            return;
        }
    }

    public void IventDeleteButton()
    {
        string str2;
        List<int> tmp = new List<int>();
        if (selectNum > 0)
        {
            Destroy(objIB[selectNum]);
            objIB.RemoveAt(selectNum);
            for (int i = selectNum; i < objIB.Count; i++) { objIB[i].GetComponent<IventButton>().buttonNum--; }//削除分の後ろはボタン番号が１減る。
            if (mapData.Count - 1 >= selectNum) { mapData.RemoveAt(selectNum); }
            for (int k = 0; k < multiSelect.Count; k++) { if (selectNum < multiSelect[k]) { multiSelect[k]--; } }
            for (int j = 0; j < multiSelect.Count; j++)
            {
                Destroy(objIB[multiSelect[j]]);
                objIB.RemoveAt(multiSelect[j]);
                for (int i = multiSelect[j]; i < objIB.Count; i++) { objIB[i].GetComponent<IventButton>().buttonNum--; }//削除分の後ろはボタン番号が１減る。
                if (mapData.Count - 1 >= multiSelect[j]) { mapData.RemoveAt(multiSelect[j]); }
                for (int k = 0; k < multiSelect.Count; k++) { if (multiSelect[j] < multiSelect[k]) { multiSelect[k]--; } }
            }
            selectNum = -1;

            str2 = "";
            for (int i = 0; i < mapData.Count; i++) { if (mapData[i].Replace("\n", "").Replace("\r", "") == "") { continue; } str2 = str2 + mapData[i].Replace("\n", "").Replace("\r", "") + "\r\n"; }
            undoList.Add(str2);
            undoListNum = undoList.Count - 1;

        }
        else
        {
            if (selectNum == 0) { GameObject.Find("Error").GetComponent<Text>().text = "スタート地点設定イベントは消去できません。"; }
            if (selectNum < 0) { GameObject.Find("Error").GetComponent<Text>().text = "イベントが選択されていません。"; }
            AudioSource bgm = GameObject.Find("BGMManager").GetComponent<AudioSource>(); bgm.loop = false; bgm.clip = errorSE; bgm.Play();
            StartCoroutine(ErrorWait());
        }
    }

    private IEnumerator ErrorWait()
    {
        for (int i = 0; i < 200; i++) { yield return null; }
        GameObject.Find("Error").GetComponent<Text>().text = "";
    }


    public void IventCreateButton()
    {
        string[] strs;
        if (selectNum > 0)
        {
            try
            {
                InputDecideButton();
                if (GameObject.Find("Error").GetComponent<Text>().text == "「<color=red>[system]</color>」という文字列は使用禁止です。(システム処理の識別語にしています)") { return; }
                MakeMapDataFile();
                strs = mapData[selectNum].Replace("\r", "").Replace("\n", "").Split(',');
                objBGM.GetComponent<BGMManager>().chapterName = strs[11];
                GetComponent<Utility>().StartCoroutine("LoadSceneCoroutine", "NovelScene");
            }
            catch
            {
            }
        }
        else
        {
            if (selectNum == 0) { GameObject.Find("Error").GetComponent<Text>().text = "スタート地点設定イベントはこの画面で作成します。"; }
            if (selectNum < 0) { GameObject.Find("Error").GetComponent<Text>().text = "イベントが選択されていません。"; }
            AudioSource bgm = GameObject.Find("BGMManager").GetComponent<AudioSource>(); bgm.loop = false; bgm.clip = errorSE; bgm.Play();
            StartCoroutine(ErrorWait());
        }
    }

    public void SetIvent()
    {
        string[] strs;
        string passtmp="";
        try
        {
            if (selectNum > 0)
            {
                FirstPlace.SetActive(false);
                IventMake.SetActive(true);
                strs = mapData[selectNum].Replace("\r", "").Replace("\n", "").Split(',');
                if (strs[10].Contains(" [system]任意イベント")) { PLIventToggle.GetComponent<Toggle>().isOn = true; strs[10] = strs[10].Replace(" [system]任意イベント", ""); } else { PLIventToggle.GetComponent<Toggle>().isOn = false; }
                inputField[0].text = strs[11].Substring(0, strs[11].Length - 4).Replace("[system]", "");
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
                IventMake.SetActive(false);
                //閲覧するエントリ
                string extractFile = "[system]command1[system]PC版スタート地点[system].txt";

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
                    strs = text.Split('\n');
                    strs = strs[1].Substring(12).Replace("\r", "").Replace("\n", "").Split(',');
                    //閉じる
                    sr.Close();
                    reader.Close();
                    mapData[selectNum] = ",,,,,,,,,,,[system]PC版スタート地点[system].txt";
                }
                else
                {
                    strs = new string[2];
                    strs[0] = "35.010348"; strs[1] = "135.768738";
                    mapData[selectNum] = ",,,,,,,,,,,[system]PC版スタート地点[system].txt";
                }

                ICSharpCode.SharpZipLib.Zip.ZipEntry ze2 = zf.GetEntry("[system]password[system].txt");
                if (ze2 != null)
                {
                    //閲覧するZIPエントリのStreamを取得
                    System.IO.Stream reader = zf.GetInputStream(ze2);
                    //文字コードを指定してStreamReaderを作成
                    System.IO.StreamReader sr = new System.IO.StreamReader(
                        reader, System.Text.Encoding.GetEncoding("UTF-8"));
                    // テキストを取り出す
                    passtmp = sr.ReadToEnd();
                    
                    //閉じる
                    sr.Close();
                    reader.Close();
                }

                //閉じる
                zf.Close();
                GameObject.Find("ScenarioNameInput").GetComponent<InputField>().text = System.IO.Path.GetFileNameWithoutExtension(PlayerPrefs.GetString("進行中シナリオ", ""));
                GameObject.Find("PassWordInput").GetComponent<InputField>().text = passtmp;
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
        string str3;
        string PLIvent = "";
        string[] tmp;
        string tmpStr;
        bool doubleconnectflag=false;
        try
        {
            if (inputField[0].text.Contains("[system]")) { GameObject.Find("Error").GetComponent<Text>().text = "「<color=red>[system]</color>」という文字列は使用禁止です。(システム処理の識別語にしています)"; StartCoroutine(ErrorWait()); return; }
            if (selectNum > 0)
            {
                if (PLIventToggle.GetComponent<Toggle>().isOn) { PLIvent = " [system]任意イベント"; }
                if (mapData.Count <= selectNum) { for (int i = mapData.Count; i <= selectNum; i++) { mapData.Add(",,,,,,,,,,,[system]空イベント.txt"); } }//mapDataの要素数をselectNumが越えたら配列の要素数を合わせて増やす。中身は空でOK。（イベント追加されるとmapData.Count以上の番号を持つイベントができるため）
                tmp = mapData[selectNum].Split(',');
                mapData[selectNum] = inputField[1].text + "," + inputField[2].text + "," + inputField[3].text + "," + inputField[4].text + "," + inputField[5].text + "," + inputField[6].text + "," + inputField[7].text + "," + inputField[8].text + "," + inputField[9].text + "," + inputField[10].text + "," + inputField[11].text + PLIvent + ",[system]" + inputField[0].text + ".txt\n";
                objIB[selectNum].GetComponentInChildren<Text>().text = MapDataToButton(mapData[selectNum]);

                //コピー元と同一イベントを参照しているものがあるか判定
                tmpStr=tmp[tmp.Length - 1].Replace("\n", "").Replace("\r", "");
                for (int i = 0; i < mapData.Count; i++) { tmp = mapData[i].Split(',');if (tmp[tmp.Length - 1].Replace("\n", "").Replace("\r", "")==tmpStr) { doubleconnectflag = true; } }
                //コピー先と同一イベント名がない場合（NCBIE内で判定）は関連コマンドファイルのイベント名変更
                StartCoroutine(NCBIE(inputField[0].text, tmpStr,doubleconnectflag));

                //ファイルチェックして（未）をつける
                //ZipFileオブジェクトの作成
                ICSharpCode.SharpZipLib.Zip.ZipFile zf =
                    new ICSharpCode.SharpZipLib.Zip.ZipFile(PlayerPrefs.GetString("進行中シナリオ", ""));
                zf.Password = Secret.SecretString.zipPass;
                ScenarioFileCheck(selectNum, zf);
                zf.Close();

                if (inputField[1].text != "" && inputField[2].text != "")
                {
                    latitude = Convert.ToDouble(inputField[1].text); longitude = Convert.ToDouble(inputField[2].text);
                }
                str3 = "";
                for (int i = 0; i < mapData.Count; i++) { if (mapData[i].Replace("\n", "").Replace("\r", "") == "") { continue; } str3 = str3 + mapData[i].Replace("\n", "").Replace("\r", "") + "\r\n"; }
                undoList.Add(str3);
                undoListNum = undoList.Count - 1;
            }
            else if (selectNum == 0)
            {
                //座標を突っ込むだけのイベントファイルを作成。内容は座標設定→マップワンス
                string str = "";//イベントファイルの１行目はファイル名入れない
                string str2 = "[system]command1[system]PC版スタート地点[system].txt\r\n";//一行目はファイル名を示す部分。
                                                                                  //ZIP書庫のパス
                string zipPath = PlayerPrefs.GetString("進行中シナリオ", "");
                //書庫に追加するファイルのパス
                string file = @GetComponent<Utility>().GetAppPath() + objBGM.GetComponent<BGMManager>().folderChar + @"[system]PC版スタート地点[system].txt";
                string file2 = @GetComponent<Utility>().GetAppPath() + objBGM.GetComponent<BGMManager>().folderChar + @"[system]command1[system]PC版スタート地点[system].txt";

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
                string f2 = System.IO.Path.GetFileName(file2);
                //ZIP書庫に一時的に書きだしておいたファイルを追加する
                zf.Add(file, f);
                zf.Add(file2, f2);
                //ZipFileの更新をコミット
                zf.CommitUpdate();

                objIB[selectNum].GetComponentInChildren<Text>().text = MapDataToButton(mapData[selectNum]);
                ScenarioFileCheck(selectNum, zf);


                //閉じる
                zf.Close();

                //一時的に書きだしたファイルを消去する。
                try
                {
                    System.IO.File.Delete(file);
                    System.IO.File.Delete(file2);
                }
                catch { }
                try
                {
                    string zipPath2 = System.IO.Path.GetDirectoryName(zipPath) + objBGM.GetComponent<BGMManager>().folderChar + GameObject.Find("ScenarioNameInput").GetComponent<InputField>().text + ".zip";
                    System.IO.File.Move(zipPath,zipPath2);
                    PlayerPrefs.SetString("進行中シナリオ", zipPath2);
                }
                catch
                {
                    GameObject.Find("Error").GetComponent<Text>().text = "同名シナリオがフォルダ内に既にある、もしくは元ファイルが見当たりません";
                    AudioSource bgm = GameObject.Find("BGMManager").GetComponent<AudioSource>(); bgm.loop = false; bgm.clip = errorSE; bgm.Play();
                    StartCoroutine(ErrorWait());
                }
                latitude = Convert.ToDouble(inputField[12].text); longitude = Convert.ToDouble(inputField[13].text);
                MakePasswordFile();
                str3 = "";
                for (int i = 0; i < mapData.Count; i++) { if (mapData[i].Replace("\n", "").Replace("\r", "") == "") { continue; } str3 = str3 + mapData[i].Replace("\n", "").Replace("\r", "") + "\r\n"; }
                undoList.Add(str3);
                undoListNum = undoList.Count - 1;
            }
            else
            {
                GameObject.Find("Error").GetComponent<Text>().text = "イベントが選択されていません。";
                AudioSource bgm = GameObject.Find("BGMManager").GetComponent<AudioSource>(); bgm.loop = false; bgm.clip = errorSE; bgm.Play();
                StartCoroutine(ErrorWait());
            }
            try { GetMap(); } catch { }
        }
        catch { }
    }

    private IEnumerator NCBIE(string str1,string str2,bool flag)
    {
        int inum = 0;
        string tmp1,tmp2;
        List<string> tmpList = new List<string>();
        tmp1 = "[system]" + str1 + ".txt";
        tmp2 = str2;
        //全てのコマンドファイル、イベントファイル、マップデータを開き、コマンド名（[system]～～××.txt）をtmp1に、イベント名（××.txt）をtmp2に変換する。

        //ZipFileオブジェクトの作成
        ICSharpCode.SharpZipLib.Zip.ZipFile zf =
            new ICSharpCode.SharpZipLib.Zip.ZipFile(PlayerPrefs.GetString("進行中シナリオ", ""));
        zf.Password = Secret.SecretString.zipPass;
        foreach (ICSharpCode.SharpZipLib.Zip.ZipEntry ze in zf)
        {

            if ((ze.Name != tmp2 && ze.Name == tmp1))
            {
                //他のイベントと名前がかぶっていれば、名前変更ではなくイベント紐付けが目的だと判断。コピーはしない。
                zf.Close();
                yield break;
            }
        }
        //ZipFileの更新を開始
        zf.BeginUpdate();

        //展開するエントリを探す
        foreach (ICSharpCode.SharpZipLib.Zip.ZipEntry ze in zf)
        {
            if (ze.Name.Substring(ze.Name.Length - 4) == ".txt" && !tmpList.Contains(ze.Name) && ze.Name!= "[system]mapdata[system].txt")
            {
                //閲覧するZIPエントリのStreamを取得
                Stream reader = zf.GetInputStream(ze);
                //文字コードを指定してStreamReaderを作成
                StreamReader sr = new StreamReader(
                    reader, System.Text.Encoding.GetEncoding("UTF-8"));
                // テキストを取り出す
                string text = sr.ReadToEnd();
                sr.Close();
                reader.Close();
                string text2 = text;
                // 読み込んだ目次テキストファイルからstring配列を作成する
                text = text.Replace(tmp2, tmp1);
                if (text2 == text) { continue; }
                StreamWriter sw = new StreamWriter(@GetComponent<Utility>().GetAppPath() + objBGM.GetComponent<BGMManager>().folderChar + "tmp" + inum.ToString() + ".txt", false, System.Text.Encoding.GetEncoding("UTF-8"));
                //TextBox1.Textの内容を書き込む
                sw.Write(text);
                //閉じる
                sw.Close();

                //ファイル名自体も置換
                string tmpName;
                tmpName = ze.Name;
                tmpName = tmpName.Replace(tmp2, tmp1);
                if (flag == false) { zf.Delete(ze.Name); }//他から関連付けされていないなら、旧コマンドファイルはもう使わないので削除。
                zf.Add("tmp" + inum.ToString() + ".txt", tmpName);
                inum++;
                tmpList.Add(tmpName);
            }
        }
        //ZipFileの更新をコミット
        zf.CommitUpdate();

        //閉じる
        zf.Close();
        for (int i = 0; i < inum; i++) { File.Delete(@GetComponent<Utility>().GetAppPath() + objBGM.GetComponent<BGMManager>().folderChar + "tmp" + i.ToString() + ".txt"); }
        for (int i = 0; i < undoList.Count; i++) { undoList[i] = undoList[i].Replace(tmp2, tmp1); }
    }


    //mapDataをボタン表示用に成型する関数
    public string MapDataToButton(string mapdata)
    {
        string[] strs;
        try
        {
            strs = mapdata.Replace("\r", "").Replace("\n", "").Replace("[system]", "").Split(',');
            if (strs[11].Length > 4) { strs[11] = strs[11].Substring(0, strs[11].Length - 4); }
            if (strs[0].Length > 0) { strs[0] = " 緯:" + strs[0]; }
            if (strs[1].Length > 0) { strs[1] = " 経:" + strs[1]; }
            if (strs[2].Length > 0) { strs[2] = " " + strs[2] + "月"; }
            if (strs[3].Length > 0) { strs[3] = strs[3] + "日"; }
            if (strs[4].Length > 0) { strs[4] = strs[4] + "時"; }
            if (strs[5].Length > 0) { strs[5] = strs[5] + "分"; }
            if (strs[6].Length > 0) { strs[6] = "～" + strs[6] + "月"; }
            if (strs[7].Length > 0) { strs[7] = strs[7] + "日"; }
            if (strs[8].Length > 0) { strs[8] = strs[8] + "時"; }
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
        //List<string> tmpList = new List<string>();
        List<string> notUseList = new List<string>();
        try
        {
            string str = "";
            //ZIP書庫のパス
            string zipPath = PlayerPrefs.GetString("進行中シナリオ", "");
            //書庫に追加するファイルのパス
            string file = @GetComponent<Utility>().GetAppPath() + objBGM.GetComponent<BGMManager>().folderChar + "[system]mapdata[system].txt";

            //先に[system]mapdata.txtを一時的に書き出しておく。
            for (int i = 0; i < mapData.Count; i++) { if (mapData[i].Replace("\n", "").Replace("\r", "") == "") { continue; } str = str + mapData[i].Replace("\n", "").Replace("\r", "") + "\r\n"; }
            str = str + "[END]";
            System.IO.File.WriteAllText(file, str);

            //ZipFileオブジェクトの作成
            ICSharpCode.SharpZipLib.Zip.ZipFile zf =
                new ICSharpCode.SharpZipLib.Zip.ZipFile(zipPath);
            zf.Password = Secret.SecretString.zipPass;

            //先にzip上のmapdataを更新しておく（以降でzip上のmapdataを基に、使っているファイルを判別して不使用ファイルを消す操作をするから）
            //ZipFileの更新を開始
            zf.BeginUpdate();
            //ZIP書庫に一時的に書きだしておいたファイルを追加する
            zf.Add(file, System.IO.Path.GetFileName(file));
            //ZipFileの更新をコミット
            zf.CommitUpdate();


            //以下、不使用データの削除処理
            ICSharpCode.SharpZipLib.Zip.ZipEntry ze = zf.GetEntry("[system]mapdata[system].txt");
            tmpList.Clear();
            FileSearchLoop(ze, zf);

            tmpList.Add("[system]mapdata[system].txt"); tmpList.Add("[system]password[system].txt"); tmpList.Add("[system]commandFileNum[system].txt");
            tmpList.Add("[system]command1[system]PC版スタート地点[system].txt"); tmpList.Add("[system]PC版スタート地点[system].txt");

            foreach (ICSharpCode.SharpZipLib.Zip.ZipEntry ze3 in zf)
            {
                bool useFlag = false;
                foreach (string tmpStr in tmpList) { if (tmpStr == ze3.Name) { useFlag = true; } }
                if (useFlag == false) { string tmpStr = ze3.Name; notUseList.Add(tmpStr); }
            }

            //ZipFileの更新を開始
            zf.BeginUpdate();

            foreach (string tmpStr in notUseList) { zf.Delete(tmpStr); }//notUseListのファイルを消す。

            //ZipFileの更新をコミット
            zf.CommitUpdate();

            //閉じる
            zf.Close();

            //一時的に書きだした[system]mapdata.txtを消去する。
            System.IO.File.Delete(file);
        }
        catch { }
    }

    private void FileSearchLoop(ICSharpCode.SharpZipLib.Zip.ZipEntry ze, ICSharpCode.SharpZipLib.Zip.ZipFile zf)
    {
        bool next;
        if (ze.Name.Length > 4)
        {
            if (ze.Name.Substring(ze.Name.Length - 4) == ".txt")
            {
                //閲覧するZIPエントリのStreamを取得
                System.IO.Stream reader = zf.GetInputStream(ze);
                //文字コードを指定してStreamReaderを作成
                System.IO.StreamReader sr = new System.IO.StreamReader(
                    reader, System.Text.Encoding.GetEncoding("UTF-8"));
                // テキストを取り出す
                string text = sr.ReadToEnd();
                foreach (ICSharpCode.SharpZipLib.Zip.ZipEntry ze2 in zf)
                {
                    if (text.Contains(ze2.Name))
                    {
                        string tmpStr = ze2.Name;
                        next = true;
                        foreach (string str in tmpList)
                        {
                            if (str == ze2.Name) { next = false; }
                        }
                        tmpList.Add(tmpStr);
                        if (next == true) { FileSearchLoop(ze2, zf); }
                    }
                }//再帰でzeから繋がるツリーを総探査する。
            }
        }
    }

    public void CopyButton()
    {
        string str = "";
        string[] strs = new string[12];
        if (selectNum < 0)
        {
            GameObject.Find("Error").GetComponent<Text>().text = "コマンドを選択してください。";
            StartCoroutine(ErrorWait());
            return;
        }
        if (selectNum == 0)
        {
            GameObject.Find("Error").GetComponent<Text>().text = "スタート地点設定イベントはコピーできません。";
            StartCoroutine(ErrorWait());
            return;
        }
        foreach (int i in multiSelect)
        {
            if (i == 0)
            {
                GameObject.Find("Error").GetComponent<Text>().text = "スタート地点設定イベントはコピーできません。";
                StartCoroutine(ErrorWait());
                return;
            }
        }
        if (multiSelect.Count == 0)
        {
            str = mapData[selectNum].Replace("\r", "").Replace("\n", "") + "\r\n";
        }
        else if (multiSelect[0] > selectNum)
        {
            str = str + mapData[selectNum].Replace("\r", "").Replace("\n", "") + "\r\n";
            for (int i = multiSelect.Count - 1; i >= 0; i--) { str = str + mapData[multiSelect[i]].Replace("\r", "").Replace("\n", "") + "\r\n"; }
        }
        else if (multiSelect[0] < selectNum)
        {
            for (int i = 0; i < multiSelect.Count; i++) { str = str + mapData[multiSelect[i]].Replace("\r", "").Replace("\n", "") + "\r\n"; }
            str = str + mapData[selectNum].Replace("\r", "").Replace("\n", "") + "\r\n";
        }
        //strの最後の\r\nはいらない
        str = str.Substring(0, str.Length - 2);
        objBGM.GetComponent<BGMManager>().copyMapString = str;
    }

    private List<string> CopyMake(string[] strs2, List<string> strList)
    {
        string str;
        string[] strs;
        List<string> strs3 = new List<string>();
        for (int j = 0; j < strs2.Length; j++)
        {
            str = strs2[j];
            strs = str.Replace("\r", "").Replace("\n", "").Split(',');

            //ファイル名をチェックして同名にならないcopy番号を取得(pastebuttonとcopynumで処理を統一しないのは、複数個コピーの際にここだけ別ループにありcopynumを正しく反映できないから)
            for (int k = 0; k < 9999; k++) { for (int x = 0; x < strList.Count; x++) { if (strList[x].Contains(strs[11].Substring(0, strs[11].Length - 4) + "copy" + k.ToString() + ".txt")) { break; } if (x == strList.Count - 1) { strs[11] = strs[11].Substring(0, strs[11].Length - 4) + "copy" + k.ToString() + ".txt"; goto e3; } } }
            e3:
            for (int i = 0; i < 11; i++) { strs[i] = strs[i] + ","; }
            strs3.Add(strs[0] + strs[1] + strs[2] + strs[3] + strs[4] + strs[5] + strs[6] + strs[7] + strs[8] + strs[9] + strs[10] + strs[11]);
        }
        return strs3;
    }

    public void PasteButton()
    {
        string str = "";
        string[] strs;
        string copyfile = "";
        string file = @GetComponent<Utility>().GetAppPath() + objBGM.GetComponent<BGMManager>().folderChar + "tmp.txt";
        if (objBGM.GetComponent<BGMManager>().copyMapString == "")
        {
            GameObject.Find("Error").GetComponent<Text>().text = "先にコピー元を選んでください。";
            StartCoroutine(ErrorWait());
            return;
        }
        if (selectNum < 0)
        {
            GameObject.Find("Error").GetComponent<Text>().text = "貼り付け先（そのイベントの後ろに挿入されます）が選択されていません。";
            StartCoroutine(ErrorWait());
            return;
        }
        List<string> strList = new List<string>();
        strList.AddRange(undoList[undoListNum].Replace("\r", "").Split('\n'));

        //コピーするイベント名の取得
        strs = objBGM.GetComponent<BGMManager>().copyMapString.Replace("\r", "").Split('\n');
        //ZipFileオブジェクトの作成
        ICSharpCode.SharpZipLib.Zip.ZipFile zf =
            new ICSharpCode.SharpZipLib.Zip.ZipFile(PlayerPrefs.GetString("進行中シナリオ", ""));
        zf.Password = Secret.SecretString.zipPass;

        for (int j = 0; j < strs.Length; j++)
        {
            string[] strs2;
            strs2 = strs[j].Split(',');
            copyfile = strs2[11].Replace("\r", "").Replace("\n", "");
            try
            {
                ICSharpCode.SharpZipLib.Zip.ZipEntry ze = zf.GetEntry(copyfile);
                tmpList.Clear();

                //コピーファイルのリスト化
                FileSearchLoop(ze, zf);
                //ファイルのコピー
                for (int i = 0; i < tmpList.Count; i++)
                {
                    ICSharpCode.SharpZipLib.Zip.ZipEntry ze2 = zf.GetEntry(tmpList[i]);
                    if (ze2.Name.Length > 4 && ze2.Name.Substring(ze2.Name.Length - 4) == ".txt")
                    {
                        //閲覧するZIPエントリのStreamを取得
                        System.IO.Stream reader = zf.GetInputStream(ze2);
                        //文字コードを指定してStreamReaderを作成
                        System.IO.StreamReader sr = new System.IO.StreamReader(
                            reader, System.Text.Encoding.GetEncoding("UTF-8"));
                        // テキストを取り出す
                        string text = sr.ReadToEnd();
                        for (int k = 0; k < 9999; k++) { for (int x = 0; x < strList.Count; x++) { if (strList[x].Contains(copyfile.Substring(0, copyfile.Length - 4) + "copy" + k.ToString() + ".txt")) { break; } if (x == strList.Count - 1) { copynum = k; goto e1; } } }
                        e1:
                        text = text.Replace(copyfile, copyfile.Substring(0, copyfile.Length - 4) + "copy" + copynum.ToString() + ".txt");
                        System.IO.File.WriteAllText(file, text);
                        //ZIP内のエントリの名前を決定する 
                        string f = "dammyfile.txt";
                        f = System.IO.Path.GetFileName(tmpList[i].Replace(copyfile, copyfile.Substring(0, copyfile.Length - 4) + "copy" + copynum.ToString() + ".txt"));
                        //ZipFileの更新を開始
                        zf.BeginUpdate();
                        //ZIP書庫に一時的に書きだしておいたファイルを追加する
                        zf.Add(file, f);
                        //ZipFileの更新をコミット
                        zf.CommitUpdate();

                    }
                }
            }
            catch
            {
            }
        }
        //一時的に書きだしたtmp.txtを消去する。
        System.IO.File.Delete(file);

        strList.InsertRange(selectNum + 1, CopyMake(objBGM.GetComponent<BGMManager>().copyMapString.Replace("\r", "").Split('\n'), strList));
        mapData.Clear();
        for (int i = 0; i < objIB.Count; i++) { Destroy(objIB[i]); }
        objIB.Clear();
        // 読み込んだ目次テキストファイルからstring配列を作成する
        mapData.AddRange(strList);
        mapData.RemoveAt(mapData.Count - 1);//最後の行は空白なので消す
                                            //コマンドをボタンとして一覧に放り込む。

        for (int i = 0; i < mapData.Count; i++)
        {
            objIB.Add(Instantiate(objIvent) as GameObject);
            objIB[i].transform.SetParent(parentObject.transform, false);
            objIB[i].GetComponentInChildren<Text>().text = MapDataToButton(mapData[i]);
            objIB[i].GetComponent<IventButton>().buttonNum = i;

            ScenarioFileCheck(i, zf);
        }
        zf.Close();
        for (int i = 0; i < mapData.Count; i++) { str = str + mapData[i].Replace("\r", "").Replace("\n", "") + "\r\n"; }
        undoList.Add(str);
        undoListNum = undoList.Count - 1;
        selectNum = -1;
        multiSelect.Clear();
        MakeMapDataFile();
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

    public void ZoomUpButton()
    {
        if (zoom < 18)
        {
            zoom++;
            objTarget.GetComponent<RectTransform>().sizeDelta = new Vector2((float)(30*Math.Pow(2, zoom-16)),(float)( 30 * Math.Pow(2, zoom-16)));            
            GetMap();
        }
    }

    public void ZoomDownButton()
    {
        if (zoom > 3)
        {
            zoom--;
            objTarget.GetComponent<RectTransform>().sizeDelta = new Vector2((float)(30 * Math.Pow(2, zoom-16)), (float)(30 * Math.Pow(2, zoom-16)));
            GetMap();
        }
    }

    public void MapPageJumpButton()
    {
        Application.OpenURL("http://user.numazu-ct.ac.jp/~tsato/webmap/sphere/coordinates/yahoo_olp/?lat=" + latitude.ToString() + "&lon=" + longitude.ToString() + "&z=18");
    }

    public void MakePasswordFile()
    {
        try
        {
            //ZIP書庫のパス
            string zipPath = PlayerPrefs.GetString("進行中シナリオ", "");
            //書庫に追加するファイルのパス
            string file = @GetComponent<Utility>().GetAppPath() + objBGM.GetComponent<BGMManager>().folderChar + "[system]password[system].txt";

            //先に[system]password.txtを一時的に書き出しておく。
            System.IO.File.WriteAllText(file, GameObject.Find("PassWordInput").GetComponent<InputField>().text);

            //ZipFileオブジェクトの作成
            ICSharpCode.SharpZipLib.Zip.ZipFile zf =
                new ICSharpCode.SharpZipLib.Zip.ZipFile(zipPath);
            zf.Password = Secret.SecretString.zipPass;

            //先にzip上のmapdataを更新しておく（以降でzip上のmapdataを基に、使っているファイルを判別して不使用ファイルを消す操作をするから）
            //ZipFileの更新を開始
            zf.BeginUpdate();
            //ZIP書庫に一時的に書きだしておいたファイルを追加する
            zf.Add(file, System.IO.Path.GetFileName(file));
            //ZipFileの更新をコミット
            zf.CommitUpdate();

            //閉じる
            zf.Close();

            //一時的に書きだした[system]password.txtを消去する。
            System.IO.File.Delete(file);
        }
        catch { }
    }

}


