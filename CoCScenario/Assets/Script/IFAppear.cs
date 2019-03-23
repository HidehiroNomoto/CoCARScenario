using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class IFAppear : MonoBehaviour
{
    public GameObject IFObj;
    public void IFAppears()
    {
        if (GetComponent<Dropdown>().value==70) { IFObj.SetActive(true); } else { IFObj.SetActive(false); }
    }
    public void IFAppears2()
    {
        if (GetComponent<Dropdown>().value == 2) { IFObj.SetActive(false); } else { IFObj.SetActive(true); }
    }

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
