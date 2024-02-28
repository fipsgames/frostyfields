using System.Collections.Generic;
using UnityEngine;
using System;
using System.Linq;
using direction;
using util;

namespace gameState
{

    public class GameState
    {
        public Ground[,] grounds { get; set; }
        public Player player { get; set; }
        public MovableObject[] balls { get; set; }
        public DieableObject[] fish { get; set; }
        public int requiredFish { get; set; }
        public int currentFish { get; set; }

        public int depth { get; set; }
        Stack<MovableObject> ballsBeeingPushed { get; set; }

        public GameState pastGameState { get; set;}
        public int n_pastGameStates {get; set;}
        public int max_pastGameStates = 20;

        public GameState(String levelString)
        {
            this.initializeFromString(levelString);
        }

        public GameState(GameState gameState)
        {
            // Clones full gamestate, so i past state is accessable when og has been altered
            // set own past gamestate null, 
            // dont want to keep track of this until a game is rendered based on this gamestate

            // also keeps a reference to the state before the past state
            
            // to enforce a limit on past gamestate memorization one would need to resursivly search the 
            // gamestate whish has no past gamestate aka the oldest on and then delete it from the next oldest one
            if(gameState.n_pastGameStates >= max_pastGameStates){
                //Logger.Log("FORGETTING OLDEST GAMESTATE NOW : " + gameState.n_pastGameStates, null, true);
                gameState.forgetVeryLastGamestate();
            }

            n_pastGameStates = gameState.n_pastGameStates;
            gameState.n_pastGameStates += 1;
            
            pastGameState = gameState.pastGameState;

            

            player = gameState.player.Clone();
            grounds = new Ground[gameState.grounds.GetLength(0), gameState.grounds.GetLength(1)];
            for (int x = 0; x < gameState.grounds.GetLength(0); x++)
            {
                for (int y = 0; y < gameState.grounds.GetLength(1); y++)
                {
                    if (gameState.grounds[x,y] == null)
                    {
                        grounds[x,y] = null; 
                    } 
                    else 
                    {
                        grounds[x,y] = gameState.grounds[x,y].Clone();
                    }
                }
            }
            balls = new MovableObject[gameState.balls.GetLength(0)];
            for (int x = 0; x < gameState.balls.GetLength(0); x++)
            {
                if (gameState.balls[x] == null)
                {
                    balls[x] = null; 
                } 
                else 
                {
                    balls[x] = gameState.balls[x].Clone();
                }
            }
            fish = new DieableObject[gameState.fish.GetLength(0)];
            for (int x = 0; x < gameState.fish.GetLength(0); x++)
            {
                if (gameState.fish[x] == null)
                {
                    fish[x] = null; 
                } 
                else 
                {
                    fish[x] = gameState.fish[x].Clone();
                }
            }
            requiredFish = gameState.requiredFish;
            currentFish = gameState.currentFish;
            //Logger.Log("DEEP CLONED GAMESTATE! ", null, true);
        }

        public void forgetVeryLastGamestate(){
            n_pastGameStates -= 1;
            if(n_pastGameStates == 0) pastGameState = null;
            else pastGameState.forgetVeryLastGamestate();
        }

        public void initializeFromString(String levelString)
        {
            pastGameState = null;
            n_pastGameStates = 0;
            int maxX = 0;
            int maxY = 0;
            int numberOfSnowBalls = 0;
            int numberOfIceBalls = 0;
            requiredFish = 0;

            ////Logger.Log( "NEW GAMESTATE Full level String " + levelString, null, true);
            string[] splitString = levelString.Split(';');

            ////Logger.Log( "length of string array: " + splitString.Length);
            for (int i = 0; i < splitString.Length; i++)
            {
                splitString[i] = splitString[i].Trim();
            }
            ////Logger.Log( "length of string array: " + splitString.Length);

            //Getting Dimensions:
            ////Logger.Log( "" + splitString[maxY + 1].First());
            while (!splitString[maxY + 1].First().Equals('B')) { maxY++; }
            maxX = Util.RemoveWhitespace(splitString[1]).Length;
            string[] levelParts = Util.RangeSubset(splitString, 1, maxY);
            ////Logger.Log( "LEVEL SIZES: " + maxX + " " + maxY);

            while (!splitString[maxY + numberOfSnowBalls + 2].First().Equals('D')) { numberOfSnowBalls++; }
            string[] snowBallParts = Util.RangeSubset(splitString, maxY + 2, numberOfSnowBalls);
            ////Logger.Log( "NUMBER OF SNOW BALLS: " + numberOfSnowBalls);

            while (!splitString[maxY + numberOfSnowBalls + numberOfIceBalls + 3].First().Equals('F')) { numberOfIceBalls++; }
            string[] iceBallParts = Util.RangeSubset(splitString, maxY + numberOfSnowBalls + 3, numberOfIceBalls);
            ////Logger.Log( "NUMBER OF ICE BALLS: " + numberOfIceBalls);

            while (!splitString[maxY + numberOfSnowBalls + numberOfIceBalls + requiredFish + 4].First().Equals('P')) { requiredFish++; }
            string[] fishParts = Util.RangeSubset(splitString, maxY + +numberOfSnowBalls + numberOfIceBalls + 4, requiredFish);
            ////Logger.Log( "NUMBER OF FISH: " + requiredFish);

            string playerPart = splitString[maxY + numberOfSnowBalls + numberOfIceBalls + requiredFish + 5];

            //calculate nessacary offset size for water extension fields for balls
            int offSet = numberOfSnowBalls + numberOfIceBalls + 1;
            int worldmaxX = maxX + 2 * offSet;
            int worldmaxY = maxY + 2 * offSet;
            grounds = new Ground[worldmaxX, worldmaxY];

            //Initialize all grounds with water
            for (int x = 0; x < worldmaxX; x++)
            {
                for (int y = 0; y < worldmaxY; y++)
                {
                    grounds[x, y] = new Ground(Ground.WATER);
                    ////Logger.Log( "GROUND: " + grounds[x, y].name + " --- " + x + " " + y);
                }
            }

            //Over write with correct grounds in inner level region
            for (int y = 0; y < maxY; y++)
            {
                levelParts[y] = Util.RemoveWhitespace(levelParts[y]);
                for (int x = 0; x < maxX; x++)
                {
                    char groundType = levelParts[y].ElementAt(x);
                    // MaxY - Y to make level files have correct up down representation
                    grounds[x + offSet, (maxY - y - 1) + offSet] = new Ground(Ground.GetIDFromChar(groundType));
                    ////Logger.Log( "GROUND: " + grounds[x, y].name + " --- " + x + " " + y);
                }
            }

            ////Logger.Log( "#balls: " + (numberOfSnowBalls + numberOfIceBalls));
            balls = new MovableObject[numberOfSnowBalls + numberOfIceBalls];
            for (int i = 0; i < numberOfSnowBalls; i++)
            {
                int ballX = Int32.Parse(snowBallParts[i].Split('-').First());
                int ballY = Int32.Parse(snowBallParts[i].Split('-').Last());
                balls[i] = new MovableObject(ballX + offSet, ballY + offSet, Ground.SNOW);
                ////Logger.Log( "SNOW BALL: " + balls[i].x + " " + balls[i].y);
            }
            for (int i = 0; i < numberOfIceBalls; i++)
            {
                int ballX = Int32.Parse(iceBallParts[i].Split('-').First());
                int ballY = Int32.Parse(iceBallParts[i].Split('-').Last());
                balls[numberOfSnowBalls + i] = new MovableObject(ballX + offSet, ballY + offSet, Ground.ICE);
                ////Logger.Log( "ICE BALL: " + balls[numberOfSnowBalls + i].x + " " + balls[numberOfSnowBalls + i].y);
            }

            ////Logger.Log( "#fish: " + requiredFish);
            fish = new DieableObject[requiredFish];
            for (int i = 0; i < requiredFish; i++)
            {
                int fishX = Int32.Parse(fishParts[i].Split('-').First());
                int fishY = Int32.Parse(fishParts[i].Split('-').Last());
                fish[i] = new DieableObject(fishX + offSet, fishY + offSet, 0);
                ////Logger.Log( "FISH: " + fish[i].x + " " + fish[i].y);
            }

            ////Logger.Log( "PLAYER STRING: " + playerPart);
            int playerX = Int32.Parse(playerPart.Split('-').First());
            int playerY = Int32.Parse(playerPart.Split('-').Last());
            player = new Player(playerX + offSet, playerY + offSet, 0);
            ////Logger.Log( "PLAYER: " + player.x + " " + player.y);

            //Logger.Log( "initialized Level, grounds dim:" + grounds.GetLength(0) + " " + grounds.GetLength(1));
        }

        public void InitializeValuesForInitialAppyStep()
        {
            this.pastGameState = new GameState(this);
            this.depth = 0;
            this.player.moving = true;
            this.ballsBeeingPushed = new Stack<MovableObject>();
        }

        // returns if another step needs to be applied
        public bool ApplyStep(Direction direction)
        {

            if (this.depth > grounds.GetLength(0) + grounds.GetLength(1))
            {
                //Logger.Log( "TO MUCH RECURSION! SOMETHING IS WRONG!", null, true);
                return false;
            }

            int sumBalls = 0;
            int sumTriggeredBrokenIce = 0;
            if (!player.moving && ballsBeeingPushed.Count == 0)
            {
                foreach (MovableObject ball in balls)
                {
                    if(ball.moving){
                        sumBalls += 1;
                    }
                }
                foreach (Ground ground in grounds)
                {
                    if (ground.isTriggered)
                    {
                        sumTriggeredBrokenIce += 1;
                    }
                }
                if (sumBalls == 0 && sumTriggeredBrokenIce == 0)
                {
                    //NOTHING MORE TO DO
                    //EXIT CONDITION
                    ////Logger.Log( "End Condition reached", null, true);
                    return false;
                }
            }
            ////Logger.Log( "Entering next apply Step. Depth: " + this.depth + ", PlayerMoves: " + player.moving + ", PushedBalls: " + ballsBeeingPushed.Count + ", FreeBalls: " + sumBalls + ", TrigBrokenIce: " + sumTriggeredBrokenIce, null, true);
            ////Logger.Log( "ABOUT TO EXECUTE APPLY STEP. PLAYER IS AT " + player.x + " " + player.y + ". GROUND UNDER PLAYER IS " + grounds[player.x,player.y].name, null, true);

            this.depth += 1;
            CheckMovements(direction);
            DoMovements(direction);
            BrokenIceInteractions();
            PlayerInteractions();
            BallInteractions();
            return true;
        }

        public void CheckMovements(Direction direction)
        {
            if (player.moving)
            {
                ExtendPushedBalls(direction);
                CheckPhysicalBlockagePlayerAndPushedBalls(direction);
            }

            CheckFreeBallsMovement(direction);

        }

        public void DoMovements(Direction direction)
        {
            MovePlayer(direction);
            MoveBalls(direction);
        }

        public void PlayerInteractions()
        {
            if (player.alive)
            {
                CheckPlayerDie();
                CheckPlayerWin();
                CheckFish();
                CheckPlayerBrokenIceTrigger();
                CheckPlayerStops();
            }

        }

        public void BallInteractions()
        {
            CheckBallsDie();
            CheckBallsBrokenIceTrigger();
        }


        public void TransferPushedToFreelyMovingBalls()
        {
            ////Logger.Log( "Transfering balls " + ballsBeeingPushed.Count());
            foreach (MovableObject ball in ballsBeeingPushed)
            {
                ball.moving = true;
            }
            ballsBeeingPushed.Clear();
        }

        public void ExtendPushedBalls(Direction direction)
        {
            MovableObject ball = FindBallByXY(player.x + (ballsBeeingPushed.Count + 1) * direction.x, player.y + (ballsBeeingPushed.Count + 1) * direction.y);
            if (ball != null)
            {
                ////Logger.Log( "adding ball to pushed balls");
                ballsBeeingPushed.Push(ball);
                ExtendPushedBalls(direction);
            }
        }

        public bool CanFreeBallMove(Direction direction, MovableObject ball)
        {
            if (!grounds[ball.x, ball.y].isSticky)
            {
                MovableObject nextBall = FindBallByXY(ball.x + direction.x, ball.y + direction.y);
                if (nextBall != null)
                {
                    nextBall.moving = CanFreeBallMove(direction, nextBall);
                    return nextBall.moving; //depends on the nextball
                }
                else
                {
                    if (XYCoordinatesInLevelBounds(ball.x + direction.x, ball.y + direction.y))
                    {
                        if (grounds[ball.x + direction.x, ball.y + direction.y].blocksBalls)
                        {
                            return false; //no nextBall but path is blocked by something
                        }
                        else
                        {
                            return true; //no nextBall but path is clear
                        }
                    }
                    else
                    {
                        return false; //no ball but out of level bounds
                    }
                }
            }
            else
            {
                return false; //the ground is stick so ball cant move
            }
        }

        public void CheckFreeBallsMovement(Direction direction)
        {
            foreach (MovableObject ball in balls)
            {
                if (ball.moving && ball.alive)
                {
                    ball.moving = CanFreeBallMove(direction, ball);
                }
            }
        }

        public void CheckPhysicalBlockagePlayerAndPushedBalls(Direction direction)
        {
            if (ballsBeeingPushed.Count == 0)
            {
                if (XYCoordinatesInLevelBounds(player.x + direction.x, player.y + direction.y))
                {
                    if (!grounds[player.x + direction.x, player.y + direction.y].blocksPlayer)
                    {
                        if (grounds[player.x + direction.x, player.y + direction.y].isGoal)
                        {
                            if (!AreAllFishCollected())
                            {
                                player.moving = false;
                            }
                        }
                    }
                    else
                    {
                        player.moving = false;
                    }
                }
                else
                {
                    player.moving = false;
                }
            }
            else
            {
                if (XYCoordinatesInLevelBounds(player.x + (ballsBeeingPushed.Count + 1) * direction.x, player.y + (ballsBeeingPushed.Count + 1) * direction.y))
                {
                    if (grounds[player.x + (ballsBeeingPushed.Count + 1) * direction.x, player.y + (ballsBeeingPushed.Count + 1) * direction.y].blocksBalls)
                    {
                        ballsBeeingPushed.Clear();
                        player.moving = false;
                    }
                }
                else
                {
                    ballsBeeingPushed.Clear();
                    player.moving = false;
                }
            }
        }


        public void MovePlayer(Direction direction)
        {
            if (player.moving)
            {
                player.Move(direction);
                ////Logger.Log( "MOVED PLAYER TO " + player.x + " " + player.y + ". GROUND UNDER PLAYER IS " + grounds[player.x, player.y].name, null, true);
            }
        }

        public void MoveBalls(Direction direction)
        {
            foreach (MovableObject ball in ballsBeeingPushed)
            {
                ball.Move(direction);
                ////Logger.Log( "pushed Ball on " + ball.x + " " + ball.y);
            }

            foreach (MovableObject ball in balls)
            {
                if (ball.moving)
                {
                    ball.Move(direction);
                    ////Logger.Log( "freely moving ball moved on " + ball.x + " " + ball.y);
                }
            }

        }

        public void CheckBrokenIceTrigger(MovableObject movObj)
        {
            if (grounds[movObj.x, movObj.y].canbreak)
            {
                ////Logger.Log( "TRIGGERING BROKEN ICE", null, true);
                grounds[movObj.x, movObj.y].Trigger();
            }
        }

        public void CheckPlayerBrokenIceTrigger()
        {
            CheckBrokenIceTrigger(player);
        }

        public void CheckBallsBrokenIceTrigger()
        {
            foreach(MovableObject ball in balls){
                CheckBrokenIceTrigger(ball);
            }
        }

        public void BrokenIceInteractions()
        {
            for (int x = 0; x < grounds.GetLength(0); x++)
            {
                for (int y = 0; y < grounds.GetLength(1); y++)
                {
                    if (grounds[x, y].isTriggered)
                    {
                        ////Logger.Log( "Turning ground into water now");
                        grounds[x, y].ChangeTo(Ground.WATER);
                    }
                }
            }

        }

        public void CheckPlayerDie()
        {
            ////Logger.Log( "Looking for player death on " + player.x + " " + player.y + ". GROUND UNDER PLAYER IS " + grounds[player.x, player.y].name + " and this ground kills: " + grounds[player.x, player.y].kills, null, true);
            if (grounds[player.x, player.y].kills)
            {
                ////Logger.Log( "Killing player in gamestate", null, true);
                player.Kill();
            }
        }

        public void CheckBallsDie()
        {
            List<MovableObject> tmpList = new List<MovableObject>();
            foreach (MovableObject ball in ballsBeeingPushed)
            {
                if (grounds[ball.x, ball.y].kills)
                {
                    ////Logger.Log( "killing pushed ball " + ball.x + " " + ball.y + " and setting ground to snow");
                    ball.Kill();
                    tmpList.Add(ball);
                    grounds[ball.x, ball.y].ReactToBall(ball.id);
                    ////Logger.Log( "ground on killed ball is now: " + grounds[ball.x, ball.y].name);
                }
            }
            ////Logger.Log( "Number of killed pushed balls in this step: " + tmpList.Count);
            foreach (MovableObject ball in tmpList)
            {
                while (ballsBeeingPushed.Peek() != ball)
                {
                    //removing ball in front of killed ball from stack and setting it as freely moving ball
                    ballsBeeingPushed.Peek().moving = true;
                    ballsBeeingPushed.Pop();
                }
                //removing ball itself from queue
                ballsBeeingPushed.Pop();
            }

            foreach (MovableObject ball in balls)
            {
                if (ball.alive && grounds[ball.x, ball.y].kills)
                {
                    ////Logger.Log( "killing free ball " + ball.x + " " + ball.y + " and setting ground to snow");
                    ball.Kill();
                    grounds[ball.x, ball.y].ReactToBall(ball.id);
                    ////Logger.Log( "ground on killed ball is now: " + grounds[ball.x, ball.y].name);
                }
            }

            ////Logger.Log( "Finished checking for dead balls! length of ballsbeeingpushed: " + ballsBeeingPushed.Count);

        }

        public void CheckPlayerWin()
        {
            if (grounds[player.x, player.y].isGoal)
            {
                player.won = true;
            }
        }

        public void CheckFish()
        {
            DieableObject fish = FindFishByXY(player.x, player.y);
            if (fish != null)
            {
                if (fish.alive)
                {
                    fish.Kill();
                    currentFish += 1;
                }
            }
        }

        public void CheckPlayerStops()
        {
            if (player.alive && player.moving && !grounds[player.x, player.y].isSticky)
            {
                return;
            }
            else
            {
                player.moving = false;
                TransferPushedToFreelyMovingBalls();
            }
        }

        //only return balls alive
        public MovableObject FindBallByXY(int x, int y)
        {
            ////Logger.Log( "search for balls on " + x + " "+y);
            foreach (MovableObject ball in balls)
            {
                ////Logger.Log( "ccchecking balllll " + ball.x + " "+ ball.y);
                if (ball.x == x && ball.y == y && ball.alive)
                {
                    //////Logger.Log( "found ball");
                    return ball;
                }
            }
            return null;
        }

        //only return fish alive
        public DieableObject FindFishByXY(int x, int y)
        {
            foreach (DieableObject fish_inst in fish)
            {
                if (fish_inst.x == x && fish_inst.y == y && fish_inst.alive)
                {
                    return fish_inst;
                }
            }
            return null;
        }


        // test wether coordinates are inside defined current Level
        public bool XYCoordinatesInLevelBounds(int x, int y)
        {
            if (x >= 0 && x < grounds.GetLength(0) && y >= 0 && y < grounds.GetLength(1))
            {
                ////Logger.Log( "BOUNDS CHECK SUCCESS: " + x + " " + y + " " + grounds.GetLength(0) + " " + grounds.GetLength(1));
                return true;
            }
            else
                ////Logger.Log( "BOUNDS CHECK FAILURE: " + x + " " + y + " " + grounds.GetLength(0) + " " + grounds.GetLength(1));
                return false;
        }

        public int GetGroundUnderPlayer(){
            return grounds[player.x, player.y].id;
        }

        public int GetGroundUnderXY(int x, int y)
        {
            return grounds[x, y].id;
        }

        public bool AreAllFishCollected(){
            return (requiredFish == currentFish);
        }
    }

    public class IdObject
    {
        public int id { get; set; }

        public IdObject(int id)
        {
            this.id = id;
        }
    }

    public class PostitionalObject : IdObject
    {
        public int x { get; set; }
        public int y { get; set; }

        public PostitionalObject(int x, int y, int id) : base(id)
        {
            this.x = x;
            this.y = y;
        }
    }

    public class DieableObject : PostitionalObject
    {
        public bool alive { get; set; }

        public DieableObject(int x, int y, int id) : base(x, y, id)
        {
            this.alive = true;
        }

        public void Kill()
        {
            this.alive = false;
        }

        public DieableObject Clone()
        {
            DieableObject copy = new DieableObject(this.x, this.y, this.id);
            copy.alive = this.alive;
            return copy;
        }
    }

    public class MovableObject : DieableObject
    {
        public bool moving { get; set; }
        public MovableObject(int x, int y, int id) : base(x, y, id)
        {
            this.moving = false;
        }

        public void Move(Direction direction)
        {
            this.x += direction.x;
            this.y += direction.y;
        }

        public new void Kill()
        {
            base.Kill();
            this.moving = false;
        }

        public new MovableObject Clone()
        {
            MovableObject copy = new MovableObject(this.x, this.y, this.id);
            copy.alive = this.alive;
            copy.moving = this.moving;
            return copy;
        }
    }

    public class Player : MovableObject
    {
        public bool won { get; set; }
        public int initialX { get; set; }
        public int initialY { get; set; }
        public Player(int x, int y, int id) : base(x, y, id)
        {
            this.won = false;
            this.initialX = x;
            this.initialY = y;
        }

        public new void Move(Direction direction)
        {
            this.x += direction.x;
            this.y += direction.y;
        }

        public new void Kill()
        {
            base.Kill();
        }

        public void Win()
        {
            this.won = true;
        }

        public new Player Clone()
        {
            Player copy = new Player(this.x, this.y, this.id);
            copy.alive = this.alive;
            copy.moving = this.moving;
            copy.won = this.won;
            return copy;
        }
    }

    public class Ground : IdObject
    {

        public const int GOAL = 0;
        public const int SNOW = 1;
        public const int ICE = 2;
        public const int CRACK = 3;
        public const int WATER = 4;
        public const int TREE = 5;
        public const int FIRE = 6;

        // dont really need name here, its just for debugging
        public string name { get; set; }
        public bool isSticky { get; set; }
        public bool blocksPlayer { get; set; }
        public bool blocksBalls { get; set; }
        public bool isGoal { get; set; }
        public bool canbreak { get; set; }
        public bool kills { get; set; }
        public bool isTriggered { get; set; }

        public Ground(int groundId) : base(groundId)
        {
            this.ChangeTo(groundId);
        }

        public void ChangeTo(int groundId)
        {
            this.id = groundId;
            this.isTriggered = false;
            switch (this.id)
            {
                case GOAL:
                    this.name = "Goal";
                    this.isSticky = true;
                    this.blocksPlayer = false;
                    this.blocksBalls = true;
                    this.isGoal = true;
                    this.canbreak = false;
                    this.kills = false;
                    break;
                case SNOW:
                    this.name = "Snow";
                    this.isSticky = true;
                    this.blocksPlayer = false;
                    this.blocksBalls = false;
                    this.isGoal = false;
                    this.canbreak = false;
                    this.kills = false;
                    break;
                case ICE:
                    this.name = "Ice";
                    this.isSticky = false;
                    this.blocksPlayer = false;
                    this.blocksBalls = false;
                    this.isGoal = false;
                    this.canbreak = false;
                    this.kills = false;
                    break;
                case CRACK:
                    this.name = "Cracky Ice";
                    this.isSticky = false;
                    this.blocksPlayer = false;
                    this.blocksBalls = false;
                    this.isGoal = false;
                    this.canbreak = true;
                    this.kills = false;
                    break;
                case WATER:
                    this.name = "Water";
                    this.isSticky = true;
                    this.blocksPlayer = false;
                    this.blocksBalls = false;
                    this.isGoal = false;
                    this.canbreak = false;
                    this.kills = true;
                    break;
                case TREE:
                    this.name = "Tree";
                    this.isSticky = true;
                    this.blocksPlayer = true;
                    this.blocksBalls = true;
                    this.isGoal = false;
                    this.canbreak = false;
                    this.kills = false;
                    break;
                case FIRE:
                    this.name = "Fire";
                    this.isSticky = true;
                    this.blocksPlayer = false;
                    this.blocksBalls = false;
                    this.isGoal = false;
                    this.canbreak = false;
                    this.kills = true;
                    break;
            }
        }

        public void ReactToBall(int ballId){
            if (this.id == Ground.WATER) {
                this.ChangeTo(ballId);
            }
        }

        public void Trigger()
        {
            this.isTriggered = true;
        }

        public static int GetIDFromChar(char groundChar){
            int id = 0;
            switch (groundChar)
            {
                case 'G':
                    id = Ground.GOAL;
                    break;
                case 'S':
                    id = Ground.SNOW;
                    break;
                case 'I':
                    id = Ground.ICE;
                    break;
                case 'C':
                    id = Ground.CRACK;
                    break;
                case 'W':
                    id = Ground.WATER;
                    break;
                case 'T':
                    id = Ground.TREE;
                    break;
                case 'F':
                    id = Ground.FIRE;
                    break;
            }
            return id;
        }

        public Ground Clone()
        {
            Ground copy = new Ground(this.id);
            copy.isTriggered = this.isTriggered;
            return copy;
        }
    }
}