using UnityEngine;

public class PlayerController : MonoBehaviour {

    Animator animator;
    public GameObject splashPSGameobject;
    public GameObject smokePSGameobject;
    private ParticleSystem splashPS;
    private ParticleSystem smokePS;
    private AudioManager audiomanager;

    public GameObject wavePrefab;

    void Start()
    {
        animator = GetComponent<Animator>();
        splashPS = splashPSGameobject.GetComponent<ParticleSystem>();
        smokePS = smokePSGameobject.GetComponent<ParticleSystem>();
        audiomanager = FindObjectOfType<AudioManager>();
    }

    public void Idle()
    {
        //Logger.Log("IDLE ANIMATION", this, true);
        animator.SetTrigger("Idle");
    }

    public void Walk()
    {
        //Logger.Log("WALK ANIMATION", this, true);
        animator.SetTrigger("Walk");
    }

    public void Slitter()
    {
        //Logger.Log("SLIDE ANIMATION", this, true);
        animator.SetTrigger("Slide");
    }

    public void Roll()
    {
        //Logger.Log("ROLL ANIMATION", this, true);
        animator.SetTrigger("Roll");
    }

    public void Bump()
    {
        //Logger.Log("BUMP ANIMATION", this, true);
        animator.SetTrigger("Bump");
    }

    public void Win()
    {
        //Logger.Log("WIN ANIMATION", this, true);
        animator.SetTrigger("Win");
    }

    public void Fall()
    {
        //Logger.Log("FALL ANIMATION", this, true);
        animator.SetTrigger("Fall");
    }

    public void Burn()
    {
        //Logger.Log("BURN ANIMATION", this, true);
        animator.SetTrigger("Burn");
    }

    public void EnableSplashPS()
    {
        //Logger.Log("ENABLING SPLASH PS NOW", this, true);
        splashPS.Play();
    }

    public void EnableSmokePS()
    {
        //Logger.Log("ENABLING SMOKE PS NOW", this, true);
        smokePS.Play();
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
        Destroy(wave, 10);
    }

}
