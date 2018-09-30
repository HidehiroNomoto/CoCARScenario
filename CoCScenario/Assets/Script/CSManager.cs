using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using UnityEngine.Advertisements;

public class CSManager : MonoBehaviour {
    private int[] status = new int[STATUSNUM];
    private int[] skills = new int[SKILLNUM];
    private int buyPoint;
    private GameObject[] statusObj = new GameObject[STATUSNUM];
    private GameObject objSkillSheet1;
    private GameObject objSkillSheet2;
    private GameObject objSkillButton;
    private GameObject objBuyPoint;
    private GameObject objChara;
    private GameObject[] objSkill = new GameObject[SKILLNUM];
    private bool skill;
    private int[] skillDefault = new int[SKILLNUM];
    private int nowHP;
    private int nowMP;
    private int nowSAN;
    private bool loadedChara = false;
    const int STATUSNUM = 12;
    const int SKILLNUM = 54;
    // Use this for initialization
    void Start() {
        DefaultMake();
        SeeCharacter();
        if (SceneManager.GetActiveScene().name == "CharacterSheet")
        {
            GameObject.Find("InputField").GetComponent<InputField>().text = PlayerPrefs.GetString("PlayerCharacterName", "");
        }
        else
        {
            GameObject.Find("PlayerCharacterName").GetComponent<Text>().text = PlayerPrefs.GetString("PlayerCharacterName", "――");
            transform.GetChild(0).gameObject.SetActive(false);
        }
    }


    public void DefaultMake()
    {
        skill = false;
        objSkillSheet1 = GameObject.Find("SkillSheet1/2").gameObject as GameObject;
        objSkillSheet2 = GameObject.Find("SkillSheet2/2").gameObject as GameObject;
        objChara = GameObject.Find("CharacterImage").gameObject as GameObject;
        objSkillButton = GameObject.Find("SkillButton").gameObject as GameObject;
        if (SceneManager.GetActiveScene().name == "CharacterSheet")
        {
            objBuyPoint = GameObject.Find("BuyPoint").gameObject as GameObject;
        }
        for (int i = 0; i < STATUSNUM; i++)
        {
            statusObj[i] = GameObject.Find("statusObj" + i.ToString()).gameObject as GameObject;
        }
        for (int i = 0; i < SKILLNUM / 2; i++)
        {
            objSkill[i] = GameObject.Find("Skill" + (i + 1).ToString()).gameObject as GameObject;
        }
        for (int i = SKILLNUM / 2; i < SKILLNUM; i++)
        {
            objSkill[i] = GameObject.Find("2Skill" + (i - SKILLNUM / 2 + 1).ToString()).gameObject as GameObject;
        }
        StartCoroutine(LoadChara(PlayerPrefs.GetString("CharacterIllustPath", "")));

        //技能初期値設定
        skillDefault[0] = 5; skillDefault[1] = 5; skillDefault[2] = 20; skillDefault[3] = 30;
        skillDefault[4] = 5; skillDefault[5] = status[2] * 2; skillDefault[6] = 1; skillDefault[7] = 1;
        skillDefault[8] = 15; skillDefault[9] = 10; skillDefault[10] = 20; skillDefault[11] = 25;
        skillDefault[12] = 5; skillDefault[13] = 10; skillDefault[14] = 1; skillDefault[15] = 1;
        skillDefault[16] = 10; skillDefault[17] = 10; skillDefault[18] = 1; skillDefault[19] = 5;
        skillDefault[20] = 15; skillDefault[21] = 5; skillDefault[22] = 1; skillDefault[23] = 25;
        skillDefault[24] = 5; skillDefault[25] = 1; skillDefault[26] = 1; skillDefault[27] = 15;
        skillDefault[28] = 1; skillDefault[29] = 1; skillDefault[30] = 25; skillDefault[31] = 10;
        skillDefault[32] = 10; skillDefault[33] = 1; skillDefault[34] = 1; skillDefault[35] = 25;
        skillDefault[36] = 40; skillDefault[37] = 25; skillDefault[38] = 10; skillDefault[39] = 5;
        skillDefault[40] = 10; skillDefault[41] = 1; skillDefault[42] = 1; skillDefault[43] = 5;
        skillDefault[44] = 1; skillDefault[45] = status[7] * 5; skillDefault[46] = 1; skillDefault[47] = 25;
        skillDefault[48] = 1; skillDefault[49] = 20; skillDefault[50] = 30; skillDefault[51] = 50;
        skillDefault[52] = 50; skillDefault[53] = 0;
    }


    public void SeeCharacter()
    {
        string[][] str = new string[STATUSNUM][];
        string[] str2;
        string str3 = "";
        for (int i = 0; i < STATUSNUM; i++)
        {
            status[i] = PlayerPrefs.GetInt("Status" + i.ToString(), 0);
        }
        for (int i = 0; i < SKILLNUM; i++)
        {
            skills[i] = PlayerPrefs.GetInt("Skill" + i.ToString(), 0);
        }
        nowHP = PlayerPrefs.GetInt("耐久力", 0);
        nowMP = PlayerPrefs.GetInt("マジック・ポイント", 0);
        nowSAN = PlayerPrefs.GetInt("正気度ポイント", 0);
        for (int i = 0; i < STATUSNUM; i++)
        {
            str[i] = statusObj[i].GetComponent<Text>().text.Split(':');
            if (i == 8)
            {
                if (status[8] > 0) { str3 = "+1D"; }
                if (status[8] < 0) { str3 = "-1D"; }
            }
            if (i == 9) { str3 = nowHP.ToString() + "/"; }
            if (i == 10) { str3 = nowMP.ToString() + "/"; }
            if (i == 11) { str3 = nowSAN.ToString() + "/"; }
            statusObj[i].GetComponent<Text>().text = str[i][0] + ':' + str3 + Mathf.Abs(status[i]);
            if (i == 11) { statusObj[i].GetComponent<Text>().text = str[i][0] + ':' + str3 + (99 - skills[53]).ToString(); }
            str3 = "";
        }
        for (int i = 0; i < SKILLNUM; i++)
        {
            str2 = objSkill[i].GetComponent<Text>().text.Split('：');
            objSkill[i].GetComponent<Text>().text = str2[0] + '：' + skills[i].ToString();
        }
        //キャラがいなければ作成
        if (status[0] == 0) { MakeCharacter(); } else { loadedChara = true; }
    }


    //キャラのステータス決定
    public void MakeCharacter()
    {
        loadedChara = false;
        Utility u1 = GetComponent<Utility>();
        string str3 = "";
        string[][] str = new string[STATUSNUM][];
        status[0] = u1.DiceRoll(3, 6);
        status[1] = u1.DiceRoll(3, 6);
        status[2] = u1.DiceRoll(3, 6);
        status[3] = u1.DiceRoll(2, 6) + 6;
        status[4] = u1.DiceRoll(3, 6);
        status[5] = u1.DiceRoll(3, 6);
        status[6] = u1.DiceRoll(2, 6) + 6;
        status[7] = u1.DiceRoll(3, 6) + 3;
        if (status[1] + status[6] < 13) { status[8] = -6; }
        else if (status[0] + status[6] < 17) { status[8] = -4; }
        else if (status[0] + status[6] < 25) { status[8] = 0; }
        else if (status[0] + status[6] < 33) { status[8] = 4; }
        else if (status[0] + status[6] < 41) { status[8] = 6; }
        status[9] = (status[1] + status[6]) / 2 + (status[1] + status[6]) % 2;
        status[10] = status[5];
        status[11] = status[5] * 5;
        nowHP = status[9];
        nowMP = status[10];
        nowSAN = status[11];
        for (int i = 0; i < STATUSNUM; i++)
        {
            str[i] = statusObj[i].GetComponent<Text>().text.Split(':');
            if (i == 8)
            {
                if (status[8] > 0) { str3 = "+1D"; }
                if (status[8] < 0) { str3 = "-1D"; }
            }
            if (i == 9) { str3 = nowHP.ToString() + "/"; }
            if (i == 10) { str3 = nowMP.ToString() + "/"; }
            if (i == 11) { str3 = nowSAN.ToString() + "/"; }
            statusObj[i].GetComponent<Text>().text = str[i][0] + ':' + str3 + Mathf.Abs(status[i]);
            if (i == 11) { statusObj[i].GetComponent<Text>().text = str[i][0] + ':' + str3 + (99 - skills[53]).ToString(); }
            str3 = "";
        }

        //技能初期値の入力
        skillDefault[5] = status[2] * 2; skillDefault[45] = status[7] * 5;
        SkillReset();
    }

    public void SkillReset()
    {
        string[] str;
        if (loadedChara == false)
        {
            for (int i = 0; i < SKILLNUM; i++)
            {
                skills[i] = skillDefault[i];
                str = objSkill[i].GetComponent<Text>().text.Split('：');
                objSkill[i].GetComponent<Text>().text = str[0] + '：' + skills[i].ToString();
            }
            buyPoint = status[3] * 10 + status[7] * 20;
            objBuyPoint.GetComponent<Text>().text = "残：" + buyPoint.ToString() + "P";
        }
        else
        {
            objBuyPoint.GetComponent<Text>().text = "決定済";
        }
    }

    private IEnumerator SkillMove()
    {
        int localchange = 0;
        if (SceneManager.GetActiveScene().name != "CharacterSheet")
        {
            localchange = 600;
        }
        if (skill == false)
        {
            skill = true;
            for (int i = 0; i <= 20; i++)
            {
                objSkillSheet2.GetComponent<RectTransform>().localPosition = new Vector3(800 - i * 40, localchange + 80, 0);
                yield return null;
            }
            if (SceneManager.GetActiveScene().name == "CharacterSheet")
            {
                objBuyPoint.GetComponent<Text>().text = "残：" + buyPoint.ToString() + "P";
            }
            yield break;
        }

        if (skill == true)
        {
            skill = false;
            for (int i = 0; i <= 20; i++)
            {
                objSkillSheet2.GetComponent<RectTransform>().localPosition = new Vector3(i * 40, localchange + 80, 0);
                yield return null;
            }
            yield break;
        }

    }

    public void SkillButton()
    {
        SkillButtonIn();
    }

    private void SkillButtonIn()
    {
        if (skill == false) { objSkillButton.GetComponent<Image>().color = new Color(0.8f, 0.8f, 0.8f); }
        if (skill == true) { objSkillButton.GetComponent<Image>().color = new Color(1.0f, 1.0f, 1.0f); }
        StartCoroutine(SkillMove());
    }

    public IEnumerator LoadChara(string path)
    {
        // 指定したファイルをロードする
        WWW request = new WWW("file://" + path);

        while ((!request.isDone) || (!string.IsNullOrEmpty(request.error)))
        {
            yield return new WaitForEndOfFrame();
        }
        if (request.texture != null)
        {
            // 画像を取り出す
            Texture2D texture = request.texture;

            // 読み込んだ画像からSpriteを作成する
            objChara.GetComponent<RectTransform>().sizeDelta = new Vector2(texture.width, texture.height);
            objChara.GetComponent<Image>().sprite = Sprite.Create(texture, new Rect(0, 0, texture.width, texture.height), new Vector2(0.5f, 0.5f));
        }
    }

    public void SkillChangeButton1()
    {
        StartCoroutine(SkillChange(1));
    }

    public void SkillChangeButton2()
    {
        StartCoroutine(SkillChange(2));
    }

    public IEnumerator SkillChange(int number)
    {
        if (number == 2)
        {
            for (int i = 0; i <= 20; i++)
            {
                objSkillSheet1.GetComponent<RectTransform>().localPosition = new Vector3(800 - i * 40, 0, 0);
                yield return null;
            }
            yield break;
        }

        if (number == 1)
        {
            for (int i = 0; i <= 20; i++)
            {
                objSkillSheet1.GetComponent<RectTransform>().localPosition = new Vector3(i * 40, 0, 0);
                yield return null;
            }
            yield break;
        }
    }

    public void SkillAdd(int number)
    {
        StartCoroutine("SkillAddCor", number);
    }

    public void SkillAddEnd(int number)
    {
        StopCoroutine("SkillAddCor");
    }

    public IEnumerator SkillAddCor(int number)
    {
        string[] str;
        if (loadedChara == false)
        {
            str = objSkill[number].GetComponent<Text>().text.Split('：');
            for (int i = 0; i < 2; i++)
            {
                skills[number]++; buyPoint--;
                if (skills[number] == 100 || buyPoint < 0) { buyPoint += (skills[number] - skillDefault[number]); skills[number] = skillDefault[number]; }
                objSkill[number].GetComponent<Text>().text = str[0] + '：' + skills[number].ToString();
                objBuyPoint.GetComponent<Text>().text = "残：" + buyPoint.ToString() + "P";
                yield return new WaitForSeconds(1.0f);
            }
            while (true)
            {
                skills[number]++; buyPoint--;
                if (skills[number] == 100 || buyPoint < 0) { buyPoint += (skills[number] - skillDefault[number]); skills[number] = skillDefault[number]; }
                objSkill[number].GetComponent<Text>().text = str[0] + '：' + skills[number].ToString();
                objBuyPoint.GetComponent<Text>().text = "残：" + buyPoint.ToString() + "P";
                yield return null; yield return null;
            }
        }
        else
        {
            objBuyPoint.GetComponent<Text>().text = "決定済";
        }
    }

    public void PushDecideButton()
    {
        PushDecideButtonIn();
    }

    private void PushDecideButtonIn()
    {
        for (int i = 0; i < STATUSNUM; i++)
        {
            PlayerPrefs.SetInt("Status" + i.ToString(), status[i]);
        }
        for (int i = 0; i < SKILLNUM; i++)
        {
            PlayerPrefs.SetInt("Skill" + i.ToString(), skills[i]);
        }
        PlayerPrefs.SetInt("耐久力", nowHP);
        PlayerPrefs.SetInt("マジック・ポイント", nowMP);
        PlayerPrefs.SetInt("正気度ポイント", nowSAN);
        PlayerPrefs.SetString("PlayerCharacterName", GameObject.Find("PlayerCharacterName").GetComponent<Text>().text);
        GetComponent<Utility>().StartCoroutine("LoadSceneCoroutine", "TitleScene");
    }

    public void PushRediceButton()
    {
        PushRediceButtonIn();
    }

    private void PushRediceButtonIn()
    {
        //スタンドアロン用は広告機能使えないのでコメントアウトする
        /*
        if (Advertisement.IsReady())
        {
            Advertisement.Show();
        }
        */
        
        for (int i = 0; i < SKILLNUM; i++)
        {
            skills[i] = 0;
        }
        MakeCharacter();
    }

    public void CSButton()
    {
        CSButtonIn();
    }

    private void CSButtonIn()
    {
        if (transform.GetChild(0).gameObject.activeSelf == false)
        {
            if (SceneManager.GetActiveScene().name == "NovelScene")
            {
                GameObject.Find("NovelManager").gameObject.GetComponent<ScenariosceneManager>().backLogCSFlag = true;
            }
            transform.GetChild(0).gameObject.SetActive(true);
            transform.GetChild(1).gameObject.GetComponent<Text>().text = "戻る";
        }
        else if (Camera.main.ScreenToWorldPoint(Input.mousePosition).y < -4.2f)//ボタン部以外の背景等も子オブジェクトなのでタップでボタン押された判定になって終了してしまう。それを避けるためにボタン部の位置をifで判定
        {
            if (SceneManager.GetActiveScene().name == "NovelScene")
            {
                GameObject.Find("NovelManager").gameObject.GetComponent<ScenariosceneManager>().backLogCSFlag = false;
            }
            transform.GetChild(0).gameObject.SetActive(false);
            transform.GetChild(1).gameObject.GetComponent<Text>().text = "Character\nsheet";
        }
    }

    public void CharacterIllustButton()
    {
        GetComponent<GracesGames.SimpleFileBrowser.Scripts.FileOpenManager>().GetFilePathWithKey("CharacterIllustPath");
    }

    // Update is called once per frame
    void Update () {
		
	}
}
