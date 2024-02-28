using UnityEngine.Audio;
using System;
using UnityEngine;

using UnityEditor;

public class AudioManager : MonoBehaviour
{

    public Boolean muted;
    public Sound[] sounds;

    public static AudioManager instance;

    private void Awake()
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

        foreach (Sound s in sounds)
        {
            s.source = gameObject.AddComponent<AudioSource>();
            s.source.clip = s.clip;
            s.source.volume = s.volume;
            s.source.pitch = s.pitch;
            s.source.loop = s.loop;
        }
    }

    public void Start()
    {
        Play("Theme");
    }

    public void Play (string name)
    {
        Sound s = Array.Find(sounds, sound => sound.name == name);
        if (s == null)
        {
            Debug.LogWarning("Sound: " + name + " was not found!");
            return;
        }
        if (muted)
        {
            return;
        }
        s.source.Play();
    }

    public void Stop (string name)
    {
        Sound s = Array.Find(sounds, sound => sound.name == name);
        if (s == null)
        {
            Debug.LogWarning("Sound: " + name + " was not found!");
            return;
        }
        if (muted)
        {
            return;
        }
        s.source.Stop();
    }

    public void StopAll(){
        foreach (Sound s in sounds)
        {
            s.source.Stop();
        }
    }


}
