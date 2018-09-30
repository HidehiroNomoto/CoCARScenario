using UnityEngine;

public class BGMManager : MonoBehaviour {

    //BGM再生に伴うフラグを管理するスクリプト（BGMManager本体がdontdestroyオブジェクトなのでシーンをまたぐ際に使うフラグがある）
    public bgmFlag b1;
    //
    public string chapterName="start.txt";
    public string saveKey = "進行中シナリオ";
    //マルチプレイのフラグ
    public int multiPlay = 0;

    public struct bgmFlag
    {
        public bool bgmChangeFlag;//新しいシーンでBGMを新たに再生するか
        public int bgmNum;//新しいシーンでも流れ続ける場合のBGMの番号（scenario.csのbgmリスト番号）※スキップの場合には新たに流す必要があるため
        public bgmFlag(bool flag, int num) { this.bgmChangeFlag = flag; this.bgmNum = num; }
    }

    // Use this for initialization
    void Start () {

    }

	
	// Update is called once per frame
	void Update () {
		
	}

    public void bgmChange(bool flag,int num)
    {
        b1=new bgmFlag(flag,num);
    }



}
