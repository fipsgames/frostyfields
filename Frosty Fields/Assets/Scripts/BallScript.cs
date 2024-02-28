using UnityEngine;
using gameState;
using action;
using direction;
using util;

public class BallScript : SpawnObjectScript<MovableObject> {
    
    BallController ballController;
    GameManagerScript gameManagerScript { get; set; }

    public void Init(MovableObject modelReference, Vector3 worldPos, float moveSpeed, GameManagerScript gameManagerScript)
    {
        base.Init(modelReference, worldPos);

        this.gameManagerScript = gameManagerScript;
        // initializing the actual visible ball gameobject with a random starting roration
        gameObject.transform.GetChild(0).gameObject.transform.rotation = Random.rotation;
        ballController = gameObject.transform.GetChild(0).GetComponent<BallController>();
        ballController.moveSpeed = moveSpeed;
    }

    public void RememberReference()
    {
        modelReferenceBefore = modelReference.Clone();
    }

    public void GenerateActions(int t, GameState gameState)
    {
        ////Logger.Log("Generating actions for this ball");

        if (modelReference.x != modelReferenceBefore.x || modelReference.y != modelReferenceBefore.y)
        {
            float xHalf = (float)modelReference.x + ((float)modelReferenceBefore.x - (float)modelReference.x) / 2f;
            float yHalf = (float)modelReference.y + ((float)modelReferenceBefore.y - (float)modelReference.y) / 2f;

            switch (gameState.GetGroundUnderXY(modelReferenceBefore.x, modelReferenceBefore.y))
            {
                case Ground.SNOW:
                    AddMoveAction(Action.MOVE, t, modelReferenceBefore.x, modelReferenceBefore.y);
                    break;
                default:
                    AddMoveAction(Action.SLITTER, t, modelReferenceBefore.x, modelReferenceBefore.y);
                    break;
            }
            switch (gameState.GetGroundUnderXY(modelReference.x, modelReference.y))
            {
                case Ground.SNOW:
                    AddMoveAction(Action.MOVE, t+1, xHalf, yHalf);
                    break;
                default:
                    AddMoveAction(Action.SLITTER, t+1, xHalf, yHalf);
                    break;
            }
        }
        if (!modelReference.alive && modelReferenceBefore.alive)
        {
            switch (gameState.GetGroundUnderXY(modelReference.x, modelReference.y))
            {
                case Ground.FIRE:
                    AddEventAction(Action.BURN_TRIGGER, t + 1);
                    break;
                default:
                    AddEventAction(Action.FALL_TRIGGER, t + 1);
                    break;
            }
        }
    }

    public void HandleEventActions(int t)
    {
        while (HasEventActions() && eventActionQueue.Peek().t == t)
        {
            Action action = eventActionQueue.Dequeue();
            //Logger.Log("Acting on: ball action with id " + action.id + " and " + action.t);
            if (action.Matches(Action.FALL_TRIGGER, t))
            {
                ////Logger.Log("Ball Fall action");
                gameManagerScript.SetCamForLevel();
                ballController.Fall();
            } 
            if (action.Matches(Action.BURN_TRIGGER, t))
            {
                ////Logger.Log("Ball Fall action");
                ballController.Burn();
            }
        }
    }

    public void HandleMovementActions(int t, float fraction, Direction direction, int levelShiftX, int levelShiftY)
    {
        if (HasMoveActions())
        {
            Action action = moveActionQueue.Peek();
            //if (!didPrintUpdateDebug) //Logger.Log("There is ball action with id " + action.id + " and " + action.t);
            if (action.t == t && (action.id == Action.MOVE || action.id == Action.SLITTER))
            {
                float x = action.x + direction.x * fraction * 0.5f;
                float y = action.y + direction.y * fraction * 0.5f;
                //if (!didPrintUpdateDebug) //Logger.Log("updating ball pos: x " + x + " y " + y);
                Vector3 pos = Util.ConvertToHexa(x, y, levelShiftX, levelShiftY);
                //ROll before move becaus we need diff from curr pos to goal pos for turn axis calculation
                if (action.id == Action.MOVE)
                {
                    ////Logger.Log("APPLAYING ROLLING TO BALL");
                    ballController.RotateTo(pos);
                }
                Move(pos);
            }
        }
    }


    void Move(Vector3 goal)
    {
        gameObject.transform.SetPositionAndRotation(goal, gameObject.transform.rotation);
    }
}
