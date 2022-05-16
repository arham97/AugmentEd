using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class logoRotate : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        this.gameObject.transform.Rotate(0,20*Time.deltaTime,0);
    }
}
