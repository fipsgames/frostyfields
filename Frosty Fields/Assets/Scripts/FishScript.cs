using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using gameState;
using action;

public class FishScript : SpawnObjectScript<DieableObject> {
    
    FishController fishController;
    Transform playerTrans;

    public void Start()
    {
        gameObject.transform.Rotate(new Vector3(0.0f, Random.Range(0, 6) * 60, 0.0f), Space.World);
    }


    public void Init(DieableObject modelReference, Vector3 worldPos, Transform playerTrans)
    {
        base.Init(modelReference, worldPos);
        this.playerTrans = playerTrans;
        fishController = gameObject.transform.GetChild(0).gameObject.GetComponent<FishController>();
    }

    public void RememberReference()
    {
        modelReferenceBefore = modelReference.Clone();
    }

    public void GenerateActions(int t)
    {
        if (!modelReference.alive && modelReferenceBefore.alive)
        {
            AddEventAction(Action.GETCOLLECTED_TRIGGER, t);
            AddEventAction(Action.ATTACHTOPLAYER_TRIGGER, t + 2);
        }
    }

    public void HandleEventActions(int t)
    {
        ////Logger.Log("Handling fish events on time " + t);

        //if(eventActionQueue.Count > 0) //Logger.Log("peek fish action: " + eventActionQueue.Peek().id + " " +eventActionQueue.Peek().t);
        while (HasEventActions() && eventActionQueue.Peek().t == t)
        {
            Action action = eventActionQueue.Dequeue();
            //Logger.Log("Acting on: fish action with id " + action.id + " and " + action.t);
            if (action.Matches(Action.GETCOLLECTED_TRIGGER, t))
            {
                ////Logger.Log("Fish GET COLLECTED action");
                fishController.GetCollected();
            }
            if (action.Matches(Action.ATTACHTOPLAYER_TRIGGER, t))
            {
                ////Logger.Log("Fish GET COLLECTED action");
                gameObject.transform.SetParent(playerTrans);
            }
        }
    }

}
