using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SceneMemory : MonoBehaviour
{
    public bool isMainMode;
    public bool initialMenu;
    public static SceneMemory instance;

    void Awake()
    {
        if(instance == null)
        {
            instance = this;
        }
        else
        {
            Destroy(gameObject);
        }

        DontDestroyOnLoad(gameObject);

        isMainMode = true;
        initialMenu = true;
    }
}
