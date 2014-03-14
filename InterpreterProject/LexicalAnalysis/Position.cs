using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace InterpreterProject.LexicalAnalysis
{
    public class Position : IComparable<Position>
    {
        int row;
        public int Row
        {
            get { return row; }
        } 

        int column;
        public int Column
        {
            get { return column; }
        } 

        public Position(int row, int col)
        {
            this.row = row;
            this.column = col;
        }

        public int CompareTo(Position other)
        {
            if (this.row < other.row)
                return -1;
            if (this.row > other.row)
                return 1;
            if (this.column < other.column)
                return -1;
            if (this.column > other.column)
                return 1;
            return 0;
        }

        public override string ToString()
        {
            return "(" + row + "," + column + ")";
        }
    }
}
