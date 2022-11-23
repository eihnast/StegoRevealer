namespace StegoRevealer.StegoCore
{
    public struct ScIndexPair : ScValuesPair<int>
    {
        public int FirstIndex { get; set; }
        public int SecondIndex { get; set; }

        public int FirstValue { get { return FirstIndex; } set { FirstIndex = value; } }
        public int SecondValue { get { return SecondIndex; } set { SecondIndex = value; } }

        public ScIndexPair(int firstIndex, int secondIndex)
        {
            this.FirstIndex = firstIndex;
            this.SecondIndex = secondIndex;
        }

        public Sc2DPoint AsPoint() => new Sc2DPoint(FirstIndex, SecondIndex);

        public (int, int) AsTuple() => (FirstIndex, SecondIndex);
    }
}
