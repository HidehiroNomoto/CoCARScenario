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
    }
}
