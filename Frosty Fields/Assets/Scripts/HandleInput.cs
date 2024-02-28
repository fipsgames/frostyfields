using UnityEngine;
using direction;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class HandleInput : MonoBehaviour {

    public GameManagerScript gameManagerScript;
    public CameraController cameraController;
    public Camera cameraRef;
    public TutorialController tutorialController;

    // distances are relative to 1 as screen size width
    public float tapTouchMaxDistance = 0.02f;
    public float moveTouchEndedDistance = 0.05f;
    public float moveTouchGoingOnDistance = 0.1f;

    Vector2 firstScreenTouch;
    Vector2 lastScreenTouch;
    Vector3 firstPlaneTouch;
    Vector3 lastPlaneTouch;
	Direction[] directions;

    bool waitingForInitialTouch;
    Direction lastBestDirection;
    Direction bestDirection;
    bool firstHitWasPlayer;

    public bool disableInput{get; set;}

    int planeLayerMask = 1 << 8;
    int playerLayerMask = 1 << 10;

    void Start () {
        directions = new Direction[6];
        for (int i = 0; i < directions.Length; i++){
            directions[i] = new Direction(i);
        }
        // scale with screen to compare against pixel values later easily
        tapTouchMaxDistance = tapTouchMaxDistance * Screen.width;
        moveTouchEndedDistance = moveTouchEndedDistance * Screen.width;
        moveTouchGoingOnDistance = moveTouchGoingOnDistance * Screen.width;
        Reset();
	}

    public void Reset(){
        waitingForInitialTouch = true;
        lastBestDirection = new Direction(-1);
        bestDirection = new Direction(-1);
        firstScreenTouch = new Vector2(0, 0);
        lastScreenTouch = new Vector2(0, 0);
        firstPlaneTouch = new Vector3(0, 0, 0);
		lastPlaneTouch = new Vector3(0, 0, 0);
        firstHitWasPlayer = false;
        disableInput = false;
        ////Logger.Log("INPUT MODULE RESET! accepcting touch input", this, true);
    }
	
    void Update () {
        if(disableInput) return;
        if (waitingForInitialTouch)
        {
#if UNITY_EDITOR
            // PRIMARY Screen input MOUSE
            if (Input.GetMouseButtonDown(0))
            {
                firstScreenTouch = Input.mousePosition;
                waitingForInitialTouch = false;
                //Vector3 worldPos = GetPointOnControlPlane(firstScreenTouch);
                ////Logger.Log("MOUSE DOWN DETECTED: " + firstPlaneTouch.ToString());
                if (GetTouchedPlayer(firstScreenTouch)){
                    firstHitWasPlayer = true;
                } else {
                    AnalyseInitialTouch();
                }
            }
#else
            // PRIMARY Screen input TOUCH
            ////Logger.Log("THIS IS REAL TOUCH!!!!!!!!!!!!!!! ", this, true);
            if (Input.touchCount == 1 && Input.GetTouch(0).phase == TouchPhase.Began)
            {
                ////Logger.Log("INITIAL TOUCH!", this, true);
                if(!EventSystem.current.IsPointerOverGameObject(Input.GetTouch(0).fingerId)){
                    firstScreenTouch = Input.GetTouch(0).position;
                    waitingForInitialTouch = false;
                    
                    //Vector3 worldPos = GetPointOnControlPlane(firstScreenTouch);
                    ////Logger.Log("TOUCH DOWN DETECTED: " + firstScreenTouch.ToString(), this, true);
                    if (GetTouchedPlayer(firstScreenTouch)){
                        firstHitWasPlayer = true;
                    } else {
                        AnalyseInitialTouch();
                    }
                } else{
                    ////Logger.Log("INITIAL TOUCH OVER UI ELEMENT: TOUCH IGNORED", this, true);
                }
            }
#endif
        }
        else
        {
#if UNITY_EDITOR
            if (Input.GetMouseButton(0) == true){
                lastScreenTouch = Input.mousePosition;
                ////Logger.Log("SECONDARY MOUSE DETECTED: " + lastScreenTouch.ToString());
                AnalyseInputPoints(moveTouchGoingOnDistance, false);
            }
            // SECONDARY Screen input MOUSE RELEASE
            if (Input.GetMouseButtonUp(0))
            {
                lastScreenTouch = Input.mousePosition;
                //Logger.Log("MOUSE UP DETECTED: " + lastScreenTouch.ToString());
                if (firstHitWasPlayer) {
                    if (GetTouchedPlayer(lastScreenTouch)){
                        disableInput = true;
                        gameManagerScript.SpawnMenu();
                        tutorialController.Trigger("menu_open");
                        return;
                    }
                }
               
                waitingForInitialTouch = true;
                AnalyseInputPoints(moveTouchEndedDistance, true);
            }
#else
            if (Input.touchCount == 1 && Input.GetTouch(0).phase == TouchPhase.Moved)
            {
                lastScreenTouch = Input.GetTouch(0).position;
                ////Logger.Log("TOUCH MOVE DETECTED: " + lastScreenTouch.ToString(), this, true);
                AnalyseInputPoints(moveTouchGoingOnDistance, false);
            }
            // SECONDARY Screen input TOUCH RELEASE
            if (Input.touchCount == 1 && Input.GetTouch(0).phase == TouchPhase.Ended)
            {
                lastScreenTouch = Input.GetTouch(0).position;
                ////Logger.Log("TOUCH UP DETECTED: " + lastScreenTouch.ToString(), this, true);
                if (firstHitWasPlayer) {
                    if (GetTouchedPlayer(lastScreenTouch)){
                        disableInput = true;
                        gameManagerScript.SpawnMenu();
                        tutorialController.Trigger("menu_open");
                        return;
                    }
                }

                waitingForInitialTouch = true;
                AnalyseInputPoints(moveTouchEndedDistance, true);
            }
#endif
        }
        
        if (Input.GetButtonDown("Up"))
        {
            gameManagerScript.RegisterInput(Direction.UP);
        }
        if (Input.GetButtonDown("Down"))
        {
            gameManagerScript.RegisterInput(Direction.DOWN);
        }
		if (Input.GetButtonDown ("UpRight")) {
            gameManagerScript.RegisterInput (Direction.UPRIGHT);
		}
		if (Input.GetButtonDown ("DownLeft")) {
            gameManagerScript.RegisterInput (Direction.DOWNLEFT);
		}
		if (Input.GetButtonDown ("UpLeft")) {
            gameManagerScript.RegisterInput (Direction.UPLEFT);
		}
		if (Input.GetButtonDown ("DownRight")) {
            gameManagerScript.RegisterInput (Direction.DOWNRIGHT);
		}
        if (Input.GetButtonDown("Reset"))
        {
            gameManagerScript.RestartCurrentLevel();
        }
        if (Input.GetButtonDown("Next"))
        {
            gameManagerScript.SwitchToNextLevel();
        }
        if (Input.GetButtonDown("Back"))
        {
            gameManagerScript.SwitchToLastLevel();
        }
	}

    public bool GetTouchedPlayer(Vector2 screenPos){
        RaycastHit hit;
        Ray ray = cameraRef.ScreenPointToRay(screenPos);
        ////Logger.Log("TRYING TO RAYCAST FOR PLAYER", this, true);
        
        //Debug.DrawRay(ray.origin, ray.direction*1000, Color.green, 10, true);
        if (Physics.Raycast(ray, out hit, Mathf.Infinity, playerLayerMask))
        {
            ////Logger.Log("RAYCAST HIT " + hit.transform.name + " ON " + hit.point, this, true);
            //Debug.DrawRay(hit.transform.position, Vector3.up * hit.distance, Color.red, 10, false);
            return true;
        }
        return false;
    }

    public Vector3 GetPointOnControlPlane(Vector2 screenPos){
        RaycastHit hit;
        Ray ray = cameraRef.ScreenPointToRay(screenPos);
        
        //Debug.DrawRay(ray.origin, ray.direction*1000, Color.red, 10, true);
        if (Physics.Raycast(ray, out hit, Mathf.Infinity, planeLayerMask))
        {
            ////Logger.Log("RAYCAST HIT " + hit.transform.name + " ON " + hit.point, this, true);
            //Debug.DrawRay(hit.transform.position, Vector3.up * hit.distance, Color.red, 10, false);
            return hit.point;
        }
        return new Vector3();
    }

    public Direction GetBestDirection(Vector3 inputVec){
        float bestDistance = 1337;
        float distance;
        int bestDirectionId = -1;
        inputVec.Normalize();
        for (int i = 0; i < directions.Length; i++)
        {
            distance = (inputVec - directions[i].vector).magnitude;
            if (distance < bestDistance)
            {
                bestDistance = distance;
                bestDirectionId = i;
            }
        }
        //Logger.Log("RESULTING BEST DIRECTION TOUCH: " + directions[bestDirectionId].name, this, true);
        return directions[bestDirectionId];
    }

    public void AnalyseInitialTouch(){
        firstPlaneTouch = GetPointOnControlPlane(firstScreenTouch);
        Vector3 directionalVector = firstPlaneTouch - gameManagerScript.GetPlayerPos();
        bestDirection = GetBestDirection(directionalVector);
        if(bestDirection.id != lastBestDirection.id){
            //Logger.Log("INITIAL TOUCH " + bestDirection.name + " - TURNING PLAYER", this, true);
            gameManagerScript.SetPlayerPreferedLookDirection(bestDirection);
            lastBestDirection.id = bestDirection.id;
        }
    }
 
    public void AnalyseInputPoints(float moveDistance, bool touchEnded){
        firstPlaneTouch = GetPointOnControlPlane(firstScreenTouch);
        lastPlaneTouch = GetPointOnControlPlane(lastScreenTouch);
        Vector3 directionalVector = lastPlaneTouch - firstPlaneTouch;
        float swipeDist = (lastScreenTouch - firstScreenTouch).magnitude;
        // is swipe input?
        if (swipeDist >= tapTouchMaxDistance)
        {
            // if touch drag is > turn dist already: pretend initial hit was not on player if it was, too prevent menu from opening
            firstHitWasPlayer = false;
            bestDirection = GetBestDirection(directionalVector);
            if(bestDirection.id != lastBestDirection.id){
                //Logger.Log("SWIPE TOUCH " + bestDirection.name + " - TURNING PLAYER", this, true);
                gameManagerScript.SetPlayerPreferedLookDirection(bestDirection);
                lastBestDirection.id = bestDirection.id;
            }
            if(swipeDist >= moveDistance){
                //Logger.Log("SWIPE TOUCH " + bestDirection.name + " - MOVING PLAYER", this, true);
                gameManagerScript.RegisterInput(bestDirection.id);
                waitingForInitialTouch = true;
                lastBestDirection.id = -1;
                tutorialController.Trigger("swipe_move");
            }
        } else // is tap input
        {
            if(touchEnded){
                //Logger.Log("TAP TOUCH DETECTED " + bestDirection.name, this, true);
                gameManagerScript.RegisterInput(bestDirection.id);
                waitingForInitialTouch = true;
                lastBestDirection.id = -1;
                tutorialController.Trigger("touch_move");
            }
        }
    }
}
