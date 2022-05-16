using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TrackingManager : MonoBehaviour
{
    // Start is called before the first frame update
    public GameManager gamemanager;

    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {
    //  Debug.Log("is tracking ");   
    }


    public void notTracking(){
        //this.gameObject.GetComponentInChildren<GameObject>().SetActive(false);
        Debug.Log("DEBUG: NOT TRACKING ");
        gamemanager.setTracking(false);
    }

    public void Tracking(int obj=0){
       // this.gameObject.GetComponentInChildren<GameObject>().SetActive(true);
        Debug.Log("DEBUG: TRACKING");
        gamemanager.setTracking(true);
        if (obj!=0 && gamemanager.chapterNumber != obj)
        {
            gamemanager.setChapter(obj);
        }
    }
}
