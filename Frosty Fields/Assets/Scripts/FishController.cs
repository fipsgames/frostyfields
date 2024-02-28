using UnityEngine;

public class FishController : MonoBehaviour {

    Animator animator;
    private AudioManager audiomanager;

    // Use this for initialization
    void Start()
    {
        animator = gameObject.GetComponent<Animator>();
        audiomanager = FindObjectOfType<AudioManager>();
        //gameObject.transform.position = gameObject.transform.position + new Vector3(0, 0.056f, 0);
  
    }

    public void GetCollected()
    {
        animator = gameObject.GetComponent<Animator>();
        animator.SetTrigger("GetCollected");
    }

    public void BecomeInactive()
    {
        //Logger.Log("animation event become inactive");
        transform.parent.gameObject.SetActive(false);
    }

    public void PlayAudio(string clip)
    {
        //Logger.Log("PLAYING " + clip + " AUDIO NOW");
        audiomanager.Play(clip);
    }
}
