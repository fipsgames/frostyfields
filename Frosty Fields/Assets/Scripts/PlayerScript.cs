using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using gameState;
using action;
using direction;
using util;

public class PlayerScript : GameObjectScript<Player> {

    Direction preferedLookDirection { get; set; }
    Direction oldLookDirection { get; set; }
    Direction currentDirection { get; set; }
    float fraction { get; set; }
    public float turnSpeed;
    GameManagerScript gameManagerScript { get; set; }
    PlayerController playerController { get; set; }
    //IndicatorController indicatorController { get; set; }
    int lastEventActionId { get; set; }

    public void Init(Player modelReference, Vector3 worldPos, GameManagerScript gameManagerScript)
    {
        base.Init(modelReference, worldPos);
        this.gameManagerScript = gameManagerScript;
    }

    public void Start()
    {
        fraction = -1.0f;
        lastEventActionId = -1;
        preferedLookDirection = new Direction(Direction.DOWN);
        oldLookDirection = new Direction(Direction.DOWN);
        currentDirection = new Direction(Direction.DOWN);
        gameObject.transform.LookAt(gameObject.transform.position + preferedLookDirection.vector);
        playerController = gameObject.transform.GetChild(0).gameObject.GetComponent<PlayerController>();
        //indicatorController = gameObject.transform.GetChild(1).gameObject.GetComponent<IndicatorController>();
    }

    public void Update()
    {
        if (fraction >= 0.0f && fraction <= 1.0f)
        {
            fraction += turnSpeed * Time.deltaTime;
            ////Logger.Log("frac: " + fraction);
            TurnTowardsPreferedDirection(fraction);
        }
    }

    public void TurnTowardsPreferedDirection(float fraction)
    {
        currentDirection.vector = Vector3.Slerp(oldLookDirection.vector, preferedLookDirection.vector, fraction);
        ////Logger.Log(fraction + " " + currentDirection.vector);
        gameObject.transform.LookAt(gameObject.transform.position + currentDirection.vector);
    }

    public void SetNewPreferedDirection(Direction preferedDirection, bool spawnIndicator)
    {
        ////Logger.Log("OLD PLAYER DIRECRION " + oldLookDirection + " PREFERED PLAYER DIR " + preferedDirection);
        oldLookDirection = currentDirection;
        preferedLookDirection = preferedDirection;
        fraction = 0.0f;
        //if (spawnIndicator) indicatorController.Spawn();
    }

    public void RememberReference()
    {
        modelReferenceBefore = modelReference.Clone();
    }

    public void GenerateActions(int t, GameState gameState)
    {
        bool silence_second_action = false;
        //FIRST CHECK IF PLAYER DIED (CAN HAPPEN EVEN WITHOUT MOVEMENT IN THIS TIMESTEP
        // IF PLAYER KEEPS STANDING ON A TRIGGERED BROKEN ICE!!!!! THIS WAS A BUG BEFORE!!!)
        //DEATH ANIMATION? FALL || BURN 
        if (!modelReference.alive && modelReferenceBefore.alive)
        {
            
            ////Logger.Log("ADDED DEATH ANIMATION AT TIME: " + t, this, true);
            switch (gameState.GetGroundUnderPlayer())
            {
                case Ground.FIRE:
                    AddEventAction(Action.BURN_TRIGGER, t);
                    AddEventAction(Action.RESTART_TRIGGER, t + 4);
                    break;
                default:
                    AddEventAction(Action.FALL_TRIGGER, t);
                    AddEventAction(Action.RESTART_TRIGGER, t + 4);
                    break;
            }
        }

        // did player move at all?
        if (modelReference.x != modelReferenceBefore.x || modelReference.y != modelReferenceBefore.y)
        {
            ////Logger.Log("GENERATING ACTIONs for T: (" + t + "/" + (t + 1) + ")  Player Moved");
            float xHalf = (float)modelReference.x + ((float)modelReferenceBefore.x - (float)modelReference.x) / 2f;
            float yHalf = (float)modelReference.y + ((float)modelReferenceBefore.y - (float)modelReference.y) / 2f;

            // First actual movement - use MOVE for all player movements, no need to differ between move and slide here
            // Adding both movements
            AddMoveAction(Action.MOVE, t, modelReferenceBefore.x, modelReferenceBefore.y);
            AddMoveAction(Action.MOVE, t + 1, xHalf, yHalf);

            //FIRST: WIN ANIMATION?
            if (modelReference.won && !modelReferenceBefore.won)
            {
                AddEventAction(Action.WIN_TRIGGER, t);
                AddEventAction(Action.SWITCH_TRIGGER, t + 4);
            }
            //SECOND: NORMAL MOVEMENT ANIMATION? WALK || SLITTER ..todo add more
            else
            {
                // MOVEMENT ANIMATION ON FIRST GROUND
                switch (gameState.GetGroundUnderXY(modelReferenceBefore.x, modelReferenceBefore.y))
                {
                    case Ground.SNOW:
                        // SPECIAL CASE: WHEN TRANSITIONIG TO ICE OR CRACK, 
                        // WE WANT TO START THE SLIDE MOTION ONE TIME EARIER
                        if (gameState.GetGroundUnderPlayer() == Ground.ICE || 
                            gameState.GetGroundUnderPlayer() == Ground.CRACK){
                            AddEventAction(Action.SLITTER_TRIGGER, t);
                        } else {
                            AddEventAction(Action.MOVE_TRIGGER, t);
                        }
                        break;
                    default:
                        // SPECIAL CASE: WHEN TRANSITIONIG FROM ICE OR CRACK TO SNOW, 
                        // WE WANT TO START THE ROLL ANIMATION ONE TIME EARLIER
                        if(gameState.GetGroundUnderPlayer() == Ground.SNOW){
                            AddEventAction(Action.ROLL_TRIGGER, t);
                            silence_second_action = true;
                        } else {
                            AddEventAction(Action.SLITTER_TRIGGER, t);
                        }
                        break;
                }
                if (!silence_second_action){
                    // MOVEMENT ANIMATION ON SECOND GROUND
                    switch (gameState.GetGroundUnderPlayer())
                    {
                        case Ground.SNOW:
                            AddEventAction(Action.MOVE_TRIGGER, t+1);
                            break;
                        default:
                            AddEventAction(Action.SLITTER_TRIGGER, t+1);
                            break;
                    }
                }
            } 
        }
        else
        {
            //.Log("GENERATING ACTIONs for T: (" + t + "/" + (t + 1) + ")  Player Stationary");
            AddEventAction(Action.IDLE_TRIGGER, t);
        }
    }

    public void HandleEventActions(int t){
        ////Logger.Log("(ANIMATION) Handling event action! before Event player dequeue " + eventActionQueue.Count + " at time t: " + t, this, true);
        //if (eventActionQueue.Count > 0) //Logger.Log("First event action: " + eventActionQueue.Peek().id + " and " + eventActionQueue.Peek().t);
        while (HasEventActions() && eventActionQueue.Peek().t == t)
        {
            Action action = eventActionQueue.Dequeue();
            if(lastEventActionId != action.id)
            {
                lastEventActionId = action.id;
                ////Logger.Log("(ANIMATION) Executing Player ACTION " + action.Name() + " at T: " + action.t, this, true);
                ////Logger.Log("(ANIMATION) after Event player dequeue " + eventActionQueue.Count, this, true);
                if (action.Matches(Action.IDLE_TRIGGER, t))
                {
                    playerController.Idle();
                }
                else if (action.Matches(Action.MOVE_TRIGGER, t))
                {
                    playerController.Walk();
                }
                else if (action.Matches(Action.SLITTER_TRIGGER, t))
                {
                    playerController.Slitter();
                }
                else if (action.Matches(Action.ROLL_TRIGGER, t))
                {
                    playerController.Roll();
                } 
                else if (action.Matches(Action.BUMP_TRIGGER, t))
                {
                    playerController.Bump();
                }
                else if (action.Matches(Action.FALL_TRIGGER, t))
                {
                    playerController.Fall();
                }
                else if (action.Matches(Action.BURN_TRIGGER, t))
                {
                    playerController.Burn();
                }
                else if (action.Matches(Action.WIN_TRIGGER, t))
                {
                    gameManagerScript.AddCurrentCustomLevelSolved();
                    playerController.Win();
                }
                else if (action.Matches(Action.RESTART_TRIGGER, t))
                {
                    gameManagerScript.TakeBackOne();
                }
                else if (action.Matches(Action.SWITCH_TRIGGER, t))
                {
                    gameManagerScript.SwitchToNextLevel();
                }
            }
            else
            {
                ////Logger.Log("IGNORING REDUNDANT EVENT ACTION " + action.Name());
            }
        }
    }

    public void HandleMovementActions(int t ,float fraction, Direction direction, int levelShiftX, int levelShiftY){
        if (HasMoveActions())
        {
            Action action = moveActionQueue.Peek();
            ////Logger.Log("CONTINOUS MOVING player action with id " + action.id + " and " + action.t);
            if (action.Matches(Action.MOVE, t) || action.Matches(Action.SLITTER, t))
            {
                ApplyMovement(action, fraction, direction, levelShiftX, levelShiftY);
            } 
        } 
    }

    public void ApplyMovement(Action action, float fraction, Direction direction, int levelShiftX, int levelShiftY){
        ////Logger.Log("updating player pos: x " + x + " y " + y);
        Vector3 pos = Util.ConvertToHexa(
            action.x + direction.x * fraction * 0.5f, 
            action.y + direction.y * fraction * 0.5f, 
            levelShiftX, levelShiftY);
        gameObject.transform.SetPositionAndRotation(pos, gameObject.transform.rotation);   
    }

    public void SetPlayerIdle(){
        playerController.Idle();
    }

    
    public void DespawnIndicator(){
        //indicatorController.Despawn();
    }
    
}
