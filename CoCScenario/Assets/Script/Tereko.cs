using System.Text;//StringBuilder用
using UnityEngine;
using UnityEngine.UI;

public class Tereko : MonoBehaviour
{

    // Start is called before the first frame update
    void Start()
    {
    }


private string before = "";
    private int start = 0;
public void TerekoBreaker()
{
        string text = gameObject.GetComponent<InputField>().text;
    StringBuilder sb = new StringBuilder(text);
        StringBuilder sb3 = new StringBuilder("");
        
        if (before.Length >= text.Length) { before = text; return; }
        //テレコになってしまったかチェック
        //二分探索でNlogNまで計算量を落とせば実用に耐える。
        for (int i = 0; i < text.Length; i++) { for (int j = 0; j < text.Length; j+=2) { } }

    StringBuilder sb2 = new StringBuilder(before);
        if (sb == sb2) {//テレコになってる時のみ
            sb.Insert(start,sb3);
            gameObject.GetComponent<InputField>().text = sb.ToString();
        }
        before = text;
}

// Update is called once per frame
void Update()
    {
    }
}
