using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class GSButton : MonoBehaviour {

    public int buttonNum = 0;
    // Use this for initialization
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {

    }

    public void PushGSButton()
    {
        ScenariosceneManager s1 = GameObject.Find("NovelManager").GetComponent<ScenariosceneManager>();
        try { s1.objGSB.GetComponent<Image>().color = new Color(1.0f, 1.0f, 1.0f); } catch { }
        s1.selectGS = buttonNum;
        this.GetComponent<Image>().color = new Color(1.0f, 1.0f, 0);
        s1.objGSB = this.gameObject;
        try { if (s1.objMake[3].activeSelf || s1.objMake[5].activeSelf) {AudioSource bgm = GameObject.Find("BGMManager").GetComponent<AudioSource>();bgm.loop=false; bgm.clip = s1.scenarioAudio[buttonNum]; bgm.Play();        //mp3ファイルの場合
                if (s1.sFileName[buttonNum].Substring(s1.sFileName[buttonNum].Length - 4) == ".mp3" || s1.sFileName[buttonNum].Substring(s1.sFileName[buttonNum].Length - 4) == ".MP3")
                {
                    GameObject.Find("Error").GetComponent<Text>().text = "※mp3ファイルはシナリオメーカーで試聴できません。\nシナリオプレイ時には通常通り再生されます。";
                }
            } } catch { }
    }
}
