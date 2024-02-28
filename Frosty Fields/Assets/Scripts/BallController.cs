using System.Collections;
using System.Collections.Generic;
using UnityEngine.Audio;
using UnityEngine;

public class BallController : MonoBehaviour {

    public float rollAngle;
    public float moveSpeed { get; set; }
    Animator animator;
    private AudioManager audiomanager;
    public GameObject smokePSGameobject;
    public GameObject splashPSGameobject;
    public GameObject wavePrefab;

    private void Start()
    {
        animator = gameObject.GetComponent<Animator>();
        audiomanager = FindObjectOfType<AudioManager>();
    }

    public void RotateTo(Vector3 goal)
    {
        gameObject.transform.RotateAround(gameObject.transform.position, Vector3.Cross(Vector3.up, goal - gameObject.transform.position), rollAngle * moveSpeed * Time.deltaTime);
    }

    public void Fall(){
        animator.SetTrigger("Fall");
    }

    public void Burn()
    {
        animator.SetTrigger("Burn");
    }

    public void EnableSmokePS(){
        //Logger.Log("ENABLING SMOKE PS NOW");
        smokePSGameobject.GetComponent<ParticleSystem>().Play();
    }

    public void EnableSplashPS()
    {
        //Logger.Log("ENABLING SPLASH PS NOW");
        splashPSGameobject.GetComponent<ParticleSystem>().Play();
    }

    public void PlayAudio(string clip)
    {
        //Logger.Log("PLAYING " + clip + " AUDIO NOW");
        audiomanager.Play(clip);
    }

    public void SpawnWave(){
        //Logger.Log("SPAWNING WAVE! at: " + transform.position.ToString(),this, true);
        
        GameObject wave = Instantiate(wavePrefab, new Vector3(transform.position.x, wavePrefab.transform.position.y, transform.position.z), wavePrefab.transform.rotation) as GameObject;
        wave.GetComponent<Animator>().SetTrigger("Start");
        Destroy(wave, 20);
    }
}
