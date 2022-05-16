using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class AnimatorController : MonoBehaviour
{
    public Animator AnimCont;
    public SceneOpener SO;
    public GameObject popup;
    public Text PopupText;
    public Text PopupHeading;
    public bool directSignin;

    // Start is called before the first frame update
    void Start()
    {
        // SO = this.GetComponent<SceneOpener>();
    }

    // Update is called once per frame
    void Update()
    {

        if(Input.GetKeyDown(KeyCode.Escape))
        {
          AnimCont.SetTrigger("back");
        }

    }


    public void EnableCont()
    {
        if (directSignin)
        {
            SO.SceneChange(1);
        }
        AnimCont.SetTrigger("TapContTrig");
    }

/*
    public void ChangeAnimStateTrue(string AnimName)
    {
        AnimCont.SetBool(AnimName,true);
    }


    public void ChangeAnimStateFalse(string AnimName)
    {
        AnimCont.SetBool(AnimName,false);
    }

*/


    public void ChangeTriggerStateTrue(string AnimName)
    {
        AnimCont.SetTrigger(AnimName);
    }

    public void showpopup(string heading, string txt){
        PopupHeading.text = heading;
        PopupText.text = txt;
        popup.SetActive(true);
    }









}
