using UnityEngine;

[System.Serializable]
public class Tutorial
{
    public string name;
    public string text;
    public string requiredTutorialName;
    public string startCondition;
    public string endCondition;
    public float fadeTime;
    public int pos;
    public bool reacurring;

    [HideInInspector]
    public GameObject tutorialText;
}
