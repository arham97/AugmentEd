using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
public class HitDetection : MonoBehaviour
{
    RaycastHit hit;
    Ray ray;
    
    // UI elements
    private Text label;
    private GameObject imagebg;

    

    void Start(){
        label = GameObject.Find("Label").GetComponent<Text>();
        label.text = "";
        imagebg = GameObject.Find("bgimage");
        imagebg.SetActive(false);
 
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetMouseButtonDown(0)){
            ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            if (Physics.Raycast(ray, out hit)) {
                imagebg.SetActive(true);
                label.text = hit.transform.name.ToString();
            }
            else {
                imagebg.SetActive (false);
                label.text = "";
            }
        }
    }
}
