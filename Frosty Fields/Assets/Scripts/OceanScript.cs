using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using util;

public class OceanScript : MonoBehaviour {

    Vector3 oldPos;
    Vector3 goalPos;
    Vector3 currPos;
    public float speed;
    float fraction;

    void Start () {
        oldPos = new Vector3();
        goalPos = new Vector3();
        currPos = new Vector3();
        fraction = -1.0f;
	}
	
	void Update () {
        if(fraction >= 0.0f && fraction <= 1.0f){
            fraction += speed * Time.deltaTime;
            currPos = Vector3.Lerp(oldPos, goalPos, Util.SmoothFraction(fraction));
            gameObject.transform.SetPositionAndRotation(currPos, Quaternion.identity);
            ////Logger.Log("OCEAN moved to " + gameObject.transform.position);
        }
	}

    public void SetGoalPos(Vector3 goalPos){
        this.oldPos = this.currPos;
        this.goalPos = goalPos;
        fraction = 0.0f;
    }
}
