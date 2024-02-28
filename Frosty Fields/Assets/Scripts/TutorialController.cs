using System.Collections;
using System.Collections.Generic;
using System;
using UnityEngine;
using TMPro;

public class TutorialController : MonoBehaviour
{

    public GameObject tutorialTextPrefab;
    public Tutorial[] tutorials;

    Persistence persistence;

    // Start is called before the first frame update
    void Awake()
    {
        foreach (Tutorial t in tutorials)
        {
            t.tutorialText = Instantiate(tutorialTextPrefab, transform) as GameObject;
            t.tutorialText.transform.SetParent(transform);
            t.tutorialText.GetComponent<RectTransform>().anchoredPosition = new Vector2(0, t.pos);
            t.tutorialText.GetComponent<TextMeshProUGUI>().text = t.text.Replace("\\n", "\n") ;
            t.tutorialText.SetActive(false);
        }
    }

    void Start()
    {
        persistence = FindObjectOfType<Persistence>();
        // HACK TO RESET ALL TUTORIALS
        
        foreach (Tutorial t in tutorials)
        {
            persistence.ResetTutorialFinished(t.name);
        }
        
    }

    private bool hasShownRequiredTuturial(string tutorialName)
    {
        if (tutorialName.Equals("")) return true;
        if (persistence.GetTutorialFinished(tutorialName)) return true;
        return false;
    }

    public void Trigger(string trigger){
        foreach (Tutorial t in tutorials)
        {
            if (String.Equals(t.endCondition, trigger))
            {
                if (t.tutorialText.activeSelf)
                {
                    t.tutorialText.GetComponent<CanvasFader>().Fade(false);
                }
            } else if (String.Equals(t.startCondition, trigger))
            {
                if (!persistence.GetTutorialFinished(t.name))
                {
                    if (hasShownRequiredTuturial(t.requiredTutorialName))
                    {
                        if (!t.reacurring)
                        {
                            persistence.AddTutorialFinished(t.name);
                        }
                        t.tutorialText.GetComponent<CanvasFader>().Fade(true);
                        t.tutorialText.GetComponent<CanvasFader>().Fade(false, t.fadeTime);
                        // this loop only temporarily disables any other tutorial, when another one has been started 
                        foreach (Tutorial t2 in tutorials)
                        {
                            if (t != t2 && t2.tutorialText.activeSelf)
                            {
                                t2.tutorialText.GetComponent<CanvasFader>().Fade(false);
                            }
                        }
                        // only one tutorial can be started at the same time
                        break;
                    }
                }
            }
        }
    }
}
