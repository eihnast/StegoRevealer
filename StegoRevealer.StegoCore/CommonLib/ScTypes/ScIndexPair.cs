namespace StegoRevealer.StegoCore.CommonLib.ScTypes
{
    /// <summary>
    /// Пара индексов
    /// </summary>
    public struct ScIndexPair : ScValuesPair<int>
    {
        /// <summary>
        /// Первый индекс
        /// </summary>
        public int FirstIndex { get; set; }

        /// <summary>
        /// Второй индекс
        /// </summary>
        public int SecondIndex { get; set; }


        /// <inheritdoc/>
        public int FirstValue { get { return FirstIndex; } set { FirstIndex = value; } }
        /// <inheritdoc/>
        public int SecondValue { get { return SecondIndex; } set { SecondIndex = value; } }


        public ScIndexPair(int firstIndex, int secondIndex)
        {
            FirstIndex = firstIndex;
            SecondIndex = secondIndex;
        }

        /// <summary>
        /// Преобразовать пару индексов в координаты точки
        /// </summary>
        public Sc2DPoint AsPoint() => new Sc2DPoint(FirstIndex, SecondIndex);

        /// <summary>
        /// Получить значения пары индексов как кортеж
        /// </summary>
        public (int, int) AsTuple() => (FirstIndex, SecondIndex);

        public override string ToString()
        {
            return $"({FirstIndex}; {SecondIndex})";
        }
    }
}
