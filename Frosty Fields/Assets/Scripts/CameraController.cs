using UnityEngine;
using util;

public class CameraController : MonoBehaviour {

    public GameManagerScript gameManagerScript;

    public Vector3 offsetVecVertical = new Vector3(0, 6, -2);
    public Vector3 offsetVecHorizontal = new Vector3(0, 6, -2);
    Vector3 scaledOffsetVec = new Vector3();
    public Vector3 switchOffsetVecVertical = new Vector3(0, 6, -2);
    public Vector3 switchOffsetVecHorizontal = new Vector3(0, 6, -2);
    Vector3 scaledswitchOffsetVec = new Vector3();
    public Vector3 menuOffsetVecVertical = new Vector3(0, 6, -2);
    public Vector3 menuOffsetVecHorizontal = new Vector3(0, 6, -2);
    Vector3 scaledMenuOffsetVec = new Vector3();
    public Vector3 levelMiddlePos { get; set; }
    public float playerToLevelPosFraction = 0.7f; // 1 == all player, 0 == all level
    public float playerToLevelFocusFraction = 0.3f; // 1 == all player, 0 == all level
    public float switchPlayerToLevelPosFraction = 0.7f; // 1 == all player, 0 == all level
    public float switchPlayerToLevelFocusFraction = 0.3f; // 1 == all player, 0 == all level

    //Camera pos and rot to interpolate
    public Vector3 camStartPos { get; set; }
    public Vector3 camCurrentPos { get; set; }
    public Vector3 camGoalPos { get; set; }

    public Vector3 camStartFocusPoint { get; set; }
    public Vector3 camCurrentFocusPoint { get; set; }
    public Vector3 camGoalFocusPoint { get; set; }

    float fraction = 1;
    float currSpeed = 1;

    public float moveSpeed;
    public float switchExitSpeed;
    public float switchEnterSpeed;
    public float menuSpeed;

    float switchSpeed;

    public float fixedScaleMultiplier;
    public float minLength;

	void Update () {
        if (fraction > 1.0f)
        {
            fraction = 1.0f;
            //make sure goal values get reached exactly
            ApplyCurrentValues();
        }
        if(fraction >= 1.0f){
            return;
        }
        fraction += Time.deltaTime * currSpeed;// / lengthOfMovement;
        MoveCamNearerToGoalPos(Util.SmoothFraction(fraction));
     }

    public void SetLevelMidPointAndAdjustToLevelSize(Vector4 PosAndSize)
    {
        levelMiddlePos = new Vector3(PosAndSize.x, PosAndSize.y, PosAndSize.z);
        ////Logger.Log("levelMIddlePos and Size: " + PosAndSize.ToString(), this, true);
        if(PosAndSize.w > 0){
            scaledOffsetVec = offsetVecVertical * this.fixedScaleMultiplier * PosAndSize.w;
            scaledswitchOffsetVec = switchOffsetVecVertical;
            scaledMenuOffsetVec = menuOffsetVecVertical;
            ////Logger.Log("Vertical Level with size:  " + result.w + ". Set offset vec to " + scaledOffsetVec);
        } else {
            scaledOffsetVec = offsetVecHorizontal * this.fixedScaleMultiplier * -PosAndSize.w;
            scaledswitchOffsetVec = switchOffsetVecHorizontal;
            scaledMenuOffsetVec = menuOffsetVecHorizontal;
            ////Logger.Log("Horizontal Level with size:  " + -result.w + ". Set offset vec to " + scaledOffsetVec);
        }
    }

    public void SetInitialCamPosForGame(){
        SetLevelMidPointAndAdjustToLevelSize(gameManagerScript.CalculateLevelMidAndSize());
        camGoalFocusPoint = levelMiddlePos;
        camGoalPos = camGoalFocusPoint + scaledOffsetVec;
        camStartPos = levelMiddlePos + switchOffsetVecVertical;
        camStartFocusPoint = camGoalFocusPoint;
        camCurrentPos = camStartPos;
        camCurrentFocusPoint = camCurrentFocusPoint;
        ////Logger.Log("Setting Initial Cam POS: " + camCurrentPos.ToString(), this, true);
        this.transform.SetPositionAndRotation(camCurrentPos, transform.rotation);
        this.transform.LookAt(camCurrentFocusPoint);
        //this.lengthOfMovement = Mathf.Max((camGoalPos - camStartPos).magnitude,(camGoalFocusPoint - camStartFocusPoint).magnitude,minLength);
        ////Logger.Log("length " + lengthOfMovement);
        fraction = 0.0f;
        ////Logger.Log("Set Cam Level Starting point " + camGoalPos.ToString() + " - " + camGoalFocusPoint);
    }

    // used for all ingame camera movements
    void SetNewCamGoalPoseBasedOnModel(Vector3 offset, float playerToLevelPosFraction, float playerToLevelFocusFraction)
    {
        SetNewCamGoalPose(gameManagerScript.GetPlayerGoalPos(), offset,playerToLevelPosFraction, playerToLevelFocusFraction);
    }

    // used for cam movements where position is directly given
    void SetNewCamGoalPose(Vector3 playerVec, Vector3 offset, float playerToLevelPosFraction, float playerToLevelFocusFraction)
    {
        camStartPos = camCurrentPos;
        camStartFocusPoint = camCurrentFocusPoint;
        camGoalPos = playerVec * playerToLevelPosFraction + levelMiddlePos * (1 - playerToLevelPosFraction) + offset;
        camGoalFocusPoint = playerVec * playerToLevelFocusFraction + levelMiddlePos * (1 - playerToLevelFocusFraction);
        fraction = 0.0f; //restart camera movement
    }

    // used for normal player tracing ingame
    public void SetNewCamPos()
    {
        this.currSpeed = this.moveSpeed;
        this.SetNewCamGoalPoseBasedOnModel(this.scaledOffsetVec, this.playerToLevelPosFraction, this.playerToLevelFocusFraction);
    }
    //used for levelcreator
    public void SetNewCamPos(Vector3 playerVec)
    {
        this.currSpeed = this.moveSpeed;
        this.SetNewCamGoalPose(playerVec, this.scaledOffsetVec, this.playerToLevelPosFraction, this.playerToLevelFocusFraction);
    }

    // camera goal for level deconstruction
    public void SetNewCamExitPos()
    {
        this.currSpeed = switchExitSpeed;
        this.SetNewCamGoalPoseBasedOnModel(this.scaledswitchOffsetVec, this.switchPlayerToLevelPosFraction , this.switchPlayerToLevelFocusFraction);
    }

    // camera goal for level construction
    public void SetNewCamEnterPos()
    {
        this.currSpeed = switchEnterSpeed;
        this.SetNewCamGoalPoseBasedOnModel(this.scaledOffsetVec, this.playerToLevelPosFraction, this.playerToLevelFocusFraction);
    }

    public void SetNewCamEnterPos(Vector3 playerVec)
    {
        this.currSpeed = switchEnterSpeed;
        this.SetNewCamGoalPose(playerVec, this.scaledOffsetVec, this.playerToLevelPosFraction, this.playerToLevelFocusFraction);
    }

    // camera goal for main/pause menu
    public void SetNewCamMenuPos()
    {
        this.currSpeed = menuSpeed;
        this.SetNewCamGoalPoseBasedOnModel(this.scaledMenuOffsetVec, 1, 1);
    }

    // camera goal for level creator
    public void SetNewCamCreatorPos(Vector3 playerVec, float zoomFraction)
    {
        ////Logger.Log("Creator Zoom Fraction: " + zoomFraction, this, true);
        this.currSpeed = switchEnterSpeed;
        this.SetNewCamGoalPose(playerVec, scaledOffsetVec, (1-zoomFraction) * playerToLevelPosFraction,(1-zoomFraction) * playerToLevelFocusFraction);
    }

    public void MoveCamNearerToGoalPos(float fraction)
    {
        camCurrentPos = Vector3.Lerp(this.camStartPos, this.camGoalPos, fraction);
        camCurrentFocusPoint = Vector3.Lerp(this.camStartFocusPoint, this.camGoalFocusPoint, fraction);
        ////Logger.Log("Interpolating cam with " + fraction + " to " + camCurrentPos + " - " + camCurrentFocusPoint);
        ApplyCurrentValues();
    }

    public void ApplyCurrentValues(){
         transform.SetPositionAndRotation(camCurrentPos, transform.rotation);
        transform.LookAt(camCurrentFocusPoint);
    }
}
