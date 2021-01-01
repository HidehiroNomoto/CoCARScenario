using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using UnityEngine.UI;

public class MapDrag : MonoBehaviour
{

    long beforelatitude;
    long beforelongitude;
    Vector3 startPos=new Vector3(0,0,0);
    Vector3 startMapLocalPosition=new Vector3(0,0,0);
    public GameObject map;
    public GameObject manager;
    public GameObject longitudeInput;
    public GameObject latitudeInput;
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void DragStart()
    {
        startPos = Input.mousePosition;
        startPos.z = 10f;
        startMapLocalPosition = map.GetComponent<RectTransform>().localPosition;
    }

    public void OnDrag()
    {
        //画像を実際に動かす
        Vector3 TapPos = Input.mousePosition;
        TapPos.z = 10f;
        map.GetComponent<RectTransform>().localPosition = startMapLocalPosition+TapPos-startPos; 
    }


    public void DragEnd()
    {
        MapScene m1=manager.GetComponent<MapScene>();
        double zoomPow = Math.Pow(2, m1.zoom) * 0.4266666666;
        Vector3 TapPos = Input.mousePosition;
        TapPos.z = 10f;
        map.GetComponent<RectTransform>().localPosition = startMapLocalPosition + TapPos - startPos;
        m1.longitude-=(TapPos.x - startPos.x)/ (2.05993652344*zoomPow*Math.Cos(m1.latitude * (Math.PI / 180)));
        m1.latitude -= (TapPos.y - startPos.y)/( 2.05993652344 * zoomPow);
        while (m1.longitude >= 180) { m1.longitude -= 360; }
        while (m1.longitude < -180) { m1.longitude +=360; }
        //緯度経度を計算(latitudeとlongitudeに代入)
        //        targetX = (float)((longitude - longitudeMap) * 2.05993652344 * zoomPow * Math.Cos(latitude * (Math.PI / 180)));
        //        targetY = (float)((latitude - latitudeMap) * 2.05993652344 * zoomPow);
        //緯度経度をインプットフィールドに入力
        if (m1.selectNum==0) { longitudeInput =GameObject.Find("InputFieldB");latitudeInput = GameObject.Find("InputFieldA"); } else { longitudeInput = GameObject.Find("InputField2"); latitudeInput = GameObject.Find("InputField1"); }
        if(longitudeInput!=null){
        longitudeInput.GetComponent<InputField>().text= m1.longitude.ToString();
        latitudeInput.GetComponent<InputField>().text = m1.latitude.ToString();
        }
            //マップ範囲外になりそうなら再読み込み
            m1.GetMap();
    }


}
