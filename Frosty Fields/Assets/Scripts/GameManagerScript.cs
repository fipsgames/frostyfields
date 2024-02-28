using System.Collections.Generic;
using UnityEngine.Events;
using UnityEngine;
using System;
using System.Linq;
using gameState;
using direction;
using util;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using TMPro;

public class GameManagerScript : MonoBehaviour
{

    // Input Script ref
    public HandleInput handleInput;

    //CAM REF
    public Camera cameraRef;
    CameraController cameraController;

    public OceanScript oceanScript;
    public LightScript lightScript;
    public PostProcessingScript postprocessingScript;

    public TutorialController tutorialController;
    Persistence persistence;
    SceneMemory sceneMemory;


    // LEVEL DEFINITION
    string[] levelStrings;
    string[] customLevelStrings;
    int maxLevel;
    int currentLevel;
    int nextLevel;
    int numberLevels;
    int numberCustomLevels;

    //PREFABS
    public GameObject snowIni;
    public GameObject iceIni;
    public GameObject crackIni;
    public GameObject fireIni;
    public GameObject treeIni;
    public GameObject goalIni;
    public GameObject waterIni;
    public GameObject snowBallIni;
    public GameObject iceBallIni;
    public GameObject fishIni;
    public GameObject playerIni;

    public Sprite soundOnSprite;
    public Sprite soundOffSprite;
    public Sprite backButtonSprite;
    public Sprite solvedLevelSprite;
    public Sprite unSolvedLevelSprite;

    public GameObject player;
    List<GameObject> groundList;
    List<GameObject> ballList;
    List<GameObject> fishList;

    public PlayerScript playerScript;
    List<GroundScript> groundScriptList;
    List<BallScript> ballScriptList;
    List<FishScript> fishScriptList;

    //CURRENT GAMESTATE
    GameState gameState { get; set; }

    //LIST OF VALID 6 DIRECTIONS
    Direction[] directions { get; set; }

    //BUFFER FOR INPUT DIRECTIONS
    Queue<Direction> inputBuffer { get; set; }

    //CURRENT / LAST MOVEMENT
    Direction currentDirection { get; set; }

    //CURRENT ACTION TIMELINE VALUE (ONE FIELD EQUALS 2 timesteps)
    int t { get; set;}

    //VALUE FROM 0 to 1 to indicate how far a single action has progressed
    float currentActionProgress { get; set; }

    //MULTIPLIER FOR THE MOVE SPEED OF ALL OBJECTS
    public float moveSpeed;

    //WHEN THE LAST ACTION STARTED
    int lastActionStarted { get; set; }

    // bool to print each update debug only once
    bool didPrintUpdateDebug = false;

    //TARGET FRAME RATE
    public int targetFrameRate = 60;

    // we dont want to move the player on level reload or next level load
    //x and y values in game logic space to shift each level implicitly so that player starting place is always 000 (plus overall player movement) (set after level load)
    int levelShiftX;
    int levelShiftY;

    //overall player movement in model space 
    public int totalPlayerModelMovementX;
    public int totalPlayerModelMovementY;

    // whether or not the next reload reloads fully or just a takeback
    bool takeback;

    // int to indicate which level switch state we are in
    // 0 == level in deconstruct mode
    // 1 == level in construct mode
    // 2 == level in play mode 
    int levelSwitchState;

    // value holding the progress of a level construct or deconstruct action (0-1)
    float levelSwitchFraction;

    // How fast the levels appear and disappear
    public float levelSwitchSpeed;

    // How far up and down the level pieces fly
    public float levelSwitchUpValue;
    public float levelSwitchDownValue;

    // Vector form of above value
    Vector3 levelSwitchUp;
    Vector3 levelSwitchDown;

    Vector3[] directionVectors;

    // LEVEL creator reference
    public LevelCreator levelCreatorManager;

    // whether to switch to level creator on next level switch
    bool switchToCreator;
    public bool isInCreatorMode;

    public float midLevelPerspectiveScalar;

    private AudioManager audiomanager;

    //UI STuff
    public Canvas canvas; 

    public CanvasFader MenuFader;
    public CanvasFader SelectLevelFader;

    public Button playButton;
    public Button restartButton;
    public Button undoButton;
    public Button selectButton;
    public Button createButton;
    public Button creditsButton;
    public Button muteButton;

    public TextMeshProUGUI levelDisplay;

    public GameObject selectLevelContent;
    public GameObject selectLevelScrollView;
    public Button selectLevelButtonPrefab;
    Button[] selectLevelButtons;
    Button[] customSelectLevelButtons;
    //Button insertLevelViaCodeButton; 
    //public Sprite addLevelImage;

    Color selectLevelClickableColor;
    Color selectLevelNonClickableColor;
    Color selectLevelCurrentSelectionColor;

    public float colorDarkenFraction;
    public float colorLightenFraction;

    private Vector4 playButtonOriginalPosition;

    void Awake()
    {
        Application.targetFrameRate = this.targetFrameRate;
    }

    // Use this for initialization
    void Start()
    {
        ////Logger.Log("Start Game Manager Script");
        InitializeGame();
        
        if (sceneMemory.initialMenu) {
            SpawnMenu();
        } else {
            InitializeLevel(currentLevel);
            handleInput.Reset();
            DespawnMenu();
            cameraController.SetInitialCamPosForGame();
        }
        
    }

    // Update is called once per frame
    void Update()
    {
        
        //level is in input accepting play mode
        if(this.levelSwitchState == 2){
            if (this.HasActions())
            {
                //use existing actions to animate game progress (THis will run 98% of the time)
                this.ProcessActions();
            }
            else
            {
                //use input to appy to model and generate new actions of that
                this.ProcessNextInput();

            }
        } else {
            //level is in switch mode - preccess switching
            this.ProccessLevelSwitch();
        }
        
       
    }

    public void InitializeGame()
    {
        sceneMemory = FindObjectOfType<SceneMemory>();
        //Logger.Log("INITIALIZING NEW MAIN SCENE WITH MAINMODE=" + sceneMemory.isMainMode + " and INITIALMENU= " + sceneMemory.initialMenu, this, true);
        audiomanager = FindObjectOfType<AudioManager>();
        persistence = gameObject.GetComponent<Persistence>();
        this.InitializeDirections();
        this.InitializeMuted();
        this.LoadLevelStrings();
        this.LoadCustomLevelStrings();

        if (sceneMemory.isMainMode)
        {
            currentLevel = persistence.LoadCurrLevel();
            maxLevel = persistence.LoadMaxLevel();
        }
        else
        {
            currentLevel = persistence.LoadCurrCustomLevel();
            maxLevel = int.MaxValue; // all custom levels are always unlocked
        }
        this.inputBuffer = new Queue<Direction>();
        this.currentDirection = new Direction(Direction.UP);
        this.takeback = false;
        this.switchToCreator = false;
        this.isInCreatorMode = false;
        //discrete model of time in gamestate logic (1 action = 1 time)
        this.t = 0;
        this.currentActionProgress = 0.0f;
        this.levelSwitchFraction = 0.0f;
        this.totalPlayerModelMovementX = 0;
        this.totalPlayerModelMovementY = 0;
        this.levelSwitchState = 2;
        this.levelSwitchUp = new Vector3(0, this.levelSwitchUpValue, 0);
        this.levelSwitchDown = new Vector3(0, this.levelSwitchDownValue, 0);
        cameraController = cameraRef.GetComponent<CameraController>();

        //connect menu ui buttons stuff:
        playButton.onClick.AddListener(DespawnMenu);
        restartButton.onClick.AddListener(RestartCurrentLevel);
        undoButton.onClick.AddListener(UndoBttn);
        selectButton.onClick.AddListener(delegate{SelectLevelBttn(true);});
        createButton.onClick.AddListener(SwitchToCreator);
        creditsButton.onClick.AddListener(Credits);
        muteButton.onClick.AddListener(SwitchMute);

        playButtonOriginalPosition = playButton.transform.position;
            
        InitializeLevelSelectButtons();
        InitializeCustomLevelSelectButtons();
        
        //Disable Level Select Screen
        selectLevelScrollView.SetActive(false);
    }

    void SwitchToCreator()
    {
        handleInput.disableInput = true;
        sceneMemory.initialMenu = false;
        switchToCreator = true;
        sceneMemory.isMainMode = false;
        nextLevel = persistence.LoadCurrCustomLevel();
        SetUIElementsActive(false);
        TriggerLevelSwitch();
    }

    public void LoadLevelStrings()
    {
        ////Logger.Log("LOADing ALL LEVEL for main game mode", this, true);
        levelStrings = persistence.LoadLevels().Split('#');
        
        //level 0 is empty string - we start at 1 - better anyway for humans - no need to convert between logic and gui
        numberLevels = levelStrings.Length - 1;
        ////Logger.Log("LOADED ALL MAIN LEVEL STRINGS (" + numberLevels + "):\n" + string.Join("\n", levelStrings), this, true);
    }
    public void LoadCustomLevelStrings()
    {
        ////Logger.Log("LOADing ALL LEVEL for custom game mode", this, true);
        customLevelStrings = persistence.LoadCustomLevels().Split('#');
        
        //level 0 is empty string - we start at 1 - better anyway for humans - no need to convert between logic and gui
        numberCustomLevels = customLevelStrings.Length - 1;
        ////Logger.Log("LOADED ALL CUSTOM LEVEL STRINGS (" + numberCustomLevels + "):\n" + string.Join("\n", customLevelStrings), this, true);
    }

    public void InitializeDirections()
    {
        directions = new Direction[6];
        for (int i = 0; i < directions.Length; i++)
        {
            directions[i] = new Direction(i);
        }
    }

    public void InitializeLevelSelectButtons(){
        // initialize select level buttons:
        selectLevelButtons = new Button[numberLevels + 1];
        // plus one for adding back button on top left
        for (int i = 0; i < selectLevelButtons.Length; i++)
        {
            int tempI = i;
            Button tmpButton = (Button)Instantiate(selectLevelButtonPrefab);
            if(i == 0){
                selectLevelClickableColor = tmpButton.transform.GetComponent<Image>().color;
                selectLevelNonClickableColor = new Color(selectLevelClickableColor.r * colorDarkenFraction, 
                                                        selectLevelClickableColor.g * colorDarkenFraction,
                                                        selectLevelClickableColor.b * colorDarkenFraction,
                                                        selectLevelClickableColor.a);
                selectLevelCurrentSelectionColor = new Color(selectLevelClickableColor.r * colorLightenFraction,
                                                        selectLevelClickableColor.g * colorLightenFraction,
                                                        selectLevelClickableColor.b * colorLightenFraction,
                                                        selectLevelClickableColor.a);
            }
            tmpButton.transform.SetParent(selectLevelContent.transform, false);
            // does not seem needed after adding layout group
            //RectTransform rt = tmpButton.GetComponent<RectTransform>();     
            //rt.SetInsetAndSizeFromParentEdge(RectTransform.Edge.Left, 0, rt.rect.width);
            //rt.SetInsetAndSizeFromParentEdge(RectTransform.Edge.Top, 0, rt.rect.height);
            //tmpButton.GetComponent<Button>().onClick.AddListener(new UnityAction(() => SelectLevel(temp = i)));//Setting what button does when clicked
            if (i == 0) {
                tmpButton.GetComponent<Button>().onClick.AddListener(delegate{BackBttn(false);});//Back button should disable select level view
                tmpButton.transform.GetComponent<Image>().sprite = backButtonSprite; //Changing sprite
                //tmpButton.transform.GetChild(0).GetComponent<TextMeshProUGUI>().text = "back";
            } else {
                tmpButton.GetComponent<Button>().onClick.AddListener(delegate{SelectLevel(tempI);});//Setting what button does when clicked
                tmpButton.transform.GetChild(0).GetComponent<TextMeshProUGUI>().text = "" + i;
            }
            selectLevelButtons[i] = tmpButton;
        }
    }
    public void InitializeCustomLevelSelectButtons(){
        // if there are buttons already: destroy 
        if (customSelectLevelButtons != null) {
            for (int i = 0; i < customSelectLevelButtons.Length; i++)
            {
                Destroy(customSelectLevelButtons[i].gameObject);
            }
        }
        //also destroy level insertion button so it can be added at the end of the list after
        /*
        if(insertLevelViaCodeButton != null)
        {
            Destroy(insertLevelViaCodeButton.gameObject);
        }
        */
        // initialize select level buttons:
        customSelectLevelButtons = new Button[numberCustomLevels]; 
        for (int i = 0; i < customSelectLevelButtons.Length; i++)
        {
            int tempI = i+1;// plus one to match naming in creator mode
            Button tmpButton = (Button)Instantiate(selectLevelButtonPrefab);
            tmpButton.transform.SetParent(selectLevelContent.transform, false);
            
            tmpButton.GetComponent<Button>().onClick.AddListener(delegate{SelectLevel(tempI, false);});//Setting what button does when clicked
            tmpButton.transform.GetChild(0).GetComponent<TextMeshProUGUI>().text = tempI + "\nCustom";
            ////Logger.Log("ADDED CUSTOM LEVEL BTN " + tempI, this, true); 
            customSelectLevelButtons[i] = tmpButton;
        }
        // add insert level button
        /*
        insertLevelViaCodeButton = (Button)Instantiate(selectLevelButtonPrefab);
        insertLevelViaCodeButton.transform.SetParent(selectLevelContent.transform, false);
        insertLevelViaCodeButton.GetComponent<Button>().onClick.AddListener(delegate { InsertButtonOnClick(); });//Setting what button does when clicked
        insertLevelViaCodeButton.GetComponent<Button>().GetComponent<Image>().sprite = addLevelImage;
        //insertLevelViaCodeButton.transform.GetChild(0).GetComponent<TextMeshProUGUI>().text = "Add Level\nVia Code";
        //Logger.Log("ADDED INSERT LEVEL BTN ", this, true);
        */
    }

    public void SwitchToNextLevel()
    {
        nextLevel = currentLevel + 1;
        TriggerLevelSwitch();
    }

    public void SwitchToLastLevel()
    {
        nextLevel = currentLevel - 1;
        TriggerLevelSwitch();
    }

    public void RestartCurrentLevel()
    {
        DespawnMenu();
        nextLevel = currentLevel;
        TriggerLevelSwitch();
    }

    public void TakeBackOne()
    { 
        takeback = true;
        TriggerLevelSwitch();
    }

    public void EnterPlayMode()
    {
        isInCreatorMode = false;
        levelSwitchFraction = 0.0f;
        currentActionProgress = 0.0f; // IMportant
        levelSwitchState = 1; // skip deconstructing step
        LoadCustomLevelStrings(); // update custom level strings
        InitializeCustomLevelSelectButtons(); // also update level select buttons with new custom levels
        nextLevel = sceneMemory.isMainMode ? persistence.LoadCurrLevel() : persistence.LoadCurrCustomLevel(); // update current level
        
        ////Logger.Log("ENTERING PLAY MODE WITH MODE: " + sceneMemory.isMainMode + " and nextlevel: " + nextLevel, this, true);
        if (gameState != null)
        {
            DestroyLevelRelatedGameObjects();
        }
        InitializeLevel(nextLevel);
    }

    public void TriggerLevelSwitch()
    {
        playerScript.DespawnIndicator();
        foreach (BallScript ball in ballScriptList) ball.SetValuesForDespawn(levelSwitchUp);
        foreach (FishScript fish in fishScriptList) fish.SetValuesForDespawn(levelSwitchUp);
        CalcNormalizedPlayerDistances();
        ClearRemainingPlayerActions();
        this.levelSwitchFraction = 0.0f;
        currentActionProgress = 0.0f; // IMportant
        this.levelSwitchState = 0;
        // dont turn player when switching levels
        // this.playerScript.SetNewPreferedDirection(new Direction(Direction.DOWN), false);
        this.cameraController.SetNewCamExitPos();
        //lightScript.SetFastAngle();
    }

    public void ClearRemainingPlayerActions(){
        playerScript.RemoveAllActions();
        ////Logger.Log("REMOVED ALL PLAYER ACTIONS!!!");
    }

    public void ProccessLevelSwitch()
    {
        this.levelSwitchFraction += this.levelSwitchSpeed * Time.deltaTime;
        if (this.levelSwitchFraction > 1.0f)
        {
            this.levelSwitchFraction = 1.0f;
        }

        if (this.levelSwitchState == 0)
        {
            this.LevelDeconstructStep(levelSwitchFraction);
        }
        else if (this.levelSwitchState == 1)
        {
            this.LevelConstructStep(levelSwitchFraction);
        }
        if (this.levelSwitchFraction >= 1.0f)
        {
            this.levelSwitchFraction = 0.0f;
            ////Logger.Log("FINISHED LEVEL PROCCESS " + this.levelSwitchState);
            this.levelSwitchState++;
            ////Logger.Log("SWITCHED TO STATE " + this.levelSwitchState);
            if (this.levelSwitchState == 1)
            {
                ////Logger.Log("ACTUAL GAME STATE LEVEL SWITCH HAPPENING NOW");
                if (gameState != null)
                {
                    DestroyLevelRelatedGameObjects();
                }
                this.InitializeLevel(this.nextLevel, false);
            } else if(this.levelSwitchState == 2){
                ////Logger.Log("Level switch completely over - back in play mode");
            }
        }
    }

    public void InitializeLevel(int levelId, bool mainMenuLevel = false)
    {
        ////Logger.Log("initializing lvl " + levelId + " mainmode: " + sceneMemory.isMainMode, this, true);
        this.t = 0; // reset internal descrete time
        this.inputBuffer.Clear();
        
        //CHANGE TO LEVEL CREATOR
        if (switchToCreator) {
            switchToCreator = false;
            isInCreatorMode = true;
        } else {
            isInCreatorMode = false;
        }

        if (!isInCreatorMode && !mainMenuLevel)
        {
            tutorialController.Trigger("levelload");
            handleInput.Reset();
        }
        string[] tmpLvlStrings = sceneMemory.isMainMode ? levelStrings : customLevelStrings;
        int tmpNumberLevels = sceneMemory.isMainMode ? numberLevels : numberCustomLevels; 
        if (levelId >= tmpLvlStrings.Count())
        {
            ////Logger.Log("NO LEVEL WITH ID: " + levelId + " looping back to first level");
            levelId = 1;
        } else if (levelId < 1)
        {
            ////Logger.Log("NO LEVEL WITH ID: " + levelId + " looping back to last level");
            levelId = tmpLvlStrings.Count()-1;
        }
        if (!mainMenuLevel){
            this.currentLevel = levelId;
            if(sceneMemory.isMainMode){
                persistence.SaveCurrLevel(currentLevel);
                persistence.SaveMaxLevel(currentLevel);
                this.maxLevel = persistence.LoadMaxLevel();
            } else {
                persistence.SaveCurrCustomlevel(currentLevel);
                this.maxLevel = int.MaxValue;
            }
        } else {
            this.currentLevel = persistence.LoadCurrLevel();
        }
        levelDisplay.text = "" + currentLevel + " / " + tmpNumberLevels; 
        //getting previous current player pos for leveloffset calculations
        if (gameState != null)
        {
            this.totalPlayerModelMovementX += (gameState.player.x - gameState.player.initialX);
            this.totalPlayerModelMovementY += (gameState.player.y - gameState.player.initialY);
        }
        ////Logger.Log("player Movement in total model space: " + this.totalPlayerModelMovementX + " " + this.totalPlayerModelMovementY, this, true);
       
        ////Logger.Log("Creating GameState for Level " + levelId);
        if(mainMenuLevel){
            this.gameState = new GameState(persistence.GetEmptyLevelString());
        } else {
            if (takeback){
                this.gameState = gameState.pastGameState;  
                takeback = false; // reset takeback value
            } else {
                if (!isInCreatorMode){
                    this.gameState = new GameState(tmpLvlStrings[levelId]);  
                }
            }
        }
        ////Logger.Log("gamestate player Pos for next level: " + gameState.player.x + " " + gameState.player.y, this, true);
        
        this.levelShiftX = gameState.player.x - totalPlayerModelMovementX;
        this.levelShiftY = gameState.player.y - totalPlayerModelMovementY;
        ////Logger.Log("FINAL levelShift for next level: " + this.levelShiftX + " " + this.levelShiftY, this, true);
        ////Logger.Log("Spawning Game Objects based on gamestate for Level " + levelId);
        
        if(!isInCreatorMode){
            this.SpawnGameObjectsFromGameState();
            this.CalcNormalizedPlayerDistances();
        } else {
            levelCreatorManager.EnterCreatorMode();
        }
        
        //update new level mid pos and size scale for cam script
        if(!isInCreatorMode){
            if (mainMenuLevel) {
                this.cameraController.SetInitialCamPosForGame();
                this.cameraController.SetNewCamMenuPos();
            } else {
                SetCamForLevel();
            }
        }
        
        //update ocean stuff to move after player
        this.oceanScript.SetGoalPos(player.transform.position);
        float relativeLevelProgress = System.Convert.ToSingle(currentLevel - 1) / System.Convert.ToSingle(tmpNumberLevels - 1);
        if (tmpNumberLevels <= 1) {
            relativeLevelProgress = 0;
        }
        //update postprocessing script to shift to correct volume
        ////Logger.Log("relative level progress: " + relativeLevelProgress, this, true);
        this.postprocessingScript.SetGoalProgress(relativeLevelProgress);
        //update light script to shift to correct light property interpolation
        this.lightScript.SetGoalProgress(relativeLevelProgress);


        // trigger level raising animation stuff
        if(!isInCreatorMode){
            this.levelSwitchFraction = 0.0f;
            this.levelSwitchState = 1;
        } else {
            this.levelSwitchFraction = 0.0f;
            this.levelSwitchState = 2;
        }
       
        // remember a real level was loaded
        if (!mainMenuLevel) {
            sceneMemory.initialMenu = false;
        }
    }

    public void SetCamForLevel()
    {
        this.cameraController.SetLevelMidPointAndAdjustToLevelSize(CalculateLevelMidAndSize());
        this.cameraController.SetNewCamEnterPos();
    }

    public void SpawnMenu()
    {
        //first disable player input:
        handleInput.disableInput = true;
        ////Logger.Log("MENU SPAWN: INPUT DISABLED!", this, true);
        this.inputBuffer.Clear();
        //enable buttons and level progress display
        if (!sceneMemory.initialMenu) {
            //playButton.GetComponentInChildren<Text>().text = "CONTINUE";
            SetPlayerPreferedLookDirection(new Direction(3), false);
            playerScript.DespawnIndicator();

            //check if there are past game states, if so display undo and restart button
            ////Logger.Log("N PAST GAMESTATES: " + gameState.n_pastGameStates, this, true);
            if(gameState.n_pastGameStates > 0){
                undoButton.gameObject.SetActive(true);
                restartButton.gameObject.SetActive(true);
                playButton.gameObject.transform.position = playButtonOriginalPosition;
            } else {
                undoButton.gameObject.SetActive(false);
                restartButton.gameObject.SetActive(false);
                playButton.gameObject.transform.position = restartButton.gameObject.transform.position;
            }
        } 
           
        
        SetUIElementsActive(true);
        if (sceneMemory.initialMenu) {
            restartButton.gameObject.SetActive(false);
            undoButton.gameObject.SetActive(false);
            playButton.gameObject.transform.position = restartButton.gameObject.transform.position;
            InitializeLevel(currentLevel, true);
        } else {
            this.cameraController.SetNewCamMenuPos();
        }
    }
    
    public void DespawnMenu()
    {
        handleInput.Reset();
        SetUIElementsActive(false);
        tutorialController.Trigger("menu_close");
        if (sceneMemory.initialMenu) {
            DestroyLevelRelatedGameObjects();
            InitializeLevel(currentLevel);
        } 
        else {
            this.cameraController.SetNewCamEnterPos();
        }
    }

    public void UndoBttn(){
        ////Logger.Log("Undo Button pressed! to be implemented", this, true);
        TakeBackOne();
        DespawnMenu();
    }

    public void BackBttn(bool active){
        ////Logger.Log("OPEN Select level pressed! to be implemented", this, true);
        SetSelectLevelActive(active);
        SetUIElementsActive(!active);
    }

    public void SelectLevelBttn(bool active){
        ////Logger.Log("OPEN Select level pressed! to be implemented", this, true);
        // TODO DO THIS INITIALLY AND THEN ONLY WHEN CHANGE IS NEEDED, NOT EVERY TIME MENU IS OPENED
        if(active) {
            SetSelectLevelButtonsState();
            SetSelectCustomLevelButtonsState();
        }
        SetSelectLevelActive(active);
        SetUIElementsActive(!active);
    }

    public void SetSelectLevelActive(bool active){
        ////Logger.Log("OPEN Select level pressed! to be implemented", this, true);
        SelectLevelFader.Fade(active);

    }

    public void SetUIElementsActive(bool active){
        MenuFader.Fade(active);
    }

    public void SetSelectLevelButtonsState(){
        int maxLevel = Mathf.Min(this.persistence.LoadMaxLevel(), selectLevelButtons.Length);
        ////Logger.Log("Setting Select Level Buttons state: max level: " + maxLevel, this, true);
        for (int i = 1; i < maxLevel ; i++){
           // //Logger.Log("i: " + i, this, true);
            selectLevelButtons[i].transform.GetComponent<Image>().sprite = solvedLevelSprite;
            selectLevelButtons[i].transform.GetComponent<Image>().color = selectLevelClickableColor;
            selectLevelButtons[i].interactable = true;
            if(sceneMemory.isMainMode && i == currentLevel)
            {
                selectLevelButtons[i].transform.GetComponent<Image>().color = selectLevelCurrentSelectionColor;
            }
        }
        selectLevelButtons[maxLevel].transform.GetComponent<Image>().color = selectLevelClickableColor; 
        selectLevelButtons[maxLevel].interactable = true;
        for (int i = maxLevel + 1; i < selectLevelButtons.Length; i++){
            ////Logger.Log("i: " + i, this, true);
            selectLevelButtons[i].transform.GetComponent<Image>().color = selectLevelNonClickableColor;
            selectLevelButtons[i].interactable = false;
        }
    }

    public void SetSelectCustomLevelButtonsState(){
        for (int i = 1; i < customSelectLevelButtons.Length +1; i++){
            if(!sceneMemory.isMainMode && i == currentLevel)
            {
                customSelectLevelButtons[i - 1].transform.GetComponent<Image>().color = selectLevelCurrentSelectionColor;
            }else
            {
                customSelectLevelButtons[i - 1].transform.GetComponent<Image>().color = selectLevelClickableColor;
            }
            if (persistence.GetCustomLevelSolved(customLevelStrings[i])){
                customSelectLevelButtons[i-1].transform.GetComponent<Image>().sprite = solvedLevelSprite;
            } else
            {
                customSelectLevelButtons[i - 1].transform.GetComponent<Image>().sprite = unSolvedLevelSprite;
            }
        }
    }

    public void SelectLevel(int level, bool mainMode = true){
        SetSelectLevelActive(false);
        ////Logger.Log("Select level " + level + " pressed! mainMode: " + mainMode, this, true);
        sceneMemory.isMainMode = mainMode;
        this.nextLevel = level;
        handleInput.Reset();
        if (sceneMemory.initialMenu) {
            DestroyLevelRelatedGameObjects();
            InitializeLevel(level);
        } 
        else {
            TriggerLevelSwitch();
        }
    }

    /*
    public void InsertButtonOnClick()
    {
        string clipBoardLevelString = Util.Decrypt(Util.DecompressString(UniClipboard.GetText()));
        //Logger.Log("INSERT NEW LEVEL BUTTON CLICKED, clipboard= " + clipBoardLevelString, this, true);
        bool levelValid = true;
        try
        {
            GameState tmpGamestate = new GameState(clipBoardLevelString);

        }
        catch (Exception e)
        {
            levelValid = false;
            //Logger.Log("Level insertion failed, invalid level string: " + clipBoardLevelString, this, true);
        }
        if (levelValid)
        {
            //Logger.Log("valid level string", this, true);

            ////Logger.Log("CHECKING LEVEL " + clipBoardLevelString, this, true);

            bool isNew = true;
            //check if this level exists already
            foreach (string lvl in customLevelStrings)
            {
                if (lvl.Equals(clipBoardLevelString)){

                    ////Logger.Log("EXISTING LEVEL " + clipBoardLevelString + " already: " + lvl, this, true);
                    isNew = false;
                    break;
                }
            }

            if (isNew)
                
            {
                //Logger.Log("new level string", this, true);
                //adding new level to level strings
                //Logger.Log("current custom levels: " + customLevelStrings.Length, this, true);
                List<string> tmp = new List<string>();
                tmp.AddRange(customLevelStrings);
                tmp.Add(clipBoardLevelString);
                customLevelStrings = tmp.ToArray();
                //Logger.Log("after inserting new level custom levels: " + customLevelStrings.Length, this, true);
                //Logger.Log("spawnining new level: " + customLevelStrings[customLevelStrings.Length - 2]);
                //saving new level in persistence
                string concat = "";
                foreach (string level in customLevelStrings)
                {
                    if (!level.Equals(""))
                    {
                        concat += "#" + level;
                    }
                }
                GetComponent<Persistence>().SaveCustomLevels(concat);
                //Logger.Log("saved new custom levels: " + concat, this, true);

                // sync list of custom levels with level creator by reloading from memory
                levelCreatorManager.LoadLevelStrings();
                LoadCustomLevelStrings();
                InitializeCustomLevelSelectButtons();
                //finally directly load into newly inserted custom level
                SelectLevel(customLevelStrings.Length - 2, false);
            }
        }
    }
    */


    public void Credits(){
        //Logger.Log("Credits pressed! to be implemented", this, true);
    }

    public void InitializeMuted(){
        if (audiomanager.muted != persistence.LoadMuted()) {
            SwitchMute();
        }
    }

    public void SwitchMute(){
        ////Logger.Log("Switching Mute!", this, true);
        audiomanager.muted = !audiomanager.muted;
        persistence.SaveMuted(audiomanager.muted);
        if(audiomanager.muted) {
            
            ////Logger.Log("Muted", this, true);
            //muteButton.GetComponentInChildren<Text>().text = "UNMUTE";
            muteButton.GetComponent<Image>().sprite = soundOffSprite;
            audiomanager.StopAll();
        } else {
            
            ////Logger.Log("Unmuted", this, true);
            muteButton.GetComponent<Image>().sprite = soundOnSprite;
            //muteButton.GetComponentInChildren<Text>().text = "MUTE";
            audiomanager.Play("Theme");
        }
    }

    public void DestroyLevelRelatedGameObjects()
    {
        ////Logger.Log("Destroying all level related game objects", this, true);
        foreach (GameObject ball in ballList) Destroy(ball);
        foreach (GameObject fish in fishList) Destroy(fish);
        foreach (GameObject ground in groundList) Destroy(ground);
    }


    public void SpawnGameObjectsFromGameState()
    {
        ballList = new List<GameObject>();
        fishList = new List<GameObject>();
        groundList = new List<GameObject>();
        ballScriptList = new List<BallScript>();
        fishScriptList = new List<FishScript>();
        groundScriptList = new List<GroundScript>();
        // only spawn player if there was no player before
        if (player == null)
        {
            Vector3 worldPos = Util.ConvertToHexa(gameState.player.x, gameState.player.y, this.levelShiftX, this.levelShiftY);
            player = Instantiate(playerIni, worldPos, Quaternion.identity) as GameObject;
            playerScript = player.GetComponent<PlayerScript>();
            playerScript.Init(gameState.player, worldPos, GetComponent<GameManagerScript>());
        }
        else
        {
            // otherwise just update the model reference
            playerScript.modelReference = gameState.player;
        }
        for (int x = 0; x < gameState.grounds.GetLength(0); x++)
        {
            for (int y = 0; y < gameState.grounds.GetLength(1); y++)
            {
                Ground ground = gameState.grounds[x, y];
                Vector3 worldPos = Util.ConvertToHexa(x, y, this.levelShiftX, this.levelShiftY);
                GameObject groundObject = null;
                GameObject snow = null;
                GroundScript groundScript = null;
                GroundScript snowScript = null;
                switch (ground.id)
                {
                    case Ground.SNOW:
                         groundObject = Instantiate(snowIni, worldPos + levelSwitchDown, Quaternion.identity) as GameObject;
                         break;
                    case Ground.ICE:
                         groundObject = Instantiate(iceIni, worldPos + levelSwitchDown, Quaternion.identity) as GameObject;
                         break;
                    case Ground.CRACK:
                         groundObject = Instantiate(crackIni, worldPos + levelSwitchDown, Quaternion.identity) as GameObject;
                         break;
                    case Ground.TREE:
                         groundObject = Instantiate(treeIni, worldPos + levelSwitchUp, Quaternion.identity) as GameObject;
                         snow = Instantiate(snowIni, worldPos + levelSwitchDown, Quaternion.identity) as GameObject;
                         break;
                    case Ground.FIRE:
                         groundObject = Instantiate(fireIni, worldPos + levelSwitchUp, Quaternion.identity) as GameObject;
                         snow = Instantiate(snowIni, worldPos + levelSwitchDown, Quaternion.identity) as GameObject;
                         break;
                    case Ground.GOAL:
                         groundObject = Instantiate(goalIni, worldPos + levelSwitchUp, Quaternion.identity) as GameObject;
                         snow = Instantiate(snowIni, worldPos + levelSwitchDown, Quaternion.identity) as GameObject;
                         break;
                    case Ground.WATER:
                        groundObject = Instantiate(waterIni, worldPos + levelSwitchDown, Quaternion.identity) as GameObject;
                        break;
                }
                if (ground != null)
                {
                    groundList.Add(groundObject);
                    groundScript = groundObject.GetComponent<GroundScript>();
                    if (groundScript != null)
                    {
                        groundScriptList.Add(groundScript);
                        groundScript.Init(ground, worldPos);
                    }
                    //else //Logger.Log("ground Script was NULLLLLL!!");
                } //else //Logger.Log("ground object was NULLLLLL!!");
                if (snow != null)
                {
                    groundList.Add(snow);
                    snowScript = snow.GetComponent<GroundScript>();
                    if (snowScript != null)
                    {
                        groundScriptList.Add(snowScript);
                        // uses faked snow model ref because it is not needed for checks
                        snowScript.Init(new Ground(Ground.SNOW), worldPos);
                    }
                    //else //Logger.Log("ground Script was NULLLLLL!!");
                } //else //Logger.Log("ground object was NULLLLLL!!");
            }
        }

        for (int i = 0; i < gameState.balls.GetLength(0); i++)
        {
            // dont instantiate "dead" balls
            if(!gameState.balls[i].alive){
                continue;
            }
            Vector3 worldPos = Util.ConvertToHexa(gameState.balls[i].x, gameState.balls[i].y, this.levelShiftX, this.levelShiftY);
            GameObject ball;
            if(gameState.balls[i].id == Ground.SNOW){
                ball = Instantiate(snowBallIni, worldPos + levelSwitchUp, Quaternion.identity) as GameObject;
            } else {
                ball = Instantiate(iceBallIni, worldPos + levelSwitchUp, Quaternion.identity) as GameObject;
            }
            BallScript ballScript = ball.GetComponent<BallScript>();
            ballScript.Init(gameState.balls[i], worldPos, moveSpeed, this);
            ballList.Add(ball);
            ballScriptList.Add(ballScript);
        }
        for (int i = 0; i < gameState.fish.GetLength(0); i++)
        {
            // dont instantiate "dead" fish
            if(!gameState.fish[i].alive){
                continue;
            }
            Vector3 worldPos = Util.ConvertToHexa(gameState.fish[i].x, gameState.fish[i].y, this.levelShiftX, this.levelShiftY);
            GameObject fish = Instantiate(fishIni, worldPos + levelSwitchUp, Quaternion.identity) as GameObject;
            FishScript fishScript = fish.GetComponent<FishScript>();
            fishScript.Init(gameState.fish[i], worldPos, player.transform);
            fishList.Add(fish);
            fishScriptList.Add(fishScript);
        }
        
        ////Logger.Log("SPAWNED GAMEOBJECTS FOR GAMESTATE. level:" + currentLevel + "" , this, true);
    }

    public void LevelDeconstructStep(float fraction)
    {
        ////Logger.Log("LEVEL DECONSTRUCT STEP : " + fraction);
        foreach(GroundScript ground in groundScriptList)
        {
            ground.LevelSwitchDeconstruct(fraction);
        }
        foreach (FishScript fish in fishScriptList)
        {
            fish.LevelSwitchDeconstruct(fraction);
        }
        foreach (BallScript ball in ballScriptList)
        {
            ball.LevelSwitchDeconstruct(fraction);
        }
    }

    public void LevelConstructStep(float fraction)
    {
        ////Logger.Log("LEVEL CONSTRUCT STEP : " + fraction, this, true);
        foreach (GroundScript ground in groundScriptList)
        {
            ground.LevelSwitchConstruct(fraction);
        }
        foreach (FishScript fish in fishScriptList)
        {
            fish.LevelSwitchConstruct(fraction);
        }
        foreach (BallScript ball in ballScriptList)
        {
            ball.LevelSwitchConstruct(fraction);
        }
    }

    public void CalcNormalizedPlayerDistances(){
        float minDist = float.PositiveInfinity;
        float maxDist = float.NegativeInfinity;
        float tmp;
        foreach (GroundScript ground in groundScriptList)
        {
            tmp = ground.CalcDistToPlayer(player);
            if(tmp < minDist){
                minDist = tmp;
            }
            if(tmp > maxDist){
                maxDist = tmp;
            }
        }
        foreach (FishScript fish in fishScriptList)
        {
            tmp = fish.CalcDistToPlayer(player);
            if (tmp < minDist)
            {
                minDist = tmp;
            }
            if (tmp > maxDist)
            {
                maxDist = tmp;
            }
        }
        foreach (BallScript ball in ballScriptList)
        {
            tmp = ball.CalcDistToPlayer(player);
            if (tmp < minDist)
            {
                minDist = tmp;
            }
            if (tmp > maxDist)
            {
                maxDist = tmp;
            }
        }
        foreach (GroundScript ground in groundScriptList)
        {
            ground.SetSmoothDist((ground.distToPlayer - minDist) / (maxDist - minDist));
        }
        foreach (FishScript fish in fishScriptList)
        {
            fish.SetSmoothDist((fish.distToPlayer - minDist) / (maxDist - minDist));
        }
        foreach (BallScript ball in ballScriptList)
        {
            ball.SetSmoothDist((ball.distToPlayer - minDist) / (maxDist - minDist));
        }
    }
    public Vector4 CalculateLevelMidAndSizeWithGivenKoords(float maxX, float minX, float maxY, float minY, float bonusMidLevelPerspectiveScalar = 0.0f)
    {
        float width = maxX - minX;
        float height = maxY - minY; 
        float screenRatio = cameraRef.aspect;

        //perhaps flip level dims and camera direction later (code with minus level size) to fit level better in screen
        Vector3 fixedMidOffset = new Vector3(0.0f, 0.0f, - height * (midLevelPerspectiveScalar + bonusMidLevelPerspectiveScalar));
        float levelSizeMultiplier = 1;
        if(width > height){
            float tmp = width;
            width = height;
            height = tmp;
            levelSizeMultiplier = -1;
            fixedMidOffset = new Vector3(-height * (midLevelPerspectiveScalar + bonusMidLevelPerspectiveScalar), 0.0f, 0.0f);
        }
        
        //plus 1 hack because the perspective is allowing to see more in front then to the side
        float levelSize = Mathf.Max(width + 1, height * screenRatio);
        ////Logger.Log("Level Width: " + width + "\nLevel Height: " + height + "\nAspect Ratio: " + screenRatio + "\nLevel Size: " + levelSize);
        Vector3 minPos = new Vector3(minX, 0.0f, minY);
        Vector3 maxPos = new Vector3(maxX, 0.0f, maxY);
        Vector3 midPos = minPos * 0.5f + maxPos * 0.5f + fixedMidOffset;
        return new Vector4(midPos.x, midPos.y, midPos.z, levelSize * levelSizeMultiplier);
    }
    public Vector4 CalculateLevelMidAndSize()
    {
        float minX = float.PositiveInfinity;
        float maxX = float.NegativeInfinity;
        float minY = float.PositiveInfinity;
        float maxY = float.NegativeInfinity;
        foreach (GroundScript ground in groundScriptList)
        {
            if (ground.id != Ground.WATER)
            {
                Vector3 pos = ground.gameObject.transform.position;
                if (pos.x > maxX)
                {
                    maxX = pos.x;
                }
                if (pos.x < minX)
                {
                    minX = pos.x;
                }
                if (pos.z > maxY)
                {
                    maxY = pos.z;
                }
                if (pos.z < minY)
                {
                    minY = pos.z;
                }
            }
        }
        return CalculateLevelMidAndSizeWithGivenKoords(maxX, minX, maxY, minY);
    }

    //accept input from handleinput class so we dont have to deal with pixel or screen or keyboard stuff here
    public void RegisterInput(int directionId)
    {
        this.inputBuffer.Enqueue(directions[directionId]);
        playerScript.DespawnIndicator();
        // deactive input indicator as soon as input is detected
        ////Logger.Log("ENQUEUED INPUT DIRECTION: " + directions[directionId].name);
    }

    //recursivly build up action queues for player balls and fish for the next move in input buffer
    public void ProcessNextInput()
    {
        ////Logger.Log("Checking for new input in buffer");
        ////Logger.Log("PROCESSING NEXT INPUT, QUEUED INPUTS: " + this.inputBuffer.Count);
        if (this.inputBuffer.Count == 0)
        {
            // if nothing happended set player to idle
            //playerScript.SetPlayerIdle();
            return;
        }
        else
        {   
            this.currentDirection = inputBuffer.Dequeue(); 
            DateTime dt = DateTime.Now;
            //getting Milliseconds only from the currenttime
            int ms0 = dt.Millisecond;
            ////Logger.Log("Processing new input " + currentDirection.name);
            this.gameState.InitializeValuesForInitialAppyStep();
            bool needToApplyAgain;
            int tOffset = 0;
            do
            {
                this.RememberReference();
                needToApplyAgain = this.gameState.ApplyStep(this.currentDirection);
                this.GenerateActions(tOffset);
                tOffset += 2; //one game state step is 2 timesteps for animation
            } while (needToApplyAgain);
            dt = DateTime.Now;
            //getting Milliseconds only from the currenttime
            int ms1 = dt.Millisecond;
            this.cameraController.SetNewCamPos();
            ////Logger.Log("Finished Model Calculations, took " + (ms1-ms0) + " ms", this, true);
        }
    }

    public void ProcessActions()
    {
        //handle onetime actions only once(beginning)
        if (this.currentActionProgress <= 0.0f)
        {
            ////Logger.Log("STARTING ACTIONS WITH TIME ID " + this.t);
            HandleEventActions();
            RemovePastEventActions(); // usually all past event actions should be automatically removed right after handing but this seems to be bugged.
            didPrintUpdateDebug = false;
            if (didPrintUpdateDebug) didPrintUpdateDebug = true; // this is throw away code for unity to stop warning about not using this bool
            ////Logger.Log("DEALING WITH MOVE ACTIONS WITH TIMEID: " + this.t);
        }
        // if levelswitch state went to level switch from events like dying or winning
        if(levelSwitchState != 2){
            ////Logger.Log("DONE WITH ACTIONS / WE ARE IN LEVEL SWITCH STATE");
            return;
        }
        if (!this.HasActions())
        {
            ////Logger.Log("Dealt with all actions, T:" + this.t);
            return;
        }
        //increase
        this.currentActionProgress += Time.deltaTime * this.moveSpeed;
        //clamp
        if (this.currentActionProgress > 1.0f)
        {
            this.currentActionProgress = 1.0f;
        }
        ////Logger.Log("CURRENT ACTION PROGRESS " + currentActionProgress + " TIME:" + Time.time * 1000);
        HandleMovementActions();
        if (this.currentActionProgress >= 1.0f)
        {
            ////Logger.Log("DONE DEALING WITH MOVE ACTIONS WITH TIMEID: " + this.t + " and increasing to " + (this.t+1));
            this.currentActionProgress = 0;
            this.t++;
            RemovePastMovementActions();
        } else {
            ////Logger.Log("NOT YET DONE WITH ACTIONS " + t + " " + currentActionProgress);
        }
    }

    public bool HasActions()
    {
        if (playerScript.HasActions()) return true;
        foreach(BallScript ball in ballScriptList) if (ball.HasActions()) return true;
        foreach (FishScript fish in fishScriptList) if (fish.HasActions()) return true;
        foreach (GroundScript ground in groundScriptList) if (ground.HasActions()) return true;
        return false;
    }

    public void RemovePastMovementActions()
    {
        playerScript.RemovePastMoveActions(t);
        foreach (BallScript ball in ballScriptList) ball.RemovePastMoveActions(t);
    }

    public void RemovePastEventActions()
    {
        playerScript.RemovePastEventActions(t);
        foreach (BallScript ball in ballScriptList) ball.RemovePastEventActions(t);
        foreach (FishScript fish in fishScriptList) fish.RemovePastEventActions(t);
        foreach (GroundScript ground in groundScriptList) ground.RemovePastEventActions(t);
    }

    public void RememberReference()
    {
        playerScript.RememberReference();
        foreach (BallScript ball in ballScriptList) ball.RememberReference();
        foreach (FishScript fish in fishScriptList) fish.RememberReference();
        foreach (GroundScript ground in groundScriptList) ground.RememberReference();
    }

    public void GenerateActions(int tOffset)
    {
        ////Logger.Log("Generating actions for timeoffset: " + tOffset, this, true);
        int time = t + tOffset;
        playerScript.GenerateActions(time,  gameState);
        foreach (BallScript ball in ballScriptList) ball.GenerateActions(time, gameState);
        foreach (FishScript fish in fishScriptList) fish.GenerateActions(time);
        foreach (GroundScript ground in groundScriptList) ground.GenerateActions(time, gameState.AreAllFishCollected());
    }

    public void HandleEventActions()
    {
        playerScript.HandleEventActions(t);
        foreach (BallScript ball in ballScriptList) ball.HandleEventActions(t);
        foreach (FishScript fish in fishScriptList) fish.HandleEventActions(t);
        foreach (GroundScript ground in groundScriptList) ground.HandleEventActions(t);
    }

    public void HandleMovementActions()
    {
        playerScript.HandleMovementActions(t, currentActionProgress, currentDirection, levelShiftX, levelShiftY);
        foreach (BallScript ball in ballScriptList) ball.HandleMovementActions(t, currentActionProgress, currentDirection, levelShiftX, levelShiftY);
        didPrintUpdateDebug = true;
    }

    public Vector3 GetPlayerPos(){
        return player.transform.position;
    }

    public Vector3 GetPlayerGoalPos(){
        return Util.ConvertToHexa(playerScript.modelReference.x, playerScript.modelReference.y, levelShiftX, levelShiftY);
    }

    public void SetPlayerPreferedLookDirection(Direction direction,bool spawnIndicator = true){
        ////Logger.Log("Set new prefered look direction for player");
        playerScript.SetNewPreferedDirection(direction,spawnIndicator);
    }

    public int GetCurrentLevel()
    {
        return currentLevel;
    }

    public Color GetCurrentSelectionColor()
    {
        return selectLevelCurrentSelectionColor;
    }

    public void AddCurrentCustomLevelSolved(){
        // only save custom level solved
        if (sceneMemory.isMainMode)
        {
            return;
        }
        //Logger.Log("Saving solved custom level: " + currentLevel, this, true);
        persistence.AddCustomLevelSolved(customLevelStrings[currentLevel]);
    }
}
