using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DontDestroy : MonoBehaviour
{
    void Awake()
    {
        Debug.Log($"{GetType().Name} Initialized");
        DontDestroyOnLoad(this.gameObject);
    }
}