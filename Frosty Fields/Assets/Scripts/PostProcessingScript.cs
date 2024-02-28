using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using util;

public class PostProcessingScript : MonoBehaviour
{

    Vector3 oldPos;
    Vector3 goalPos;
    Vector3 currPos;
    public float speed;
    public int numberVolumes;
    float fraction;

    void Start()
    {
        oldPos = new Vector3();
        goalPos = new Vector3();
        currPos = new Vector3();
        fraction = -1.0f;
        transform.localPosition = currPos;
    }

    void Update()
    {
        if (fraction >= 0.0f && fraction <= 1.0f)
        {
            fraction += speed * Time.deltaTime;
            ////Logger.Log("PostProcessingPalette fraction: " + fraction + " smooth: " + Util.SmoothFraction(fraction), this, true);
            currPos = Vector3.Lerp(oldPos, goalPos, Util.SmoothFraction(fraction));
            transform.localPosition = currPos;
            ////Logger.Log("PostProcessingPalette moved to " + gameObject.transform.position, this, true);
        }
    }

    public void SetGoalProgress(float goalProgress)
    {
        ////Logger.Log("PostProcessingPalette set goalProgess to " + goalProgress, this, true);
        this.oldPos = this.currPos;
        this.goalPos = new Vector3(goalProgress * (numberVolumes-1), 0, 0);
        ////Logger.Log("PostProcessingPalette set goallocalPosition to " + goalPos.ToString(), this, true);
        fraction = 0.0f;
    }
}
