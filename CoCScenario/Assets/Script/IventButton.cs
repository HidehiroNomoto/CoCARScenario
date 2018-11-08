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
        m1.selectNum = buttonNum; 
        this.GetComponent<Image>().color = new Color(1.0f,1.0f,0);
        m1.SetIvent();
        try {m1.GetMap(); }catch{ }
        if (Input.GetKey(KeyCode.LeftShift) || Input.GetKey(KeyCode.RightShift))
        {
            if (m1.selectBefore>=0)
            {
                if (m1.selectNum > 0)
                {
                    if (m1.selectNum > m1.selectBefore)
                    {
                        for (int i = m1.selectBefore; i < m1.selectNum; i++) { m1.multiSelect.Add(i); m1.objIB[i].GetComponent<Image>().color = new Color(1.0f, 1.0f, 0); }
                    }
                    if (m1.selectNum < m1.selectBefore)
                    {
                        for (int i = m1.selectBefore; i > m1.selectNum; i--) { m1.multiSelect.Add(i); m1.objIB[i].GetComponent<Image>().color = new Color(1.0f, 1.0f, 0); }
                    }
                    m1.selectBefore = -1;
                }
                else
                {
                    foreach (GameObject tmpObj in m1.objIB) { if (tmpObj != m1.objIB[m1.selectNum]) { tmpObj.GetComponent<Image>().color = new Color(1.0f, 1.0f, 1.0f); } }
                }
            }
            else
            {
                if (m1.selectNum > 0)
                {
                    m1.selectBefore = m1.selectNum;
                    foreach (GameObject tmpObj in m1.objIB) { if (tmpObj != m1.objIB[m1.selectNum]) { tmpObj.GetComponent<Image>().color = new Color(1.0f, 1.0f, 1.0f); } }
                    m1.multiSelect.Clear();
                }//スタート座標を複数選択に含まれると困るので、0は抜く。
            }
        }
        else
        {
            m1.selectBefore = -1;
            m1.multiSelect.Clear();
            foreach (GameObject tmpObj in m1.objIB) { if (tmpObj != m1.objIB[m1.selectNum]) {tmpObj.GetComponent<Image>().color = new Color(1.0f, 1.0f, 1.0f); } }
        }
    }
}
