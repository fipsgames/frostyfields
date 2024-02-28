using UnityEngine;
using gameState;
using action;

public class GroundScript : SpawnObjectScript<Ground> {
    
    public int id { get; set; }
    public GameObject snow;
    public GameObject ice;
    public GameObject water;
    bool bounces = false;

    public void Start()
    {
        gameObject.transform.Rotate(new Vector3(0.0f, Random.Range(0, 6) * 60 , 0.0f), Space.World);
    }

    public new void Init(Ground modelReference, Vector3 worldPos){
        base.Init(modelReference, worldPos);
        id = modelReference.id;
    }

    public void RememberReference()
    {
        this.modelReferenceBefore = modelReference.Clone();
    }

    public void GenerateActions(int t, bool allFish)
    {
        if (modelReference.id != modelReferenceBefore.id)
        {
            if (modelReference.id == Ground.SNOW)
            {
                AddEventAction(Action.TO_SNOW_TRIGGER, t + 1);
            }
            else if (modelReference.id == Ground.ICE)
            {
                AddEventAction(Action.TO_ICE_TRIGGER, t + 1);
            }
            else if (modelReference.id == Ground.WATER)
            {
                AddEventAction(Action.BREAK_TRIGGER, t);
                AddEventAction(Action.TO_WATER_TRIGGER, t + 2);
            }
        } 
        else if(id == Ground.GOAL && allFish && !bounces)
        {
            bounces = true; // this action should only be added once
            AddEventAction(Action.BOUNCE_TRIGGER, t+2);
            ////Logger.Log("Added bounce action");
        }
    }

    public void HandleEventActions(int t)
    {
        while (HasEventActions() && eventActionQueue.Peek().t == t)
        {
            Action action = eventActionQueue.Dequeue();
            //Logger.Log("Acting on: ground action with id " + action.id + " and " + action.t);
            if (action.Matches(Action.BREAK_TRIGGER, t))
            {
                ////Logger.Log("Ground BREAK action");
                gameObject.transform.GetChild(0).gameObject.GetComponent<CrackController>().Break();
            }
            if (action.Matches(Action.TO_SNOW_TRIGGER, t))
            {
                ////Logger.Log("Ground TO SNOW action");
                ChangeTo(modelReference.id);
            }
            if (action.Matches(Action.TO_ICE_TRIGGER, t))
            {
                ////Logger.Log("Ground TO ICE action");
                ChangeTo(modelReference.id);
            }
            if (action.Matches(Action.TO_WATER_TRIGGER, t))
            {
                ////Logger.Log("Ground TO SNOW action");
                ChangeTo(modelReference.id);
            }
            /*
            if (action.Matches(Action.BOUNCE_TRIGGER, t))
            {
                ////Logger.Log("sTART BOUNCE action execution");
                // bouncing goal action changed into smoke particlesystem
                gameObject.transform.GetChild(0).gameObject.GetComponent<GoalController>().EnableSmokePS();
            }
            */
        }
    }

    public void ChangeTo(int id)
    {
        Transform gameObjTransform = gameObject.transform;
        GameObject child = gameObject.transform.GetChild(0).gameObject; 
        Destroy(child);
        switch (id)
        {
            case Ground.SNOW:
                child = Instantiate(snow, gameObjTransform.position, gameObjTransform.rotation, gameObject.transform) as GameObject;
                child.GetComponent<GroundSpawnController>().Spawn();
                break;
            case Ground.ICE:
                child = Instantiate(ice, gameObjTransform.position, gameObjTransform.rotation, gameObject.transform) as GameObject;
                child.GetComponent<GroundSpawnController>().Spawn();
                break;
            case Ground.WATER:
                child = Instantiate(water, gameObjTransform.position, gameObjTransform.rotation, gameObject.transform) as GameObject;
                GameObject tmp = gameObject.transform.GetChild(0).gameObject;
                //Logger.Log("changed to water " + tmp.name);
                break;
        }
        this.id = id;
    }
}
