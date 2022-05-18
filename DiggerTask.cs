using System;
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
            return conflictedObject is Player;
        }

        public int GetDrawingPriority()
        {
            return 2;
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
            var deltaX = 0;
            var deltaY = 0;
            if (Game.KeyPressed == Keys.Up)
            {
                deltaY--;
            }
            if (Game.KeyPressed == Keys.Down)
            {
                deltaY++;
            }
            if (Game.KeyPressed == Keys.Right)
            {
                deltaX++;
            }
            if (Game.KeyPressed == Keys.Left)
            {
                deltaX--;
            }
            deltaX = x + deltaX < Game.MapWidth && x + deltaX >= 0 ? deltaX : 0;
            deltaY = y + deltaY < Game.MapHeight && y + deltaY >= 0 ? deltaY : 0;
            if (Game.Map[x + deltaX, y + deltaY] is Sack)
            {
                deltaX = 0;
                deltaY = 0;
            }
            return new CreatureCommand { DeltaX = deltaX, DeltaY = deltaY };
        }

        public bool DeadInConflict(ICreature conflictedObject)
        {
            return conflictedObject is Monster || conflictedObject is Sack sack && sack.IsFalling;
        }

        public int GetDrawingPriority()
        {
            return 0;
        }

        public string GetImageFileName()
        {
            return "Digger.png";
        }
    }


    public class Sack : ICreature
    {
        private int _fallsCount;


        public bool IsFalling => _fallsCount > 0;


        public CreatureCommand Act(int x, int y)
        {
            if (y + 1 < Game.MapHeight
                && (Game.Map[x, y + 1] == null
                || (_fallsCount > 0 && (Game.Map[x, y + 1] is Player || Game.Map[x, y + 1] is Monster))))
            {
                _fallsCount++;
                return new CreatureCommand { DeltaY = 1 };
            }
            if (_fallsCount > 1)
            {
                return new CreatureCommand { TransformTo = new Gold() };
            }
            _fallsCount = 0;
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
            if (conflictedObject is Player)
            {
                Game.Scores += 10;
                return true;
            }
            if (conflictedObject is Monster)
            {
                return true;
            }
            return false;
        }

        public int GetDrawingPriority()
        {
            return 1;
        }

        public string GetImageFileName()
        {
            return "Gold.png";
        }
    }


    public class Monster : ICreature
    {
        public CreatureCommand Act(int x, int y)
        {
            (int x, int y)? playerPos = null;
            for (int i = 0; i < Game.MapWidth; i++)
            {
                for (int j = 0; j < Game.MapHeight; j++)
                {
                    if (Game.Map[i, j] is Player)
                    {
                        playerPos = (i, j);
                    }
                }
            }
            if (!playerPos.HasValue)
            {
                return new CreatureCommand();
            }
            var deltaX = Math.Sign(playerPos.Value.x - x);
            if (IsAllowToMoveX(x, y, deltaX))
            {
                return new CreatureCommand { DeltaX = deltaX };
            }
            var deltaY = Math.Sign(playerPos.Value.y - y);
            if (IsAllowToMoveY(x, y, deltaY))
            {
                return new CreatureCommand { DeltaY = deltaY };
            }
            return new CreatureCommand();
        }

        private static bool IsAllowToMoveX(int x, int y, int deltaX)
        {
            return deltaX != 0 && !(Game.Map[x + deltaX, y] is Monster)
                            && !(Game.Map[x + deltaX, y] is Terrain)
                            && !(Game.Map[x + deltaX, y] is Sack);
        }

        private static bool IsAllowToMoveY(int x, int y, int deltaY)
        {
            return deltaY != 0 && !(Game.Map[x, y + deltaY] is Monster)
                            && !(Game.Map[x, y + deltaY] is Terrain)
                            && !(Game.Map[x, y + deltaY] is Sack);
        }

        public bool DeadInConflict(ICreature conflictedObject)
        {
            return conflictedObject is Monster || 
                conflictedObject is Sack sack && sack.IsFalling;
        }

        public int GetDrawingPriority()
        {
            return 0;
        }

        public string GetImageFileName()
        {
            return "Monster.png";
        }
    }
}
