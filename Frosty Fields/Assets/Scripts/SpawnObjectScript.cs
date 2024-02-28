using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using util;

public class SpawnObjectScript<T> : GameObjectScript<T>
{

    public float randomMin;
    public float randomMax;
    public float random { get; set; }
    public float distToPlayer { get; set; }
    private float speedScale = 5.0f;
    private  float delayScale = 0.8f;

    new public void Init(T modelReference, Vector3 worldPos)
    {
        base.Init(modelReference, worldPos);
        random = Random.Range(randomMin, randomMax);
        ////Logger.Log("random " + random);
    }

    public void SetSmoothDist(float dist)
    {
        distToPlayer = Util.DeAccelerationFraction(dist);
    }

    //get dist to player
    public float CalcDistToPlayer(GameObject player)
    {
        if (gameObject == null)
        {
            //Logger.Log("main gameobject was null");
            return 1.0f;
        }
        Vector2 player2D = new Vector2(player.transform.position.x, player.transform.position.z);
        Vector2 object2D = new Vector2(gameObject.transform.position.x, gameObject.transform.position.z);
        distToPlayer = (Mathf.Abs((player2D - object2D).magnitude + random));
        return distToPlayer;
    }

    public void SetValuesForDespawn(Vector3 spawnOffset)
    {
        initialMainPos = gameObject.transform.position;
        initialMainSpawnPos = initialMainPos + spawnOffset;
    }

    public void LevelSwitchConstruct(float fraction)
    {
        if (gameObject == null) return;
        float frac = 0;
        if (fraction > distToPlayer * delayScale)
        {
            frac = Util.DeAccelerationFraction(Mathf.Min((fraction - distToPlayer * delayScale) * speedScale, 1.0f));
            ////Logger.Log("end frac " + frac);
        }
        gameObject.transform.SetPositionAndRotation(Vector3.Lerp(initialMainSpawnPos, initialMainPos, frac), gameObject.transform.rotation);
        gameObject.transform.localScale = new Vector3(frac, frac, frac);
    }

    public void LevelSwitchDeconstruct(float fraction)
    {
        if (gameObject == null) return;
        float frac = 0;
        if (fraction > (1.0f - distToPlayer) * delayScale)
        {
            frac = Util.AccelerationFraction(Mathf.Min((fraction - (1.0f - distToPlayer) * delayScale) * speedScale, 1.0f));
            ////Logger.Log("end frac " + frac);
        }
        gameObject.transform.SetPositionAndRotation(Vector3.Lerp(initialMainPos, initialMainSpawnPos, frac), gameObject.transform.rotation);
        gameObject.transform.localScale = new Vector3(1 - frac, 1 - frac, 1 - frac);
    }
}
