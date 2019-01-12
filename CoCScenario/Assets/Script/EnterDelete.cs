using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class EnterDelete : MonoBehaviour
{

    public void EnterDeletes()
    {
        GetComponent<InputField>().text = GetComponent<InputField>().text.Replace("\n","");
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
