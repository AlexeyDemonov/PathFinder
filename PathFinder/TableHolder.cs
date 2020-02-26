using System;

namespace PathFinder
{
    class TableHolder<T>
    {
        //========================================================
        //Fields
        T[,] table;

        byte sizeX;
        byte sizeY;

        //========================================================
        //Methods
        public void CreateNewEmptyTable(byte sizeX, byte sizeY)
        {
            this.sizeX = sizeX;
            this.sizeY = sizeY;
            table = new T[this.sizeX, this.sizeY];
        }

        public void SetValue(byte PosX, byte PosY, T value)
        {
            CheckTable("SetValue");

            table[PosX, PosY] = value;
        }

        public T GetValue(byte PosX, byte PosY)
        {
            CheckTable("GetValue");

            return table[PosX, PosY];
        }

        public T[,] GetTableCopy()
        {
            CheckTable("GetTableCopy");

            T[,] copy = new T[sizeX, sizeY];
            Array.Copy(/*from*/table,/*to*/copy,/*count*/table.Length);
            return copy;
        }

        void CheckTable(string methodName)
        {
            if (table == null)
                throw new Exception($"TableHolder.{methodName}: table must be created with CreateNewEmptyTable method before operating");
        }
    }
}