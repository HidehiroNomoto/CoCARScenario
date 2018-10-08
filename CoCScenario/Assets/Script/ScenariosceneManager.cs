using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using System;
using System.IO;
using System.Collections.Generic;

//選択解除のボタン作る
//画像音声は、選択したファイル名を横にテキスト表示
//音声ファイルはクリックで鳴らす。
//動作テスト
//既作成コマンドを選択した際に、記入した状態でコマンド入力欄を開く(デバッグである程度動くの確認できてからでOK)
//画像サウンドデータの削除(デバッグである程度動くの確認できてからでOK)
//分岐コマンドを置くと、そのコマンドオブジェクトが子として分岐先の横に→オブジェクトを持つ（後でやる）
public class ScenariosceneManager : MonoBehaviour
{
    const int STATUSNUM = 12;
    const int SKILLNUM = 54;
    public string[] scenarioText = new string[100];          //シナリオテキスト保存変数
    public Sprite[] scenarioGraphic = new Sprite[100];       //シナリオ画像保存変数
    public string[] scenarioFilePath = new string[100];      //シナリオ用ファイルのアドレス
    public AudioClip[] scenarioAudio = new AudioClip[40];    //シナリオＢＧＭ・ＳＥ保存変数
    public Sprite batten;
    private string sectionName = "";
    GameObject objText;
    GameObject objTextBox;
    GameObject[] objCharacter = new GameObject[5];
    GameObject objBackImage;
    GameObject objBackText;
    GameObject objRollText;
    GameObject objName;
    GameObject objBGM;
    GameObject[] objDice = new GameObject[2];
    GameObject[] objBox=new GameObject[4];
    GameObject objInput;
    GameObject objSkip;
    List<GameObject> objCB = new List<GameObject>();
    List<GameObject> objGS = new List<GameObject>();
    List<string> commandData = new List<string>();
    List<string> gFileName = new List<string>();
    List<string> sFileName = new List<string>();
    public GameObject[] objMake = new GameObject[25];
    public bool backLogCSFlag = false;
    public int selectNum=-1;
    private string commandName;
    private int logNum=0;
    string _FILE_HEADER;
    const int CHARACTER_Y = -300;
    const int BUTTON_NUM = 25;
    //public GameObject objIvent;
    public GameObject objCommand;
    public GameObject parentObject;
    public GameObject objGSB;
    public GameObject objCCB;
    private GameObject parentGS;
    public GameObject objGraSou;
    public int GSButton=-1;
    public int selectGS = -1;
    List<string> backFileLog = new List<string>();
    private int commandFileNum = 0;

    // Use this for initialization
    void Start()
    {
        _FILE_HEADER = PlayerPrefs.GetString("進行中シナリオ", "");                      //ファイル場所の頭
        if (_FILE_HEADER == null || _FILE_HEADER == "") {  GetComponent<Utility>().StartCoroutine("LoadSceneCoroutine", "TitleScene"); }
        objName = GameObject.Find("CharacterName").gameObject as GameObject;
        objRollText = GameObject.Find("Rolltext").gameObject as GameObject; objRollText.gameObject.SetActive(false);
        for (int i = 0; i < 5; i++) { objCharacter[i] = GameObject.Find("Chara" + (i + 1).ToString()).gameObject as GameObject; objCharacter[i].gameObject.SetActive(false); }
        objInput= GameObject.Find("Input").gameObject as GameObject; objInput.gameObject.SetActive(false);
        objText = GameObject.Find("MainText").gameObject as GameObject;
        objTextBox = GameObject.Find("TextBox").gameObject as GameObject;
        objBackImage = GameObject.Find("BackImage").gameObject as GameObject;
        objBackText = GameObject.Find("BackText").gameObject as GameObject; objBackText.gameObject.SetActive(false);
        objBGM = GameObject.Find("BGMManager").gameObject as GameObject;
        for (int i = 0; i < 4; i++) { objBox[i] = GameObject.Find("select" + (i + 1).ToString()).gameObject as GameObject; objBox[i].gameObject.SetActive(false); }
        for (int i = 0; i < 2; i++) { objDice[i] = GameObject.Find("Dice" + (i + 1).ToString()).gameObject as GameObject; objDice[i].gameObject.SetActive(false); }
        ReadCommandFileNum(@GetComponent<Utility>().GetAppPath() + @"\" + "[system]commandFileNum.txt");
        commandName = "[system]command1" + objBGM.GetComponent<BGMManager>().chapterName;
        StartScene();
    }


    // Update is called once per frame
    void Update()
    {

    }

    private void ReadCommandFileNum(string path)
    {
        List<string> texts = new List<string>();
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
                    Stream reader = zf.GetInputStream(ze);
                    //文字コードを指定してStreamReaderを作成
                    StreamReader sr = new StreamReader(
                        reader, System.Text.Encoding.GetEncoding("UTF-8"));
                    // テキストを取り出す
                    string text = sr.ReadToEnd();

                    // 読み込んだ目次テキストファイルからstring配列を作成する
                    texts.AddRange(text.Replace("\r", "").Split('\n'));
                    commandFileNum= int.Parse(texts[0]);
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
            GetComponent<Utility>().StartCoroutine("LoadSceneCoroutine", "TitleScene");
        }
    }

    private void SaveCommandFileNum()
    {
        string str = commandFileNum.ToString();
        //ZIP書庫のパス
        string zipPath = PlayerPrefs.GetString("進行中シナリオ", "");
        //書庫に追加するファイルのパス
        string file = @GetComponent<Utility>().GetAppPath() + @"\" + "[system]commandFileNum.txt";

        //先にテキストファイルを一時的に書き出しておく。
        File.WriteAllText(file, str);

        //ZipFileオブジェクトの作成
        ICSharpCode.SharpZipLib.Zip.ZipFile zf =
            new ICSharpCode.SharpZipLib.Zip.ZipFile(zipPath);
        zf.Password = Secret.SecretString.zipPass;
        //ZipFileの更新を開始
        zf.BeginUpdate();

        //ZIP内のエントリの名前を決定する 
        string f = Path.GetFileName(file);
        //ZIP書庫に一時的に書きだしておいたファイルを追加する
        zf.Add(file, f);
        //イベントファイルと画像サウンドファイルを追加
        AddIventGS(zf);
        //ZipFileの更新をコミット
        zf.CommitUpdate();

        //閉じる
        zf.Close();

        //一時的に書きだしたファイルを消去する。
        File.Delete(file);
    }


    public void CommandButton(int num)
    {
        for (int i = 0; i < BUTTON_NUM; i++) { objMake[i].SetActive(false); }
        objGS.Clear();
        objMake[num].SetActive(true);
        selectGS = -1;
        parentGS = GameObject.Find("GSContents");
        if (num == 2 || num==6 || num==7 || num==12)
        {
            for (int i = 0; i < scenarioGraphic.Length; i++)
            {
                if (scenarioGraphic[i] == null) { break; }
                objGS.Insert(objGS.Count, Instantiate(objGraSou) as GameObject);
                objGS[objGS.Count - 1].transform.SetParent(parentGS.transform, false);
                objGS[objGS.Count - 1].GetComponent<GSButton>().buttonNum = objGS.Count - 1;
                objGS[objGS.Count - 1].GetComponent<Image>().sprite= scenarioGraphic[i];
            }
            if (num == 6)
            {
                objGS.Insert(objGS.Count, Instantiate(objGraSou) as GameObject);
                objGS[objGS.Count - 1].transform.SetParent(parentGS.transform, false);
                objGS[objGS.Count - 1].GetComponent<Image>().sprite = batten;
                objGS[objGS.Count - 1].GetComponent<GSButton>().buttonNum = - 1;
            }//キャラ選択の場合、表示しないを選択したいことがある。それを×マークでフォロー。
        }
        if (num == 3 || num==5)
        {
            for (int i = 0; i < scenarioAudio.Length; i++)
            {
                if (scenarioAudio[i] == null) { break; }
                objGS.Insert(objGS.Count, Instantiate(objGraSou) as GameObject);
                objGS[objGS.Count - 1].transform.SetParent(parentGS.transform, false);
                objGS[objGS.Count - 1].GetComponent<GSButton>().buttonNum = objGS.Count - 1;
            }
        }
    }

    //エクスプローラーのドラッグ＆ドロップ機能は使わない。
    //Graphic,Soundフォルダを作り、そこにファイルを投入してもらう。

    public void CommandDecide(int num)
    {
        string commandText="";
        if (num == 0) { commandText = "Text:" + GameObject.Find("InputFieldName").GetComponent<InputField>().text + "," + GameObject.Find("InputFieldText").GetComponent<InputField>().text; }
        if (num == 1) { commandText = "BackText:" + GameObject.Find("InputFieldText").GetComponent<InputField>().text; }
        if (num == 2) { if (selectGS == -1) { return; } commandText = "Back:" + selectGS.ToString(); }
        if (num == 3) { if (selectGS == -1) { return; } commandText = "BGM:" + GameObject.Find("InputFieldName").GetComponent<InputField>().text + ":" + selectGS.ToString() ; }
        if (num == 4) { commandText = "BGMStop:" + GameObject.Find("InputFieldName").GetComponent<InputField>().text; }
        if (num == 5) { if (selectGS == -1) { return; } commandText = "SE:" + selectGS.ToString(); }
        if (num == 6) { if (selectGS == -1) { return; } commandText = "Chara" + ((int)(GameObject.Find("Slider").GetComponent<Slider>().value)).ToString() + ":" + selectGS.ToString(); }
        if (num == 7) { if (selectGS == -1) { return; } commandText = "Item:" + selectGS.ToString(); }
        if (num == 8) { commandText = "Shake:"; }
        if (num == 9) { commandText = "Jump:" + ((int)(GameObject.Find("Slider").GetComponent<Slider>().value)).ToString(); }
        if (num == 10) { commandText = "Select:" + GameObject.Find("InputFieldText").GetComponent<InputField>().text + "," + GameObject.Find("InputFieldText (1)").GetComponent<InputField>().text + "," + GameObject.Find("InputFieldText (2)").GetComponent<InputField>().text + "," + GameObject.Find("InputFieldText (3)").GetComponent<InputField>().text; }
        if (num == 11) { commandText="Hantei:" + GameObject.Find("InputFieldText (2)").GetComponent<InputField>().text + "," + GameObject.Find("InputFieldText (3)").GetComponent<InputField>().text; }
        if (num == 12) { if (selectGS == -1) { return; } commandText = "Battle:" + selectGS.ToString() + "," + GameObject.Find("Label1").GetComponent<Text>().text + "," + GameObject.Find("InputFieldText (2)").GetComponent<InputField>().text + "," + GameObject.Find("InputFieldText (3)").GetComponent<InputField>().text + "," + GameObject.Find("InputFieldText (4)").GetComponent<InputField>().text + "," + GameObject.Find("Label2").GetComponent<Text>().text + "," + GameObject.Find("Label3").GetComponent<Text>().text.Replace("D","") + "," + (GameObject.Find("Toggle1").GetComponent<Toggle>().isOn).ToString().ToLower() + "," + GameObject.Find("InputFieldText (5)").GetComponent<InputField>().text + "," + GameObject.Find("InputFieldText (6)").GetComponent<InputField>().text + "," + GameObject.Find("InputFieldText (7)").GetComponent<InputField>().text + "," + GameObject.Find("InputFieldText (8)").GetComponent<InputField>().text + "," + (GameObject.Find("Toggle2").GetComponent<Toggle>().isOn).ToString().ToLower(); }
        if (num == 13) { commandText="FlagBranch:" + GameObject.Find("InputFieldText (2)").GetComponent<InputField>().text + "," + GameObject.Find("InputFieldText (3)").GetComponent<InputField>().text; }
        if (num == 14) { commandText = "FlagChange:" + GameObject.Find("InputFieldText (2)").GetComponent<InputField>().text + "," + GameObject.Find("InputFieldText (3)").GetComponent<InputField>().text; }
        if (num == 15) { commandText = "GetTime:"; }
        if (num == 16) { commandText = "FlagName:" + GameObject.Find("InputFieldText (2)").GetComponent<InputField>().text + "," + GameObject.Find("InputFieldText (3)").GetComponent<InputField>().text; }
        if (num == 17) { commandText = "Difference:" + GameObject.Find("InputFieldText (2)").GetComponent<InputField>().text + "," + GameObject.Find("InputFieldText (3)").GetComponent<InputField>().text + "," + GameObject.Find("InputFieldText (4)").GetComponent<InputField>().text; }
        if (num == 18) { if (GameObject.Find("InputFieldText").GetComponent<InputField>().text == "") { commandText = "StatusChange:" + GameObject.Find("Label1").GetComponent<Text>().text +"," + GameObject.Find("Label2").GetComponent<Text>().text + "," + GameObject.Find("Label3").GetComponent<Text>().text; } else { commandText = "StatusChange:" + GameObject.Find("Label1").GetComponent<Text>().text + "," + GameObject.Find("InputFieldText").GetComponent<InputField>().text; }  }
        if (num == 19) { commandText = "Input:" + GameObject.Find("InputFieldText (2)").GetComponent<InputField>().text; }
        if (num == 20) { commandText = "Equal:" + GameObject.Find("InputFieldText (2)").GetComponent<InputField>().text + "," + GameObject.Find("InputFieldText (3)").GetComponent<InputField>().text; }
        if (num == 21) { commandText = "Lost:"; }
        if (num == 22) { commandText = "Title:"; }
        if (num == 23) { commandText = "Map:" + (GameObject.Find("Toggle").GetComponent<Toggle>().isOn).ToString().ToLower(); }
        if (num == 24) { if (GameObject.Find("InputFieldText (2)").GetComponent<InputField>().text == "") { commandText = "NextFile:" + "[system]" + commandFileNum.ToString() + objBGM.GetComponent<BGMManager>().chapterName; } else { commandText = "NextFile:" + "[system]" + GameObject.Find("InputFieldText (2)").GetComponent<InputField>().text + objBGM.GetComponent<BGMManager>().chapterName; } }
        if (selectNum >= 0)
        {
            if (commandData.Count <= selectNum) { for (int i = commandData.Count; i <= selectNum; i++) { commandData.Add(""); } }//commandDataの要素数をselectNumが越えたら配列の要素数を合わせて増やす。中身は空でOK。（イベント追加されるとcommandData.Count以上の番号を持つイベントができるため）
            commandData[selectNum] = commandText;
            objCB[selectNum].GetComponentInChildren<Text>().text = commandData[selectNum];
        }
    }

    private void StartScene()
    {
        List<string> tmp = LoadIventData(objBGM.GetComponent<BGMManager>().chapterName);
        for (int i = 0; i < tmp.Count; i++)
        {
            ZipRead(tmp[i]);//zipにあるイベントに関連するpngやwavを読み込む。
        }
        string[] files = Directory.GetFiles(@GetComponent<Utility>().GetAppPath() + @"\シナリオに使うpngやwavを入れるフォルダ", "*", SearchOption.AllDirectories);
        for (int i = 0; i < files.Length; i++)
        {
            StartCoroutine(LoadFile(files[i],tmp));//素材フォルダのファイルを読み込む。
        }
        LoadCommandData("[system]command1" + objBGM.GetComponent<BGMManager>().chapterName);
    }

    private void ZipRead(string path)
    {
        int j;
        byte[] buffer;
        try
        {
            //ZipFileオブジェクトの作成
            ICSharpCode.SharpZipLib.Zip.ZipFile zf =
                new ICSharpCode.SharpZipLib.Zip.ZipFile(PlayerPrefs.GetString("進行中シナリオ", ""));
            zf.Password = Secret.SecretString.zipPass;

            //閲覧するエントリ
            string extractFile = path;
            //展開するエントリを探す
            ICSharpCode.SharpZipLib.Zip.ZipEntry ze = zf.GetEntry(extractFile);

            if (ze != null)
            {
                //pngファイルの場合
                if (path.Substring(path.Length - 4) == ".png")
                {
                    //閲覧するZIPエントリのStreamを取得
                    Stream fs = zf.GetInputStream(ze);
                    buffer = ReadBinaryData(fs);//bufferにbyte[]になったファイルを読み込み

                    // 画像を取り出す
                    //横サイズ
                    int pos = 16;
                    int width = 0;
                    for (int i = 0; i < 4; i++)
                    {
                        width = width * 256 + buffer[pos++];
                    }
                    //縦サイズ
                    int height = 0;
                    for (int i = 0; i < 4; i++)
                    {
                        height = height * 256 + buffer[pos++];
                    }
                    //byteからTexture2D作成
                    Texture2D texture = new Texture2D(width, height);
                    texture.LoadImage(buffer);

                    // 読み込んだ画像からSpriteを作成する
                    for (j = 0; j < 100; j++) { if (scenarioGraphic[j] == null) { break; } }
                    scenarioGraphic[j] = Sprite.Create(texture, new Rect(0, 0, texture.width, texture.height), new Vector2(0.5f, 0.5f));
                    gFileName[j] = path;
                    //閉じる
                    fs.Close();
                }

                //wavファイルの場合
                if (path.Substring(path.Length - 4) == ".wav")
                {
                    //閲覧するZIPエントリのStreamを取得
                    Stream fs = zf.GetInputStream(ze);
                    buffer = ReadBinaryData(fs);//bufferにbyte[]になったファイルを読み込み
                    for (j = 0; j < 40; j++) { if (scenarioAudio[j] == null) { break; } }
                    scenarioAudio[j] = WavUtility.ToAudioClip(buffer);
                    sFileName[j] = path;
                    //閉じる
                    fs.Close();
                }

            }
            zf.Close();
        }
        catch
        {
            GetComponent<Utility>().StartCoroutine("LoadSceneCoroutine", "TitleScene");
        }
    }




    //イベントデータを読みだして、ファイル名を返す。
    private List<string> LoadIventData(string path)
    {
        List<string> texts = new List<string>();
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
                    Stream reader = zf.GetInputStream(ze);
                    //文字コードを指定してStreamReaderを作成
                    StreamReader sr = new StreamReader(
                        reader, System.Text.Encoding.GetEncoding("UTF-8"));
                    // テキストを取り出す
                    string text = sr.ReadToEnd();

                    // 読み込んだ目次テキストファイルからstring配列を作成する
                    texts.AddRange(text.Replace("\r","").Split('\n'));
                    //閉じる
                    sr.Close();
                    reader.Close();
                    texts.RemoveAt(texts.Count - 1);//最終行は[END]なので除去。
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
        return texts;
    }
    // ストリームからデータを読み込み、バイト配列に格納
    static public byte[] ReadBinaryData(Stream st)
    {
        using (MemoryStream ms = new MemoryStream())
        {
            st.CopyTo(ms);
            return ms.ToArray();
        }
    }






    private IEnumerator LoadFile(string path,List<string> tmp)
    {
        int i;
        int j;
        bool k;

        path = path.Replace("\n", "").Replace("\r","");
        // 指定したファイルをロードする
        WWW request = new WWW(path);

        while (!request.isDone)
        {
            yield return new WaitForEndOfFrame();
        }
        //pngファイルの場合
        if (path.Substring(path.Length - 4) == ".png")
        {
            // 画像を取り出す
            Texture2D texture = request.texture;

            // 読み込んだ画像からSpriteを作成する
            for (i = 0; i < 100; i++) { if (scenarioGraphic[i] == null) { break; } }
            k = true;
            //同名ファイルが既にzipに入っていれば上書き
            for (j = 0; j < tmp.Count; j++)
            {
                if (Path.GetFileName(@path) == tmp[j]) { i = j;gFileName[i] = path;k = false; break; }
            }
            if(k){ gFileName.Add(path); }
            scenarioGraphic[i] = Sprite.Create(texture, new Rect(0, 0, texture.width, texture.height), new Vector2(0.5f, 0.5f));
        }

        //wavファイルの場合
        if (path.Substring(path.Length - 4) == ".wav")
        {
            yield return StartCoroutine(LoadBGM(request,tmp,path));
        }
        yield return null;
    }

    private IEnumerator LoadBGM(WWW request,List<string> tmp,string path)
    {
        int i;
        bool k;
        for (i = 0; i < 40; i++) { if (scenarioAudio[i] == null) { break; } }
        k = true;
        //同名ファイルが既にzipに入っていれば上書き
        for (int j = 0; j < tmp.Count; j++)
        {
            if (Path.GetFileName(@path) == tmp[j]) { i = j; sFileName[i] = path;k = false; break; }
        }
        if(k){ sFileName.Add(path); }
        scenarioAudio[i] = request.GetAudioClip(false, true);
        while (scenarioAudio[i].loadState == AudioDataLoadState.Loading)
        {
            // ロードが終わるまで待つ
            yield return new WaitForEndOfFrame();
        }

        if (scenarioAudio[i].loadState != AudioDataLoadState.Loaded)
        {
            // 読み込み失敗
            Debug.Log("Failed to Load!");
            yield break;
        }
    }

    //コマンドファイルを読み込む。
    private void LoadCommandData(string path)
    {
        commandData.Clear();
        objCB.Clear();
        try
        {
            //閲覧するエントリ
            string extractFile = path;

            //ZipFileオブジェクトの作成
            ICSharpCode.SharpZipLib.Zip.ZipFile zf =
                new ICSharpCode.SharpZipLib.Zip.ZipFile(PlayerPrefs.GetString("進行中シナリオ", ""));
            zf.Password = Secret.SecretString.zipPass;
            try
            {
                //展開するエントリを探す
                ICSharpCode.SharpZipLib.Zip.ZipEntry ze = zf.GetEntry(extractFile);


                if (ze != null)
                {
                    //閲覧するZIPエントリのStreamを取得
                    Stream reader = zf.GetInputStream(ze);
                    //文字コードを指定してStreamReaderを作成
                    StreamReader sr = new StreamReader(
                        reader, System.Text.Encoding.GetEncoding("UTF-8"));
                    // テキストを取り出す
                    string text = sr.ReadToEnd();

                    // 読み込んだ目次テキストファイルからstring配列を作成する
                    commandData.AddRange(text.Split('\n'));
                    //閉じる
                    sr.Close();
                    reader.Close();
                    commandData.RemoveAt(commandData.Count - 1);//最終行は[END]なので除去。
                    //コマンドをボタンとして一覧に放り込む。
                    for (int i = 0; i < commandData.Count; i++)
                    {
                        objCB.Add(Instantiate(objCommand) as GameObject);
                        objCB[i].transform.SetParent(parentObject.transform, false);
                        objCB[i].GetComponentInChildren<Text>().text = commandData[i];
                        objCB[i].GetComponent<CommandButton>().buttonNum = i;
                    }
                }
                else
                {
                    objCB.Add(Instantiate(objCommand) as GameObject);
                    objCB[0].transform.SetParent(parentObject.transform, false);
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

    public void CommandAddButton()
    {
        //追加ボタンが押されたらコマンドボタンを追加する。
        if (selectNum >= 0)
        {
            objCB.Insert(selectNum, Instantiate(objCommand) as GameObject);
            objCB[selectNum].transform.SetParent(parentObject.transform, false);
            objCB[selectNum].GetComponent<CommandButton>().buttonNum = selectNum;
            objCB[selectNum].GetComponent<Transform>().SetSiblingIndex(selectNum);
            for (int i = selectNum + 1; i < objCB.Count; i++) { objCB[i].GetComponent<CommandButton>().buttonNum++; }//追加分の後ろはボタン番号が１増える。
        }
        if (selectNum == -1)//選択されたコマンドがなければ末尾に追加
        {
            objCB.Insert(objCB.Count, Instantiate(objCommand) as GameObject);
            objCB[objCB.Count - 1].transform.SetParent(parentObject.transform, false);
            objCB[objCB.Count - 1].GetComponent<CommandButton>().buttonNum = objCB.Count - 1;
        }
    }

    public void CommandDeleteButton()
    {
        if (selectNum >= 0)
        {
            Destroy(objCB[selectNum]);
            objCB.RemoveAt(selectNum);
            for (int i = selectNum; i < objCB.Count; i++) { objCB[i].GetComponent<CommandButton>().buttonNum--; }//削除分の後ろはボタン番号が１減る。
            commandData.RemoveAt(selectNum);
            selectNum = -1;
        }
    }

    public void SetCommand()
    {
        string[] strs;
        try
        {
            
            strs = commandData[selectNum].Split(',');
            /*
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
            */
        }
        catch
        {
        }
    }

    public void BackButton()
    {
        SaveCommandFile();
        if (backFileLog.Count == 0) { GetComponent<Utility>().StartCoroutine("LoadSceneCoroutine", "MapScene"); }
        else { commandName = backFileLog[backFileLog.Count - 1]; backFileLog.RemoveAt(backFileLog.Count - 1); LoadCommandData(commandName);  }//一つ戻って、履歴からはそこを消す
    }

    public void NextFileButton()
    {
        string path;
        commandFileNum++;
        SaveCommandFileNum();
        path = GameObject.Find("InputFieldNextPath").GetComponent<InputField>().text;
        CommandDecide(24);//ネクストファイルの「決定」ボタンを押したのと同じ効果。
        SaveCommandFile();
        backFileLog.Add(commandName);//現在のコマンドファイル名をログに保存
        commandName = "[system]" + path + objBGM.GetComponent<BGMManager>().chapterName;//次のファイルのコマンドファイル名に入れ替え
        LoadCommandData(commandName);    
    }

    //コマンドファイルを書き出す関数
    public void SaveCommandFile()
    {
        string str = "";
        //ZIP書庫のパス
        string zipPath = PlayerPrefs.GetString("進行中シナリオ", "");
        //書庫に追加するファイルのパス
        string file= @GetComponent<Utility>().GetAppPath() + @"\" + commandName;

        //先にテキストファイルを一時的に書き出しておく。
        for (int i = 0; i < commandData.Count; i++) { if (commandData[i].Replace("\n", "").Replace("\r", "") == "") { continue; } str = str + commandData[i].Replace("\n", "").Replace("\r", "") + "\r\n"; }
        str = str + "[END]";
        File.WriteAllText(file, str);

        //ZipFileオブジェクトの作成
        ICSharpCode.SharpZipLib.Zip.ZipFile zf =
            new ICSharpCode.SharpZipLib.Zip.ZipFile(zipPath);
        zf.Password = Secret.SecretString.zipPass;
        //ZipFileの更新を開始
        zf.BeginUpdate();

        //ZIP内のエントリの名前を決定する 
        string f = Path.GetFileName(file);
        //ZIP書庫に一時的に書きだしておいたファイルを追加する
        zf.Add(file, f);
        //イベントファイルと画像サウンドファイルを追加
        AddIventGS(zf);
        //ZipFileの更新をコミット
        zf.CommitUpdate();

        //閉じる
        zf.Close();

        //一時的に書きだしたファイルを消去する。
        File.Delete(file);
    }

    private void AddIventGS(ICSharpCode.SharpZipLib.Zip.ZipFile zf)
    {
        //イベントファイルの作成
        string str = "[system]command1" + objBGM.GetComponent<BGMManager>().chapterName;
        //先にテキストファイルを一時的に書き出しておく。
        for (int i = 0; i < gFileName.Count; i++) { if (gFileName[i].Replace("\n", "").Replace("\r", "") == "") { continue; } str = str + gFileName[i].Replace("\n", "").Replace("\r", "") + "\r\n"; }
        for (int i = 0; i < sFileName.Count; i++) { if (sFileName[i].Replace("\n", "").Replace("\r", "") == "") { continue; } str = str + sFileName[i].Replace("\n", "").Replace("\r", "") + "\r\n"; }
        str = str + "[END]";
        File.WriteAllText(@GetComponent<Utility>().GetAppPath() + @"\" + objBGM.GetComponent<BGMManager>().chapterName, str);
        zf.Add(@GetComponent<Utility>().GetAppPath() + @"\" + objBGM.GetComponent<BGMManager>().chapterName, Path.GetFileName(objBGM.GetComponent<BGMManager>().chapterName));
        File.Delete(@GetComponent<Utility>().GetAppPath() + @"\" + objBGM.GetComponent<BGMManager>().chapterName);

        //画像サウンドファイルの作成
        for (int i = 0; i < gFileName.Count; i++) { if (gFileName[i] != Path.GetFileName(gFileName[i])) { zf.Add(gFileName[i], Path.GetFileName(gFileName[i])); } }
        for (int i = 0; i < sFileName.Count; i++) { if (sFileName[i] != Path.GetFileName(gFileName[i])) { zf.Add(sFileName[i], Path.GetFileName(sFileName[i])); } }
    }



    private IEnumerator InputText(string str)
    {
        selectNum = -1;
        objInput.gameObject.SetActive(true);
        InputField inputField = objInput.GetComponent<InputField>();
        inputField.text = "";
        objBox[3].gameObject.SetActive(true);
        objBox[3].GetComponentInChildren<Text>().text = "決定";
        SelectBoxMake(0, 0, 0, 2, false);
        while (selectNum==-1) { yield return null; }
        objInput.gameObject.SetActive(false);
        objBox[3].gameObject.SetActive(false);
    }

    private void StatusChange(string[] separateText)
    {
        int x1,x2, y2;
        string targetStr;
        Utility u1 = GetComponent<Utility>();
        string[] separate3Text;
        targetStr=SkillList(separateText[0]);
        if (int.TryParse(separateText[1].Replace("\r", "").Replace("\n", ""), out x1))
        {
            x2 = x1;
            if (x2 > 0)
            {
                TextDraw("", separateText[0] + "の能力が" + x2.ToString() + "点上昇した。");
            }
            else
            {
                TextDraw("", separateText[0] + "の能力が" + (-1*x2).ToString() + "点減少した。");
            }
        }
        else
        {
            separate3Text = separateText[1].Split('D');
            int.TryParse(separate3Text[0],out y2);
                if (y2 > 0)
                {
                    TextDraw("", separateText[0] + "の能力が" + separateText[1]  + "点上昇した。");
                }
                else
                {
                separateText[1].Replace("-","");
                    TextDraw("", separateText[0] + "の能力が" + separateText[1] + "点減少した。");
                }
            
        }
    }

    private void SelectBoxMake(int choiceA, int choiceB, int choiceC, int choiceD,bool inBattleFlag)
    {
        if (inBattleFlag == false)
        {
            if (choiceA > 0 && choiceB > 0 && choiceC > 0 && choiceD > 0) { objBox[0].GetComponent<RectTransform>().localPosition = new Vector3(0, 500, 0); objBox[1].GetComponent<RectTransform>().localPosition = new Vector3(0, 350, 0); objBox[2].GetComponent<RectTransform>().localPosition = new Vector3(0, 200, 0); objBox[3].GetComponent<RectTransform>().localPosition = new Vector3(0, 50, 0); for (int i = 0; i < 4; i++) { objBox[i].GetComponent<RectTransform>().sizeDelta = new Vector2(660, 100); } }
            if (choiceA > 0 && choiceB > 0 && choiceC > 0 && choiceD == 0) { objBox[0].GetComponent<RectTransform>().localPosition = new Vector3(0, 350, 0); objBox[1].GetComponent<RectTransform>().localPosition = new Vector3(0, 200, 0); objBox[2].GetComponent<RectTransform>().localPosition = new Vector3(0, 50, 0); for (int i = 0; i < 3; i++) { objBox[i].GetComponent<RectTransform>().sizeDelta = new Vector2(660, 100); } }
            if (choiceA > 0 && choiceB > 0 && choiceC == 0 && choiceD == 0) { objBox[0].GetComponent<RectTransform>().localPosition = new Vector3(0, 350, 0); objBox[1].GetComponent<RectTransform>().localPosition = new Vector3(0, 200, 0); for (int i = 0; i < 2; i++) { objBox[i].GetComponent<RectTransform>().sizeDelta = new Vector2(660, 100); } }
            if (choiceA > 0 && choiceB == 0 && choiceC == 0 && choiceD == 0) { objBox[0].GetComponent<RectTransform>().localPosition = new Vector3(0, 200, 0); for (int i = 0; i < 1; i++) { objBox[i].GetComponent<RectTransform>().sizeDelta = new Vector2(660, 100); } }
        }
        else
        {
            objBox[0].GetComponent<RectTransform>().localPosition = new Vector3(-200, -250, 0); objBox[1].GetComponent<RectTransform>().localPosition = new Vector3(-200, -400, 0); objBox[2].GetComponent<RectTransform>().localPosition = new Vector3(200, -250, 0); objBox[3].GetComponent<RectTransform>().localPosition = new Vector3(200, -400, 0);
            for(int i=0;i<4;i++){ objBox[i].GetComponent<RectTransform>().sizeDelta = new Vector2(300, 100); }
        }
    }

    private IEnumerator Select(string choiceA,string choiceB,string choiceC,string choiceD,bool inBattleFlag)
    {
        objBox[0].gameObject.SetActive(true); objBox[0].GetComponentInChildren<Text>().text = choiceA;
        if (choiceB.Length>0) { objBox[1].gameObject.SetActive(true); objBox[1].GetComponentInChildren<Text>().text = choiceB; }
        if (choiceC.Length>0) { objBox[2].gameObject.SetActive(true); objBox[2].GetComponentInChildren<Text>().text = choiceC; }
        if (choiceD.Length>0) { objBox[3].gameObject.SetActive(true); objBox[3].GetComponentInChildren<Text>().text = choiceD; }
        SelectBoxMake(choiceA.Length, choiceB.Length, choiceC.Length, choiceD.Length,inBattleFlag);
        //ボタンがクリックされるまでループ。
        selectNum = -1;
        while (selectNum == -1)
        {
            yield return null;
        }
        for (int i = 0; i < 4; i++) { objBox[i].gameObject.SetActive(false); }
    }

    private int SkillCheck(string targetStr)
    {
        int target = 0;
        int num = 0;
        string[] separate = new string[2];
        separate[0] = "";
        separate[1] = "";
        if (targetStr.Contains("*"))
        {
            separate = targetStr.Split('*');
        }
        else if (targetStr.Contains("/"))
        {
            separate = targetStr.Split('/');
        }
        else
        {
            separate[0] = targetStr;
        }
        target = PlayerPrefs.GetInt(SkillList(separate[0]), target); 
        int.TryParse(separate[1], out num);
        if (targetStr.Contains("*"))
        {
            target = target * num;
        }
        if (targetStr.Contains("/"))
        {
            target = target / num;
        }
        return target;
    }

    private string SkillList(string targetStr)
    {
        string target = targetStr;
        if (targetStr == "言いくるめ") { target = "Skill0"; }
        if (targetStr == "医学") { target = "Skill1"; }
        if (targetStr == "運転") { target = "Skill2"; }
        if (targetStr == "応急手当") { target = "Skill3"; }
        if (targetStr == "オカルト") { target = "Skill4"; }
        if (targetStr == "回避") { target = "Skill5"; }
        if (targetStr == "化学") { target = "Skill6"; }
        if (targetStr == "鍵開け") { target = "Skill7"; }
        if (targetStr == "隠す") { target = "Skill8"; }
        if (targetStr == "隠れる") { target = "Skill9"; }
        if (targetStr == "機械修理") { target = "Skill10"; }
        if (targetStr == "聞き耳") { target = "Skill11"; }
        if (targetStr == "芸術") { target = "Skill12"; }
        if (targetStr == "経理") { target = "Skill13"; }
        if (targetStr == "考古学") { target = "Skill14"; }
        if (targetStr == "コンピューター") { target = "Skill15"; }
        if (targetStr == "忍び歩き") { target = "Skill16"; }
        if (targetStr == "写真術") { target = "Skill17"; }
        if (targetStr == "重機械操作") { target = "Skill18"; }
        if (targetStr == "乗馬") { target = "Skill19"; }
        if (targetStr == "信用") { target = "Skill20"; }
        if (targetStr == "心理学") { target = "Skill21"; }
        if (targetStr == "人類学") { target = "Skill22"; }
        if (targetStr == "水泳") { target = "Skill23"; }
        if (targetStr == "製作") { target = "Skill24"; }
        if (targetStr == "精神分析") { target = "Skill25"; }
        if (targetStr == "生物学") { target = "Skill26"; }
        if (targetStr == "説得") { target = "Skill27"; }
        if (targetStr == "操縦") { target = "Skill28"; }
        if (targetStr == "地質学") { target = "Skill29"; }
        if (targetStr == "跳躍") { target = "Skill30"; }
        if (targetStr == "追跡") { target = "Skill31"; }
        if (targetStr == "電気修理") { target = "Skill32"; }
        if (targetStr == "電子工学") { target = "Skill33"; }
        if (targetStr == "天文学") { target = "Skill34"; }
        if (targetStr == "投擲") { target = "Skill35"; }
        if (targetStr == "登攀") { target = "Skill36"; }
        if (targetStr == "図書館") { target = "Skill37"; }
        if (targetStr == "ナビゲート") { target = "Skill38"; }
        if (targetStr == "値切り") { target = "Skill39"; }
        if (targetStr == "博物学") { target = "Skill40"; }
        if (targetStr == "物理学") { target = "Skill41"; }
        if (targetStr == "変装") { target = "Skill42"; }
        if (targetStr == "法律") { target = "Skill43"; }
        if (targetStr == "ほかの言語") { target = "Skill44"; }
        if (targetStr == "母国語") { target = "Skill45"; }
        if (targetStr == "マーシャルアーツ") { target = "Skill46"; }
        if (targetStr == "目星") { target = "Skill47"; }
        if (targetStr == "薬学") { target = "Skill48"; }
        if (targetStr == "歴史") { target = "Skill49"; }
        if (targetStr == "火器") { target = "Skill50"; }
        if (targetStr == "格闘") { target = "Skill51"; }
        if (targetStr == "武器術") { target = "Skill52"; }
        if (targetStr == "クトゥルフ神話") { target = "Skill53"; }
        if (targetStr == "STR") { target = "Status0"; }
        if (targetStr == "DEX") { target = "Status2"; }
        if (targetStr == "CON") { target = "Status1"; }
        if (targetStr == "POW") { target = "Status5"; }
        if (targetStr == "INT") { target = "Status3"; }
        if (targetStr == "EDU") { target = "Status7"; }
        if (targetStr == "SIZ") { target = "Status6"; }
        if (targetStr == "APP") { target = "Status4"; }
        if (targetStr == "MP") { target = "Status10"; }
        if (targetStr == "HP") { target = "Status9"; }
        return target;
    }

    private int Hantei(string targetStr,int bonus)
    {
        int target=0;
        string bonusStr="";
        target=SkillCheck(targetStr);
        if (bonus > 0) { bonusStr = " + " + bonus.ToString(); }
        if (bonus < 0) { bonusStr = " - " + (-1*bonus).ToString(); }
        objRollText.gameObject.SetActive(true);
        if (target > -bonus) { objRollText.GetComponent<Text>().text = targetStr + bonusStr + "\n" + "<color=#88ff88ff>" + (target + bonus).ToString() + "</color>"; } else { objRollText.GetComponent<Text>().text = targetStr + bonusStr + "\n" + "<color=#88ff88ff>" + "自動失敗" + "</color>"; }
        Utility u1 = GetComponent<Utility>();
        objTextBox.gameObject.SetActive(true);
        return 0;
    }

    private void TextDraw(string name,string text)
    {
        objTextBox.gameObject.SetActive(true);
        objText.GetComponent<Text>().text = text;
        if (name == "[PC]")
        {
            objName.GetComponent<Text>().text = "　" + PlayerPrefs.GetString("PlayerCharacterName","あなた");
        }
        else
        {
            objName.GetComponent<Text>().text = "　" + name;
        }
    }

    private void BackTextDraw(string text)
    {
        //背景テキスト表示の際は通常テキスト欄は消す
        objTextBox.gameObject.SetActive(false);
        objBackText.gameObject.SetActive(true);
        objBackText.GetComponent<Text>().text = text;
    }

    private void BackDraw(int back)
    {
        objBackImage.GetComponent<Image>().sprite = scenarioGraphic[back];
    }

    private void CharacterDraw(int character, int position)
    {
        if (character == -1) { objCharacter[position - 1].gameObject.SetActive(false); return; }
        objCharacter[position - 1].gameObject.SetActive(true);
        objCharacter[position - 1].GetComponent<Image>().sprite = scenarioGraphic[character];
        ObjSizeChangeToGraph(position-1,scenarioGraphic[character]);
    }

    //画像サイズに合わせて立ち絵サイズを変更
    private void ObjSizeChangeToGraph(int position,Sprite sprite)
    {
        objCharacter[position].GetComponent<RectTransform>().sizeDelta=new Vector2(sprite.pixelsPerUnit * sprite.bounds.size.x, sprite.pixelsPerUnit * sprite.bounds.size.y);
    }
}