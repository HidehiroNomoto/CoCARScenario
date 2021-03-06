﻿using UnityEngine;

public class BGMManager : MonoBehaviour {

    //BGM再生に伴うフラグを管理するスクリプト（BGMManager本体がdontdestroyオブジェクトなのでシーンをまたぐ際に使うフラグがある）
    public bgmFlag b1;
    //
    public string chapterName="start.txt";
    public string saveKey = "進行中シナリオ";
    //マルチプレイのフラグ
    public int multiPlay = 0;
    public string copyString = "";
    public string copyMapString = "";
    public string copyivent = "";
    public string[] gFileName = new string[99];
    public string[] sFileName = new string[40];
    public Sprite[] scenarioGraphic = new Sprite[100];       //シナリオ画像保存変数
    public AudioClip[] scenarioAudio = new AudioClip[40];    //シナリオＢＧＭ・ＳＥ保存変数
    public string folderChar = "";

    public struct bgmFlag
    {
        public bool bgmChangeFlag;//新しいシーンでBGMを新たに再生するか
        public int bgmNum;//新しいシーンでも流れ続ける場合のBGMの番号（scenario.csのbgmリスト番号）※スキップの場合には新たに流す必要があるため
        public bgmFlag(bool flag, int num) { this.bgmChangeFlag = flag; this.bgmNum = num; }
    }

    // Use this for initialization
    void Start () {
        if (Application.platform == RuntimePlatform.WindowsPlayer)
        {
           folderChar= @"\";
        }
        else
        {
            folderChar = @"/";
        }
    }

	
	// Update is called once per frame
	void Update () {
		
	}

    public void bgmChange(bool flag,int num)
    {
        b1=new bgmFlag(flag,num);
    }



}
