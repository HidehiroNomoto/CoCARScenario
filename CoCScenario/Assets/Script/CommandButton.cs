using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class CommandButton : MonoBehaviour {

    public int buttonNum = 0;

    // Use this for initialization
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {

    }

    public void PushCommandButton()
    {
        ScenariosceneManager s1 = GameObject.Find("NovelManager").GetComponent<ScenariosceneManager>();
        try { s1.objCCB.GetComponent<Image>().color = new Color(1.0f, 1.0f, 1.0f); } catch { }
        s1.selectNum = buttonNum;
        this.GetComponent<Image>().color = new Color(1.0f, 1.0f, 0);
        s1.objCCB = this.gameObject;
        s1.SetCommand();
        if (Input.GetKey(KeyCode.LeftShift) || Input.GetKey(KeyCode.RightShift))
        {
            if (s1.selectBefore >= 0)
            {
                if (s1.selectNum >= 0)
                {
                    if (s1.selectNum > s1.selectBefore)
                    {
                        for (int i = s1.selectBefore; i < s1.selectNum; i++) { s1.multiSelect.Add(i); s1.objCB[i].GetComponent<Image>().color = new Color(1.0f, 1.0f, 0); }
                    }
                    if (s1.selectNum < s1.selectBefore)
                    {
                        for (int i = s1.selectBefore; i > s1.selectNum; i--) { s1.multiSelect.Add(i); s1.objCB[i].GetComponent<Image>().color = new Color(1.0f, 1.0f, 0); }
                    }
                    s1.selectBefore = -1;
                }
                else
                {
                    foreach (GameObject tmpObj in s1.objCB) { if (tmpObj != s1.objCB[s1.selectNum]) { tmpObj.GetComponent<Image>().color = new Color(1.0f, 1.0f, 1.0f); } }
                }
            }
            else
            {
                if (s1.selectNum >= 0)
                {
                    s1.selectBefore = s1.selectNum;
                    foreach (GameObject tmpObj in s1.objCB) { if (tmpObj != s1.objCB[s1.selectNum]) { tmpObj.GetComponent<Image>().color = new Color(1.0f, 1.0f, 1.0f); } }
                    s1.multiSelect.Clear();
                }
            }
        }
        else
        {
            s1.selectBefore = -1;
            s1.multiSelect.Clear();
            foreach (GameObject tmpObj in s1.objCB) { if (tmpObj != s1.objCB[s1.selectNum]) { tmpObj.GetComponent<Image>().color = new Color(1.0f, 1.0f, 1.0f); } }
        }



    }
}
