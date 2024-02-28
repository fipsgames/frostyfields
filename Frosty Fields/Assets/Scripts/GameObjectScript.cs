using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using action;

public class GameObjectScript<T> : MonoBehaviour {

    public T modelReference { get; set; }
    public T modelReferenceBefore { get; set; }
    public Queue<Action> eventActionQueue { get; set; }
    public Queue<Action> moveActionQueue { get; set; }
    public Vector3 initialMainSpawnPos { get; set; }
    public Vector3 initialMainPos { get; set; }

    public void Init(T modelReference, Vector3 worldPos){
        initialMainPos = worldPos;
        initialMainSpawnPos = gameObject.transform.position;
        this.modelReference = modelReference;
        modelReferenceBefore = default(T);
        eventActionQueue = new Queue<Action>();
        moveActionQueue = new Queue<Action>();
    }

    /*
    void Update()
    {
        if (gameObject == null) return;
        if (eventActionQueue == null) return;
        if (moveActionQueue == null) return;
        if (HasEventActions()) //Logger.Log("" + gameObject.name + " has EVENT actions");
        if (HasMoveActions()) //Logger.Log("" + gameObject.name + " has MOVE actions");
    }
    */

    public void AddMoveAction(int id, int t)
    {
        moveActionQueue.Enqueue(new Action(id, t));
        ////Logger.Log("Added action (id:" + id + " t:" + t + ")");
    }

    public void AddMoveAction(int id, int t, float x, float y)
    {
        moveActionQueue.Enqueue(new Action(id, t, x, y));
        ////Logger.Log("Added action (id:" + id + " t:" + t + ")");
    }

    public void AddEventAction(int id, int t)
    {
        eventActionQueue.Enqueue(new Action(id, t));
        ////Logger.Log("Added action (id:" + id + " t:" + t + ")");
    }

    public void AddEventAction(int id, int t, float x, float y)
    {
        eventActionQueue.Enqueue(new Action(id, t, x, y));
        ////Logger.Log("Added action (id:" + id + " t:" + t + ")");
    }

    public bool HasActions(){
        if(HasMoveActions() || HasEventActions()){
            ////Logger.Log("THIS GAME OBJECT HAS STILL ACTIONS!! " + gameObject.name + " move: " + moveActionQueue.Count + " event: " + eventActionQueue.Count );
            foreach (Action action in eventActionQueue){
                ////Logger.Log("ACTION: " + action.id + " T:" + action.t);
            }
        }
        return (HasMoveActions() || HasEventActions());
    }

    public bool HasMoveActions()
    {
        return (moveActionQueue.Count != 0);
    }

    public bool HasEventActions()
    {
        return (eventActionQueue.Count != 0);
    }

    public void RemoveAllActions(){
        moveActionQueue = new Queue<Action>();
        eventActionQueue = new Queue<Action>();
    }

    //removes actions that are earlier than time t from queue
    public void RemovePastMoveActions(int t){
        while (moveActionQueue.Count != 0 && moveActionQueue.Peek().t < t) { moveActionQueue.Dequeue(); }
    }

    //removes actions that are earlier than time t from queue
    public void RemovePastEventActions(int t)
    {
        while (eventActionQueue.Count != 0 && eventActionQueue.Peek().t < t) { int id = eventActionQueue.Dequeue().id;}
    }
}
