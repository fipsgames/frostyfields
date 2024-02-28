using UnityEngine;

public class GroundSpawnController : MonoBehaviour {

    Animator animator;

	// Use this for initialization
	void Start () {
        ////Logger.Log("initialized snow animator");
        animator = gameObject.GetComponent<Animator>();
	}
	
    public void Spawn()
    {
        animator = gameObject.GetComponent<Animator>();
        animator.SetTrigger("Spawn");
    }
}
