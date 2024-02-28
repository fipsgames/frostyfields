using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Logger : MonoBehaviour
{
    private static Logger instance = null;
    private static RectTransform content;
    public string filter;
    public bool hide;

    private void Awake()
    {
        /*
        if (instance == null)
        {
            instance = this;
            content = FindContent(this.gameObject);
        }
        else if (instance != this)
            Destroy(gameObject);

        if (hide)
        {
            this.gameObject.SetActive(false);
        }
        */
    }

    public RectTransform FindContent(GameObject ScrollViewObject)
    {
        RectTransform RetVal = null;
        Transform[] Temp = ScrollViewObject.GetComponentsInChildren<Transform>();
        foreach (Transform Child in Temp)
        {
            if (Child.name == "Content") { RetVal = Child.gameObject.GetComponent<RectTransform>(); }
        }
        return RetVal;
    }

    public static void Log(string value = "", MonoBehaviour sender = null, bool console = false, bool screen = true)
    {
        if (!value.Contains(instance.filter))
        {
            return;
        }
        String msg = value;
        if(sender != null)
        { 
            msg = sender.name + " " + msg;
        }
        if (console)
        {
            Debug.Log(msg);
        }
        if (screen)
        {
            DefaultControls.Resources TempResource = new DefaultControls.Resources();
            GameObject NewText = DefaultControls.CreateText(TempResource);
            NewText.GetComponent<Text>().text = msg;
            NewText.GetComponent<Text>().color = Color.red;
            NewText.AddComponent<LayoutElement>();
            NewText.transform.SetParent(content);
            Canvas.ForceUpdateCanvases();
            instance.GetComponent<ScrollRect>().verticalScrollbar.value = 0f;
            Canvas.ForceUpdateCanvases();
        }
    }
}
