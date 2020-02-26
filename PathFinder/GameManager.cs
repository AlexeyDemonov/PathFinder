using System;
using System.Collections.Generic;
using System.Drawing;

namespace PathFinder
{
    class GameManager
    {
        //========================================================
        //Fields
        TableHolder<byte> gameTableHolder = new TableHolder<byte>();

        byte fieldSizeX = 10;
        byte fieldSizeY = 10;

        Color defaultColor = SystemColors.ButtonFace;
        Color pathColor = Color.LightCoral;
        Color blockColor = Color.Black;

        byte blockValue = byte.MaxValue;
        byte unblockValue = default(byte);

        InterfaceMode currentmode;

        List<Coord> lastpath;
        byte[,] lastSavedTable;

        bool clockwise = true;
        bool showNumbers = false;

        //========================================================
        //Constructor
        public GameManager()
        {
            gameTableHolder.CreateNewEmptyTable(fieldSizeX, fieldSizeY);
            currentmode = InterfaceMode.GAME;
        }

        //========================================================
        //Events
        public event Action<byte, byte, Color> Command_MarkInterfaceCell;

        public event Action<byte, byte, string> Command_PrintInterfaceCell;

        //========================================================
        //Handlers
        public void Handle_InterfaceModeChanged(InterfaceMode mode)
        {
            this.currentmode = mode;

            if (mode == InterfaceMode.SETUP)
            {
                if (lastpath != null)
                {
                    PaintThePath(lastpath, defaultColor);
                    lastpath = null;
                }

                if (showNumbers == true)
                    EraseNumbers();

                if (lastSavedTable != null)
                    lastSavedTable = null;
            }
        }

        public void Handle_XYcellClick(byte X, byte Y)
        {
            switch (currentmode)
            {
                case InterfaceMode.SETUP:
                    DoSetupAt(X, Y);
                    break;

                case InterfaceMode.GAME:
                    DoGameAt(X, Y);
                    break;

                default:
                    throw new Exception("GameManager.Handle_XYcellClick: Undefined game mode");
            }
        }

        public void Handle_ShowNumbersChanged(bool showNow)
        {
            if /*currently*/(showNumbers == true && showNow == false)
            {
                EraseNumbers();
            }

            if /*currently*/(showNumbers == false && showNow == true && lastSavedTable != null)
            {
                PrintNumbers(lastSavedTable);
            }

            showNumbers = showNow;
        }

        //========================================================
        //Methods
        void PaintThePath(List<Coord> path, Color color)
        {
            foreach (Coord step in path)
                Command_MarkInterfaceCell?.Invoke(step.X, step.Y, color);
        }

        void EraseNumbers()
        {
            for (byte x = 0; x < fieldSizeX; x++)
            {
                for (byte y = 0; y < fieldSizeY; y++)
                {
                    Command_PrintInterfaceCell?.Invoke(x, y, string.Empty);
                }
            }
        }

        void PrintNumbers(byte[,] table)
        {
            for (byte x = 0; x < fieldSizeX; x++)
            {
                for (byte y = 0; y < fieldSizeY; y++)
                {
                    Command_PrintInterfaceCell?.Invoke(x, y, table[x, y].ToString());
                }
            }
        }

        //=============================
        //Game setup part
        void DoSetupAt(byte X, byte Y)
        {
            byte currentCellValue = gameTableHolder.GetValue(X, Y);

            if (currentCellValue == blockValue)
            {
                gameTableHolder.SetValue(X, Y, unblockValue);
                Command_MarkInterfaceCell?.Invoke(X, Y, defaultColor);
            }
            else if (currentCellValue == unblockValue)
            {
                gameTableHolder.SetValue(X, Y, blockValue);
                Command_MarkInterfaceCell?.Invoke(X, Y, blockColor);
            }
            else
            {
                throw new Exception("GameManager.DoSetupAt: Cell value was invalid (not 0 and not 255), was table at tableholder itself changed somwhere else?");
            }
        }

        //=============================
        //Game play part
        void DoGameAt(byte X, byte Y)
        {
            /*guardian*/
            if (gameTableHolder.GetValue(X, Y) == blockValue)
                return;

            if (lastpath == null)/*first step of the game*/
            {
                lastpath = new List<Coord>();
                lastpath.Add(new Coord(X, Y));
            }
            else
            {
                PaintThePath(lastpath, defaultColor);
                Coord lastcell = lastpath[0];

                var pond = gameTableHolder.GetTableCopy();
                var disturbedPond = ThrowAStoneAt(pond, lastcell.X, lastcell.Y);

                //Show all cells markings for user
                if (showNumbers == true)
                {
                    PrintNumbers(disturbedPond);
                }

                /*guardian*/
                if (disturbedPond[X, Y] == default(byte) && lastpath != null)//There are no path between two cells at all
                {
                    PaintThePath(lastpath, defaultColor);
                    lastpath = null;
                    return;
                }

                var path = SurfBackToOriginFrom(disturbedPond, X, Y);
                PaintThePath(path, pathColor);
                lastpath = path;
                lastSavedTable = disturbedPond;
            }
        }

        byte[,] ThrowAStoneAt(byte[,] pond, byte PosX, byte PosY)
        {
            try
            {
                pond[PosX, PosY] = 1;
            }
            catch (IndexOutOfRangeException)
            {
                throw new Exception("GameManager.ThrowAStoneAt: First mark was out of range");
            }

            List<Coord> markedcells = new List<Coord>();
            markedcells.Add(new Coord(PosX, PosY));//Add first cell

            do
            {
                List<Coord> nextmarked = new List<Coord>();

                foreach (var cell in markedcells)
                {
                    nextmarked.AddRange(MarkNeighbourCellsAndReturnThem(pond, cell));
                }

                markedcells.Clear();
                markedcells = nextmarked;
            } while (markedcells.Count > 0);

            return pond;
        }

        List<Coord> MarkNeighbourCellsAndReturnThem(byte[,] pond, Coord cell)
        {
            List<Coord> neighboursToMark = FindMatchingNeighbours
                (
                /*where:*/pond,
                /*origin cell:*/cell,
                /*predicate for matching:*/(neighbour) => pond[neighbour.X, neighbour.Y] == unblockValue
                );

            if (neighboursToMark != null && neighboursToMark.Count > 0)
            {
                byte currentMark = pond[cell.X, cell.Y];
                byte nextMark = (byte)(currentMark + 1);

                foreach (var neighbour in neighboursToMark)
                    pond[neighbour.X, neighbour.Y] = nextMark;
            }

            return neighboursToMark;
        }

        List<Coord> SurfBackToOriginFrom(byte[,] disturbedPond, byte PosX, byte PosY)
        {
            try
            {
                if (disturbedPond[PosX, PosY] == blockValue)
                    throw new Exception("GameManager.SurfBackToOriginFrom: provided coordinates point to blocked field");
            }
            catch (IndexOutOfRangeException)
            {
                throw new Exception("GameManager.SurfBackToOriginFrom: provided coordinates are out of game field");
            }

            List<Coord> result = new List<Coord>();
            result.Add(new Coord(PosX, PosY));

            byte currentMark = disturbedPond[PosX, PosY];

            while (currentMark != 1)
            {
                Coord nextstep = FindNextStep(disturbedPond, result[result.Count - 1]);
                currentMark = disturbedPond[nextstep.X, nextstep.Y];
                result.Add(nextstep);
            }

            clockwise = !clockwise;//swap the flag

            return result;
        }

        Coord FindNextStep(byte[,] disturbedPond, Coord cell)
        {
            byte currentmark = disturbedPond[cell.X, cell.Y];
            byte nextStepMark = (byte)(currentmark - 1);

            List<Coord> possibleSteps = FindMatchingNeighbours
                (
                /*where:*/disturbedPond,
                /*origin cell:*/ cell,
                /*predicate for matching:*/ (neighbour) => disturbedPond[neighbour.X, neighbour.Y] == nextStepMark
                );

            if (possibleSteps != null && possibleSteps.Count > 0)
            {
                if (clockwise)
                    return possibleSteps[0];
                else
                    return possibleSteps[possibleSteps.Count - 1];
            }
            else//All the neighbours are not valid for the next step which shouldn't be possible
                throw new Exception("GameManager.FindNextStep: did not found next step at all");
        }

        List<Coord> FindMatchingNeighbours(byte[,] pond, Coord cell, Func<Coord, bool> neighbourMatchPredicate)
        {
            List<Coord> matching = new List<Coord>(capacity: 4);

            var neighbours = new Coord[]
            {
                new Coord((byte)(cell.X - 1),        cell.Y     ),//Left
                new Coord((byte)(cell.X + 1),        cell.Y     ),//Right
                new Coord(       cell.X     , (byte)(cell.Y + 1)),//Up
                new Coord(       cell.X     , (byte)(cell.Y - 1)),//Down
            };

            foreach (Coord neighbour in neighbours)
            {
                if (neighbour.X < fieldSizeX && neighbour.Y < fieldSizeY)
                {
                    if (neighbourMatchPredicate.Invoke(neighbour) == true)
                        matching.Add(neighbour);
                }
            }

            return matching;
        }
    }
}