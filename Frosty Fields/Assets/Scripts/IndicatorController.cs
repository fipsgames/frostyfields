using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class IndicatorController : MonoBehaviour {

    Animator animator;
    bool spawned;

	// Use this for initialization
	void Start () 
    {
        animator = gameObject.GetComponent<Animator>();
        spawned = false;
	}

    public void Spawn()
    {
        if(!spawned) animator.SetTrigger("Spawn"); spawned = true;
    }

    public void Despawn()
    {
        animator.SetTrigger("Despawn");
        spawned = false;
    }
}
