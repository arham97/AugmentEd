using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SceneOpener : MonoBehaviour
{

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void SceneChange(int numb)
    {
        UnityEngine.SceneManagement.SceneManager.LoadScene(numb);
    }
}
