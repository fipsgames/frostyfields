namespace action{
    
    public class Action
    {
        // storing action names in static array
        public static string[] name =  {"MOVE", 
                                        "SLITTER",
                                        "MOVE_TRIGGER",
                                        "SLITTER_TRIGGER",
                                        "ROLL_TRIGGER",
                                        "BUMP_TRIGGER",
                                        "FALL_TRIGGER",
                                        "BURN_TRIGGER",
                                        "WIN_TRIGGER",
                                        "BREAK_TRIGGER",
                                        "TO_SNOW_TRIGGER",
                                        "TO_ICE_TRIGGER",
                                        "TO_WATER_TRIGGER",
                                        "BOUNCE_TRIGGER",
                                        "ATTACHTOPLAYER_TRIGGER",
                                        "GETCOLLECTED_TRIGGER",
                                        "IDLE_TRIGGER",
                                        "SWITCH_TRIGGER",
                                        "RESTART_TRIGGER"};
        // CONTINOUS ACTIONS (THAT MOVE A GAME OBJET TO ANOTHER COORDINATE)
        public const int MOVE = 0;
        public const int SLITTER = 1;
        // TRIGGER ANIMATOR STATES OR GAMESTATE CHANGES (ONE TIME EVENTS)
        public const int MOVE_TRIGGER = 2;
        public const int SLITTER_TRIGGER = 3;
        
        public const int ROLL_TRIGGER = 4;
        
        public const int BUMP_TRIGGER = 5;
        public const int FALL_TRIGGER = 6;
        public const int BURN_TRIGGER = 7;
        public const int WIN_TRIGGER = 8;
        public const int BREAK_TRIGGER = 9;
        public const int TO_SNOW_TRIGGER = 10;
        public const int TO_ICE_TRIGGER = 11;
        public const int TO_WATER_TRIGGER = 12;
        public const int BOUNCE_TRIGGER = 13;
        public const int ATTACHTOPLAYER_TRIGGER = 14;
        public const int GETCOLLECTED_TRIGGER = 15;
        public const int IDLE_TRIGGER = 16;
        public const int SWITCH_TRIGGER = 17;
        public const int RESTART_TRIGGER = 18;

        public int t { get; set; }
        public float x { get; set; }
        public float y { get; set; }
        public int id { get; set; }

        public Action(int id, int t)
        {
            this.id = id;
            this.t = t;
            //Logger.Log("ACTION " + this.Name() + " was created; time: " + this.t);
        }

        public Action(int id, int t, float x, float y)
        {
            this.id = id;
            this.t = t;
            this.x = x;
            this.y = y;
            //Logger.Log("ACTION " + this.Name() + " was created; time: " + this.t);
        }

        public bool Matches(int id, int t){
            return (this.id == id && this.t == t);
        }

        public string Name()
        {
            return Action.name[this.id];
        }
    }
}


