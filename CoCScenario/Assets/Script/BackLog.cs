using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class BackLog : MonoBehaviour {
    GameObject obj;
    GameObject obj2;
    GameObject obj3;
    GameObject objGame;
    private bool backLog = false;
    public int startLog = 0;
	// Use this for initialization
	void Start () {
        obj = transform.GetChild(0).gameObject;
        obj2 = transform.GetChild(1).gameObject;
        obj3 = transform.GetChild(2).gameObject;
        obj.gameObject.SetActive(false);
        obj2.gameObject.SetActive(false);
        //PlayerPrefs.DeleteAll();
    }
	
	// Update is called once per frame
	void Update () {
	}

    public IEnumerator BackLogScroll(string[] logText,int startLog2)
    {
        Vector3 mousePos=Vector3.zero;
        int startLog3=startLog2;
        while (backLog == true)
        {
            //
            if (Input.GetMouseButtonDown(0))
            {
                mousePos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            }
            mousePos.z = 0;
            if (Input.GetMouseButton(0))
            {
                
                Vector3 diff = Camera.main.ScreenToWorldPoint(Input.mousePosition) - mousePos;

                //タッチ対応デバイス向け、1本目の指にのみ反応
                if (Input.touchSupported)
                {
                    diff = Camera.main.ScreenToWorldPoint(Input.GetTouch(0).position) - mousePos;
                }
                diff.z = 0;
                if (diff.y > mousePos.y+0.2f) { if (startLog < startLog2) { startLog++; }; mousePos.y +=0.2f; }
                if (diff.y < mousePos.y-0.2f) { if (startLog > 0) { startLog--; }; mousePos.y -=0.2f; }
            }

            if (Input.GetMouseButtonUp(0))
            {
                mousePos = Vector3.zero;
            }
            if (startLog3 != startLog)
            {
                startLog3 = startLog;
                obj2.GetComponent<Text>().text = "";
                for (int j = startLog; j < startLog2; j++)
                {
                    obj2.GetComponent<Text>().text+=logText[j];
                }
            }
            yield return null;
        }
        if (SceneManager.GetActiveScene().name=="NovelScene")
        {
            GameObject.Find("NovelManager").gameObject.GetComponent<ScenariosceneManager>().backLogCSFlag = false;
        }
    }
    
    public void BackLogButton()
    {
        BackLogButtonIn();
    }

    private void BackLogButtonIn()
    {
        int logNum = 0;
        int logNum2 = 0;
        string[] logText = new string[1000];
        int startLog2 = 0;
        if (SceneManager.GetActiveScene().name == "NovelScene")
        {
            GameObject.Find("NovelManager").gameObject.GetComponent<ScenariosceneManager>().backLogCSFlag = true;
        }
        if (backLog == false)
        {
            obj.gameObject.SetActive(true);
            obj2.gameObject.SetActive(true);
            obj2.GetComponent<Text>().text = "";
            logNum2 = PlayerPrefs.GetInt("最新ログ番号", 0);
            obj3.GetComponent<Text>().text = "戻る";
            if (PlayerPrefs.GetString("バックログ" + (logNum2 + 1).ToString(), "[NoLog!]") == "[NoLog!]") { logNum = 0; } else { logNum = logNum2 + 1; if (logNum >= 1000) { logNum = 0; } }
            for (int i = 0; i < 1000; i++)
            {
                logText[i] = PlayerPrefs.GetString("バックログ" + logNum.ToString(), "") + "\n";
                logNum++;
                if (logNum >= 1000) { logNum = 0; }
            }
            if (logNum2 >= 4) { startLog = logNum2 - 4; } else { startLog = 0; }
            startLog2 = startLog + 4;
            backLog = true;
            StartCoroutine(BackLogScroll(logText, startLog2));
        }
        else if (Camera.main.ScreenToWorldPoint(Input.mousePosition).y < -4.2f)//ボタン部以外の背景等も子オブジェクトなのでタップでボタン押された判定になってバックログが終了してしまう。それを避けるためにボタン部の位置をifで判定
        {
            obj.gameObject.SetActive(false);
            obj2.gameObject.SetActive(false);
            obj3.GetComponent<Text>().text = "BackLog";
            backLog = false;
        }
    }
}
