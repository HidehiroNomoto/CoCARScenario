﻿using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using System.Text;

[DefaultExecutionOrder(-1)]//utilityは他から引用されるのでstartを先行処理させる。
public class Utility : MonoBehaviour {
    public GameObject objBGM;                                  //BGMのオブジェクト
    private bool fadeFlag;                                      //フェードイン・フェードアウト中か否か
    public bool pushObjectFlag;                                 //ボタンオブジェクトのタップ(true)か画面自体（ストーリー進行）のタップ(false)かの判定
    public bool selectFlag;                                     //選択待ち中、どれかが選択されたか否かの判定
    public bool prints=false;
    // Use this for initialization
    void Start () {
        objBGM = GameObject.Find("BGMManager").gameObject as GameObject;
        pushObjectFlag = false;
    }

	
	// Update is called once per frame
	void Update () {
        if (Input.GetKey(KeyCode.LeftControl) && Input.GetKey(KeyCode.P)) { if (prints == false) { ScreenCapture.CaptureScreenshot(PlayerPrefs.GetInt("スクリーンショット", 0).ToString() + ".png"); PlayerPrefs.SetInt("スクリーンショット", PlayerPrefs.GetInt("スクリーンショット", 0) + 1); prints = true; } }
        else { prints = false; }
	}

    public IEnumerator LoadSceneCoroutine(string scene)
    {
        SceneManager.LoadScene(scene);
        yield return null;
    }

    public void BGMVolume(float volume)
    {
        PlayerPrefs.SetFloat("BGMVolume", volume);
        objBGM.GetComponent<AudioSource>().volume = volume;
    }

    public void SEVolume(float volume)
    {
        PlayerPrefs.SetFloat("SEVolume", volume);
    }

    public void BGMStop()
    {
        objBGM.GetComponent<AudioSource>().Stop();
    }

    public IEnumerator BGMFadeOut(int time)
    {
        while (fadeFlag == true) { yield return null; }//他でフェイドインフェイドアウト中なら待つ。
        fadeFlag = true;
        for (int i = 0; i < time; i++)
        {
            objBGM.GetComponent<AudioSource>().volume = PlayerPrefs.GetFloat("BGMVolume",0.8f) * (1.0f-(float)i/time);
            yield return null;
        }
        objBGM.GetComponent<AudioSource>().volume = 0f;//最終的には０に。（for文をi<=timeにするとtime=0で０除算が発生しうる構造になるので、最後のvol=0のみfor文から隔離）
        fadeFlag = false;
        yield return null;
    }

    public IEnumerator BGMFadeIn(int time)
    {
        while (fadeFlag == true) { yield return null; }//他でフェイドインフェイドアウト中なら待つ。
        fadeFlag = true;
        for (int i = 0; i < time; i++)
        {
            objBGM.GetComponent<AudioSource>().volume = PlayerPrefs.GetFloat("BGMVolume", 0.8f) * ((float)i / time);
            yield return null;
        }
        objBGM.GetComponent<AudioSource>().volume = PlayerPrefs.GetFloat("BGMVolume", 0.8f);//最終的には０に。（for文をi<=timeにするとtime=0で０除算が発生しうる構造になるので、最後のvol=BGMVolumeのみfor文から隔離）
        fadeFlag = false;
        yield return null;
    }

    public void BGMPlay(AudioClip bgm)
    {
        objBGM.GetComponent<AudioSource>().clip = bgm;
        objBGM.GetComponent<AudioSource>().Play();
    }

    public void SEPlay(AudioClip se)
    {
        GetComponent<AudioSource>().volume = PlayerPrefs.GetFloat("SEVolume", 0.8f);
        GetComponent<AudioSource>().PlayOneShot(se);
    }

    //画面が押されたかチェックするコルーチン
    public IEnumerator PushWait()
    {
        while (true)//ブレークするまでループを続ける。
        {
            if (Input.GetMouseButtonDown(0) == true)
            {
                yield return null;//本体に処理を返して他のオブジェクトのイベントトリガーを確認。
                if (pushObjectFlag == false)//フラグが立っていたらオブジェクト処理のためのタップだったと判定。
                {
                    yield break;//falseならコルーチン脱出
                }
                else
                {
                    yield return null;//trueならコルーチン継続
                }
            }
            else
            {
                yield return null;
            }
        }
    }

    public IEnumerator SelectWait()
    {
        selectFlag = false;
        while (true)//ループを続ける。
        {
            yield return null;
            if (selectFlag == true) { break; }
        }
    }

    public int DiceRoll(int diceNum,int diceMax)
    {
        int x=0;
        for (int i = 0; i < diceNum; i++) { x += Random.Range(0, diceMax)+1; }
        return x;
    }

    public string GetAppPath()
    {
        if (Application.platform == RuntimePlatform.WindowsEditor) { return @"C:\Users\hoto\Documents\GitHub\CoCARScenario\CoCScenario";}
        if (Application.platform == RuntimePlatform.Android) {
            string path;
            using (AndroidJavaClass androidJavaClass = new AndroidJavaClass("android.os.Environment"))
            {
                path = androidJavaClass.CallStatic<AndroidJavaObject>("getExternalStorageDirectory")
                    .Call<string>("getAbsolutePath") + "/Download";
            }
            return path; }
        return System.Windows.Forms.Application.StartupPath;
    }

    //URLへの遷移と、その前の演出等を見せるための待機をセットにしたコルーチン
    public IEnumerator GoToURL(string URL,float waitTime)
    {
        yield return new WaitForSeconds(waitTime);
        Application.OpenURL(URL);
    }






}
