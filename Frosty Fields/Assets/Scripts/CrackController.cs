using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CrackController : MonoBehaviour {

    Animator animator;
    private AudioManager audiomanager;

    // Use this for initialization
    void Start()
    {
        animator = gameObject.GetComponent<Animator>();
        audiomanager = FindObjectOfType<AudioManager>();
    }

    public void Break()
    {
        animator = gameObject.GetComponent<Animator>();
        animator.SetTrigger("Break");
        //Logger.Log("CRACK BREAK ANIMATION TRIGGERD");
    }

    public void PlayAudio(string clip)
    {
        //Logger.Log("PLAYING " + clip + " AUDIO NOW");
        audiomanager.Play(clip);
    }
}
