using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using System;
using System.IO;
using System.Collections.Generic;


public class ScenariosceneManager : MonoBehaviour
{
    const int STATUSNUM = 12;
    const int SKILLNUM = 54;
    public string[] scenarioText = new string[100];          //シナリオテキスト保存変数
    public Sprite[] scenarioGraphic = new Sprite[100];       //シナリオ画像保存変数
    public AudioClip[] scenarioAudio = new AudioClip[40];    //シナリオＢＧＭ・ＳＥ保存変数
    public AudioClip errorSE;
    public Sprite batten;
    private string sectionName = "";
    GameObject objText;
    GameObject objTextBox;
    GameObject[] objCharacter = new GameObject[5];
    GameObject objBackImage;
    GameObject objBackText;
    GameObject objName;
    GameObject objBGM;
    public List<GameObject> objCB = new List<GameObject>();
    List<GameObject> objGS = new List<GameObject>();
    public List<string> commandData = new List<string>();
    private string[] gFileName = new string[99];
    private string[] sFileName = new string[40];
    public GameObject[] objMake = new GameObject[30];
    public int selectNum = -1;
    private string commandName;
    string _FILE_HEADER;
    const int CHARACTER_Y = -615;
    public GameObject objCommand;
    public GameObject parentObject;
    public GameObject objGSB;
    public GameObject objCCB;
    private GameObject parentGS;
    public GameObject objGraSou;
    public int GSButton = -1;
    public int selectGS = -1;
    public List<string> backFileLog = new List<string>();
    public List<int[]> backGraphLog = new List<int[]>();
    public List<string> backBTLog = new List<string>();
    public List<string[]> backTLog = new List<string[]>();
    private int commandFileNum = 0;//同一シナリオのコマンドファイルの通し番号。名前をつけないコマンドファイルは、NoName通し番号が名前になる（＝名前の重複は起きない）。
    private int graphicNum, soundNum;
    public int[] backGraphLogTemp = { 0, -1, -1, -1, -1, -1 };
    public string backBTLogTemp;
    public string[] backTLogTemp = new string[2];
    public GameObject titleText;
    public int selectBefore = -1;
    public List<int> multiSelect = new List<int>();
    public int errorFlag=0;
    private bool NCBFlag = false;
    private List<string> undoList = new List<string>();
    private int undoListNum=0;
    private bool URBool;
    private bool copyBool;
    private int time=0;
    public InputField[] inputs = new InputField[42];
    public int fallNum=0;

    // Use this for initialization
    void Start()
    {
        _FILE_HEADER = PlayerPrefs.GetString("進行中シナリオ", "");                      //ファイル場所の頭
        if (_FILE_HEADER == null || _FILE_HEADER == "") { GetComponent<Utility>().StartCoroutine("LoadSceneCoroutine", "TitleScene"); }
        objName = GameObject.Find("CharacterName").gameObject as GameObject;
        for (int i = 0; i < 5; i++) { objCharacter[i] = GameObject.Find("Chara" + (i + 1).ToString()).gameObject as GameObject; objCharacter[i].gameObject.SetActive(false); }
        objText = GameObject.Find("MainText").gameObject as GameObject;
        objTextBox = GameObject.Find("TextBox").gameObject as GameObject;
        objBackImage = GameObject.Find("BackImage").gameObject as GameObject;
        objBackText = GameObject.Find("BackText").gameObject as GameObject; objBackText.gameObject.SetActive(false);
        objBGM = GameObject.Find("BGMManager").gameObject as GameObject;
        ReadCommandFileNum("[system]commandFileNum[system].txt");
        commandName = "[system]command1" + objBGM.GetComponent<BGMManager>().chapterName;
        titleText.GetComponent<Text>().text = "[コマンド]command1" + "\n" + objBGM.GetComponent<BGMManager>().chapterName.Substring(0, objBGM.GetComponent<BGMManager>().chapterName.Length - 4).Replace("[system]", "[イベント]");
        StartScene();
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

// Update is called once per frame
void Update()
    {
        bool textFlag = false;
        if (time % 36000 == 0) { File.Copy(PlayerPrefs.GetString("進行中シナリオ", ""), "BackUp.zip", true); }
        time++;
        for (int x = 0; x < inputs.Length; x++) { if (inputs[x].GetComponent<InputField>().isFocused) { textFlag = true; } }
        if (textFlag==false && (Input.GetKey(KeyCode.LeftControl) || Input.GetKey(KeyCode.RightControl)))
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
        if (textFlag==false && (Input.GetKey(KeyCode.LeftControl) || Input.GetKey(KeyCode.RightControl)))
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

    public void CopyButton()
    {
        string str="";
        if (selectNum < 0)
        {
            GameObject.Find("Error").GetComponent<Text>().text = "コマンドを選択してください。";
            StartCoroutine(ErrorWait());
            return;
        }
        bool[] gF = new bool[scenarioGraphic.Length - 1];
        bool[] sF = new bool[scenarioAudio.Length];
        if (multiSelect.Count == 0)
        {
            str = commandData[selectNum].Replace("\r","").Replace("\n","") + "\r\n";
        }
        else if (multiSelect[0] > selectNum)
        {
            str = str + commandData[selectNum].Replace("\r", "").Replace("\n", "") + "\r\n";
            for (int i = multiSelect.Count-1; i>=0; i--) { str = str + commandData[multiSelect[i]].Replace("\r", "").Replace("\n", "") + "\r\n"; }
        }
        else if (multiSelect[0] < selectNum)
        {
            for (int i = 0; i <multiSelect.Count; i++) { str = str + commandData[multiSelect[i]].Replace("\r", "").Replace("\n", "") + "\r\n"; }
            str = str + commandData[selectNum].Replace("\r", "").Replace("\n", "") + "\r\n";
        }
        //strの最後の\r\nはいらない
        str = str.Substring(0, str.Length - 2);
        str = str.Replace(objBGM.GetComponent<BGMManager>().chapterName,"[system]元ファイル名");

        //コピーされたコマンドが参照する画像サウンドファイルの番号を取得
        str = "\n" + str;
        for (int i = 0; i < scenarioGraphic.Length - 1; i++)
        {
            if (str.Contains("\nBack:" + i.ToString()) || str.Contains("\nChara:" + i.ToString()) || str.Contains("\nItem:" + i.ToString()) || str.Contains("\nBattle:" + i.ToString())) { gF[i] = true; }
        }
        for (int i = 0; i < scenarioAudio.Length; i++)
        {
            if (str.Contains("\nBGM:" + i.ToString()) || str.Contains("\nSE:" + i.ToString())) { sF[i] = true; }
        }
        str = str.Substring(1);

        //コピーされたコマンドが参照する画像サウンドファイルの名前を取得。画像サウンドファイル自体も一時保存。
        for (int i = 0; i < gF.Length; i++) { if (gF[i] == true) { objBGM.GetComponent<BGMManager>().gFileName[i] = gFileName[i]; objBGM.GetComponent<BGMManager>().scenarioGraphic[i]=scenarioGraphic[i]; } }
        for (int i = 0; i < sF.Length; i++) { if (sF[i] == true) { objBGM.GetComponent<BGMManager>().sFileName[i] = sFileName[i]; objBGM.GetComponent<BGMManager>().scenarioAudio[i] = scenarioAudio[i]; } }

        objBGM.GetComponent<BGMManager>().copyString=str;
    }

    public void PasteButton()
    {
        string str="";
        if (selectNum < 0)
        {
            GameObject.Find("Error").GetComponent<Text>().text = "貼り付け先（そのコマンドの後ろに挿入されます）が選択されていません。";
            StartCoroutine(ErrorWait());
            return;
        }
        bool[] gF = new bool[scenarioGraphic.Length - 1];
        bool[] sF = new bool[scenarioAudio.Length];
        if (objBGM.GetComponent<BGMManager>().copyString == "")
        {
            GameObject.Find("Error").GetComponent<Text>().text = "先にコピー元を選んでください。";
            StartCoroutine(ErrorWait());
            return;
        }
        List<string> strList = new List<string>();
        strList.AddRange(undoList[undoListNum].Replace("\r", "").Split('\n'));


        //画像・サウンドファイルの番号はイベントによって違うので、名前から参照して番号をつけなおす。
        string tmp;
        tmp = objBGM.GetComponent<BGMManager>().copyString.Replace("[system]元ファイル名", objBGM.GetComponent<BGMManager>().chapterName).Replace("\r", "");
        //コピーされたコマンドが参照する画像サウンドファイルの番号を取得
        tmp = "\n" + tmp;
        for (int i = 0; i < scenarioGraphic.Length - 1; i++)
        {
            if (tmp.Contains("\nBack:" + i.ToString() + "\r\n") || tmp.Contains("\nBack:" + i.ToString() + "\n") || tmp.Contains("\nChara:" + i.ToString() + ",") || tmp.Contains("\nItem:" + i.ToString() + "\r\n") || tmp.Contains("\nItem:" + i.ToString() + "\n") || tmp.Contains("\nBattle:" + i.ToString() + ",")) { gF[i] = true; }
        }
        for (int i = 0; i < scenarioAudio.Length; i++)
        {
            if (tmp.Contains("\nBGM:" + i.ToString() + ",") || tmp.Contains("\nSE:" + i.ToString() + "\r\n") || tmp.Contains("\nSE:" + i.ToString() + "\n")) { sF[i] = true; }
        }
        for (int i = 0; i < gF.Length; i++)
        {
            if (gF[i] == true)
            {
                for (int j = 0; j < gFileName.Length; j++)
                {
                    if (Path.GetFileName(objBGM.GetComponent<BGMManager>().gFileName[i]) == Path.GetFileName(gFileName[j]))
                    {
                        gF[i] = false;
                        tmp=tmp.Replace("\nBack:" + i.ToString() + "\r\n", "\nBackTemp:" + j.ToString() + "\r\n").Replace("\nBack:" + i.ToString() + "\n", "\nBackTemp:" + j.ToString() + "\n").Replace("\nChara:" + i.ToString() + ",", "\nCharaTemp:" + j.ToString() + ",").Replace("\nItem:" + i.ToString() + "\r\n", "\nItemTemp:" + j.ToString() + "\r\n").Replace("\nItem:" + i.ToString() + "\n", "\nItemTemp:" + j.ToString() + "\n").Replace("\nBattle:" + i.ToString() + ",", "\nBattleTemp:" + j.ToString() + ",");
                    }
                }
                if (gF[i] == true)
                {
                    int j;
                    for (j = 0; j < gFileName.Length; j++) { if (scenarioGraphic[j] == null) { scenarioGraphic[j]=objBGM.GetComponent<BGMManager>().scenarioGraphic[i];gFileName[j] = objBGM.GetComponent<BGMManager>().gFileName[i]; break; } }
                    if (j == gFileName.Length)
                    {
                        GameObject.Find("Error").GetComponent<Text>().text = "<size=20>そのままでは貼りつけ後の画像ファイル数が100以上になるので、貼りつけたコマンドの選択画像を差し替えました。</size>";
                        StartCoroutine(ErrorWait());
                        j = 0;
                    }
                    tmp=tmp.Replace("\nBack:" + i.ToString() + "\r\n", "\nBackTemp:" + j.ToString() + "\r\n").Replace("\nBack:" + i.ToString() + "\n", "\nBackTemp:" + j.ToString() + "\n").Replace("\nChara:" + i.ToString() + ",", "\nCharaTemp:" + j.ToString() + ",").Replace("\nItem:" + i.ToString() +"\r\n", "\nItemTemp:" + j.ToString() + "\r\n").Replace("\nItem:" + i.ToString() + "\n", "\nItemTemp:" + j.ToString() + "\n").Replace("\nBattle:" + i.ToString() + ",", "\nBattleTemp:" + j.ToString() + ",");
                }
            }
        }//画像番号(i)をjに置き換える。
        for (int i = 0; i < sF.Length; i++)
        {
            if (sF[i] == true)
            {
                for (int j = 0; j < sFileName.Length; j++)
                {
                    if (Path.GetFileName(objBGM.GetComponent<BGMManager>().sFileName[i]) == Path.GetFileName(sFileName[j]))
                    {
                        sF[i] = false;
                        tmp=tmp.Replace("\nBGM:" + i.ToString() + ",", "\nBGMTemp:" + j.ToString() + ",").Replace("\nSE:" + i.ToString() +"\r\n", "\nSETemp:" + j.ToString() + "\r\n").Replace("\nSE:" + i.ToString() + "\n", "\nSETemp:" + j.ToString() + "\n");
                    }
                }
                if (sF[i] == true)
                {
                    int j;
                    for (j = 0; j < sFileName.Length; j++) { if (scenarioAudio[j] == null) { scenarioAudio[j] = objBGM.GetComponent<BGMManager>().scenarioAudio[i]; sFileName[j] = objBGM.GetComponent<BGMManager>().sFileName[i]; break; } }
                    if (j == sFileName.Length)
                    {
                        GameObject.Find("Error").GetComponent<Text>().text = "<size=20>そのままでは貼りつけ後のサウンドファイル数が41以上になるので、貼りつけたコマンドの選択音声を差し替えました。</size>";
                        StartCoroutine(ErrorWait());
                        j = 0;
                    }
                    tmp=tmp.Replace("\nBGM:" + i.ToString() +",", "\nBGMTemp:" + j.ToString() + ",").Replace("\nSE:" + i.ToString() + "\r\n", "\nSETemp:" + j.ToString() + "\r\n").Replace("\nSE:" + i.ToString() + "\n", "\nSETemp:" + j.ToString() + "\n");
                }
            }
        }//サウンド番号(i)をjに置き換える。
        tmp=tmp.Replace("\nBackTemp:", "\nBack:").Replace("\nCharaTemp:", "\nChara:").Replace("\nItemTemp:", "\nItem:").Replace("\nBattleTemp:", "\nBattle:").Replace("\nBGMTemp:", "\nBGM:").Replace("\nSETemp:", "\nSE:");//コマンドの名前を戻す（一度変換してるのは番号差替え後のコマンドを再度差替えることにならないように）
        tmp = tmp.Substring(1);

        List<string> tmpList = new List<string>();
        tmpList.AddRange(tmp.Split('\n'));
        strList.InsertRange(selectNum+1, tmpList);

        if (strList.Count > 90) //undoList(strList)の段階では最後に空白行があるので90はセーフ。
        {
            GameObject.Find("Error").GetComponent<Text>().text = "貼りつけ後のコマンド数が90以上になるので貼りつけられません。";
            StartCoroutine(ErrorWait());
            return;
        }

        commandData.Clear();
        for (int i = 0; i < objCB.Count; i++) { Destroy(objCB[i]); }
        objCB.Clear();
        // 読み込んだ目次テキストファイルからstring配列を作成する
        commandData.AddRange(strList);
        commandData.RemoveAt(commandData.Count - 1);//最後の行は空白なので消す
                                                    //コマンドをボタンとして一覧に放り込む。
        for (int i = 0; i < commandData.Count; i++)
        {
            if (i < 90)//90以降は全部タイトル戻しで埋めるのでボタン表示しない
            {
                objCB.Add(Instantiate(objCommand) as GameObject);
                objCB[i].transform.SetParent(parentObject.transform, false);
                objCB[i].transform.Find("Text").GetComponent<Text>().text = commandData[i].Replace("</size>", "");
                objCB[i].GetComponent<CommandButton>().buttonNum = i;

                //分岐コマンドの場合は分岐先表示を出す
                if (commandData[i].Length > 7 && commandData[i].Substring(0, 7) == "Select:") { NextSkipMake(10, i); }
                else if (commandData[i].Length > 7 && commandData[i].Substring(0, 7) == "Hantei:") { NextSkipMake(11, i); }
                else if (commandData[i].Length > 7 && commandData[i].Substring(0, 7) == "Battle:") { NextSkipMake(12, i); }
                else if (commandData[i].Length > 11 && commandData[i].Substring(0, 11) == "FlagBranch:") { NextSkipMake(13, i); }
                else if (commandData[i].Length > 11 && commandData[i].Substring(0, 11) == "Difference:") { NextSkipMake(17, i); }
                else if (commandData[i].Length > 6 && commandData[i].Substring(0, 6) == "Equal:") { NextSkipMake(20, i); }
                else if (commandData[i].Length > 9 && commandData[i].Substring(0, 9) == "SANCheck:") { NextSkipMake(28, i); }
                else { NextSkipMake(0, i); }
            }
        }
        for (int i = 0; i < commandData.Count; i++) { str = str + commandData[i].Replace("\r","").Replace("\n","") + "\r\n"; }
        undoList.Add(str);
        undoListNum = undoList.Count - 1;
        selectNum = -1;
        multiSelect.Clear();
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
            commandData.Clear();
            for (int i = 0; i < objCB.Count; i++) { Destroy(objCB[i]); }
            objCB.Clear();

            // 読み込んだ目次テキストファイルからstring配列を作成する
            commandData.AddRange(str.Split('\n'));
            commandData.RemoveAt(commandData.Count - 1);//最後の行は空白なので消す
                                    //コマンドをボタンとして一覧に放り込む。
            for (int i = 0; i < commandData.Count; i++)
            {
                if (i < 90)//90以降は全部タイトル戻しで埋めるのでボタン表示しない
                {
                    objCB.Add(Instantiate(objCommand) as GameObject);
                    objCB[i].transform.SetParent(parentObject.transform, false);
                    objCB[i].transform.Find("Text").GetComponent<Text>().text = commandData[i].Replace("</size>", "");
                    objCB[i].GetComponent<CommandButton>().buttonNum = i;

                    //分岐コマンドの場合は分岐先表示を出す
                    if (commandData[i].Length > 7 && commandData[i].Substring(0, 7) == "Select:") { NextSkipMake(10, i); }
                    else if (commandData[i].Length > 7 && commandData[i].Substring(0, 7) == "Hantei:") { NextSkipMake(11, i); }
                    else if (commandData[i].Length > 7 && commandData[i].Substring(0, 7) == "Battle:") { NextSkipMake(12, i); }
                    else if (commandData[i].Length > 11 && commandData[i].Substring(0, 11) == "FlagBranch:") { NextSkipMake(13, i); }
                    else if (commandData[i].Length > 11 && commandData[i].Substring(0, 11) == "Difference:") { NextSkipMake(17, i); }
                    else if (commandData[i].Length > 6 && commandData[i].Substring(0, 6) == "Equal:") { NextSkipMake(20, i); }
                    else if (commandData[i].Length > 9 && commandData[i].Substring(0, 9) == "SANCheck:") { NextSkipMake(28, i); }
                    else { NextSkipMake(0, i); }
                }
            }
            selectNum = -1;
            multiSelect.Clear();
        }
        catch
        {
            if (undoFlag == true) { GameObject.Find("Error").GetComponent<Text>().text = "これ以上戻れません。"; }
            if (undoFlag == false) { GameObject.Find("Error").GetComponent<Text>().text = "これ以上進めません。"; }
            StartCoroutine(ErrorWait());
        }
    }

    public void NameChangeButton()
    {
        GameObject.Find("CNameField").GetComponent<RectTransform>().localPosition = new Vector2(0,50);
        GameObject.Find("CNameField").GetComponent<InputField>().text = commandName.Replace(objBGM.GetComponent<BGMManager>().chapterName, "").Replace("[system]", "");
        GameObject.Find("INameField").GetComponent<InputField>().text = objBGM.GetComponent<BGMManager>().chapterName.Substring(0, objBGM.GetComponent<BGMManager>().chapterName.Length - 4).Replace("[system]", "");
        GameObject.Find("InputZone").GetComponent<RectTransform>().localPosition = new Vector2(0, -500);
    }

    public void NameCancelButton()
    {
        GameObject.Find("CNameField").GetComponent<RectTransform>().localPosition = new Vector2(-800, 50);
        GameObject.Find("InputZone").GetComponent<RectTransform>().localPosition = new Vector2(0, -200);
    }

    public void NameDecideButton()
    {
        if (NCBFlag == false)
        {
            NCBFlag = true;
            StartCoroutine(NCBIE());
        }
    }

    private IEnumerator NCBIE()
    {
        int inum = 0;
        string tmp1, tmp2, tmp3;
        List<string> tmpList = new List<string>();
        tmp1 = "[system]" + GameObject.Find("INameField").GetComponent<InputField>().text + ".txt";
        tmp2 = "[system]" + GameObject.Find("CNameField").GetComponent<InputField>().text + objBGM.GetComponent<BGMManager>().chapterName;
        tmp3 = "[system]" + GameObject.Find("CNameField").GetComponent<InputField>().text + tmp1;
        if (commandName.Contains("[system]command1") && commandName != tmp2)
        {
            GameObject.Find("InputZone").GetComponent<RectTransform>().localPosition = new Vector2(0, -200);
            GameObject.Find("CNameField").GetComponent<RectTransform>().localPosition = new Vector2(-800, 50);
            GameObject.Find("Error").GetComponent<Text>().text = "イベント開始時のコマンドファイル名は変更不可です。";
            NCBFlag = false;
            StartCoroutine(ErrorWait());
            yield break;
        }
        if (GameObject.Find("INameField").GetComponent<InputField>().text.Contains("[system]") || GameObject.Find("CNameField").GetComponent<InputField>().text.Contains("[system]"))
        {
            GameObject.Find("InputZone").GetComponent<RectTransform>().localPosition = new Vector2(0, -200);
            GameObject.Find("CNameField").GetComponent<RectTransform>().localPosition = new Vector2(-800, 50);
            GameObject.Find("Error").GetComponent<Text>().text = "「<color=red>[system]</color>」という文字列は使用禁止です。(システム処理の識別語にしています)";
            NCBFlag = false;
            StartCoroutine(ErrorWait());
            yield break;
        }

        //全てのコマンドファイル、イベントファイル、マップデータを開き、コマンド名（[system]～～××.txt）をtmp1に、イベント名（××.txt）をtmp2に変換する。

        //ZipFileオブジェクトの作成
        ICSharpCode.SharpZipLib.Zip.ZipFile zf =
            new ICSharpCode.SharpZipLib.Zip.ZipFile(PlayerPrefs.GetString("進行中シナリオ", ""));
        zf.Password = Secret.SecretString.zipPass;
        foreach (ICSharpCode.SharpZipLib.Zip.ZipEntry ze in zf)
        {
            if ((ze.Name != objBGM.GetComponent<BGMManager>().chapterName && ze.Name == tmp1) || (ze.Name != commandName && ze.Name == tmp3))
            {
                GameObject.Find("Error").GetComponent<Text>().text = "他のファイルと名前が重複しています。上書きしますか？";
                GameObject.Find("ErrorButton").GetComponent<RectTransform>().localPosition = new Vector2(0, -70);
                errorFlag = 0;
                while (errorFlag==0) { yield return null; }
                if (errorFlag == 1) { GameObject.Find("Error").GetComponent<Text>().text = ""; GameObject.Find("ErrorButton").GetComponent<RectTransform>().localPosition = new Vector2(0, -470); break; }//OKならそのまま処理
                if (errorFlag == -1)
                {
                    //閉じる
                    zf.Close();
                    GameObject.Find("InputZone").GetComponent<RectTransform>().localPosition = new Vector2(0, -200);
                    GameObject.Find("CNameField").GetComponent<RectTransform>().localPosition = new Vector2(-800, 50);
                    GameObject.Find("Error").GetComponent<Text>().text = "";
                    GameObject.Find("ErrorButton").GetComponent<RectTransform>().localPosition = new Vector2(0, -470);
                    NCBFlag = false;
                    yield break;
                }//NGならキャンセル処理
            }
        }
        //ZipFileの更新を開始
        zf.BeginUpdate();
        //展開するエントリを探す
        foreach (ICSharpCode.SharpZipLib.Zip.ZipEntry ze in zf)
        {
            if (ze.Name.Substring(ze.Name.Length - 4) == ".txt" && !tmpList.Contains(ze.Name))
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
                text = text.Replace(commandName, tmp2);
                text = text.Replace(objBGM.GetComponent<BGMManager>().chapterName, tmp1);
                if (text2 == text) { continue; }
                StreamWriter sw = new StreamWriter(@GetComponent<Utility>().GetAppPath() + @"\" + "tmp" + inum.ToString() + ".txt", false, System.Text.Encoding.GetEncoding("UTF-8"));
                //TextBox1.Textの内容を書き込む
                sw.Write(text);
                //閉じる
                sw.Close();

                //ファイル名自体も置換
                string tmpName;
                tmpName = ze.Name;
                tmpName = tmpName.Replace(commandName, tmp2);
                tmpName = tmpName.Replace(objBGM.GetComponent<BGMManager>().chapterName, tmp1);
                zf.Delete(ze.Name);
                zf.Add("tmp" + inum.ToString() + ".txt", tmpName);
                inum++;
                tmpList.Add(tmpName);
            }
        }
        //ZipFileの更新をコミット
        zf.CommitUpdate();

        //閉じる
        zf.Close();
        for (int i = 0; i < inum; i++) { File.Delete(@GetComponent<Utility>().GetAppPath() + @"\" + "tmp" + i.ToString() + ".txt"); }
        for (int i = 0; i < commandData.Count; i++) { commandData[i] = commandData[i].Replace(commandName, tmp2).Replace(objBGM.GetComponent<BGMManager>().chapterName, tmp1); }
        for (int i = 0; i < objCB.Count; i++) { objCB[i].transform.Find("Text").GetComponent<Text>().text = commandData[i]; }
        for (int i = 0; i < backFileLog.Count; i++) { backFileLog[i] = backFileLog[i].Replace(commandName, tmp2).Replace(objBGM.GetComponent<BGMManager>().chapterName, tmp1); }
        for (int i = 0; i < undoList.Count; i++) { undoList[i] = undoList[i].Replace(commandName, tmp2).Replace(objBGM.GetComponent<BGMManager>().chapterName, tmp1); }
        objBGM.GetComponent<BGMManager>().chapterName = tmp1;
        commandName = tmp3;
        GameObject.Find("CNameField").GetComponent<RectTransform>().localPosition = new Vector2(-800, 50);
        titleText.GetComponent<Text>().text = tmp3.Replace(tmp1, "").Replace("[system]", "[コマンド]") + "\n" + tmp1.Replace("[system]", "[イベント]");
        GameObject.Find("InputZone").GetComponent<RectTransform>().localPosition = new Vector2(0, -200);
        NCBFlag = false;
    }

    public void ErrorOKButton()
    {
        errorFlag = 1;
    }
    public void ErrorNGButton()
    {
        errorFlag = -1;
    }

    private void SceneGraphic()
    {
        string[] separate;
        BackTextDraw(" ");
        TextDraw(" ", " ");
        for (int i=1;i<6;i++) { CharacterDraw(-1, i); }
        try { BackDraw(backGraphLog[backGraphLog.Count - 1][0]); backGraphLogTemp[0] = backGraphLog[backGraphLog.Count - 1][0];} catch { }
        try { for (int i = 1; i <= 5; i++) { CharacterDraw(backGraphLog[backGraphLog.Count - 1][i], i); backGraphLogTemp[i] = backGraphLog[backGraphLog.Count - 1][i]; } } catch { }
        try { if (backBTLog[backBTLog.Count - 1] != "") { BackTextDraw(backBTLog[backBTLog.Count - 1]); backBTLogTemp = backBTLog[backBTLog.Count - 1]; } } catch { }
        try { if (backTLog[backTLog.Count - 1][0] != "" || backTLog[backTLog.Count - 1][1] != "") { TextDraw(backTLog[backTLog.Count - 1][0], backTLog[backTLog.Count - 1][1]); backTLogTemp = backTLog[backTLog.Count - 1]; } } catch { }

        for (int i = 0; i <= selectNum; i++)
        {
            objBackText.gameObject.SetActive(false);
            separate = commandData[i].Split(','); 
            if (separate[0].Length > 5 && separate[0].Substring(0, 5) == "Text:") { TextDraw(separate[0].Substring(5), separate[1].Replace("\r","").Replace("\n",""));backTLogTemp[0] = separate[0].Substring(5);backTLogTemp[1] = separate[1].Replace("\r", "").Replace("\n", ""); backBTLogTemp= ""; }
            if (separate[0].Length > 9 && separate[0].Substring(0, 9) == "BackText:") { BackTextDraw(separate[0].Substring(9).Replace("\r", "").Replace("\n", ""));backBTLogTemp = separate[0].Substring(9).Replace("\r", "").Replace("\n", "");backTLogTemp[0] = ""; backTLogTemp[1] = ""; }
            if (separate[0].Length > 6 && separate[0].Substring(0, 6) == "Chara:") { CharacterDraw(int.Parse(separate[0].Substring(6)),int.Parse(separate[1].Replace("\r", "").Replace("\n", ""))); backGraphLogTemp[int.Parse(separate[1])] = int.Parse(separate[0].Substring(6)); }
            if (separate[0].Length > 5 && separate[0].Substring(0, 5) == "Back:") { BackDraw(int.Parse(separate[0].Substring(5).Replace("\r", "").Replace("\n", ""))); backGraphLogTemp[0] = int.Parse(separate[0].Substring(5).Replace("\r", "").Replace("\n", "")); }
            if (separate[0].Length > 7 && separate[0].Substring(0, 7) == "Battle:") { for (int j = 1; j <= 5; j++) { CharacterDraw(-1, j); backGraphLogTemp[j] = -1; } }
        }
    }




    public void SelectReset()
    {
        selectNum = -1;
        try { objCCB.GetComponent<Image>().color = new Color(1.0f, 1.0f, 1.0f); } catch { }
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
        try
        {
            string str = commandFileNum.ToString();
            //ZIP書庫のパス
            string zipPath = PlayerPrefs.GetString("進行中シナリオ", "");
            //書庫に追加するファイルのパス
            string file = @GetComponent<Utility>().GetAppPath() + @"\" + "[system]commandFileNum[system].txt";

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
        catch { }
    }


    public void CommandButton(int num)
    {
        if (selectNum == -1)
        {
            GameObject.Find("Error").GetComponent<Text>().text = "コマンドが選択されていません。";
            AudioSource bgm = GameObject.Find("BGMManager").GetComponent<AudioSource>(); bgm.loop = false; bgm.clip = errorSE; bgm.Play();
            StartCoroutine(ErrorWait());
            return;
        }
        foreach (GameObject tempObject in objGS) { Destroy(tempObject); }
        objGS.Clear();
        for (int i = 0; i < objMake.Length; i++) { objMake[i].SetActive(false); }
        objMake[num].SetActive(true);
        selectGS = -1;
        parentGS = GameObject.Find("GSContents");
        if (num == 24)
        {
            GameObject.Find("InputFieldText (2)").GetComponent<InputField>().text = "";
        }
        if (num == 2 || num==6 || num==7 || num==12)
        {
            for (int i = 0; i < scenarioGraphic.Length-1; i++)
            {
                if (scenarioGraphic[i] == null) { continue; }
                objGS.Insert(objGS.Count, Instantiate(objGraSou) as GameObject);
                objGS[objGS.Count - 1].transform.SetParent(parentGS.transform, false);
                objGS[objGS.Count-1].name ="GS" + i.ToString();
                objGS[objGS.Count - 1].GetComponent<GSButton>().buttonNum = i;
                objGS[objGS.Count - 1].GetComponent<Image>().sprite= scenarioGraphic[i];
                objGS[objGS.Count - 1].GetComponentInChildren<Text>().text = Path.GetFileName(gFileName[i]);
            }
            if (num == 6)
            {
                objGS.Insert(objGS.Count, Instantiate(objGraSou) as GameObject);
                objGS[objGS.Count - 1].transform.SetParent(parentGS.transform, false);
                objGS[objGS.Count - 1].name = "GS99";
                objGS[objGS.Count - 1].GetComponent<Image>().sprite = scenarioGraphic[99];
                objGS[objGS.Count - 1].GetComponent<GSButton>().buttonNum = 99;
                objGS[objGS.Count - 1].GetComponentInChildren<Text>().text = "＜プレイヤーキャラ＞";

                objGS.Insert(objGS.Count, Instantiate(objGraSou) as GameObject);
                objGS[objGS.Count - 1].transform.SetParent(parentGS.transform, false);
                objGS[objGS.Count - 1].name = "GS-1";
                objGS[objGS.Count - 1].GetComponent<Image>().sprite = batten;
                objGS[objGS.Count - 1].GetComponent<GSButton>().buttonNum = - 1;
                objGS[objGS.Count - 1].GetComponentInChildren<Text>().text = "＜出ている立ち絵を消す＞";



            }//キャラ選択の場合、表示しないを選択したいことがある。それを×マークでフォロー。自キャラを表示する場合も同様に。
        }
        if (num == 3 || num==5)
        {
            for (int i = 0; i < scenarioAudio.Length; i++)
            {
                if (scenarioAudio[i] == null) { continue; }
                objGS.Insert(objGS.Count, Instantiate(objGraSou) as GameObject);
                objGS[objGS.Count - 1].transform.SetParent(parentGS.transform, false);
                objGS[objGS.Count - 1].name = "GS" + i.ToString();
                objGS[objGS.Count - 1].GetComponent<GSButton>().buttonNum = i;
                objGS[objGS.Count - 1].GetComponentInChildren<Text>().text = Path.GetFileName(sFileName[i]);
            }
        }

        //分岐コマンドの場合は分岐先表示を出す
        try//※コマンド選択せずにコマンド種別ボタンを押した時に無視できるようにtry入れておく
        {
            NextSkipMake(num, selectNum);
        }
        catch { }
    }

    public void NextSkipMake(int kindNum,int objCBNum)
    {
        string[] strs;
        string str="";
        char[] chars = { ':', ',' };
        if (kindNum == 10) { strs=commandData[objCBNum].Replace("\r","").Replace("\n","").Split(chars);if (strs[1]!="") { str = "１→　　\r\n"; }if (strs[2] != "") {str = "１→　　\r\n２→　　\r\n"; } if (strs[3] != "") { str = "１→　　\r\n２→　　\r\n３→　　\r\n"; } if (strs[4] != "") { str = "１→　　\r\n２→　　\r\n３→　　\r\n４→　　\r\n"; } objCB[objCBNum].transform.Find("NextSkip").GetComponent<Text>().text = "↓　　<color=white>" + (objCBNum + 1).ToString() + "</color>\r\n" + str; }
        else if (kindNum == 11) { objCB[objCBNum].transform.Find("NextSkip").GetComponent<Text>().text = "↓　　<color=white>" + (objCBNum + 1).ToString() + "</color>\r\nSP→　　\r\n成功→　　\r\n失敗→　　"; }
        else if (kindNum == 12) { objCB[objCBNum].transform.Find("NextSkip").GetComponent<Text>().text = "↓　　<color=white>" + (objCBNum + 1).ToString() + "</color>\r\n技能→　\r\n勝(殺)→　\r\n勝(両)→　\r\n勝(生)→　\r\n負(生)→　\r\n負(死)→　"; }
        else if (kindNum == 13) { objCB[objCBNum].transform.Find("NextSkip").GetComponent<Text>().text = "↓　　<color=white>" + (objCBNum + 1).ToString() + "</color>\r\nOn→　　\r\nOff→　　"; }
        else if (kindNum == 17) { objCB[objCBNum].transform.Find("NextSkip").GetComponent<Text>().text = "↓　　<color=white>" + (objCBNum + 1).ToString() + "</color>\r\n１→　　\r\n２→　　"; }
        else if (kindNum == 20) { objCB[objCBNum].transform.Find("NextSkip").GetComponent<Text>().text = "↓　　<color=white>" + (objCBNum + 1).ToString() + "</color>\r\n有→　　\r\n無→　　"; }
        else if (kindNum == 28) { objCB[objCBNum].transform.Find("NextSkip").GetComponent<Text>().text = "↓　　<color=white>" + (objCBNum + 1).ToString() + "</color>\r\nセーフ→　　\r\n発狂→　　"; }
        else { objCB[objCBNum].transform.Find("NextSkip").GetComponent<Text>().text = "<color=white>" + (objCBNum + 1).ToString() + "</color>"; }
    }

    //エクスプローラーのドラッグ＆ドロップ機能は使わない。
    //Graphic,Soundフォルダを作り、そこにファイルを投入してもらう。

    public void CommandDecide(int num)
    {
        string str2;
        string commandText="";
        System.Text.RegularExpressions.Regex r =
    new System.Text.RegularExpressions.Regex(
        "[\\x00-\\x1f<>:\"/\\\\|?*,]" +
        "|^(CON|PRN|AUX|NUL|COM[0-9]|LPT[0-9]|CLOCK\\$)(\\.|$)" +
        "|[\\. ]$",
    System.Text.RegularExpressions.RegexOptions.IgnoreCase);
        if (num == 0) { if (GameObject.Find("InputFieldText").GetComponent<InputField>().text == "") { GameObject.Find("InputFieldText").GetComponent<InputField>().text = "　"; } if (GameObject.Find("InputFieldName").GetComponent<InputField>().text == "") { GameObject.Find("InputFieldName").GetComponent<InputField>().text = "　"; } commandText = "Text:" + GameObject.Find("InputFieldName").GetComponent<InputField>().text.Replace(",","，").Replace(":", "：").Replace(" ", " ") + "," + GameObject.Find("InputFieldText").GetComponent<InputField>().text.Replace(",", "，").Replace(":", "：").Replace(" ", " "); if (GameObject.Find("Toggle").GetComponent<Toggle>().isOn == false) { commandText = commandText + ",false"; } else { commandText = commandText + ",true"; } }
        if (num == 1) { if (GameObject.Find("InputFieldText").GetComponent<InputField>().text == "") { GameObject.Find("InputFieldText").GetComponent<InputField>().text = "　"; } commandText = "BackText:" + GameObject.Find("InputFieldText").GetComponent<InputField>().text.Replace(",", "，").Replace(":", "：").Replace(" ", " "); if (GameObject.Find("Toggle").GetComponent<Toggle>().isOn == false) { commandText = commandText + ",false"; } else { commandText = commandText + ",true"; } }
        if (num == 2) { if (selectGS == -1) { GameObject.Find("Error").GetComponent<Text>().text = "画像を選択してください。"; StartCoroutine(ErrorWait()); return; } commandText = "Back:" + selectGS.ToString(); }
        if (num == 3) { if (selectGS == -1) { GameObject.Find("Error").GetComponent<Text>().text = "ファイルを選択してください。"; StartCoroutine(ErrorWait()); return; } if (GameObject.Find("InputFieldName").GetComponent<InputField>().text == "") { GameObject.Find("InputFieldName").GetComponent<InputField>().text = "0"; } commandText = "BGM:" + selectGS.ToString() + "," + GameObject.Find("InputFieldName").GetComponent<InputField>().text;}
        if (num == 4) { if (GameObject.Find("InputFieldName").GetComponent<InputField>().text == "") { GameObject.Find("InputFieldName").GetComponent<InputField>().text = "0"; } commandText = "BGMStop:" + GameObject.Find("InputFieldName").GetComponent<InputField>().text; }
        if (num == 5) { if (selectGS == -1) { GameObject.Find("Error").GetComponent<Text>().text = "ファイルを選択してください。"; StartCoroutine(ErrorWait()); return; } commandText = "SE:" + selectGS.ToString(); }
        if (num == 6) { string str=""; if(GameObject.Find("Slider2").GetComponent<Slider>().value==1){ str = "L"; } if (GameObject.Find("Slider2").GetComponent<Slider>().value == 2) { str = "N"; } if (GameObject.Find("Slider2").GetComponent<Slider>().value == 3) { str = "R"; } commandText = "Chara:" + selectGS.ToString() + "," + ((int)(GameObject.Find("Slider").GetComponent<Slider>().value)).ToString() + "," + str; }
        if (num == 7) { if (selectGS == -1) { GameObject.Find("Error").GetComponent<Text>().text = "画像を選択してください。"; StartCoroutine(ErrorWait()); return; } commandText = "Item:" + selectGS.ToString(); }
        if (num == 8) { commandText = "Shake:"; }
        if (num == 9) { commandText = "Jump:" + ((int)(GameObject.Find("Slider").GetComponent<Slider>().value)).ToString(); }
        if (num == 10) { commandText = "Select:" + GameObject.Find("InputFieldText").GetComponent<InputField>().text.Replace(",", "，").Replace(":", "：") + "," + GameObject.Find("InputFieldText (1)").GetComponent<InputField>().text.Replace(",", "，").Replace(":", "：") + "," + GameObject.Find("InputFieldText (2)").GetComponent<InputField>().text.Replace(",", "，").Replace(":", "：") + "," + GameObject.Find("InputFieldText (3)").GetComponent<InputField>().text.Replace(",", "，").Replace(":", "："); }
        if (num == 11)
        {
            string tmp="";
            string tmp2 = "";
            if (GameObject.Find("InputFieldText (4)").GetComponent<InputField>().text != "" && GameObject.Find("InputFieldText (2)").GetComponent<InputField>().text != "") { GameObject.Find("Error").GetComponent<Text>().text = "割算か掛算のどちらか一つだけを入力してください。"; StartCoroutine(ErrorWait()); return; }
            if (GameObject.Find("InputFieldText (2)").GetComponent<InputField>().text!="") { tmp = "*" + GameObject.Find("InputFieldText (2)").GetComponent<InputField>().text; }
            if (GameObject.Find("InputFieldText (4)").GetComponent<InputField>().text != "") { tmp = "/" + GameObject.Find("InputFieldText (4)").GetComponent<InputField>().text; }
            if (GameObject.Find("InputFieldText (3)").GetComponent<InputField>().text== "") { GameObject.Find("InputFieldText (3)").GetComponent<InputField>().text = "0"; }
            if (GameObject.Find("Label1").GetComponent<Text>().text != "フラグの値で判定する") { tmp2 = GameObject.Find("Label1").GetComponent<Text>().text; } else { tmp2 = GameObject.Find("InputFieldText (5)").GetComponent<InputField>().text; }
            commandText ="Hantei:" + tmp2 + tmp + "," + GameObject.Find("InputFieldText (3)").GetComponent<InputField>().text;
        }
        if (num == 12) { for (int i = 2; i < 5; i++) { if (GameObject.Find("InputFieldText (" + i.ToString() + ")").GetComponent<InputField>().text == "") { GameObject.Find("InputFieldText (" + i.ToString() + ")").GetComponent<InputField>().text = "0"; } }
             if (GameObject.Find("InputFieldText (5)").GetComponent<InputField>().text == "") { GameObject.Find("InputFieldText (5)").GetComponent<InputField>().text = "逃走"; }
            if (GameObject.Find("InputFieldText (7)").GetComponent<InputField>().text == "") { GameObject.Find("InputFieldText (7)").GetComponent<InputField>().text = "0";  }
            if (GameObject.Find("InputFieldText (8)").GetComponent<InputField>().text == "") { GameObject.Find("InputFieldText (8)").GetComponent<InputField>().text = "-1"; }
            if (selectGS == -1) { GameObject.Find("Error").GetComponent<Text>().text = "画像を選択してください。"; StartCoroutine(ErrorWait()); return; } commandText = "Battle:" + selectGS.ToString() + "," + GameObject.Find("Label1").GetComponent<Text>().text + "," + GameObject.Find("InputFieldText (2)").GetComponent<InputField>().text + "," + GameObject.Find("InputFieldText (3)").GetComponent<InputField>().text + "," + GameObject.Find("InputFieldText (4)").GetComponent<InputField>().text + "," + GameObject.Find("Label2").GetComponent<Text>().text + "," + GameObject.Find("Label3").GetComponent<Text>().text.Replace("D","") + "," + (GameObject.Find("Toggle1").GetComponent<Toggle>().isOn).ToString().ToLower() + "," + GameObject.Find("InputFieldText (5)").GetComponent<InputField>().text + "," + GameObject.Find("Label4").GetComponent<Text>().text + "," + GameObject.Find("InputFieldText (7)").GetComponent<InputField>().text + "," + GameObject.Find("InputFieldText (8)").GetComponent<InputField>().text + "," + (GameObject.Find("Toggle2").GetComponent<Toggle>().isOn).ToString().ToLower(); }
        if (num == 13) { if (GameObject.Find("InputFieldText (2)").GetComponent<InputField>().text == "") { GameObject.Find("Error").GetComponent<Text>().text = "必要項目が入力されていません。"; StartCoroutine(ErrorWait()); return; } if (GameObject.Find("InputFieldText (3)").GetComponent<InputField>().text == "") { GameObject.Find("InputFieldText (3)").GetComponent<InputField>().text ="1" ; } commandText ="FlagBranch:" + GameObject.Find("InputFieldText (2)").GetComponent<InputField>().text.Replace(",", "，").Replace(":", "：") + "," + GameObject.Find("InputFieldText (3)").GetComponent<InputField>().text; }
        if (num == 14)
        {
            if (GameObject.Find("InputFieldText (2)").GetComponent<InputField>().text == "" || (GameObject.Find("InputFieldText (3)").GetComponent<InputField>().text == "" && GameObject.Find("InputFieldText (4)").GetComponent<InputField>().text == "")) { GameObject.Find("Error").GetComponent<Text>().text = "必要項目が入力されていません。"; StartCoroutine(ErrorWait()); return; }
            if (GameObject.Find("InputFieldText (3)").GetComponent<InputField>().text != "") { GameObject.Find("InputFieldText (4)").GetComponent<InputField>().text = ""; }
            if (GameObject.Find("InputFieldText (2)").GetComponent<InputField>().text.Contains("[system]")) { GameObject.Find("Error").GetComponent<Text>().text = "「<color=red>[system]</color>」という文字列は使用禁止です。(システム処理の識別語にしています)"; StartCoroutine(ErrorWait()); return; } else { commandText = "FlagChange:" + GameObject.Find("InputFieldText (2)").GetComponent<InputField>().text.Replace(",", "，").Replace(":", "：") + "," + GameObject.Find("InputFieldText (3)").GetComponent<InputField>().text + "," + GameObject.Find("InputFieldText (4)").GetComponent<InputField>().text; }
        }
        if (num == 15) { commandText = "GetTime:"; }
        if (num == 16) { if (GameObject.Find("InputFieldText (2)").GetComponent<InputField>().text == "" || GameObject.Find("InputFieldText (3)").GetComponent<InputField>().text == "") { GameObject.Find("Error").GetComponent<Text>().text = "必要項目が入力されていません。"; StartCoroutine(ErrorWait()); return; } commandText = "FlagCopy:" + GameObject.Find("InputFieldText (2)").GetComponent<InputField>().text.Replace(",", "，").Replace(":", "：") + "," + GameObject.Find("InputFieldText (3)").GetComponent<InputField>().text.Replace(",", "，").Replace(":", "："); }
        if (num == 17) { commandText = "Difference:" + GameObject.Find("InputFieldText (2)").GetComponent<InputField>().text + "," + GameObject.Find("InputFieldText (3)").GetComponent<InputField>().text + "," + GameObject.Find("InputFieldText (4)").GetComponent<InputField>().text; }
        if (num == 18) { if (GameObject.Find("Label2").GetComponent<Text>().text != " " && GameObject.Find("Label3").GetComponent<Text>().text != " ") { commandText = "StatusChange:" + GameObject.Find("Label1").GetComponent<Text>().text + "," + GameObject.Find("Label2").GetComponent<Text>().text + GameObject.Find("Label3").GetComponent<Text>().text + "+" + GameObject.Find("InputFieldText").GetComponent<InputField>().text; } else { if (GameObject.Find("InputFieldText").GetComponent<InputField>().text != "") { commandText = "StatusChange:" + GameObject.Find("Label1").GetComponent<Text>().text + ",+" + GameObject.Find("InputFieldText").GetComponent<InputField>().text; } else { commandText = "StatusChange:" + GameObject.Find("Label1").GetComponent<Text>().text + ",+0"; } }}
        if (num == 19) { if (GameObject.Find("InputFieldText (2)").GetComponent<InputField>().text == "") { GameObject.Find("Error").GetComponent<Text>().text = "必要項目が入力されていません。"; StartCoroutine(ErrorWait()); return; } commandText = "Input:" + GameObject.Find("InputFieldText (2)").GetComponent<InputField>().text.Replace(",", "，").Replace(":", "："); }
        if (num == 20) { if (GameObject.Find("InputFieldText (2)").GetComponent<InputField>().text == "" || GameObject.Find("InputFieldText (3)").GetComponent<InputField>().text == "") { GameObject.Find("Error").GetComponent<Text>().text = "必要項目が入力されていません。"; StartCoroutine(ErrorWait()); return; } commandText = "Equal:" + GameObject.Find("InputFieldText (2)").GetComponent<InputField>().text.Replace(",", "，").Replace(":", "：") + "," + GameObject.Find("InputFieldText (3)").GetComponent<InputField>().text.Replace(",", "，").Replace(":", "："); }
        if (num == 21) { commandText = "Lost:"; }
        if (num == 22) { commandText = "Title:"; }
        if (num == 23) { if (GameObject.Find("Toggle1").GetComponent<Toggle>().isOn) { commandText = "Map:Everytime"; } else { commandText = "Map:Once"; } }
        if (num == 24)
        {
            if (GameObject.Find("InputFieldText (2)").GetComponent<InputField>().text == "") {commandFileNum++;SaveCommandFileNum(); commandText = "NextFile:" + "[system]NoName" + commandFileNum.ToString() + objBGM.GetComponent<BGMManager>().chapterName; }
            else
            {
                if (GameObject.Find("InputFieldText (2)").GetComponent<InputField>().text.Contains("[system]")) {GameObject.Find("Error").GetComponent<Text>().text = "「<color=red>[system]</color>」という文字列は使用禁止です。(システム処理の識別語にしています)";StartCoroutine(ErrorWait());return; }
                else
                {
                    if (r.IsMatch(GameObject.Find("InputFieldText (2)").GetComponent<InputField>().text)) { GameObject.Find("Error").GetComponent<Text>().text = "ファイル名に使えない文字、または,(半角コンマ)が入っています。"; StartCoroutine(ErrorWait()); return; }
                    else
                    {
                        commandText = "NextFile:" + "[system]" + GameObject.Find("InputFieldText (2)").GetComponent<InputField>().text + objBGM.GetComponent<BGMManager>().chapterName;
                    }
                }
            }
        }
        if (num == 25) { if (GameObject.Find("InputFieldText").GetComponent<InputField>().text == "" ){ GameObject.Find("InputFieldText").GetComponent<InputField>().text = "0"; }
            if (GameObject.Find("InputFieldText (1)").GetComponent<InputField>().text == "") { GameObject.Find("InputFieldText (1)").GetComponent<InputField>().text = "0"; }
            if (GameObject.Find("InputFieldText (2)").GetComponent<InputField>().text == "") { GameObject.Find("InputFieldText (2)").GetComponent<InputField>().text = "0"; }
            if (GameObject.Find("InputFieldText (3)").GetComponent<InputField>().text == "") { GameObject.Find("InputFieldText (3)").GetComponent<InputField>().text = "3"; }
            commandText = "BlackOut:" + GameObject.Find("InputFieldText").GetComponent<InputField>().text + "," + GameObject.Find("InputFieldText (1)").GetComponent<InputField>().text + "," + GameObject.Find("InputFieldText (2)").GetComponent<InputField>().text + "," + GameObject.Find("InputFieldText (3)").GetComponent<InputField>().text; }
        if (num == 26) { if (GameObject.Find("InputFieldText").GetComponent<InputField>().text == "") { GameObject.Find("InputFieldText").GetComponent<InputField>().text = "0"; } if (GameObject.Find("InputFieldText (1)").GetComponent<InputField>().text == "") { GameObject.Find("InputFieldText (1)").GetComponent<InputField>().text = "0"; } commandText = "PlaceChange:" + GameObject.Find("InputFieldText").GetComponent<InputField>().text + "," + GameObject.Find("InputFieldText (1)").GetComponent<InputField>().text; }
        if (num == 27) { if (GameObject.Find("InputFieldName").GetComponent<InputField>().text == "") { GameObject.Find("InputFieldName").GetComponent<InputField>().text = "0"; } commandText = "Wait:" + GameObject.Find("InputFieldName").GetComponent<InputField>().text; }
        if (num == 28)
        {
            string tmpstr1;
            string tmpstr2;
            if (GameObject.Find("Label0").GetComponent<Text>().text != " " && GameObject.Find("Label1").GetComponent<Text>().text != " ") { tmpstr1 = "SANCheck:" + GameObject.Find("Label0").GetComponent<Text>().text + GameObject.Find("Label1").GetComponent<Text>().text + "+" + GameObject.Find("InputFieldText").GetComponent<InputField>().text;
            } else {
                if (GameObject.Find("InputFieldText").GetComponent<InputField>().text != "") { tmpstr1 = "SANCheck:+" + GameObject.Find("InputFieldText").GetComponent<InputField>().text; } else { tmpstr1 = "SANCheck:+0"; }
            }
            if (GameObject.Find("Label2").GetComponent<Text>().text != " " && GameObject.Find("Label3").GetComponent<Text>().text != " ")
            {
                tmpstr2 = GameObject.Find("Label2").GetComponent<Text>().text + GameObject.Find("Label3").GetComponent<Text>().text + "+" + GameObject.Find("InputFieldText2").GetComponent<InputField>().text;
            }
            else
            {
                if (GameObject.Find("InputFieldText2").GetComponent<InputField>().text != "") { tmpstr2 = "+" + GameObject.Find("InputFieldText2").GetComponent<InputField>().text; } else { tmpstr2 = "+0"; }
            }
            commandText = tmpstr1 + "," + tmpstr2;
        }
        if (num == 29) { commandText = "FlagReset:"; }

        if (selectNum >= 0)
        {
            commandData[selectNum] = commandText.Replace("\n", "[system]改行");
            objCB[selectNum].transform.Find("Text").GetComponent<Text>().text = commandData[selectNum].Replace("</size>", "");
            SceneGraphic();
            NextSkipMake(num, selectNum);

            str2= "";
            for (int i = 0; i < commandData.Count; i++) { if (commandData[i].Replace("\n", "").Replace("\r", "") == "") { str2 = str2 + "Title:(未設定コマンド。タイトルバックとして機能します)\r\n"; continue; } str2 = str2 + commandData[i].Replace("\n", "").Replace("\r", "") + "\r\n"; }
            undoList.Add(str2);
            undoListNum=undoList.Count-1;
        }
        else
        {
            GameObject.Find("Error").GetComponent<Text>().text = "コマンドが選択されていません。";
            AudioSource bgm = GameObject.Find("BGMManager").GetComponent<AudioSource>(); bgm.loop = false; bgm.clip = errorSE; bgm.Play();
            StartCoroutine(ErrorWait());
        }
    }

    private void StartScene()
    {
        graphicNum = 0;soundNum = 0;
        List<string> tmp = LoadIventData(objBGM.GetComponent<BGMManager>().chapterName);
        for (int i = 0; i < tmp.Count; i++)
        {
            ZipRead(tmp[i]);//zipにあるイベントに関連するpngやwavを読み込む。
        }
        string[] files = Directory.GetFiles(@GetComponent<Utility>().GetAppPath() + @"\シナリオに使うpngやwavを入れるフォルダ", "*", SearchOption.AllDirectories);
        for (int i = 0; i < scenarioGraphic.Length - 1; i++) { if (gFileName[i] != "" && gFileName != null) { graphicNum = i; } }
        for (int i = 0; i < scenarioAudio.Length; i++) { if (sFileName[i] != "" && sFileName != null) { soundNum = i; } }
        for (int i = 0; i < files.Length; i++)
        {
            StartCoroutine(LoadFile(files[i]));//素材フォルダのファイルを読み込む。
        }
        LoadCommandData("[system]command1" + objBGM.GetComponent<BGMManager>().chapterName);
    }

    private void ZipRead(string path)
    {
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
                    scenarioGraphic[graphicNum] = Sprite.Create(texture, new Rect(0, 0, texture.width, texture.height), new Vector2(0.5f, 0.5f));
                    gFileName[graphicNum] = path;
                    graphicNum++;
                    //閉じる
                    fs.Close();
                }

                //wavファイルの場合
                if (path.Substring(path.Length - 4) == ".wav")
                {
                    //閲覧するZIPエントリのStreamを取得
                    Stream fs = zf.GetInputStream(ze);
                    buffer = ReadBinaryData(fs);//bufferにbyte[]になったファイルを読み込み
                    scenarioAudio[soundNum] = WavUtility.ToAudioClip(buffer);
                    sFileName[soundNum] = path;
                    soundNum++;
                    //閉じる
                    fs.Close();
                }
            }
            else
            {
                //空要素符合なら空要素を加える。
                if (path == "g")
                {
                    scenarioGraphic[graphicNum] = null;
                    graphicNum++;
                }
                if (path == "s")
                {
                    scenarioAudio[soundNum] = null;
                    soundNum++;
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






    private IEnumerator LoadFile(string path)
    {
        path = path.Replace("\n", "").Replace("\r","");
        // 指定したファイルをロードする
        WWW request = new WWW(path);

        while (!request.isDone)
        {
            yield return new WaitForEndOfFrame();
        }

        if (graphicNum >= scenarioGraphic.Length - 1)
        {
            GameObject.Find("Error").GetComponent<Text>().text = "画像ファイル数が９９個を越えたため読み込めなかったファイルがあります。";
            StartCoroutine(ErrorWait());
        }
        if (soundNum >= scenarioAudio.Length)
        {
            GameObject.Find("Error").GetComponent<Text>().text = "サウンドファイル数が４０個を越えたため読み込めなかったファイルがあります。";
            StartCoroutine(ErrorWait());
        }

        //pngファイルの場合
        if (path.Substring(path.Length - 4) == ".png")
        {
            // 画像を取り出す
            Texture2D texture = request.texture;
            
            //同名ファイルが既にzipに入っていれば上書き
            if(Array.IndexOf(gFileName, Path.GetFileName(path)) >= 0){ scenarioGraphic[Array.IndexOf(gFileName, Path.GetFileName(path))] = Sprite.Create(texture, new Rect(0, 0, texture.width, texture.height), new Vector2(0.5f, 0.5f)); gFileName[Array.IndexOf(gFileName, Path.GetFileName(path))] = path;yield break; }

            //空要素があればそこに代入。
            for (int j=0;j<scenarioGraphic.Length-1;j++) {
                if (scenarioGraphic[j] == null)
                {
                    scenarioGraphic[j] = Sprite.Create(texture, new Rect(0, 0, texture.width, texture.height), new Vector2(0.5f, 0.5f)); gFileName[j] = path; yield break;
                }
            }

            //どちらでもなければ追加
            if (graphicNum<scenarioGraphic.Length-1)
            {
                gFileName[graphicNum]=path;
                scenarioGraphic[graphicNum] = Sprite.Create(texture, new Rect(0, 0, texture.width, texture.height), new Vector2(0.5f, 0.5f));
                graphicNum++;
            }
        }

        //wavファイルの場合
        if (path.Substring(path.Length - 4) == ".wav")
        {
            yield return StartCoroutine(LoadBGM(request,path));
        }
        yield return null;
    }

    private IEnumerator LoadBGM(WWW request,string path)
    {
        //同名ファイルが既にzipに入っていれば上書き

        if (Array.IndexOf(sFileName, Path.GetFileName(path))>=0)
        {
            scenarioAudio[Array.IndexOf(sFileName, Path.GetFileName(path))] = request.GetAudioClip(false, true);
            while (scenarioAudio[Array.IndexOf(sFileName, Path.GetFileName(path))].loadState == AudioDataLoadState.Loading)
            {
                // ロードが終わるまで待つ
                yield return new WaitForEndOfFrame();
            }

            if (scenarioAudio[Array.IndexOf(sFileName, Path.GetFileName(path))].loadState != AudioDataLoadState.Loaded)
            {
                // 読み込み失敗
                Debug.Log("Failed to Load!");
                yield break;
            }
            sFileName[Array.IndexOf(sFileName, Path.GetFileName(path))] = path;
            yield break;
        }
        //空要素があればそこに代入
        for (int j = 0; j < scenarioAudio.Length; j++)
        {
            if (scenarioAudio[j] == null)
            {
                scenarioAudio[j] = request.GetAudioClip(false, true);
                while (scenarioAudio[j].loadState == AudioDataLoadState.Loading)
                {
                    // ロードが終わるまで待つ
                    yield return new WaitForEndOfFrame();
                }

                if (scenarioAudio[j].loadState != AudioDataLoadState.Loaded)
                {
                    // 読み込み失敗
                    Debug.Log("Failed to Load!");
                    yield break;
                }
                sFileName[j] = path;
                yield break;
            }
        }

        //どちらでもなければ追加
        if (soundNum < scenarioAudio.Length)
        {
            sFileName[soundNum]=path;
            scenarioAudio[soundNum] = request.GetAudioClip(false, true);
            soundNum++;
            while (scenarioAudio[soundNum].loadState == AudioDataLoadState.Loading)
            {
                // ロードが終わるまで待つ
                yield return new WaitForEndOfFrame();
            }

            if (scenarioAudio[soundNum].loadState != AudioDataLoadState.Loaded)
            {
                // 読み込み失敗
                Debug.Log("Failed to Load!");
                yield break;
            }
        }
    }

    //コマンドファイルを読み込む。
    private void LoadCommandData(string path)
    {
        string str;
        commandData.Clear();
        for (int i = 0; i < objCB.Count; i++) { Destroy(objCB[i]); }
        objCB.Clear();
        undoList.Clear();
        undoListNum = 0;
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
                    commandData.RemoveAt(0);//一行目はコメント欄なので除く
                    //閉じる
                    sr.Close();
                    reader.Close();
                    commandData.RemoveAt(commandData.Count - 1);//最終行は[END]なので除去。
                    //コマンドをボタンとして一覧に放り込む。
                    for (int i = 0; i < commandData.Count; i++)
                    {
                        if (i < 90)//90以降は全部タイトル戻しで埋めるのでボタン表示しない
                        {
                            objCB.Add(Instantiate(objCommand) as GameObject);
                            objCB[i].transform.SetParent(parentObject.transform, false);
                            objCB[i].transform.Find("Text").GetComponent<Text>().text = commandData[i].Replace("</size>","");
                            objCB[i].GetComponent<CommandButton>().buttonNum = i;

                            //分岐コマンドの場合は分岐先表示を出す
                            if (commandData[i].Length > 7 && commandData[i].Substring(0, 7) == "Select:") { NextSkipMake(10, i); }
                            else if (commandData[i].Length > 7 && commandData[i].Substring(0, 7) == "Hantei:") { NextSkipMake(11, i); }
                            else if (commandData[i].Length > 7 && commandData[i].Substring(0, 7) == "Battle:") { NextSkipMake(12, i); }
                            else if (commandData[i].Length > 11 && commandData[i].Substring(0, 11) == "FlagBranch:") { NextSkipMake(13, i); }
                            else if (commandData[i].Length > 11 && commandData[i].Substring(0, 11) == "Difference:") { NextSkipMake(17, i); }
                            else if (commandData[i].Length > 6 && commandData[i].Substring(0, 6) == "Equal:") { NextSkipMake(20, i); }
                            else if (commandData[i].Length > 9 && commandData[i].Substring(0, 9) == "SANCheck:") { NextSkipMake(28, i); }
                            else { NextSkipMake(0,i); }
                        }
                    }
                }
                else
                {
                    objCB.Add(Instantiate(objCommand) as GameObject);
                    objCB[0].transform.SetParent(parentObject.transform, false);
                    commandData.Add("");
                    objCB[0].transform.Find("NextSkip").GetComponent<Text>().text = "<color=white>1</color>";
                }
            }
            catch { }
            //閉じる
            zf.Close();

            str = "";
            for (int i = 0; i < commandData.Count; i++) { if (commandData[i].Replace("\n", "").Replace("\r", "") == "") { str = str + "Title:(未設定コマンド。タイトルバックとして機能します)\r\n"; continue; } str = str + commandData[i].Replace("\n", "").Replace("\r", "") + "\r\n"; }
            undoList.Add(str);
            undoListNum = undoList.Count - 1;
        }
        catch
        {
            GetComponent<Utility>().StartCoroutine("LoadSceneCoroutine", "TitleScene");
        }
    }

    public void CommandAddButton()
    {
        string str;
        //追加ボタンが押されたらコマンドボタンを追加する。
        if (objCB.Count < 90-1)//90コまで
        {
                objCB.Insert(selectNum+1, Instantiate(objCommand) as GameObject);
                commandData.Insert(selectNum+1, "");
                objCB[selectNum+1].transform.SetParent(parentObject.transform, false);
                objCB[selectNum+1].GetComponent<CommandButton>().buttonNum = selectNum+1;
                objCB[selectNum+1].GetComponent<Transform>().SetSiblingIndex(selectNum+1);
            NextSkipMake(0,selectNum+1);
                for (int i = selectNum + 2; i < objCB.Count; i++)
            {
                objCB[i].GetComponent<CommandButton>().buttonNum++;

                if (commandData[i].Length > 7 && commandData[i].Substring(0, 7) == "Select:") { NextSkipMake(10, i); }
                else if (commandData[i].Length > 7 && commandData[i].Substring(0, 7) == "Hantei:") { NextSkipMake(11, i); }
                else if (commandData[i].Length > 7 && commandData[i].Substring(0, 7) == "Battle:") { NextSkipMake(12, i); }
                else if (commandData[i].Length > 11 && commandData[i].Substring(0, 11) == "FlagBranch:") { NextSkipMake(13, i); }
                else if (commandData[i].Length > 11 && commandData[i].Substring(0, 11) == "Difference:") { NextSkipMake(17, i); }
                else if (commandData[i].Length > 6 && commandData[i].Substring(0, 6) == "Equal:") { NextSkipMake(20, i); }
                else if (commandData[i].Length > 9 && commandData[i].Substring(0, 9) == "SANCheck:") { NextSkipMake(28, i); }
                else { NextSkipMake(0, i); }
            }//追加分の後ろはボタン番号が１増える。

            str = "";
            for (int i = 0; i < commandData.Count; i++) { if (commandData[i].Replace("\n", "").Replace("\r", "") == "") { str = str + "Title:(未設定コマンド。タイトルバックとして機能します)\r\n"; continue; } str = str + commandData[i].Replace("\n", "").Replace("\r", "") + "\r\n"; }
            undoList.Add(str);
            undoListNum = undoList.Count - 1;
        }
        else
        {
            AudioSource bgm = GameObject.Find("BGMManager").GetComponent<AudioSource>(); bgm.loop = false; bgm.clip = errorSE; bgm.Play();
            GameObject.Find("Error").GetComponent<Text>().text = "コマンド数オーバーです。次ファイルに移ってください。"; 
            StartCoroutine(ErrorWait());
        }
    }

    private IEnumerator ErrorWait()
    {
        for (int i = 0; i < 200; i++) { yield return null; }
        GameObject.Find("Error").GetComponent<Text>().text = "";
    }

    public void CommandDeleteButton()
    {
        string str;
        if (selectNum >= 0)
        {
            Destroy(objCB[selectNum]);
            objCB.RemoveAt(selectNum);
            try { commandData.RemoveAt(selectNum); } catch { }
            for (int i = selectNum; i < objCB.Count; i++)
            {
                objCB[i].GetComponent<CommandButton>().buttonNum--;

                if (commandData[i].Length > 7 && commandData[i].Substring(0, 7) == "Select:") { NextSkipMake(10, i); }
                else if (commandData[i].Length > 7 && commandData[i].Substring(0, 7) == "Hantei:") { NextSkipMake(11, i); }
                else if (commandData[i].Length > 7 && commandData[i].Substring(0, 7) == "Battle:") { NextSkipMake(12, i); }
                else if (commandData[i].Length > 11 && commandData[i].Substring(0, 11) == "FlagBranch:") { NextSkipMake(13, i); }
                else if (commandData[i].Length > 11 && commandData[i].Substring(0, 11) == "Difference:") { NextSkipMake(17, i); }
                else if (commandData[i].Length > 6 && commandData[i].Substring(0, 6) == "Equal:") { NextSkipMake(20, i); }
                else if (commandData[i].Length > 9 && commandData[i].Substring(0, 9) == "SANCheck:") { NextSkipMake(28, i); }
                else { NextSkipMake(0, i); }
            }//削除分の後ろはボタン番号が１減る。

            for (int k = 0; k < multiSelect.Count; k++) { if (selectNum < multiSelect[k]) { multiSelect[k]--; } }
            for (int j = 0; j < multiSelect.Count; j++)
            {
                Destroy(objCB[multiSelect[j]]);
                objCB.RemoveAt(multiSelect[j]);
                try { commandData.RemoveAt(multiSelect[j]); } catch { }
                for (int i = selectNum; i < objCB.Count; i++)
                {
                    objCB[i].GetComponent<CommandButton>().buttonNum--;

                    if (commandData[i].Length > 7 && commandData[i].Substring(0, 7) == "Select:") { NextSkipMake(10, i); }
                    else if (commandData[i].Length > 7 && commandData[i].Substring(0, 7) == "Hantei:") { NextSkipMake(11, i); }
                    else if (commandData[i].Length > 7 && commandData[i].Substring(0, 7) == "Battle:") { NextSkipMake(12, i); }
                    else if (commandData[i].Length > 11 && commandData[i].Substring(0, 11) == "FlagBranch:") { NextSkipMake(13, i); }
                    else if (commandData[i].Length > 11 && commandData[i].Substring(0, 11) == "Difference:") { NextSkipMake(17, i); }
                    else if (commandData[i].Length > 6 && commandData[i].Substring(0, 6) == "Equal:") { NextSkipMake(20, i); }
                    else if (commandData[i].Length > 9 && commandData[i].Substring(0, 9) == "SANCheck:") { NextSkipMake(28, i); }
                    else { NextSkipMake(0, i); }
                }//削除分の後ろはボタン番号が１減る。
                for (int k = 0; k < multiSelect.Count; k++) { if (multiSelect[j] < multiSelect[k]) { multiSelect[k]--; } }
            }
            selectNum = -1;

            str = "";
            for (int i = 0; i < commandData.Count; i++) { if (commandData[i].Replace("\n", "").Replace("\r", "") == "") { str = str + "Title:(未設定コマンド。タイトルバックとして機能します)\r\n"; continue; } str = str + commandData[i].Replace("\n", "").Replace("\r", "") + "\r\n"; }
            undoList.Add(str);
            undoListNum = undoList.Count - 1;
        }
        else
        {
            GameObject.Find("Error").GetComponent<Text>().text = "コマンドが選択されていません。";
            AudioSource bgm = GameObject.Find("BGMManager").GetComponent<AudioSource>(); bgm.loop = false; bgm.clip = errorSE; bgm.Play();
            StartCoroutine(ErrorWait());
        }
    }

    public void SetCommand()
    {
        string[] strs;
        try
        {
            char[] tmp = { ',', ':' };
            strs = commandData[selectNum].Replace("\n", "").Replace("\r", "").Split(tmp);
            try { objGSB.GetComponent<Image>().color = new Color(1.0f, 1.0f, 1.0f); } catch { }
            if (strs[0] == "Text") { if (objMake[0].activeSelf == false) { CommandButton(0); } GameObject.Find("InputFieldName").GetComponent<InputField>().text = strs[1]; GameObject.Find("InputFieldText").GetComponent<InputField>().text = strs[2].Replace("[system]改行", "\n"); if (strs[3] == "true") { GameObject.Find("Toggle").GetComponent<Toggle>().isOn = true; } else { GameObject.Find("Toggle").GetComponent<Toggle>().isOn = false; } }
            if (strs[0] == "BackText") { if (objMake[1].activeSelf == false) { CommandButton(1); } GameObject.Find("InputFieldText").GetComponent<InputField>().text = strs[1].Replace("[system]改行", "\n"); if (strs[2] == "true") { GameObject.Find("Toggle").GetComponent<Toggle>().isOn = true; } else { GameObject.Find("Toggle").GetComponent<Toggle>().isOn = false; } }
            if (strs[0] == "Back") { if (objMake[2].activeSelf == false) { CommandButton(2); } selectGS = int.Parse(strs[1]); try { objGSB = GameObject.Find("GS" + selectGS.ToString()); objGSB.GetComponent<Image>().color = new Color(1.0f, 1.0f, 0); } catch { } }
            if (strs[0] == "BGM") { if (objMake[3].activeSelf == false) { CommandButton(3); } selectGS = int.Parse(strs[1]); try { objGSB = GameObject.Find("GS" + selectGS.ToString()); objGSB.GetComponent<Image>().color = new Color(1.0f, 1.0f, 0); } catch { } GameObject.Find("InputFieldName").GetComponent<InputField>().text = strs[2]; }
            if (strs[0] == "BGMStop") { if (objMake[4].activeSelf == false) { CommandButton(4); } GameObject.Find("InputFieldName").GetComponent<InputField>().text = strs[1]; }
            if (strs[0] == "SE") { if (objMake[5].activeSelf == false) { CommandButton(5); } selectGS = int.Parse(strs[1]); try { objGSB = GameObject.Find("GS" + selectGS.ToString()); objGSB.GetComponent<Image>().color = new Color(1.0f, 1.0f, 0); } catch { } }
            if (strs[0] == "Chara") { if (objMake[6].activeSelf == false) { CommandButton(6); } int k = 2; if (strs[3] == "L") { k = 1; } if (strs[3] == "N") { k = 2; } if (strs[3] == "R") { k = 3; } selectGS = int.Parse(strs[1]); try { objGSB = GameObject.Find("GS" + selectGS.ToString()); objGSB.GetComponent<Image>().color = new Color(1.0f, 1.0f, 0); } catch { } GameObject.Find("Slider").GetComponent<Slider>().value = int.Parse(strs[2]); GameObject.Find("Slider2").GetComponent<Slider>().value = k; }
            if (strs[0] == "Item") { if (objMake[7].activeSelf == false) { CommandButton(7); } selectGS = int.Parse(strs[1]); try { objGSB = GameObject.Find("GS" + selectGS.ToString()); objGSB.GetComponent<Image>().color = new Color(1.0f, 1.0f, 0); } catch { } }
            if (strs[0] == "Shake") { if (objMake[8].activeSelf == false) { CommandButton(8); } }
            if (strs[0] == "Jump") { if (objMake[9].activeSelf == false) { CommandButton(9); } GameObject.Find("Slider").GetComponent<Slider>().value = int.Parse(strs[1]); }
            if (strs[0] == "Select") { if (objMake[10].activeSelf == false) { CommandButton(10); } GameObject.Find("InputFieldText").GetComponent<InputField>().text = strs[1]; GameObject.Find("InputFieldText (1)").GetComponent<InputField>().text = strs[2]; GameObject.Find("InputFieldText (2)").GetComponent<InputField>().text = strs[3]; GameObject.Find("InputFieldText (3)").GetComponent<InputField>().text = strs[4]; }
            if (strs[0] == "Hantei") { if (objMake[11].activeSelf == false) { CommandButton(11); } string[] tmpstr = strs[1].Split(new char[] { '/', '*' }); GameObject.Find("Dropdown1").GetComponent<Dropdown>().value = SkillList2(tmpstr[0]);if (GameObject.Find("Dropdown1").GetComponent<Dropdown>().value == 70) { GameObject.Find("InputFieldText (5)").GetComponent<InputField>().text = tmpstr[0]; } else { GameObject.Find("InputFieldText (5)").GetComponent<InputField>().text = ""; } if (strs[1].Contains("*")) { GameObject.Find("InputFieldText (2)").GetComponent<InputField>().text = tmpstr[1]; } if (strs[1].Contains("/")) { GameObject.Find("InputFieldText (4)").GetComponent<InputField>().text = tmpstr[1]; } GameObject.Find("InputFieldText (3)").GetComponent<InputField>().text = strs[2]; }
            if (strs[0] == "Battle") { if (objMake[12].activeSelf == false) { CommandButton(12); } selectGS = int.Parse(strs[1]); try { objGSB = GameObject.Find("GS" + selectGS.ToString()); objGSB.GetComponent<Image>().color = new Color(1.0f, 1.0f, 0); } catch { } GameObject.Find("Dropdown1").GetComponent<Dropdown>().value = int.Parse(strs[2]) - 1; GameObject.Find("InputFieldText (2)").GetComponent<InputField>().text = strs[3]; GameObject.Find("InputFieldText (3)").GetComponent<InputField>().text = strs[4]; GameObject.Find("InputFieldText (4)").GetComponent<InputField>().text = strs[5]; GameObject.Find("Dropdown2").GetComponent<Dropdown>().value = int.Parse(strs[6]) - 1; int k = 0; if (strs[7] == "4") { k = 0; } if (strs[7] == "6") { k = 1; } if (strs[7] == "10") { k = 2; } if (strs[7] == "100") { k = 3; } GameObject.Find("Dropdown3").GetComponent<Dropdown>().value = k; if (strs[8] == "true") { GameObject.Find("Toggle1").GetComponent<Toggle>().isOn = true; } else { GameObject.Find("Toggle1").GetComponent<Toggle>().isOn = false; } GameObject.Find("InputFieldText (5)").GetComponent<InputField>().text = strs[9]; GameObject.Find("Dropdown4").GetComponent<Dropdown>().value = SkillList2(strs[10]); GameObject.Find("InputFieldText (7)").GetComponent<InputField>().text = strs[11]; GameObject.Find("InputFieldText (8)").GetComponent<InputField>().text = strs[12]; if (strs[13] == "true") { GameObject.Find("Toggle2").GetComponent<Toggle>().isOn = true; } else { GameObject.Find("Toggle2").GetComponent<Toggle>().isOn = false; } }
            if (strs[0] == "FlagBranch") { if (objMake[13].activeSelf == false) { CommandButton(13); } GameObject.Find("InputFieldText (2)").GetComponent<InputField>().text = strs[1]; GameObject.Find("InputFieldText (3)").GetComponent<InputField>().text = strs[2]; }
            if (strs[0] == "FlagChange") { if (objMake[14].activeSelf == false) { CommandButton(14); } GameObject.Find("InputFieldText (2)").GetComponent<InputField>().text = strs[1]; GameObject.Find("InputFieldText (3)").GetComponent<InputField>().text = strs[2]; GameObject.Find("InputFieldText (4)").GetComponent<InputField>().text = strs[3]; }
            if (strs[0] == "GetTime") { if (objMake[15].activeSelf == false) { CommandButton(15); } }
            if (strs[0] == "FlagCopy") { if (objMake[16].activeSelf == false) { CommandButton(16); } GameObject.Find("InputFieldText (2)").GetComponent<InputField>().text = strs[1]; GameObject.Find("InputFieldText (3)").GetComponent<InputField>().text = strs[2]; }
            if (strs[0] == "Difference") { if (objMake[17].activeSelf == false) { CommandButton(17); } GameObject.Find("InputFieldText (2)").GetComponent<InputField>().text = strs[1]; GameObject.Find("InputFieldText (3)").GetComponent<InputField>().text = strs[2]; GameObject.Find("InputFieldText (4)").GetComponent<InputField>().text = strs[3]; }
            if (strs[0] == "StatusChange") { if (objMake[18].activeSelf == false) { CommandButton(18); } GameObject.Find("Dropdown1").GetComponent<Dropdown>().value = SkillList2(strs[1]); int k = 1; if (strs[2].Contains("-1D")) { k = 2; } else if (strs[2].Contains("1D")) { k = 0; } else if (strs[2].Contains("-2D")) { k = 3; } else if (strs[2].Contains("2D")) { k = 1; } else { k = 4; } GameObject.Find("Dropdown2").GetComponent<Dropdown>().value = k; if (strs[2].Contains("D4")) { k = 0; } else if (strs[2].Contains("D6")) { k = 1; } else if (strs[2].Contains("D100")) { k = 3; } else if (strs[2].Contains("D10")) { k = 2; } else { k = 4; } GameObject.Find("Dropdown3").GetComponent<Dropdown>().value = k; if (strs[2].Contains("+")) { string[] tmpstr = strs[2].Split('+'); GameObject.Find("InputFieldText").GetComponent<InputField>().text = tmpstr[1]; } else { GameObject.Find("InputFieldText").GetComponent<InputField>().text = ""; } }
            if (strs[0] == "Input") { if (objMake[19].activeSelf == false) { CommandButton(19); } GameObject.Find("InputFieldText (2)").GetComponent<InputField>().text = strs[1]; }
            if (strs[0] == "Equal") { if (objMake[20].activeSelf == false) { CommandButton(20); } GameObject.Find("InputFieldText (2)").GetComponent<InputField>().text = strs[1]; GameObject.Find("InputFieldText (3)").GetComponent<InputField>().text = strs[2]; }
            if (strs[0] == "Lost") { if (objMake[21].activeSelf == false) { CommandButton(21); } }
            if (strs[0] == "Title") { if (objMake[22].activeSelf == false) { CommandButton(22); } }
            if (strs[0] == "Map") { if (objMake[23].activeSelf == false) { CommandButton(23); }if (strs[1]=="Once") { GameObject.Find("Toggle1").GetComponent<Toggle>().isOn = false; } else { GameObject.Find("Toggle1").GetComponent<Toggle>().isOn = true; } }
            if (strs[0] == "NextFile") { if (objMake[24].activeSelf == false) { CommandButton(24); } GameObject.Find("InputFieldText (2)").GetComponent<InputField>().text = strs[1].Substring(8, strs[1].Length - 8 - objBGM.GetComponent<BGMManager>().chapterName.Length); }
            if (strs[0] == "BlackOut") { if (objMake[25].activeSelf == false) { CommandButton(25); } GameObject.Find("InputFieldText").GetComponent<InputField>().text = strs[1]; GameObject.Find("InputFieldText (1)").GetComponent<InputField>().text = strs[2]; GameObject.Find("InputFieldText (2)").GetComponent<InputField>().text = strs[3]; GameObject.Find("InputFieldText (3)").GetComponent<InputField>().text = strs[4]; }
            if (strs[0] == "PlaceChange") { if (objMake[26].activeSelf == false) { CommandButton(26); } GameObject.Find("InputFieldText").GetComponent<InputField>().text = strs[1]; GameObject.Find("InputFieldText (1)").GetComponent<InputField>().text = strs[2]; }
            if (strs[0] == "Wait") { if (objMake[27].activeSelf == false) { CommandButton(27); } GameObject.Find("InputFieldName").GetComponent<InputField>().text = strs[1]; }
            if (strs[0] == "SANCheck") { if (objMake[28].activeSelf == false) { CommandButton(28); } int k = 1; if (strs[1].Contains("1D")) { k = 0; } else if (strs[1].Contains("2D")) { k = 1; } else { k = 2; } GameObject.Find("Dropdown0").GetComponent<Dropdown>().value = k; if (strs[1].Contains("D4")) { k = 0; } else if (strs[1].Contains("D6")) { k = 1; } else if (strs[1].Contains("D100")) { k = 3; } else if (strs[1].Contains("D10")) { k = 2; } else { k = 4; } GameObject.Find("Dropdown1").GetComponent<Dropdown>().value = k; if (strs[1].Contains("+")) { string[] tmpstr = strs[1].Split('+'); GameObject.Find("InputFieldText").GetComponent<InputField>().text = tmpstr[1]; } else { GameObject.Find("InputFieldText").GetComponent<InputField>().text = ""; } if (strs[2].Contains("1D")) { k = 0; } else if (strs[2].Contains("2D")) { k = 1; } else { k = 2; } GameObject.Find("Dropdown2").GetComponent<Dropdown>().value = k; if (strs[2].Contains("D4")) { k = 0; } else if (strs[2].Contains("D6")) { k = 1; } else if (strs[2].Contains("D100")) { k = 3; } else if (strs[2].Contains("D10")) { k = 2; } else { k = 4; } GameObject.Find("Dropdown3").GetComponent<Dropdown>().value = k; if (strs[2].Contains("+")) { string[] tmpstr = strs[2].Split('+'); GameObject.Find("InputFieldText2").GetComponent<InputField>().text = tmpstr[1]; } else { GameObject.Find("InputFieldText2").GetComponent<InputField>().text = ""; }}
            if (strs[0] == "FlagReset") { if (objMake[29].activeSelf == false) { CommandButton(29); } }
            if (strs[0] == "" || strs[0]==null)
            {
                for (int i = 0; i < objMake.Length; i++) { objMake[i].SetActive(false); }
                foreach (GameObject tempObject in objGS) { Destroy(tempObject); }
                objGS.Clear();
            }


            SceneGraphic();
        }
        catch
        {
        }
    }

    public void BackButton()
    {
        SaveCommandFile();
        if (backFileLog.Count == 0) { GetComponent<Utility>().StartCoroutine("LoadSceneCoroutine", "MapScene"); }
        else
        {
            commandName = backFileLog[backFileLog.Count - 1];
            backFileLog.RemoveAt(backFileLog.Count - 1);
            backGraphLog.RemoveAt(backGraphLog.Count - 1);
            backBTLog.RemoveAt(backBTLog.Count - 1);
            backTLog.RemoveAt(backTLog.Count - 1);
            foreach (GameObject tempObject in objCB) { Destroy(tempObject); }
            objCB.Clear();
            for (int i = 0; i < objMake.Length; i++) { objMake[i].SetActive(false); }
            selectNum = -1;
            LoadCommandData(commandName);
            titleText.GetComponent<Text>().text = commandName.Replace(objBGM.GetComponent<BGMManager>().chapterName, "").Replace("[system]", "[コマンド]") + "\n" + objBGM.GetComponent<BGMManager>().chapterName.Substring(0, objBGM.GetComponent<BGMManager>().chapterName.Length - 4).Replace("[system]", "[イベント]");
        }//一つ戻って、履歴からはそこを消す
    }

    public void NextFileButton()
    {
        string path;
        int[] temp1= { backGraphLogTemp[0], backGraphLogTemp[1], backGraphLogTemp[2], backGraphLogTemp[3], backGraphLogTemp[4], backGraphLogTemp[5], };
        string[] temp2= { backTLogTemp[0],backTLogTemp[1] };//配列や文字列をそのまま代入すると参照型なのでアドレス自体を渡してしまう（＝元データをいじるとリストまで変更されてしまう）
        CommandDecide(24);//ネクストファイルの「決定」ボタンを押したのと同じ効果。
        if (GameObject.Find("Error").GetComponent<Text>().text == "「<color=red>[system]</color>」という文字列は使用禁止です。(システム処理の識別語にしています)" || GameObject.Find("Error").GetComponent<Text>().text == "ファイル名に使えない文字、または,(半角コンマ)が入っています。") { return; }
        path = commandData[selectNum].Substring(9).Replace("\r","").Replace("\n","");
        SaveCommandFile();
        backFileLog.Add(commandName);//現在のコマンドファイル名をログに保存
        backBTLog.Add(backBTLogTemp);
        backTLog.Add(temp2);
        backGraphLog.Add(temp1);
        commandName = path;//次のファイルのコマンドファイル名に入れ替え
        foreach (GameObject tempObject in objCB) { Destroy(tempObject); }
        objCB.Clear();
        for (int i = 0; i < objMake.Length; i++) { objMake[i].SetActive(false); }
        selectNum = -1;
        LoadCommandData(commandName);
        titleText.GetComponent<Text>().text = commandName.Replace(objBGM.GetComponent<BGMManager>().chapterName, "").Replace("[system]", "[コマンド]") + "\n" + objBGM.GetComponent<BGMManager>().chapterName.Substring(0, objBGM.GetComponent<BGMManager>().chapterName.Length - 4).Replace("[system]", "[イベント]");
    }

    //コマンドファイルを書き出す関数
    public void SaveCommandFile()
    {
        try
        {
            string str = commandName + "\r\n";//一行目はファイル名を示す部分。
                                              //ZIP書庫のパス
            string zipPath = PlayerPrefs.GetString("進行中シナリオ", "");
            //書庫に追加するファイルのパス
            string file = @GetComponent<Utility>().GetAppPath() + @"\" + commandName;

            //先にテキストファイルを一時的に書き出しておく。
            for (int i = 0; i < commandData.Count; i++) { if (commandData[i].Replace("\n", "").Replace("\r", "") == "") { str = str + "Title:(未設定コマンド。タイトルバックとして機能します)\r\n"; continue; } str = str + commandData[i].Replace("\n", "").Replace("\r", "") + "\r\n"; }
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
            zf.CommitUpdate();
            //イベントファイルと画像サウンドファイルを追加
            zf.BeginUpdate();
            AddIventGS(zf);
            //ZipFileの更新をコミット
            zf.CommitUpdate();

            //閉じる
            zf.Close();

            //一時的に書きだしたファイルを消去する。
                File.Delete(file);
                File.Delete(@GetComponent<Utility>().GetAppPath() + @"\" + objBGM.GetComponent<BGMManager>().chapterName);
        }
        catch { }
    }

    //イベントファイルを書き出す関数
    private void AddIventGS(ICSharpCode.SharpZipLib.Zip.ZipFile zf)
    {
        bool[] gF=new bool[scenarioGraphic.Length-1];
        bool[] sF = new bool[scenarioAudio.Length];
        //冒頭コマンドファイルを入れる。
        string str = "[system]command1" + objBGM.GetComponent<BGMManager>().chapterName + "\r\n";
        //先にテキストファイルを一時的に書き出しておく。※コマンドファイルで使われていないモノは保存しない。
        foreach (ICSharpCode.SharpZipLib.Zip.ZipEntry ze in zf)
        {
        if (ze.Name.Length>8 && ze.Name.Substring(0, 8) == "[system]" && ze.Name.Contains(Path.GetFileName(objBGM.GetComponent<BGMManager>().chapterName)))
            {
                //閲覧するZIPエントリのStreamを取得
                Stream reader = zf.GetInputStream(ze);
                //文字コードを指定してStreamReaderを作成
                StreamReader sr = new StreamReader(
                    reader, System.Text.Encoding.GetEncoding("UTF-8"));
                // テキストを取り出す
                string text = sr.ReadToEnd();
                for (int i = 0; i < scenarioGraphic.Length-1; i++)
                {
                    if (text.Contains("\nBack:" + i.ToString() + "\r\n") || text.Contains("\nBack:" + i.ToString() + "\n") || text.Contains("\nChara:" + i.ToString() + ",") || text.Contains("\nItem:" + i.ToString() + "\r\n") || text.Contains("\nItem:" + i.ToString() + "\n") || text.Contains("\nBattle:" + i.ToString() + ",")) { gF[i] = true; }
                }
                for (int i = 0; i < scenarioAudio.Length; i++)
                {
                    if (text.Contains("\nBGM:" + i.ToString() + ",") || text.Contains("\nSE:" + i.ToString() + "\r\n") || text.Contains("\nSE:" + i.ToString() + "\n")) { sF[i]=true; }
                }
                //閉じる
                sr.Close();
                reader.Close();
            }
        }

        for (int i = 0; i < gF.Length; i++) {if (gF[i] == false) { str = str + "g\r\n"; continue; } str = str + Path.GetFileName(gFileName[i]).Replace("\n", "").Replace("\r", "") + "\r\n";}
        for (int i = 0; i < sF.Length; i++) { if (sF[i]==false ) { str = str + "s\r\n"; continue; } str = str + Path.GetFileName(sFileName[i]).Replace("\n", "").Replace("\r", "") + "\r\n"; }
        str = str + "[END]";
        File.WriteAllText(@GetComponent<Utility>().GetAppPath() + @"\" + objBGM.GetComponent<BGMManager>().chapterName, str);
        zf.Add(@GetComponent<Utility>().GetAppPath() + @"\" + objBGM.GetComponent<BGMManager>().chapterName, Path.GetFileName(objBGM.GetComponent<BGMManager>().chapterName));
        
        //画像サウンドファイルの作成※コマンドファイルで使われていないモノは保存しない＋zipから読み込んだ（既に同じものがzipにある）ファイルは保存しない。（というかファイルじゃないので参照しても取得に失敗する）
        for (int i = 0; i < gFileName.Length; i++) { if (gFileName[i] != Path.GetFileName(gFileName[i]) && gF[i]==true){ try { zf.Add(@GetComponent<Utility>().GetAppPath() + @"\シナリオに使うpngやwavを入れるフォルダ\" + Path.GetFileName(gFileName[i]), Path.GetFileName(gFileName[i]));gFileName[i] = Path.GetFileName(gFileName[i]); } catch { } }  }//ファイルがなかったら（主に空き要素の場合）そのままスキップ
        for (int i = 0; i < sFileName.Length; i++) { if (sFileName[i] != Path.GetFileName(sFileName[i]) && sF[i]==true) { try { zf.Add(@GetComponent<Utility>().GetAppPath() + @"\シナリオに使うpngやwavを入れるフォルダ\" + Path.GetFileName(sFileName[i]), Path.GetFileName(sFileName[i]));sFileName[i] = Path.GetFileName(sFileName[i]); } catch { } } }//一度更新したら、そのイベントを開いている間は同じデータを更新することはない。
    }

    private int SkillList2(string targetStr)
    {
        int target = 70;
        if (targetStr == "言いくるめ") { target = 11; }
        if (targetStr == "医学") { target = 12; }
        if (targetStr == "運転") { target = 13; }
        if (targetStr == "応急手当") { target = 14; }
        if (targetStr == "オカルト") { target = 15; }
        if (targetStr == "回避") { target = 16; }
        if (targetStr == "化学") { target = 17; }
        if (targetStr == "鍵開け") { target = 18; }
        if (targetStr == "隠す") { target = 19; }
        if (targetStr == "隠れる") { target = 20; }
        if (targetStr == "機械修理") { target = 21; }
        if (targetStr == "聞き耳") { target = 22; }
        if (targetStr == "芸術") { target = 23; }
        if (targetStr == "経理") { target = 24; }
        if (targetStr == "考古学") { target = 25; }
        if (targetStr == "コンピューター") { target = 26; }
        if (targetStr == "忍び歩き") { target = 27; }
        if (targetStr == "写真術") { target = 28; }
        if (targetStr == "重機械操作") { target = 29; }
        if (targetStr == "乗馬") { target = 30; }
        if (targetStr == "信用") { target = 31; }
        if (targetStr == "心理学") { target = 32; }
        if (targetStr == "人類学") { target = 33; }
        if (targetStr == "水泳") { target = 34; }
        if (targetStr == "製作") { target = 35; }
        if (targetStr == "精神分析") { target = 36; }
        if (targetStr == "生物学") { target = 37; }
        if (targetStr == "説得") { target = 38; }
        if (targetStr == "操縦") { target = 39; }
        if (targetStr == "地質学") { target = 40; }
        if (targetStr == "跳躍") { target = 41; }
        if (targetStr == "追跡") { target = 42; }
        if (targetStr == "電気修理") { target = 43; }
        if (targetStr == "電子工学") { target = 44; }
        if (targetStr == "天文学") { target = 45; }
        if (targetStr == "投擲") { target = 46; }
        if (targetStr == "登ハン") { target = 47; }
        if (targetStr == "図書館") { target = 48; }
        if (targetStr == "ナビゲート") { target = 49; }
        if (targetStr == "値切り") { target = 50; }
        if (targetStr == "博物学") { target = 51; }
        if (targetStr == "物理学") { target = 52; }
        if (targetStr == "変装") { target = 53; }
        if (targetStr == "法律") { target = 54; }
        if (targetStr == "ほかの言語") { target = 55; }
        if (targetStr == "母国語") { target = 56; }
        if (targetStr == "マーシャルアーツ") { target = 57; }
        if (targetStr == "目星") { target = 58; }
        if (targetStr == "薬学") { target = 59; }
        if (targetStr == "歴史") { target = 60; }
        if (targetStr == "火器") { target = 61; }
        if (targetStr == "格闘") { target = 62; }
        if (targetStr == "武器術") { target = 63; }
        if (targetStr == "耐久力") { target = 64; }
        if (targetStr == "マジック・ポイント") { target = 65; }
        if (targetStr == "正気度ポイント") { target = 66; }
        if (targetStr == "幸運") { target = 67; }
        if (targetStr == "知識") { target = 68; }
        if (targetStr == "アイデア") { target = 69; }
        if (targetStr == "クトゥルフ神話") { target = 10; }
        if (targetStr == "STR") { target = 0; }
        if (targetStr == "DEX") { target = 1; }
        if (targetStr == "CON") { target = 2; }
        if (targetStr == "POW") { target = 3; }
        if (targetStr == "INT") { target = 4; }
        if (targetStr == "EDU") { target = 5; }
        if (targetStr == "SIZ") { target = 6; }
        if (targetStr == "APP") { target = 7; }
        if (targetStr == "最大マジック・ポイント") { target = 8; }
        if (targetStr == "最大耐久力") { target = 9; }
        if (targetStr == "フラグの値で判定する") { target = 70; }
        return target;
    }

    private void TextDraw(string name,string text)
    {
        objBackText.gameObject.SetActive(false);
        objTextBox.gameObject.SetActive(true);
        text = text.Replace("[system]改行", "\r\n").Replace("[PC]", PlayerPrefs.GetString("[system]PlayerCharacterName", "あなた"));
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
        text = text.Replace("[system]改行","\r\n").Replace("[PC]", PlayerPrefs.GetString("[system]PlayerCharacterName", "あなた"));
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

    public void MapBackButton(int num)
    {
        CommandDecide(num);//ネクストファイルの「決定」ボタンを押したのと同じ効果。
        SaveCommandFile();
        GetComponent<Utility>().StartCoroutine("LoadSceneCoroutine", "MapScene");
    }

}