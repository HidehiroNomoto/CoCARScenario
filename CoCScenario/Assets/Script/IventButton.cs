using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class IventButton : MonoBehaviour {

    public int buttonNum=0;

	// Use this for initialization
	void Start () {
		
	}
	
	// Update is called once per frame
	void Update () {
		
	}

    public void PushIventButton()
    {
        MapScene m1 = GameObject.Find("GameObject").GetComponent<MapScene>();
        try { m1.objCCB.GetComponent<Image>().color = new Color(1.0f, 1.0f, 1.0f); } catch { }
        m1.selectNum = buttonNum; 
        this.GetComponent<Image>().color = new Color(1.0f,1.0f,0);
        m1.objCCB = this.gameObject;
        m1.SetIvent();
        try {m1.GetMap(); }catch{ }
    }
}
