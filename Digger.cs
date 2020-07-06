using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Digger
{
    public class Terrain : ICreature
    {
        public CreatureCommand Act(int x, int y)
        {
            return new CreatureCommand();
        }

        public bool DeadInConflict(ICreature conflictedObject)
        {
            return conflictedObject.GetImageFileName() == "Digger.png";
        }

        public int GetDrawingPriority()
        {
            return 5;
        }

        public string GetImageFileName()
        {
           return "Terrain.png";
        }
    }

    public class Player : ICreature
    {
        public CreatureCommand Act(int x, int y)
        {
            switch (Game.KeyPressed)
            {
                case Keys.Up:
                    if (y - 1 >= 0 && (Game.Map[x, y - 1] == null 
                        || Game.Map[x, y - 1].GetImageFileName() != "Sack.png"))
                        return new CreatureCommand() { DeltaX = 0, DeltaY = -1 };
                    break;
                case Keys.Left:
                    if (x - 1 >= 0 && (Game.Map[x - 1, y] == null
                        || Game.Map[x - 1, y].GetImageFileName() != "Sack.png"))
                        return new CreatureCommand() { DeltaX = -1, DeltaY = 0 };
                    break;
                case Keys.Down:
                    if (y + 1 < Game.MapHeight && (Game.Map[x, y + 1] == null
                        || Game.Map[x, y + 1].GetImageFileName() != "Sack.png"))
                        return new CreatureCommand() { DeltaX = 0, DeltaY = 1 };
                    break;
                case Keys.Right:
                    if (x + 1 < Game.MapWidth && (Game.Map[x + 1, y] == null
                        || Game.Map[x + 1, y].GetImageFileName() != "Sack.png"))
                        return new CreatureCommand() { DeltaX = 1, DeltaY = 0 };
                    break;
            }
            return new CreatureCommand();
        }

        public bool DeadInConflict(ICreature conflictedObject)
        {
            return conflictedObject.GetImageFileName() == "Sack.png"
                || conflictedObject.GetImageFileName() == "Monster.png";
        }

        public int GetDrawingPriority()
        {
            return 3;
        }

        public string GetImageFileName()
        {
            return "Digger.png";
        }
    }

    public class Sack : ICreature
    {
        int timeInAir = 0;
        public CreatureCommand Act(int x, int y)
        {
            if (y + 1 < Game.MapHeight && (Game.Map[x, y + 1] == null 
                || ((Game.Map[x, y + 1].GetImageFileName() == "Digger.png"
                || Game.Map[x, y + 1].GetImageFileName() == "Monster.png") && timeInAir > 0)))
            {
                timeInAir++;
                return new CreatureCommand() { DeltaY = 1 };
            }

            if (y + 1 == Game.MapHeight || !(Game.Map[x, y + 1] == null 
                || Game.Map[x, y + 1].GetImageFileName() == "Digger.png"
                || Game.Map[x, y + 1].GetImageFileName() == "Monster.png"))
            {
                if (timeInAir == 1) timeInAir = 0;
                if (timeInAir > 1)
                {
                    timeInAir = 0;
                    return new CreatureCommand() { TransformTo = new Gold() };
                }
            }
            return new CreatureCommand();
        }

        public bool DeadInConflict(ICreature conflictedObject)
        {
            return false;
        }

        public int GetDrawingPriority()
        {
            return 1;
        }

        public string GetImageFileName()
        {
            return "Sack.png";
        }
    }

    public class Gold : ICreature
    {
        public CreatureCommand Act(int x, int y)
        {
            return new CreatureCommand();
        }

        public bool DeadInConflict(ICreature conflictedObject)
        {
            if (conflictedObject.GetImageFileName() == "Digger.png")
            {
                Game.Scores += 10;
                return true;
            }
            return conflictedObject.GetImageFileName() == "Monster.png";
        }

        public int GetDrawingPriority()
        {
            return 4;
        }

        public string GetImageFileName()
        {
            return "Gold.png";
        }
    }

    public class Monster : ICreature
    {
        enum Direction
        {
            Up,
            Left,
            Down,
            Right
        }

        public CreatureCommand Act(int x, int y)
        {
            Direction mainDirection = (Direction)10;
            Direction secondaryDirection = (Direction)10;

            int distanceToPlayer = 0;
            int playerX;
            int playerY = 0;
            var helpLogic = new int[2];
            var breakFlag = false;

            for (playerX = 0; playerX < Game.MapWidth; playerX++)
            {
                for (playerY = 0; playerY < Game.MapHeight; playerY++)
                {
                    if (Game.Map[playerX, playerY] != null && Game.Map[playerX, playerY].GetImageFileName() == "Digger.png")
                    {
                        distanceToPlayer = Math.Abs(playerX - x) + Math.Abs(playerY - y);
                        helpLogic[0] = playerX > x ? 1 : 0;
                        helpLogic[1] = playerY > y ? 1 : 0;
                        int t = Math.Abs(playerX - x) > Math.Abs(playerY - y) ? 1 : 0;
                        var e = (3 - 2 * helpLogic[1]) * (helpLogic[0] + helpLogic[1]);
                        mainDirection = (Direction)((e + ((e + t) % 2)) % 4);
                        secondaryDirection = (Direction)((e + 1 - ((e + t) % 2)) % 4);
                        breakFlag = true;
                        break;
                    }
                }
                if (breakFlag) break;
            }

            if (mainDirection == (Direction)10) 
                return new CreatureCommand();

            var availableDirection = new bool[4];
            if (y - 1 >= 0 && (Game.Map[x, y - 1] == null
                || Game.Map[x, y - 1].GetImageFileName() == "Digger.png"
                || Game.Map[x, y - 1].GetImageFileName() == "Gold.png")) availableDirection[0] = true;
            if (x - 1 >= 0 && (Game.Map[x - 1, y] == null
                || Game.Map[x - 1, y].GetImageFileName() == "Digger.png"
                || Game.Map[x - 1, y].GetImageFileName() == "Gold.png")) availableDirection[1] = true;
            if (y + 1 < Game.MapHeight && (Game.Map[x, y + 1] == null
                || Game.Map[x, y + 1].GetImageFileName() == "Digger.png"
                || Game.Map[x, y + 1].GetImageFileName() == "Gold.png")) availableDirection[2] = true;
            if (x + 1 < Game.MapWidth && (Game.Map[x + 1, y] == null
                || Game.Map[x + 1, y].GetImageFileName() == "Digger.png"
                || Game.Map[x + 1, y].GetImageFileName() == "Gold.png")) availableDirection[3] = true;

            int result;
            if (availableDirection[(int)mainDirection]) result = (int)mainDirection;
            else if (availableDirection[(int)secondaryDirection]) result = (int)secondaryDirection;
            else return new CreatureCommand();

            helpLogic[0] = result % 2 * (result / 2 * 2 - 1);
            helpLogic[1] = (result + 1) % 2 * (result / 2 * 2 - 1);

            if (Math.Abs(playerX - x - helpLogic[0]) + Math.Abs(playerY - y - helpLogic[1]) > distanceToPlayer)
                return new CreatureCommand();

            return new CreatureCommand() { DeltaX = helpLogic[0], DeltaY = helpLogic[1] };
        }

        public bool DeadInConflict(ICreature conflictedObject)
        {
            return conflictedObject.GetImageFileName() == "Sack.png" 
                || conflictedObject.GetImageFileName() == "Monster.png";
        }

        public int GetDrawingPriority()
        {
            return 2;
        }

        public string GetImageFileName()
        {
            return "Monster.png";
        }
    }
}
