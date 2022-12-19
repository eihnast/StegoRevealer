namespace StegoRevealer.StegoCore.ImageHandlerLib
{
    /// <summary>
    /// Координаты цветового значения в матрице пикселей
    /// </summary>
    public struct ScPointCoords
    {
        /// <summary>
        /// Y-координата (индекс строки)
        /// </summary>
        public int Y { get; set; }

        /// <summary>
        /// X-координата (индекс столбца)
        /// </summary>
        public int X { get; set; }

        /// <summary>
        /// Индекс канала
        /// </summary>
        public int ChannelId { get; set; }


        public ScPointCoords(int y, int x, int channelId)
        {
            Y = y;
            X = x;
            ChannelId = channelId;
        }

        /// <summary>
        /// Получить координаты цветового значения в виде кортежа
        /// </summary>
        public (int, int, int) AsTuple() => (Y, X, ChannelId);
    }
}
