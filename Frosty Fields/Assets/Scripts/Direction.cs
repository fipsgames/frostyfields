using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using util;

namespace direction
{
    public class Direction : gameState.PostitionalObject
    {
        public const int UP = 0;
        public const int UPRIGHT = 1;
        public const int DOWNRIGHT = 2;
        public const int DOWN = 3;
        public const int DOWNLEFT = 4;
        public const int UPLEFT = 5;

        public string name { get; set; }
        public Vector3 vector { get; set; }

        public Direction(int directionId) : base(0, 0, directionId)
        {
            switch (this.id)
            {
                case UP:
                    x = 0;
                    y = 1;
                    name = "Up";
                    break;
                case DOWN:
                    x = 0;
                    y = -1;
                    name = "Down";
                    break;
                case UPRIGHT:
                    x = 1;
                    y = 0;
                    name = "Up Right";
                    break;
                case DOWNLEFT:
                    x = -1;
                    y = 0;
                    name = "Down Left";
                    break;
                case UPLEFT:
                    x = -1;
                    y = 1;
                    name = "Up Left";
                    break;
                case DOWNRIGHT:
                    x = 1;
                    y = -1;
                    name = "Down Right";
                    break;
            }
            vector = Util.ConvertToHexa(x, y);
            ////Logger.Log("Created Direction " + name + " " + vector);
        }
    }
}