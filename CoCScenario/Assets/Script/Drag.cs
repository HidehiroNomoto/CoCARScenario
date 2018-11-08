using UnityEngine;
using UnityEngine.UI;

public class Drag : MonoBehaviour
{
    public GameObject copy;
    bool dragFlag=false;
    float beforepoint;
    float afterpoint;

    private void Start()
    {
        copy = GameObject.Find("Copy");
    }

    public void DragStart()
    {
        if (this.name.Contains("Ivent"))
        {
            if (this.GetComponentInChildren<Text>().text.Contains("[system]")) { return; }
            MapScene m1 = GameObject.Find("GameObject").GetComponent<MapScene>();
            this.GetComponent<IventButton>().PushIventButton();
        }
        if (this.name.Contains("Command"))
        {
            ScenariosceneManager s1 = GameObject.Find("NovelManager").GetComponent<ScenariosceneManager>();
            this.GetComponent<CommandButton>().PushCommandButton();
        }
        dragFlag = true;
        copy.SetActive(true);
        copy.transform.Find("Text").GetComponent<Text>().text = this.GetComponentInChildren<Text>().text;
        Vector3 TapPos = Input.mousePosition;
        TapPos.z = 10f;
        beforepoint = TapPos.y;
    }

    public void OnDrag()
    {
        if (dragFlag==false) { return; }
        Vector3 TapPos = Input.mousePosition;
        TapPos.z = 10f;
        copy.GetComponent<RectTransform>().position = TapPos;
    }


    public void DragEnd()
    {
        string tmp;
        string tmp2;
        if (dragFlag == false) { return; }
        copy.SetActive(false);
        dragFlag = false;
        int afterSelect = 0;
        Vector3 TapPos = Input.mousePosition;
        TapPos.z = 10f;
        afterpoint = TapPos.y;
        if (this.name.Contains("Ivent"))
        {
            MapScene m1 = GameObject.Find("GameObject").GetComponent<MapScene>();
            afterSelect = m1.selectNum - (int)(afterpoint - beforepoint) / 25;
            if (afterpoint - beforepoint > 0) { afterSelect--; }
            if (afterpoint - beforepoint < 0) { afterSelect++; }
            if (afterSelect > 0 && afterSelect<m1.mapData.Count)
            {
                if (afterSelect < m1.selectNum)
                {
                    tmp = m1.mapData[m1.selectNum];//後のを一時保存
                    tmp2 = m1.objIB[m1.selectNum].GetComponentInChildren<Text>().text;
                    for (int i = m1.selectNum; i > afterSelect; i--)
                    {
                        m1.mapData[i] = m1.mapData[i-1];//後ずらし
                        m1.objIB[i].GetComponentInChildren<Text>().text = m1.objIB[i-1].GetComponentInChildren<Text>().text;
                    }
                    m1.mapData[afterSelect] = tmp;//先頭に入れる
                    m1.objIB[afterSelect].GetComponentInChildren<Text>().text = tmp2;
                }
                if (afterSelect > m1.selectNum)
                {
                    tmp = m1.mapData[m1.selectNum];//前のを一時保存
                    tmp2 = m1.objIB[m1.selectNum].GetComponentInChildren<Text>().text;
                    for (int i = m1.selectNum; i < afterSelect; i++)
                    {
                        m1.mapData[i] = m1.mapData[i+1];//前ずらし
                        m1.objIB[i].GetComponentInChildren<Text>().text = m1.objIB[i + 1].GetComponentInChildren<Text>().text;
                    }
                    m1.mapData[afterSelect] = tmp;//最後に入れる
                    m1.objIB[afterSelect].GetComponentInChildren<Text>().text = tmp2;
                }
            }
            m1.selectNum = afterSelect;
            m1.objIB[m1.selectNum].GetComponent<IventButton>().PushIventButton();
        }
        if (this.name.Contains("Command"))
        {
            ScenariosceneManager s1 = GameObject.Find("NovelManager").GetComponent<ScenariosceneManager>();
            afterSelect = s1.selectNum - (int)(afterpoint - beforepoint) / 25;
            if (afterpoint - beforepoint > 0) { afterSelect--; }
            if (afterpoint - beforepoint < 0) { afterSelect++; }
            if (afterSelect >= 0 && afterSelect<s1.commandData.Count)
            {
                if (afterSelect < s1.selectNum)
                {
                    tmp = s1.commandData[s1.selectNum];//後のを一時保存
                    tmp2 = s1.objCB[s1.selectNum].GetComponentInChildren<Text>().text;
                    for (int i = s1.selectNum; i > afterSelect; i--)
                    {
                        s1.commandData[i] = s1.commandData[i - 1];//後ずらし
                        s1.objCB[i].GetComponentInChildren<Text>().text = s1.objCB[i - 1].GetComponentInChildren<Text>().text;
                    }
                    s1.commandData[afterSelect] = tmp;//先頭に入れる
                    s1.objCB[afterSelect].GetComponentInChildren<Text>().text = tmp2;
                }
                if (afterSelect > s1.selectNum)
                {
                    tmp = s1.commandData[s1.selectNum];//前のを一時保存
                    tmp2 = s1.objCB[s1.selectNum].GetComponentInChildren<Text>().text;
                    for (int i = s1.selectNum; i < afterSelect; i++)
                    {
                        s1.commandData[i] = s1.commandData[i + 1];//前ずらし
                        s1.objCB[i].GetComponentInChildren<Text>().text = s1.objCB[i + 1].GetComponentInChildren<Text>().text;
                    }
                    s1.commandData[afterSelect] = tmp;//最後に入れる
                    s1.objCB[afterSelect].GetComponentInChildren<Text>().text = tmp2;
                }
            }
            s1.selectNum = afterSelect;
            s1.objCB[s1.selectNum].GetComponent<CommandButton>().PushCommandButton();
            for (int i = 0; i < s1.objCB.Count; i++)
            {
                if (s1.commandData[i].Length > 7 && s1.commandData[i].Substring(0, 7) == "Select:") { s1.NextSkipMake(10, i); }
                else if (s1.commandData[i].Length > 7 && s1.commandData[i].Substring(0, 7) == "Hantei:") { s1.NextSkipMake(11, i); }
                else if (s1.commandData[i].Length > 7 && s1.commandData[i].Substring(0, 7) == "Battle:") { s1.NextSkipMake(12, i); }
                else if (s1.commandData[i].Length > 11 && s1.commandData[i].Substring(0, 11) == "FlagBranch:") { s1.NextSkipMake(13, i); }
                else if (s1.commandData[i].Length > 11 && s1.commandData[i].Substring(0, 11) == "Difference:") { s1.NextSkipMake(17, i); }
                else if (s1.commandData[i].Length > 6 && s1.commandData[i].Substring(0, 6) == "Equal:") { s1.NextSkipMake(20, i); }
                else { s1.NextSkipMake(0, i); }
            }//分岐表示のつけなおし
        }
    }
}
