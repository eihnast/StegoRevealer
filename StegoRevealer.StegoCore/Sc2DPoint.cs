using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StegoRevealer.StegoCore
{
    public struct Sc2DPoint : ScValuesPair<int>
    {
        public int Y { get; set; }
        public int X { get; set; }

        public int FirstValue { get { return Y; } set { Y = value; } }
        public int SecondValue { get { return X; } set { X = value; } }

        public Sc2DPoint(int y, int x)
        {
            this.Y = y;
            this.X = x;
        }

        public ScIndexPair AsPair() => new ScIndexPair(Y, X);

        public (int, int) AsTuple() => (Y, X);
    }
}
