using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace InterpreterProject.LexicalAnalysis
{
    public class Position : IComparable<Position>
    {
        public readonly int row;
        public readonly int col;

        public Position(int row, int col)
        {
            this.row = row;
            this.col = col;
        }

        public int CompareTo(Position other)
        {
            if (this.row < other.row)
                return -1;
            if (this.row > other.row)
                return 1;
            if (this.col < other.col)
                return -1;
            if (this.col > other.col)
                return 1;
            return 0;
        }

        public override string ToString()
        {
            return "(" + row + "," + col + ")";
        }
    }
}
