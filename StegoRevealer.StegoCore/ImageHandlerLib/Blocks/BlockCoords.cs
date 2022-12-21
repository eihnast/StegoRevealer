using StegoRevealer.StegoCore.CommonLib.ScTypes;

namespace StegoRevealer.StegoCore.ImageHandlerLib.Blocks
{
    /// <summary>
    /// Структура хранения координат пикселей блока
    /// </summary>
    public readonly struct BlockCoords
    {
        /// <summary>
        /// Координаты левого верхнего пикселя блока
        /// </summary>
        public Sc2DPoint Lt { get; }

        /// <summary>
        /// Координаты правого нижнего пикселя блока
        /// </summary>
        public Sc2DPoint Rd { get; }

        /// <summary>
        /// Индекс канала
        /// </summary>
        public int? ChannelId { get; }


        /// <summary>
        /// Создаёт структуру хранения координат пикселей блока
        /// </summary>
        /// <param name="lt">Координаты левого верхнего пикселя блока</param>
        /// <param name="rd">Координаты правого нижнего пикселя блока</param>
        public BlockCoords(Sc2DPoint lt, Sc2DPoint rd) : this(lt, rd, null) { }

        /// <summary>
        /// Создаёт структуру хранения координат пикселей блока
        /// </summary>
        /// <param name="lt">Координаты левого верхнего пикселя блока</param>
        /// <param name="rd">Координаты правого нижнего пикселя блока</param>
        /// <param name="channelId">Индекс цветового канала</param>
        public BlockCoords(Sc2DPoint lt, Sc2DPoint rd, int? channelId)
        {
            Lt = lt;
            Rd = rd;
            ChannelId = channelId;
        }
    }
}
