using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ButtonSound : MonoBehaviour
{

    private AudioManager audiomanager;
    public string audio_clip = "Button";
    // Start is called before the first frame update
    void Start()
    {
        audiomanager = FindObjectOfType<AudioManager>();
    }

    public void PlayButtonAudio()
    {
        //Logger.Log("BUTTON PLAYING " + audio_clip + " AUDIO NOW", this, true);
        audiomanager.Play(audio_clip);
    }
}
