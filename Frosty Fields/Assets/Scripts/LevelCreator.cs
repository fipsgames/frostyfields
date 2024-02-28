using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;
using util;
using gameState;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;
using TMPro;
using System.Collections;

public class LevelCreator : MonoBehaviour {

    public GameManagerScript gameManager;

    public TutorialController tutorialController;

    Persistence persistence;
    //levels
    string[] levelStrings;
    int numberLevels;

    int playerOffsetX = 0;
    int playerOffsetY = 0;

    int currLevel;
    
    public int maxLvlSize = 20;

    // level world size
    float maxWorldX; 
    float maxWorldY;
    float minWorldX;
    float minWorldY;
    public float bonusZoomOutForCamX;
    public float bonusZoomOutForCamY;
    public float maxSizeForCam;

    float currCamZoom;

    //UI STUFF
    public Button buttonRecieveLevel;
    public Button buttonDelete;

    //public Button buttonMoveUp;
    //public Button buttonMoveDown;

    public Button buttonSnow;
    public Button buttonIce;
    public Button buttonCrack;
    public Button buttonTree;
    public Button buttonFire;
    public Button buttonGoal;
    public Button buttonFish;
    public Button buttonSnowBall;
    public Button buttonIceBall;
    public Button buttonPlayer;
    public Button buttonPlayCustom;
    //public Button buttonPlayMain;
    public Button selectButton;
    public Button shareButton;

    public TextMeshProUGUI currentLevelDisplay;

    
    public CanvasFader UIFader;
    public CanvasFader SelectLevelFader;

    public GameObject selectLevelContent;
    public GameObject selectLevelScrollView;
    Button[] selectLevelButtons;
    Button newButton;
    public Sprite addButtonImage;

    //PREFABS
    public GameObject snowIni;
    public GameObject iceIni;
    public GameObject crackIni;
    public GameObject fireIni;
    public GameObject treeIni;
    public GameObject goalIni;
    public GameObject snowBallIni;
    public GameObject iceBallIni;
    public GameObject fishIni;
    public GameObject playerIni;
    public Button selectLevelButtonPrefab;


    public Sprite backButtonSprite;

    public Sprite solvedLevelSprite;
    public Sprite unSolvedLevelSprite;

    public Camera cameraRef;
    public CameraController cameraController;

    char[,] groundsCode;
    GameObject[,] grounds;

    bool[,] snowBallsCode;
    GameObject[,] snowBalls;

    bool[,] iceBallsCode;
    GameObject[,] iceBalls;

    bool[,] fishCode;
    GameObject[,] fishs;

    int playerX, playerY;
    int layerMask = 1 << 8;

    int startX, endX, startY, endY;

    char currentSelection = 'W';

    Vector2 lastPos = new Vector2();
    Vector2 lastScreenPos = new Vector2();

    SceneMemory sceneMemory;

    public bool active;

    public GraphicRaycaster graphicRaycaster;
    public PointerEventData pointerEventData;   
    
    public LightScript lightScript;
    public PostProcessingScript postprocessingScript;

    private Color unselectedButtonColor;
    private Color selectedButtonColor;
    private Color nonClickableButtonColor;

    public float colorDarkenFraction;
    public float colorLightenFraction;

    void Start()
    {   
        
        ////Logger.Log("LEVEL CREATOR START ROUTINE", this, true);
        active = false;
        persistence = gameObject.GetComponent<Persistence>();
        sceneMemory = FindObjectOfType<SceneMemory>();
        groundsCode = new char[maxLvlSize, maxLvlSize];
        grounds = new GameObject[maxLvlSize, maxLvlSize];
        snowBallsCode = new bool[maxLvlSize, maxLvlSize];
        snowBalls = new GameObject[maxLvlSize, maxLvlSize];
        iceBallsCode = new bool[maxLvlSize, maxLvlSize];
        iceBalls = new GameObject[maxLvlSize, maxLvlSize];
        fishCode = new bool[maxLvlSize, maxLvlSize];
        fishs = new GameObject[maxLvlSize, maxLvlSize];
        currLevel = GetComponent<Persistence>().LoadCurrCustomLevel();
        unselectedButtonColor = buttonDelete.GetComponent<Image>().color;
        selectedButtonColor = new Color(unselectedButtonColor.r * colorLightenFraction, 
                                                        unselectedButtonColor.g * colorLightenFraction,
                                                        unselectedButtonColor.b * colorLightenFraction,
                                                        unselectedButtonColor.a);
        nonClickableButtonColor = new Color(unselectedButtonColor.r * colorDarkenFraction,
                                                        unselectedButtonColor.g * colorDarkenFraction,
                                                        unselectedButtonColor.b * colorDarkenFraction,
                                                        unselectedButtonColor.a);
        SetButtonOnClick();
        clearData();
        LoadLevelStrings(); 
        InitializeLevelSelectButtons();
        DeselectAllButtons();
        //Select(buttonSnow, 'S');
        
    }

    /*
    public void SwapLevel(int i)
    {

        //Logger.Log("swapping custom level " + currLevel + " and " + (currLevel+i), this, true);
        String tmp = levelStrings[currLevel];
        levelStrings[currLevel] = levelStrings[currLevel + i];
        levelStrings[currLevel + i] = tmp;
        currLevel += i;
        Save();
        Load();
    }
    */

    public void EnterCreatorMode(){
        SetUIElementsActive(true);
        Load();
        active = true;
        tutorialController.Trigger("creator_open");
    }

    public void ExitCreatorMode(){
        active = false;
        clearData();
        SetUIElementsActive(false);
    }
    public void SetUIElementsActive(bool active){
        UIFader.Fade(active);
    }
    public void SetSelectLevelActive(bool active){
        SelectLevelFader.Fade(active);
        if(!active)
        {
            tutorialController.Trigger("creator_interacted_with_delete_select_add");

        }
    }

    public void SetButtonOnClick(){
        Button ReceiveBtn = buttonRecieveLevel.GetComponent<Button>();
        Button deleteBtn = buttonDelete.GetComponent<Button>();
        //Button moveUpBtn = buttonMoveUp.GetComponent<Button>();
        //Button moveDownBtn = buttonMoveDown.GetComponent<Button>();
        Button snowBtn = buttonSnow.GetComponent<Button>();
        Button iceBtn = buttonIce.GetComponent<Button>();
        Button crackBtn = buttonCrack.GetComponent<Button>();
        Button treeBtn = buttonTree.GetComponent<Button>();
        Button fireBtn = buttonFire.GetComponent<Button>();
        Button goalBtn = buttonGoal.GetComponent<Button>();
        Button fishBtn = buttonFish.GetComponent<Button>();
        Button snowBallBtn = buttonSnowBall.GetComponent<Button>();
        Button iceBallBtn = buttonIceBall.GetComponent<Button>();
        Button playerBtn = buttonPlayer.GetComponent<Button>();
        Button playCustomBtn = buttonPlayCustom.GetComponent<Button>();
        //Button playMainBtn = buttonPlayMain.GetComponent<Button>();
        //Button moveBtn = buttonMove.GetComponent<Button>();
        Button selectBtn = selectButton.GetComponent<Button>();
        Button shareBtn = shareButton.GetComponent<Button>();
        // level buttons
        ReceiveBtn.onClick.AddListener(Receive);
        deleteBtn.onClick.AddListener(Delete);
        /*
        moveUpBtn.onClick.AddListener(delegate {
            Move(1);
        });
        moveDownBtn.onClick.AddListener(delegate {
            Move(-1);
        });
        */
        //selection buttons
        snowBtn.onClick.AddListener(delegate {
            Select(snowBtn, 'S');
        });
        iceBtn.onClick.AddListener(delegate {
            Select(iceBtn, 'I');
        });
        crackBtn.onClick.AddListener(delegate {
            Select(crackBtn, 'C');
        });
        treeBtn.onClick.AddListener(delegate {
            Select(treeBtn, 'T');
        });
        fireBtn.onClick.AddListener(delegate {
            Select(fireBtn, 'F');
        });
        goalBtn.onClick.AddListener(delegate {
            Select(goalBtn, 'G');
        });

        fishBtn.onClick.AddListener(delegate {
            Select(fishBtn, 'H');
        });
        snowBallBtn.onClick.AddListener(delegate {
            Select(snowBallBtn, 'B');
        });
        iceBallBtn.onClick.AddListener(delegate {
            Select(iceBallBtn, 'D');
        });
        playerBtn.onClick.AddListener(delegate {
            Select(playerBtn, 'P');
        });
        /*
        moveBtn.onClick.AddListener(delegate {
            Select(moveBtn, 'M');
        });
        */

        playCustomBtn.onClick.AddListener(delegate {
            SwitchToMainScene(false);
        });
        /*
        playMainBtn.onClick.AddListener(delegate {
            SwitchToMainScene(true);
        });
        */
        selectBtn.onClick.AddListener(delegate {
            SelectLevelBttn(true);
        }); 
        shareBtn.onClick.AddListener(delegate {
            ShareBttnOnClick(true);
        });
    }


    void SetShareButtonState()
    {
        //set share button state
        if (persistence.GetCustomLevelSolved(levelStrings[currLevel]))
        {
            shareButton.transform.GetComponent<Image>().color = unselectedButtonColor;
            shareButton.interactable = true;
            //Logger.Log("Share button clickable", this, true);
            tutorialController.Trigger("creator_level_shareable");
        } else
        {
            shareButton.transform.GetComponent<Image>().color = nonClickableButtonColor;
            shareButton.interactable = false;
            //Logger.Log("Share button NOT clickable", this, true);

        }
    }

    void ShareBttnOnClick(bool mode)
    {
        //Logger.Log("Share button was clicked", this, true);
        //Logger.Log(levelStrings[currLevel], this, true);
        if (persistence.GetCustomLevelSolved(levelStrings[currLevel]))
        {
            //shareButton.transform.GetChild(0).GetComponent<TextMeshProUGUI>().text = "Copied";
            String encrypted = Util.CompressString(Util.Encrypt(levelStrings[currLevel]));

            //StartCoroutine(ShareNative(encrypted));

            UniClipboard.SetText(encrypted);
            //Logger.Log("level copied to clipboard", this, true);
            tutorialController.Trigger("creator_share");
        } else
        {
            //Logger.Log("Level has to be solved before sharing, not copied", this, true);
        }
    }
    /*
    private IEnumerator ShareNative(String text)
    {
        //Logger.Log("tried to open native share dialog", this, true);
        new NativeShare().SetText(text)
            .SetCallback((result, shareTarget) => //Logger.Log("Share result: " + result + ", selected app: " + shareTarget, this, true))
            .Share();
        yield return null;
    }
    */
    void SwitchToMainScene(bool mode)
    {
        ExitCreatorMode();
        sceneMemory.isMainMode = mode;
        gameManager.EnterPlayMode();
    }

    void Select(Button bttn, char selection){
        DeselectAllButtons();
        if (currentSelection == selection){
            currentSelection = 'W';
            ////Logger.Log("selected: " + currentSelection, this, true);
            return;
        }
        currentSelection = selection;
        bttn.GetComponent<ButtonController>().SetSelected(true);
        ////Logger.Log("selected: " + currentSelection, this, true);
        tutorialController.Trigger("creator_button_select");
    }

    void DeselectAllButtons(){
        buttonSnow.GetComponent<ButtonController>().SetSelected(false);
        buttonIce.GetComponent<ButtonController>().SetSelected(false);
        buttonCrack.GetComponent<ButtonController>().SetSelected(false);
        buttonTree.GetComponent<ButtonController>().SetSelected(false);
        buttonFire.GetComponent<ButtonController>().SetSelected(false);
        buttonGoal.GetComponent<ButtonController>().SetSelected(false);
        buttonFish.GetComponent<ButtonController>().SetSelected(false);
        buttonSnowBall.GetComponent<ButtonController>().SetSelected(false);
        buttonIceBall.GetComponent<ButtonController>().SetSelected(false);
        buttonPlayer.GetComponent<ButtonController>().SetSelected(false);
    }

    public void Load()
    {
        ////Logger.Log("Loading Level " + currLevel, this, true);
        this.clearData();
        this.LoadLevel(levelStrings[currLevel]);
        currentLevelDisplay.text = "" + currLevel + " / " + numberLevels; 
        GetComponent<Persistence>().SaveCurrCustomlevel(currLevel);
        this.SetShareButtonState();
    }

    public void Save()
    {
        levelStrings[currLevel] = BuildLevelString();
        string allLevelsString = concatLevels();
        GetComponent<Persistence>().SaveCustomLevels(allLevelsString);
        this.SetShareButtonState();
        ////Logger.Log("Saved Change to ALL LEVELS: " + allLevelsString, this, true);
    }

    public string concatLevels(){
        string concat = "";
        foreach (string level in levelStrings){
            if(!level.Equals("")){
                concat += "#" + level;
            }
        }
        ////Logger.Log("ALL LEVELS: " + concat, this, true);
        return concat;
    }

    public void Delete(){
        if (numberLevels == 1) {
            //Logger.Log("Only One level, replacing with empty level", this, true); 
            List<string> tmp = new List<string>();
            tmp.AddRange(levelStrings);
            tmp.RemoveAt(currLevel);
            tmp.Add(GetComponent<Persistence>().GetEmptyLevelString());
            levelStrings = tmp.ToArray();
            Load();
            Save();
        }else {
            ////Logger.Log("DELETE ACTION WITH CURRLEVEL: " + currLevel, this, true);
            List<string> tmp = new List<string>();
            tmp.AddRange(levelStrings);
            ////Logger.Log("created tmp list of levels: " + tmp.Count, this, true);
            tmp.RemoveAt(currLevel);
            ////Logger.Log("removed level from tmp list at " + (currLevel), this, true);
            levelStrings = tmp.ToArray();
            numberLevels -= 1;
            ////Logger.Log("decreased number of levels from " + (numberLevels+1) + " to " + numberLevels, this, true);
            currLevel -= 1;
            ////Logger.Log("decreased currLevel from " + (currLevel + 1) + " to " + currLevel, this, true);
            if (currLevel == 0) currLevel = 1;
            
            Load();
            Save();

            InitializeLevelSelectButtons();
        }
        tutorialController.Trigger("creator_interacted_with_delete_select_add");


    }

    public void New()
    {
        ////Logger.Log("New Button Click");
        List<string> tmp = new List<string>();
        tmp.AddRange(levelStrings);
        tmp.Add(GetComponent<Persistence>().GetEmptyLevelString());
        levelStrings = tmp.ToArray();

        numberLevels += 1;
        ////Logger.Log("NEW LEVEL ADDED, nLevels: (" + numberLevels + "):\n" + string.Join("\n", levelStrings), this, true);
        SelectLevel(numberLevels);
        Save();
        
        InitializeLevelSelectButtons();

        tutorialController.Trigger("creator_interacted_with_delete_select_add");
    }

    public void Receive()
    {
        ////Logger.Log("INSERT NEW LEVEL BUTTON CLICKED, clipboard= " + UniClipboard.GetText(), this, true);
        bool levelValid = true;
        string clipBoardLevelString = "";
        try
        {

            clipBoardLevelString = Util.Decrypt(Util.DecompressString(UniClipboard.GetText()));
            GameState tmpGamestate = new GameState(clipBoardLevelString);

        }
        catch
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
            foreach (string lvl in levelStrings)
            {
                if (lvl.Equals(clipBoardLevelString))
                {

                    ////Logger.Log("EXISTING LEVEL " + clipBoardLevelString + " already: " + lvl, this, true);
                    isNew = false;
                    break;
                }
            }

            if (isNew)
            {
                //Logger.Log("new level string", this, true);
                //adding new level to level strings
                //Logger.Log("current custom levels: " + levelStrings.Length, this, true);
                List<string> tmp = new List<string>();
                tmp.AddRange(levelStrings);
                tmp.Add(clipBoardLevelString);
                levelStrings = tmp.ToArray();
                //Logger.Log("after inserting new level custom levels: " + levelStrings.Length, this, true);
                //saving new level in persistence
                string concat = "";
                foreach (string level in levelStrings)
                {
                    if (!level.Equals(""))
                    {
                        concat += "#" + level;
                    }
                }
                numberLevels += 1;
                currLevel = numberLevels; 

                Load();
                Save();

                InitializeLevelSelectButtons();
                tutorialController.Trigger("creator_receive_success");
            }
            else
            {
                tutorialController.Trigger("creator_receive_level_not_new");
            }
        } else
        {
            tutorialController.Trigger("creator_receive_fail");
        }
    }

    // moving levels not supported
    /*
    public void Move(int targetOffset)
    {
        ////Logger.Log("BUTTON MOVE " + targetOffset);
        Util.Swap<string>(levelStrings, currLevel, currLevel + targetOffset);
        ////Logger.Log("Moved Level " + targetOffset, this, true);
        Save();
        Load();
    }
    */

    public void clearData(){
        maxWorldX = float.NegativeInfinity; 
        maxWorldY = float.NegativeInfinity;
        minWorldX = float.PositiveInfinity;
        minWorldY = float.PositiveInfinity;
        for (int x = 0; x < grounds.GetLength(0); x++)
        {
            for (int y = 0; y < grounds.GetLength(1); y++)
            {
                groundsCode[x, y] = 'W';
                Destroy(grounds[x, y]);
                grounds[x, y] = Instantiate(new GameObject(), Util.ConvertToHexa(x, y), Quaternion.identity) as GameObject;
                snowBallsCode[x, y] = false;
                Destroy(snowBalls[x, y]);
                snowBalls[x, y] = new GameObject();
                iceBallsCode[x, y] = false;
                Destroy(iceBalls[x, y]);
                iceBalls[x, y] = new GameObject();
                fishCode[x, y] = false;
                Destroy(fishs[x, y]);
                fishs[x, y] = new GameObject();
                playerX = playerY = 0;
            }
        }
    }

    public void LoadLevelStrings()
    {
        levelStrings = GetComponent<Persistence>().LoadCustomLevels().Split('#');
        numberLevels = levelStrings.Length -1;
        //Logger.Log("LOADED ALL CUSTOM LEVEL STRINGS (" + numberLevels + "):\n" + string.Join("\n", levelStrings), this, true);
    }

    public void InitializeLevelSelectButtons(){
        // if there are buttons already: destroy 
        if (selectLevelButtons != null) {
            for (int i = 0; i < selectLevelButtons.Length; i++)
            {
                Destroy(selectLevelButtons[i].gameObject);
            }
        }
        if (newButton != null)
        {
            Destroy(newButton.gameObject);
        }
        // initialize select level buttons:
        selectLevelButtons = new Button[numberLevels + 1];
        // plus one for adding back button on top left
        for (int i = 0; i < selectLevelButtons.Length; i++)
        {
            int tempI = i;
            Button tmpButton = (Button)Instantiate(selectLevelButtonPrefab);
           
            tmpButton.transform.SetParent(selectLevelContent.transform, false);
            if (i == currLevel)
            {
                tmpButton.transform.GetComponent<Image>().color = selectedButtonColor;
            }
            if (i == 0) {
                tmpButton.GetComponent<Button>().onClick.AddListener(delegate{SelectLevelBttn(false);});//Back button should disable select level view
                tmpButton.transform.GetComponent<Image>().sprite = backButtonSprite; //Changing sprite
          
            } else {
                tmpButton.GetComponent<Button>().onClick.AddListener(delegate{SelectLevel(tempI);});//Setting what button does when clicked
                tmpButton.transform.GetChild(0).GetComponent<TextMeshProUGUI>().text = i + "\nCustom";
                if (persistence.GetCustomLevelSolved(levelStrings[i])){
                    tmpButton.transform.GetComponent<Image>().sprite = solvedLevelSprite;
                } else
                {
                    tmpButton.transform.GetComponent<Image>().sprite = unSolvedLevelSprite;
                }
            }
            selectLevelButtons[i] = tmpButton;
        }
        newButton = (Button)Instantiate(selectLevelButtonPrefab);
        newButton.transform.SetParent(selectLevelContent.transform, false);
        newButton.GetComponent<Button>().onClick.AddListener(New);//Setting what button does when clicked
        newButton.GetComponent<Button>().GetComponent<Image>().sprite = addButtonImage;
        //Logger.Log("ADDED INSERT LEVEL BTN ", this, true);
    }

    public void SelectLevelBttn(bool active){
        ////Logger.Log("OPEN Select level pressed! to be implemented", this, true);
        
        InitializeLevelSelectButtons();
        SetSelectLevelActive(active);
        SetUIElementsActive(!active);
    }

    public void SelectLevel(int level){
        SetSelectLevelActive(false);
        SetUIElementsActive(true);
        currLevel = level;
        ////Logger.Log("selected creator level " + currLevel, this, true);
        if (active) Load();
    }

    public void LoadLevel(string levelString){
        ////Logger.Log("LEVEL CREATOR loading level: " + levelString, this, true);
        ////Logger.Log("LEVEL CREATOR total player movement: " + gameManager.totalPlayerModelMovementX + " - " + gameManager.totalPlayerModelMovementX, this, true);
        if (levelString == "") levelString = GetComponent<Persistence>().GetEmptyLevelString();

        int maxX = 0;
        int maxY = 0;
        int numberOfSnowBalls = 0;
        int numberOfIceBalls = 0;
        int requiredFish = 0;
        string[] splitString = levelString.Split(';');
        for (int i = 0; i < splitString.Length; i++)
        {
            splitString[i] = splitString[i].Trim();
        }
        ////Logger.Log("length of string array: " + splitString.Length);

        //Getting Dimensions:
        while (!splitString[maxY + 1].First().Equals('B')) { maxY++; }
        maxX = Util.RemoveWhitespace(splitString[1]).Length;
        string[] levelParts = Util.RangeSubset(splitString, 1, maxY);
        ////Logger.Log("LEVEL SIZES: " + maxX + " " + maxY);

        while (!splitString[maxY + numberOfSnowBalls + 2].First().Equals('D')) { numberOfSnowBalls++; }
        string[] snowBallParts = Util.RangeSubset(splitString, maxY + 2, numberOfSnowBalls);
        ////Logger.Log("NUMBER OF SNOW BALLS: " + numberOfSnowBalls);

        while (!splitString[maxY + numberOfSnowBalls + numberOfIceBalls + 3].First().Equals('F')) { numberOfIceBalls++; }
        string[] iceBallParts = Util.RangeSubset(splitString, maxY + numberOfSnowBalls + 3, numberOfIceBalls);
        ////Logger.Log("NUMBER OF ICE BALLS: " + numberOfIceBalls);

        while (!splitString[maxY + numberOfSnowBalls + numberOfIceBalls + requiredFish + 4].First().Equals('P')) { requiredFish++; }
        string[] fishParts = Util.RangeSubset(splitString, maxY + +numberOfSnowBalls + numberOfIceBalls + 4, requiredFish);
        ////Logger.Log("NUMBER OF FISH: " + requiredFish);

        string playerPart = splitString[maxY + numberOfSnowBalls + numberOfIceBalls + requiredFish + 5];

        int offSetX = (maxLvlSize / 2) - (maxX / 2);
        int offSetY = (maxLvlSize / 2) - (maxY / 2);

        ////Logger.Log("LEVEL CREATOR orignial level string dimensions: " + maxX + " - " + maxY, this, true);
        ////Logger.Log("LEVEL CREATOR calculated offset: " + offSetX + " - " + offSetY, this, true);
        ////Logger.Log("LEVEL CREATOR player orignial level string position: " + Int32.Parse(playerPart.Split('-').First()) + " - " + Int32.Parse(playerPart.Split('-').Last()), this, true);
        

        playerX = Int32.Parse(playerPart.Split('-').First()) + offSetX;
        playerY = Int32.Parse(playerPart.Split('-').Last()) + offSetY;

        playerOffsetX = -gameManager.totalPlayerModelMovementX + playerX;
        playerOffsetY = -gameManager.totalPlayerModelMovementY + playerY;
        
        ////Logger.Log("LEVEL CREATOR player offset: " + playerOffsetX + " - " + playerOffsetY, this, true);
        
        for (int x = 0; x < grounds.GetLength(0); x++)
        {
            for (int y = 0; y < grounds.GetLength(1); y++)
            {
                grounds[x, y] = Instantiate(new GameObject(), Util.ConvertToHexa(x, y, playerOffsetX, playerOffsetY), Quaternion.identity) as GameObject;
            }
        }
        
        for (int y = 0; y < maxY; y++)
        {
            levelParts[y] = Util.RemoveWhitespace(levelParts[y]);
            for (int x = 0; x < maxX; x++)
            {
                char groundType = levelParts[y].ElementAt(x);
                int realX = x  + offSetX;
                int realY = (maxY - y - 1)  + offSetY;
                ////Logger.Log("LEVEL CREATOR instantiate coords: " + realX + " - " + realY, this, true);
        
                groundsCode[realX, realY] = groundType;
                Destroy(grounds[realX, realY]);
                     
                switch (groundType) {
                    case 'W':
                        grounds[realX, realY] = Instantiate(new GameObject(), Util.ConvertToHexa(realX, realY, playerOffsetX, playerOffsetY), Quaternion.identity) as GameObject;
                        break;
                    case 'S':
                        grounds[realX, realY] = Instantiate(snowIni, Util.ConvertToHexa(realX, realY, playerOffsetX, playerOffsetY), Quaternion.identity) as GameObject;
                        break;
                    case 'I':
                        grounds[realX, realY] = Instantiate(iceIni, Util.ConvertToHexa(realX, realY, playerOffsetX, playerOffsetY), Quaternion.identity) as GameObject;
                        break;
                    case 'C':
                        grounds[realX, realY] = Instantiate(crackIni, Util.ConvertToHexa(realX, realY, playerOffsetX, playerOffsetY), Quaternion.identity) as GameObject;
                        break;
                    case 'T':
                        grounds[realX, realY] = Instantiate(treeIni, Util.ConvertToHexa(realX, realY, playerOffsetX, playerOffsetY), Quaternion.identity) as GameObject;
                        GameObject snowGroundtmp1 = Instantiate(snowIni, Util.ConvertToHexa(realX, realY, playerOffsetX, playerOffsetY), Quaternion.identity) as GameObject;
                        snowGroundtmp1.transform.SetParent(grounds[realX, realY].transform, true);
                        break;
                    case 'F':
                        grounds[realX, realY] = Instantiate(fireIni, Util.ConvertToHexa(realX, realY, playerOffsetX, playerOffsetY), Quaternion.identity) as GameObject;
                        GameObject snowGroundtmp2 = Instantiate(snowIni, Util.ConvertToHexa(realX, realY, playerOffsetX, playerOffsetY), Quaternion.identity) as GameObject;
                        snowGroundtmp2.transform.SetParent(grounds[realX, realY].transform, true);
                        break;
                    case 'G':
                        grounds[realX, realY] = Instantiate(goalIni, Util.ConvertToHexa(realX, realY, playerOffsetX, playerOffsetY), Quaternion.identity) as GameObject;
                        GameObject snowGroundtmp3 = Instantiate(snowIni, Util.ConvertToHexa(realX, realY, playerOffsetX, playerOffsetY), Quaternion.identity) as GameObject;
                        snowGroundtmp3.transform.SetParent(grounds[realX, realY].transform, true);
                        break;
                }
            }
        }

        for (int i = 0; i < numberOfSnowBalls; i++)
        {
            int ballX = Int32.Parse(snowBallParts[i].Split('-').First());
            int ballY = Int32.Parse(snowBallParts[i].Split('-').Last());
            snowBallsCode[ballX + offSetX, ballY + offSetY] = true;
            Destroy(snowBalls[ballX + offSetX, ballY + offSetY]);
            snowBalls[ballX + offSetX, ballY + offSetY] = Instantiate(snowBallIni, Util.ConvertToHexa(ballX + offSetX, ballY + offSetY, playerOffsetX, playerOffsetY), Quaternion.identity) as GameObject;
        }

        for (int i = 0; i < numberOfIceBalls; i++)
        {
            int ballX = Int32.Parse(iceBallParts[i].Split('-').First());
            int ballY = Int32.Parse(iceBallParts[i].Split('-').Last());
            iceBallsCode[ballX + offSetX, ballY + offSetY] = true;
            Destroy(iceBalls[ballX + offSetX, ballY + offSetY]);
            iceBalls[ballX + offSetX, ballY + offSetY] = Instantiate(iceBallIni, Util.ConvertToHexa(ballX + offSetX, ballY + offSetY, playerOffsetX, playerOffsetY), Quaternion.identity) as GameObject;
        }

        for (int i = 0; i < requiredFish; i++)
        {
            int fishX = Int32.Parse(fishParts[i].Split('-').First());
            int fishY = Int32.Parse(fishParts[i].Split('-').Last());
            fishCode[fishX + offSetX, fishY + offSetY] = true;
            Destroy(fishs[fishX + offSetX, fishY + offSetY]);
            fishs[fishX + offSetX, fishY + offSetY] = Instantiate(fishIni, Util.ConvertToHexa(fishX + offSetX, fishY + offSetY, playerOffsetX, playerOffsetY), Quaternion.identity) as GameObject;
        }

        MeasureLevelSizeAndSetCam();


        float relativeLevelProgress = System.Convert.ToSingle(currLevel - 1) / System.Convert.ToSingle(numberLevels - 1);
        if (numberLevels <= 1) {
            relativeLevelProgress = 0;
        }
        //update postprocessing script to shift to correct volume
        ////Logger.Log("relative level progress: " + relativeLevelProgress, this, true);
        this.postprocessingScript.SetGoalProgress(relativeLevelProgress);
        //update light script to shift to correct light property interpolation
        this.lightScript.SetGoalProgress(relativeLevelProgress);
    }

    // Update is called once per frame
    void Update()
    {
        if(!active) return; // aint doing nothin while non active
        // Touch and Mouse inputs
        Vector2 pos = Vector2.zero;
        bool input = false;

        if (Input.touchCount == 1)
        {
            if (Input.GetTouch(0).phase == TouchPhase.Began)
            {
                if(EventSystem.current.IsPointerOverGameObject(Input.GetTouch(0).fingerId)){
                    ////Logger.Log("TOUCH OVER UI IGNORED", this, true);
                    return;
                }
                ////Logger.Log("TOUCH BEGAN", this, true);
                if(currentSelection.Equals('M')){
                    lastScreenPos = Input.GetTouch(0).position;
                } else {
                    input = true;
                    pos = GetIndecesOfClosestGameElement(Input.GetTouch(0).position);
                    lastPos = pos;
                }
            }
            else if (Input.GetTouch(0).phase == TouchPhase.Moved)
            {
                if(EventSystem.current.IsPointerOverGameObject(Input.GetTouch(0).fingerId)){
                    ////Logger.Log("TOUCH OVER UI IGNORED", this, true);
                    return;
                }
                ////Logger.Log("TOUCH MOVED", this, true);
                if (currentSelection.Equals('M')) {
                    Vector2 screenPos = Input.GetTouch(0).position;
                    Vector2 diff = lastScreenPos - screenPos;
                    lastScreenPos = screenPos;
                } else {
                    pos = GetIndecesOfClosestGameElement(Input.GetTouch(0).position);
                    if ((int)pos.x != (int)lastPos.x || (int)pos.y != (int)lastPos.y)
                    {
                        input = true;
                        lastPos = pos;
                    }
                }
            }
            
        }

#if UNITY_EDITOR
        if (Input.GetMouseButtonDown(0))
        {
            ////Logger.Log("CLICK ", this, true);
            if (!EventSystem.current.IsPointerOverGameObject())
            {
                if (currentSelection.Equals('M'))
                {
                    lastScreenPos = Input.mousePosition;
                }
                else
                {
                    input = true;
                    pos = GetIndecesOfClosestGameElement(Input.mousePosition);
                    lastPos = pos;
                }
            }
        }
        else if (Input.GetMouseButton(0))
        {
            if (!EventSystem.current.IsPointerOverGameObject())
            {
                if (currentSelection.Equals('M'))
                {
                    Vector2 screenPos = Input.mousePosition;
                    Vector2 diff = lastScreenPos - screenPos;
                    lastScreenPos = screenPos;
                }
                else
                {
                    pos = GetIndecesOfClosestGameElement(Input.mousePosition);
                    if ((int)pos.x != (int)lastPos.x || (int)pos.y != (int)lastPos.y)
                    {
                        input = true;
                        lastPos = pos;
                    }
                }
            }
        }
#endif
    
        if(input){
            tutorialController.Trigger("creator_changed_level");
            ////Logger.Log("INPUT: " + input, this, true);
            switch (currentSelection)
            {
                case 'H':
                    TriggerFish(pos);
                    break;
                case 'B':
                    TriggerSnowBalls(pos);
                    break;
                case 'D':
                    TriggerIceBalls(pos);
                    break;
                case 'P':
                    SetPlayerPos(pos);
                    break;
                case 'W':
                    ChangeGround(pos, new GameObject(), currentSelection);
                    break;
                case 'S':
                    ChangeGround(pos, snowIni, currentSelection);
                    break;
                case 'I':
                    ChangeGround(pos, iceIni, currentSelection);
                    break;
                case 'C':
                    ChangeGround(pos, crackIni, currentSelection);
                    break;
                case 'T':
                    ChangeGround(pos, treeIni, currentSelection);
                    break;
                case 'F':
                    ChangeGround(pos, fireIni, currentSelection);
                    break;
                case 'G':
                    ChangeGround(pos, goalIni, currentSelection);
                    break;
                default:
                    break;
            }   
        }
    }

    public bool ValidPos(Vector2 pos){
        return (pos.x >= 0 && pos.y >= 0);
    }

    public void ChangeGround(Vector2 pos, GameObject ground, char code){
        ////Logger.Log("CHANGING GROUND " + pos.x + " " + pos.y + " TO " + code, this, true);
        if (!ValidPos(pos)) return;
        if(playerX == (int) pos.x && playerY == (int) pos.y && "FTWGC".Contains(code)){
            // player is standing on location and it is illegal to stand on 'code'
            return;
        }
        if("FTWGC".Contains(code)){
            RemoveAnyBalls(pos);
        }
        if("FTWG".Contains(code)){
            RemoveFish(pos);
        }
        
        Destroy(grounds[(int)pos.x, (int)pos.y]);
        grounds[(int) pos.x, (int) pos.y] = Instantiate(ground, Util.ConvertToHexa(pos.x, pos.y, playerOffsetX, playerOffsetY), Quaternion.identity) as GameObject;
        // Adds etra snow under FIRE TREE AND GOAL to support
        if("FTG".Contains(code)){
            GameObject snowGround = Instantiate(snowIni, Util.ConvertToHexa(pos.x, pos.y, playerOffsetX, playerOffsetY), Quaternion.identity) as GameObject;
            snowGround.transform.SetParent(grounds[(int) pos.x, (int) pos.y].transform, true);
        }
        groundsCode[(int)pos.x, (int)pos.y] = code;
        MeasureLevelSizeAndSetCam();
        Save();
        ////Logger.Log("SAVED CHANGE", this, true);
    }

    public void SetPlayerPos(Vector2 pos){
        int diffX = (int)pos.x - playerX; 
        int diffY = (int)pos.y - playerY; 
        
        ////Logger.Log("CURRENT PLAYER POS " + playerX + " " + playerY, this, true);
        ////Logger.Log("TARGET PLAYER POS " + pos.x + " " + pos.y, this, true);
        ////Logger.Log("DIFF PLAYER POS " + diffX + " " + diffY, this, true);

        //do nothing if same coords
        if (diffX == 0 && diffY == 0) return;
        // out of bounds check
        if (!ValidPos(pos)) return;

        // make move legal
        RemoveAnyBalls(pos);
        PutSnowIfThereIs(pos, "FCTGW");
        RemoveFish(pos);
        
        // update player pos in model space position for creator
        playerX = (int)pos.x;
        playerY = (int)pos.y;

        // move real player object in gamemanager
        gameManager.player.transform.position = Util.ConvertToHexa(pos.x, pos.y, playerOffsetX, playerOffsetY);

        // update total player movement in gamemanager
        gameManager.totalPlayerModelMovementX += diffX;
        gameManager.totalPlayerModelMovementY += diffY;

        // move cam after player
        //cameraController.SetNewCamPos(gameManager.player.transform.position);
        Save();
    }

    public void TriggerSnowBalls(Vector2 pos){
        ////Logger.Log("CHANGING SNOWBALLS ON " + pos.x + " " + pos.y, this, true);
        if (!ValidPos(pos)) return;
        if(snowBallsCode[(int)pos.x, (int)pos.y]){
            RemoveAnyBalls(pos);
        } else {
            if (playerX == (int)pos.x && playerY == (int)pos.y)
            {
                // player is standing on location and it is illegal to put anything there
                return;
            }
            RemoveAnyBalls(pos);
            PutSnowIfThereIs(pos, "FCTGW");
            RemoveFish(pos);
            snowBalls[(int)pos.x, (int)pos.y] = Instantiate(snowBallIni, Util.ConvertToHexa(pos.x, pos.y, playerOffsetX, playerOffsetY), Quaternion.identity) as GameObject;
            snowBallsCode[(int)pos.x, (int)pos.y] = true;
        }
        Save();
    }

    public void TriggerIceBalls(Vector2 pos){
        ////Logger.Log("CHANGING ICEBALLS ON " + pos.x + " " + pos.y, this, true);
        if (!ValidPos(pos)) return;
        if (iceBallsCode[(int)pos.x, (int)pos.y]){
            RemoveAnyBalls(pos);
        } else {
            if (playerX == (int)pos.x && playerY == (int)pos.y)
            {
                // player is standing on location and it is illegal to put anything there
                return;
            }
            RemoveAnyBalls(pos);
            PutSnowIfThereIs(pos, "FCTGW");
            RemoveFish(pos);
            iceBalls[(int)pos.x, (int)pos.y] = Instantiate(iceBallIni, Util.ConvertToHexa(pos.x, pos.y, playerOffsetX, playerOffsetY), Quaternion.identity) as GameObject;
            iceBallsCode[(int)pos.x, (int)pos.y] = true;
        }
        Save();
    }

    public void TriggerFish(Vector2 pos){
        ////Logger.Log("CHANGING FISH ON " + pos.x + " " + pos.y, this, true);
        if (!ValidPos(pos)) return;
        Destroy(fishs[(int)pos.x, (int)pos.y]);
        if(fishCode[(int)pos.x, (int)pos.y]){
            fishCode[(int)pos.x, (int)pos.y] = false;
        } else {
            if (playerX == (int)pos.x && playerY == (int)pos.y)
            {
                // player is standing on location and it is illegal to put anything there
                return;
            }
            RemoveAnyBalls(pos);
            PutSnowIfThereIs(pos, "FTGW");
            fishs[(int)pos.x, (int)pos.y] = Instantiate(fishIni, Util.ConvertToHexa(pos.x, pos.y, playerOffsetX, playerOffsetY), Quaternion.identity) as GameObject;
            fishCode[(int)pos.x, (int)pos.y] = true;
        }
        Save();
    }

    public void PutSnowIfThereIs(Vector2 pos, string chars){
        if(chars.Contains(groundsCode[(int)pos.x, (int) pos.y])){
            ChangeGround(pos, snowIni, 'S');
        }
    }

    public void RemoveAnyBalls(Vector2 pos){
        Destroy(snowBalls[(int)pos.x, (int)pos.y]);
        snowBallsCode[(int)pos.x, (int)pos.y] = false;
        Destroy(iceBalls[(int)pos.x, (int)pos.y]);
        iceBallsCode[(int)pos.x, (int)pos.y] = false;
    }

    public void RemoveFish(Vector2 pos){
        Destroy(fishs[(int)pos.x, (int)pos.y]);
        fishCode[(int)pos.x, (int)pos.y] = false;
    }

    public Vector2 GetIndecesOfClosestGameElement(Vector2 screenPos){
        Vector3 targetPos = GetGameCoords(screenPos);
        
        ////Logger.Log("INPUT PLANE HIT ON "  + targetPos.ToString(), this, true);
        float bestDist = float.PositiveInfinity;
        int bestX = -1;
        int bestY = -1;
        if(targetPos == new Vector3()){
            ////Logger.Log("NOT A VALID INPUT PLANE HIT - sending -1 -1 pos", this, true);
            return new Vector2(bestX, bestY);
        }

        for (int x = 0; x < grounds.GetLength(0); x++)
        {
            for (int y = 0; y < grounds.GetLength(1); y++)
            {
                Vector3 pos = grounds[x,y].transform.position;
                float dist = (targetPos - pos).magnitude;
                ////Logger.Log("Dist for " + pos.ToString()+  " on index " + x + " " + y + " : " + dist, this, true);
                if (dist < bestDist)
                {
                    bestDist = dist;
                    bestX = x;
                    bestY = y;
                    ////Logger.Log("new best dist on " + bestX + " " + bestY, this, true);
                }
            }
        }
        ////Logger.Log("Closest GameObject on: " + bestX + " " + bestY, this, true);
        return new Vector2(bestX, bestY);
    }

    public Vector3 GetGameCoords(Vector2 screenPos)
    {
        RaycastHit hit;
        Ray ray = cameraRef.ScreenPointToRay(screenPos);
        if (Physics.Raycast(ray, out hit, Mathf.Infinity, layerMask))
        {
            ////Logger.Log("RAYCAST HIT " + hit.transform.name + " ON " + hit.point);
            if(hit.transform.name.Equals("InputPlane")){
                return hit.point;
            }
        }
        return new Vector3();
    }

    public string BuildLevelString(){
        string levelString = ";";
        FindLevelBorders();
        for (int y = endY; y >= startY; y--){
            for (int x = startX; x <= endX; x++)
            {
                levelString += groundsCode[x, y];
            }
            levelString += ";";
        }
        levelString += "B" + ";";
        for (int x = startX; x <= endX; x++)
        {
            for (int y = startY; y <= endY; y++)
            {
                if (snowBallsCode[x, y]) levelString += (x-startX) + "-" + (y-startY) + ";";
            }
        }
        levelString += "D" + ";";
        for (int x = startX; x <= endX; x++)
        {
            for (int y = startY; y <= endY; y++)
            {
                if (iceBallsCode[x, y]) levelString += (x - startX) + "-" + (y - startY) + ";";
            }
        }
        levelString += "F" + ";";
        for (int x = startX; x <= endX; x++)
        {
            for (int y = startY; y <= endY; y++)
            {
                if (fishCode[x, y]) levelString += (x - startX) + "-" + (y-startY) + ";";
            }
        }
        levelString += "P" + ";";
        levelString += ( playerX - startX) + "-";
        levelString += ( playerY - startY) + ";";
        return levelString;
    }

    public void FindLevelBorders(){
        startX = 0;
        endX = maxLvlSize-1;
        startY = 0;
        endY = maxLvlSize-1;
        for (int x = 0; x < grounds.GetLength(0); x++)
        {
            if(!isAllWaterForSetX(x)){
                startX = x;
                break;
            }
        }
        for (int x = grounds.GetLength(0) -1; x >= 0; x--)
        {
            if (!isAllWaterForSetX(x))
            {
                endX = x;
                break;
            }
        }
        for (int y = 0; y < grounds.GetLength(1); y++)
        {
            if (!isAllWaterForSetY(y))
            {
                startY = y;
                break;
            }
        }
        for (int y = grounds.GetLength(1) - 1; y >= 0; y--)
        {
            if (!isAllWaterForSetY(y))
            {
                endY = y;
                break;
            }
        }
        ////Logger.Log("Determined level boorders: X: " + startX + "-" + endX + " Y: " + startY + "-" + endY);
 
        
    }

    public bool isAllWaterForSetX(int x)
    {
        for (int y = 0; y < grounds.GetLength(1); y++)
        {
            if (!groundsCode[x,y].Equals('W'))
            {
                return false;
            }
        }
        return true;
    }

    public bool isAllWaterForSetY(int y)
    {
        for (int x = 0; x < grounds.GetLength(0); x++)
        {
            if (!groundsCode[x, y].Equals('W'))
            {
                return false;
            }
        }
        return true;
    }

    public void MeasureLevelSizeAndSetCam(){
        float OldMaxWorldX = maxWorldX;
        float OldMinWorldX = minWorldX;
        float OldMaxWorldY = maxWorldY;
        float OldMinWorldY = minWorldY;

        maxWorldX = float.NegativeInfinity;
        minWorldX = float.PositiveInfinity;
        maxWorldY = float.NegativeInfinity;
        minWorldY = float.PositiveInfinity;
        
        Vector3 pos = new Vector3();
        for (int x = 0; x < grounds.GetLength(0); x++)
        {
            for (int y = 0; y < grounds.GetLength(1); y++)
            {
                if(groundsCode[x, y] != 'W'){
                    
                    pos = grounds[x, y].transform.position;
                    if (pos.x > maxWorldX)
                    {
                        maxWorldX = pos.x;
                    }
                    if (pos.x < minWorldX)
                    {
                        minWorldX = pos.x;
                    }
                    if (pos.z > maxWorldY)
                    {
                        maxWorldY = pos.z;
                    }
                    if (pos.z < minWorldY)
                    {
                        minWorldY = pos.z;
                    }
                }
            }
        }
        
        ////Logger.Log("Calculated new level size", this, true);

        // if level dims have changed -> adjust camera
        if(OldMaxWorldX != maxWorldX 
            || OldMinWorldX != minWorldX
            || OldMaxWorldY != maxWorldY
            || OldMinWorldY != minWorldY){
            ////Logger.Log("Changing Camera because level size changed", this, true);
            this.cameraController.SetLevelMidPointAndAdjustToLevelSize(
                ClampForMaxSize(gameManager.CalculateLevelMidAndSizeWithGivenKoords(
                    maxWorldX+bonusZoomOutForCamX, 
                    minWorldX-bonusZoomOutForCamX,
                    maxWorldY+bonusZoomOutForCamY,
                    minWorldY-bonusZoomOutForCamY,
                    0.1f)));
            this.cameraController.SetNewCamCreatorPos(gameManager.player.transform.position, currCamZoom);
        }
    }

    public Vector4 ClampForMaxSize(Vector4 input){
        if(input.w > 0){
            currCamZoom = Math.Min(input.w, maxSizeForCam) / maxSizeForCam;
            return new Vector4(input.x, input.y, input.z, Math.Min(input.w, maxSizeForCam));
        } else {
            currCamZoom = Math.Max(input.w, -maxSizeForCam) / -maxSizeForCam;
            return new Vector4(input.x, input.y, input.z, Math.Max(input.w, -maxSizeForCam));
        }
    }

}