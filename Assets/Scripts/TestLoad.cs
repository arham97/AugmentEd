using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

public class TestLoad : MonoBehaviour
{
    public bool switc;
    GameObject prefab; 
    GameObject prefab2;
    public GameObject p;
    // Start is called before the first frame update
    void Start()
    {
        switc = false;
        var myLoadedAssetBundle
            = AssetBundle.LoadFromFile(Path.Combine("Assets/AssetBundles", "chapter1"));
        if (myLoadedAssetBundle == null)
        {
            Debug.Log("Failed to load AssetBundle!");
            return;
        }
        prefab = myLoadedAssetBundle.LoadAsset<GameObject>("1");
        prefab2 = myLoadedAssetBundle.LoadAsset<GameObject>("2");
        
        
        Instantiate(prefab).transform.SetParent(p.transform);

    }

    // Update is called once per frame
    void Update()
    {
        if (switc == true){
            Destroy(prefab);
            Instantiate(prefab2);
            switc = false;
        }
    }



}
